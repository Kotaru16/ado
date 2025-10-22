public class ATM
{
    private DatabaseHelper dbHelper;
    private Card? currentCard;
    private Account? currentAccount;
    private Client? currentClient;
    private int currentAtmId = 1; // ID текущего банкомата

    public ATM(string connectionString)
    {
        dbHelper = new DatabaseHelper(connectionString);
    }

    public bool Authenticate(string? cardNumber, string? pin)
    {
        if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(pin))
            return false;

        var (card, account, client) = dbHelper.Authenticate(cardNumber, pin);

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

        var history = dbHelper.GetTransactionHistory(currentCard.Id);
        Console.WriteLine("\n=== История операций ===");
        foreach (var transaction in history)
        {
            Console.WriteLine(transaction);
        }
    }

    public void Logout()
    {
        currentCard = null;
        currentAccount = null;
        currentClient = null;
    }
}