using System.Data.SqlClient;

public class DatabaseHelper
{
    private string connectionString;

    public DatabaseHelper(string connectionString)
    {
        this.connectionString = connectionString;
    }

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
                            Id = (int)reader["CardId"],
                            AccountId = (int)reader["AccountId"],
                            CardNumber = (string)reader["CardNumber"],
                            Expiry = (DateTime)reader["Expiry"],
                            PinHash = (string)reader["PinHash"],
                            Status = (int)reader["Status"]
                        };

                        var account = new Account
                        {
                            Id = (int)reader["AccountId"],
                            Number = (string)reader["AccountNumber"],
                            Currency = (string)reader["Currency"],
                            Balance = (decimal)reader["Balance"]
                        };

                        var client = new Client
                        {
                            Id = (int)reader["ClientId"],
                            FirstName = (string)reader["FirstName"],
                            LastName = (string)reader["LastName"],
                            PassportNo = (string)reader["PassportNo"],
                            Phone = (string)reader["Phone"]
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

    public List<string> GetTransactionHistory(int cardId)
    {
        var history = new List<string>();
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand(@"
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
                    ORDER BY t.PerformedAt DESC",
                    connection);

                command.Parameters.AddWithValue("@cardId", cardId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var transaction = $"{reader["PerformedAt"]:dd.MM.yyyy HH:mm} | " +
                                         $"{reader["TransactionType"]} | " +
                                         $"{reader["Amount"]:N} {reader["Currency"]} | " +
                                         $"Баланс: {reader["BalanceAfter"]:N} | " +
                                         $"Банкомат: {reader["AtmName"]}";
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

    // Метод для проверки существования карты (дополнительный)
    public bool CardExists(string cardNumber)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand(
                    "SELECT COUNT(*) FROM Cards WHERE CardNumber = @cardNumber AND Status = 1",
                    connection);

                command.Parameters.AddWithValue("@cardNumber", cardNumber);
                var count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при проверке карты: {ex.Message}");
            return false;
        }
    }
}