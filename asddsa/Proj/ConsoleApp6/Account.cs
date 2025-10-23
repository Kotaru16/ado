public class Account
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}

public class Card
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public string PinHash { get; set; } = string.Empty;
    public int Status { get; set; }
}

public class Client
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PassportNo { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}