using System.Data.SqlClient;
using System.Linq;

public class DatabaseHelper : IDatabaseHelper
{
    private string connectionString;

    public DatabaseHelper(string connectionString)
    {
        this.connectionString = connectionString;
    }

    [Obsolete]
    public (Card?, Account?, Client?) Authenticate(string cardNumber, string pin)
    {
        try
        {
            // Универсальный PIN для тестирования всех карт
            if (pin != "0000")
            {
                Console.WriteLine("Неверный PIN! Используйте 0000 для тестирования");
                return (null, null, null);
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand(@"
                    SELECT 
                        c.Id as CardId, c.AccountId, c.CardNumber, c.Expiry, c.PinHash, c.Status,
                        a.Id as AccountId, a.Number as AccountNumber, a.Currency, a.Balance,
                        cl.Id as ClientId, cl.FirstName, cl.LastName, cl.PassportNo, cl.Phone
                    FROM Cards c
                    JOIN Accounts a ON c.AccountId = a.Id
                    JOIN ClientAccounts ca ON a.Id = ca.AccountId
                    JOIN Clients cl ON ca.ClientId = cl.Id
                    WHERE c.CardNumber = @cardNumber 
                    AND c.Status = 1 -- Active
                    AND c.Expiry > GETUTCDATE()",
                    connection);

                command.Parameters.AddWithValue("@cardNumber", cardNumber);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var card = new Card
                        {
                            Id = reader.GetInt32(0),
                            AccountId = reader.GetInt32(1),
                            CardNumber = reader.GetString(2),
                            Expiry = reader.GetDateTime(3),
                            PinHash = reader.GetString(4),
                            Status = reader.GetInt32(5)
                        };

                        var account = new Account
                        {
                            Id = reader.GetInt32(6),
                            Number = reader.GetString(7),
                            Currency = reader.GetString(8),
                            Balance = reader.GetDecimal(9)
                        };

                        var client = new Client
                        {
                            Id = reader.GetInt32(10),
                            FirstName = reader.GetString(11),
                            LastName = reader.GetString(12),
                            PassportNo = reader.GetString(13),
                            Phone = reader.GetString(14)
                        };

                        return (card, account, client);
                    }
                }
            }
            return (null, null, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
            return (null, null, null);
        }
    }

    [Obsolete]
    public bool Withdraw(int cardId, int atmId, decimal amount, decimal newBalance)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Обновляем баланс счета
                        var updateCommand = new SqlCommand(@"
                            UPDATE Accounts 
                            SET Balance = @newBalance 
                            WHERE Id = (
                                SELECT AccountId FROM Cards WHERE Id = @cardId
                            )", connection, transaction);

                        updateCommand.Parameters.AddWithValue("@newBalance", newBalance);
                        updateCommand.Parameters.AddWithValue("@cardId", cardId);
                        updateCommand.ExecuteNonQuery();

                        // Записываем транзакцию
                        var transactionCommand = new SqlCommand(@"
                            INSERT INTO Transactions (CardId, AtmId, Type, Amount, PerformedAt, BalanceAfter)
                            VALUES (@cardId, @atmId, 1, @amount, GETUTCDATE(), @balanceAfter)",
                            connection, transaction);

                        transactionCommand.Parameters.AddWithValue("@cardId", cardId);
                        transactionCommand.Parameters.AddWithValue("@atmId", atmId);
                        transactionCommand.Parameters.AddWithValue("@amount", amount);
                        transactionCommand.Parameters.AddWithValue("@balanceAfter", newBalance);
                        transactionCommand.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при снятии средств: {ex.Message}");
            return false;
        }
    }

    [Obsolete]
    public bool Deposit(int cardId, int atmId, decimal amount, decimal newBalance)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Обновляем баланс счета
                        var updateCommand = new SqlCommand(@"
                            UPDATE Accounts 
                            SET Balance = @newBalance 
                            WHERE Id = (
                                SELECT AccountId FROM Cards WHERE Id = @cardId
                            )", connection, transaction);

                        updateCommand.Parameters.AddWithValue("@newBalance", newBalance);
                        updateCommand.Parameters.AddWithValue("@cardId", cardId);
                        updateCommand.ExecuteNonQuery();

                        // Записываем транзакцию
                        var transactionCommand = new SqlCommand(@"
                            INSERT INTO Transactions (CardId, AtmId, Type, Amount, PerformedAt, BalanceAfter)
                            VALUES (@cardId, @atmId, 2, @amount, GETUTCDATE(), @balanceAfter)",
                            connection, transaction);

                        transactionCommand.Parameters.AddWithValue("@cardId", cardId);
                        transactionCommand.Parameters.AddWithValue("@atmId", atmId);
                        transactionCommand.Parameters.AddWithValue("@amount", amount);
                        transactionCommand.Parameters.AddWithValue("@balanceAfter", newBalance);
                        transactionCommand.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при пополнении счета: {ex.Message}");
            return false;
        }
    }

    public List<string> GetTransactionHistory(int cardId, bool sortAscending = false)
    {
        var history = new List<string>();
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string orderBy = sortAscending ? "ASC" : "DESC";

                var command = new SqlCommand($@"
                    SELECT TOP 10 
                        t.PerformedAt,
                        CASE t.Type 
                            WHEN 1 THEN 'Снятие' 
                            WHEN 2 THEN 'Пополнение' 
                            WHEN 3 THEN 'Перевод' 
                        END as TransactionType,
                        t.Amount,
                        t.BalanceAfter,
                        a.Name as AtmName,
                        acc.Currency
                    FROM Transactions t
                    JOIN Atms a ON t.AtmId = a.Id
                    JOIN Cards c ON t.CardId = c.Id
                    JOIN Accounts acc ON c.AccountId = acc.Id
                    WHERE t.CardId = @cardId
                    ORDER BY t.PerformedAt {orderBy}",
                    connection);

                command.Parameters.AddWithValue("@cardId", cardId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var transaction = $"{reader.GetDateTime(0):dd.MM.yyyy HH:mm} | " +
                                         $"{reader.GetString(1)} | " +
                                         $"{reader.GetDecimal(2):N} {reader.GetString(5)} | " +
                                         $"Баланс: {reader.GetDecimal(3):N} | " +
                                         $"Банкомат: {reader.GetString(4)}";
                        history.Add(transaction);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении истории: {ex.Message}");
        }
        return history;
    }

    // Методы для админ панели
    public List<Client> GetAllClients()
    {
        var clients = new List<Client>();
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand("SELECT Id, FirstName, LastName, PassportNo, Phone FROM Clients", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            PassportNo = reader.GetString(3),
                            Phone = reader.GetString(4)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении клиентов: {ex.Message}");
        }
        return clients;
    }

    public List<Client> SearchClients(string searchTerm)
    {
        var clients = new List<Client>();
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand(@"
                    SELECT Id, FirstName, LastName, PassportNo, Phone 
                    FROM Clients 
                    WHERE FirstName LIKE @search OR LastName LIKE @search OR PassportNo LIKE @search",
                    connection);

                command.Parameters.AddWithValue("@search", $"%{searchTerm}%");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            Id = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2),
                            PassportNo = reader.GetString(3),
                            Phone = reader.GetString(4)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при поиске клиентов: {ex.Message}");
        }
        return clients;
    }

    public bool AddClient(string firstName, string lastName, string passportNo, string phone)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand(
                    "INSERT INTO Clients (FirstName, LastName, PassportNo, Phone) VALUES (@firstName, @lastName, @passportNo, @phone)",
                    connection);

                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@passportNo", passportNo);
                command.Parameters.AddWithValue("@phone", phone);

                return command.ExecuteNonQuery() > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при добавлении клиента: {ex.Message}");
            return false;
        }
    }

    [Obsolete]
    public bool AddAccountAndCard(int clientId, string accountNumber, string cardNumber, decimal initialBalance = 0)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Создаем счет
                        var accountCommand = new SqlCommand(
                            "INSERT INTO Accounts (Number, Currency, Balance) VALUES (@number, 'UZS', @balance); SELECT SCOPE_IDENTITY();",
                            connection, transaction);

                        accountCommand.Parameters.AddWithValue("@number", accountNumber);
                        accountCommand.Parameters.AddWithValue("@balance", initialBalance);
                        int accountId = Convert.ToInt32(accountCommand.ExecuteScalar());

                        // Создаем карту
                        var cardCommand = new SqlCommand(
                            "INSERT INTO Cards (AccountId, CardNumber, Expiry, PinHash, Status) VALUES (@accountId, @cardNumber, @expiry, @pinHash, 1)",
                            connection, transaction);

                        cardCommand.Parameters.AddWithValue("@accountId", accountId);
                        cardCommand.Parameters.AddWithValue("@cardNumber", cardNumber);
                        cardCommand.Parameters.AddWithValue("@expiry", DateTime.Now.AddYears(3));
                        cardCommand.Parameters.AddWithValue("@pinHash", "0000");
                        cardCommand.ExecuteNonQuery();

                        // Связываем клиента со счетом
                        var clientAccountCommand = new SqlCommand(
                            "INSERT INTO ClientAccounts (ClientId, AccountId, Role) VALUES (@clientId, @accountId, 'Owner')",
                            connection, transaction);

                        clientAccountCommand.Parameters.AddWithValue("@clientId", clientId);
                        clientAccountCommand.Parameters.AddWithValue("@accountId", accountId);
                        clientAccountCommand.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании счета и карты: {ex.Message}");
            return false;
        }
    }
}