public class ATM
{
    private IDatabaseHelper dbHelper;
    private Card? currentCard;
    private Account? currentAccount;
    private Client? currentClient;
    private int currentAtmId = 1; // ID текущего банкомата

    public ATM(string connectionString)
    {
        dbHelper = new DatabaseHelper(connectionString);
    }

    // Конструктор для тестов - принимает IDatabaseHelper напрямую
    public ATM(IDatabaseHelper databaseHelper)
    {
        dbHelper = databaseHelper;
    }

    public bool Authenticate(string? cardNumber, string? pin)
    {
        if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(pin))
            return false;

        var result = dbHelper.Authenticate(cardNumber, pin);
        var card = result.Item1;
        var account = result.Item2;
        var client = result.Item3;

        if (card != null && account != null && client != null)
        {
            currentCard = card;
            currentAccount = account;
            currentClient = client;
            return true;
        }

        return false;
    }

    public void DisplayClientInfo()
    {
        if (currentClient != null && currentAccount != null)
        {
            Console.WriteLine($"\nКлиент: {currentClient.FirstName} {currentClient.LastName}");
            Console.WriteLine($"Счет: {currentAccount.Number}");
            Console.WriteLine($"Баланс: {currentAccount.Balance:N} {currentAccount.Currency}");
        }
    }

    public decimal CheckBalance()
    {
        return currentAccount?.Balance ?? 0;
    }

    public bool Withdraw(decimal amount)
    {
        if (currentCard == null || currentAccount == null ||
            amount <= 0 || amount > currentAccount.Balance)
            return false;

        decimal newBalance = currentAccount.Balance - amount;
        bool success = dbHelper.Withdraw(currentCard.Id, currentAtmId, amount, newBalance);

        if (success)
            currentAccount.Balance = newBalance;

        return success;
    }

    public bool Deposit(decimal amount)
    {
        if (currentCard == null || currentAccount == null || amount <= 0)
            return false;

        decimal newBalance = currentAccount.Balance + amount;
        bool success = dbHelper.Deposit(currentCard.Id, currentAtmId, amount, newBalance);

        if (success)
            currentAccount.Balance = newBalance;

        return success;
    }

    public void ShowTransactionHistory()
    {
        if (currentCard == null) return;

        Console.WriteLine("\n=== ИСТОРИЯ ОПЕРАЦИЙ ===");
        Console.WriteLine("1. Сначала новые операции");
        Console.WriteLine("2. Сначала старые операции");
        Console.Write("Выберите сортировку: ");

        string? choice = Console.ReadLine();
        bool sortAscending = false; // по умолчанию новые сначала

        if (choice == "2")
        {
            sortAscending = true; // старые сначала
            Console.WriteLine("\n=== ИСТОРИЯ ОПЕРАЦИЙ (сначала старые) ===");
        }
        else
        {
            Console.WriteLine("\n=== ИСТОРИЯ ОПЕРАЦИЙ (сначала новые) ===");
        }

        var history = dbHelper.GetTransactionHistory(currentCard.Id, sortAscending);

        if (history.Count == 0)
        {
            Console.WriteLine("Операций не найдено");
            return;
        }

        foreach (var transaction in history)
        {
            Console.WriteLine(transaction);
        }
        Console.WriteLine($"\nВсего операций: {history.Count}");
    }

    public void Logout()
    {
        currentCard = null;
        currentAccount = null;
        currentClient = null;
    }
}