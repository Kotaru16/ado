public interface IDatabaseHelper
{
    (Card?, Account?, Client?) Authenticate(string cardNumber, string pin);
    bool Withdraw(int cardId, int atmId, decimal amount, decimal newBalance);
    bool Deposit(int cardId, int atmId, decimal amount, decimal newBalance);
    List<string> GetTransactionHistory(int cardId, bool sortAscending = false);
    List<Client> GetAllClients();
    List<Client> SearchClients(string searchTerm);
    bool AddClient(string firstName, string lastName, string passportNo, string phone);
    bool AddAccountAndCard(int clientId, string accountNumber, string cardNumber, decimal initialBalance = 0);
}