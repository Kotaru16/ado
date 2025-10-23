public class ATM
{
    private IDatabaseHelper dbHelper;
    private Card? currentCard;
    private Account? currentAccount;
    private Client? currentClient;
    private int currentAtmId = 1; // ID �������� ���������

    public ATM(string connectionString)
    {
        dbHelper = new DatabaseHelper(connectionString);
    }

    // ����������� ��� ������ - ��������� IDatabaseHelper ��������
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
            Console.WriteLine($"\n������: {currentClient.FirstName} {currentClient.LastName}");
            Console.WriteLine($"����: {currentAccount.Number}");
            Console.WriteLine($"������: {currentAccount.Balance:N} {currentAccount.Currency}");
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

        Console.WriteLine("\n=== ������� �������� ===");
        Console.WriteLine("1. ������� ����� ��������");
        Console.WriteLine("2. ������� ������ ��������");
        Console.Write("�������� ����������: ");

        string? choice = Console.ReadLine();
        bool sortAscending = false; // �� ��������� ����� �������

        if (choice == "2")
        {
            sortAscending = true; // ������ �������
            Console.WriteLine("\n=== ������� �������� (������� ������) ===");
        }
        else
        {
            Console.WriteLine("\n=== ������� �������� (������� �����) ===");
        }

        var history = dbHelper.GetTransactionHistory(currentCard.Id, sortAscending);

        if (history.Count == 0)
        {
            Console.WriteLine("�������� �� �������");
            return;
        }

        foreach (var transaction in history)
        {
            Console.WriteLine(transaction);
        }
        Console.WriteLine($"\n����� ��������: {history.Count}");
    }

    public void Logout()
    {
        currentCard = null;
        currentAccount = null;
        currentClient = null;
    }
}