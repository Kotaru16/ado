using System;
using System.Collections.Generic;

public class SimpleTests
{
    public void RunAllTests()
    {
        Console.WriteLine("тесты");

        TestAuthentication();
        TestWithdraw();
        TestDeposit();
        TestBalanceCheck();

     
    }

    public void TestAuthentication()
    {
        Console.WriteLine("\n Тест 1: Аутентификация");

        // Создаем тестовый ATM с тестовым DatabaseHelper
        var testDbHelper = new TestDatabaseHelper();
        var atm = new ATM(testDbHelper);

        // Тест 1.1: Успешная аутентификация
        bool result1 = atm.Authenticate("8600123456781234", "0000");
        if (result1)
        {
            Console.WriteLine(" Успешная аутентификация - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Успешная аутентификация - ПРОВАЛЕН");
        }

        // Тест 1.2: Неудачная аутентификация (неправильный PIN)
        bool result2 = atm.Authenticate("8600123456781234", "9999");
        if (!result2)
        {
            Console.WriteLine(" Неудачная аутентификация - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Неудачная аутентификация - ПРОВАЛЕН");
        }

        // Тест 1.3: Неудачная аутентификация (несуществующая карта)
        bool result3 = atm.Authenticate("9999999999999999", "0000");
        if (!result3)
        {
            Console.WriteLine(" Аутентификация с несуществующей картой - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Аутентификация с несуществующей картой - ПРОВАЛЕН");
        }
    }

    public void TestBalanceCheck()
    {
        Console.WriteLine("\n Тест 2: Проверка баланса");

        var testDbHelper = new TestDatabaseHelper();
        var atm = new ATM(testDbHelper);

        // Аутентифицируемся
        atm.Authenticate("8600123456781234", "0000");
        decimal balance = atm.CheckBalance();

        if (balance == 1500000.00m)
        {
            Console.WriteLine(" Проверка баланса - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine($" Проверка баланса - ПРОВАЛЕН (ожидалось 1500000, получено {balance})");
        }

        // Проверка баланса без аутентификации
        var atm2 = new ATM(testDbHelper);
        decimal balanceWithoutAuth = atm2.CheckBalance();
        if (balanceWithoutAuth == 0)
        {
            Console.WriteLine(" Проверка баланса без аутентификации - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine($" Проверка баланса без аутентификации - ПРОВАЛЕН (ожидалось 0, получено {balanceWithoutAuth})");
        }
    }

    public void TestWithdraw()
    {
        Console.WriteLine("\n Тест 3: Снятие средств");

        var testDbHelper = new TestDatabaseHelper();
        var atm = new ATM(testDbHelper);

        // Аутентифицируемся
        atm.Authenticate("8600123456781234", "0000");
        decimal initialBalance = atm.CheckBalance();

        // Тест 3.1: Успешное снятие
        bool result1 = atm.Withdraw(100000.00m);
        decimal balanceAfterWithdraw = atm.CheckBalance();

        if (result1 && balanceAfterWithdraw == initialBalance - 100000.00m)
        {
            Console.WriteLine(" Успешное снятие - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Успешное снятие - ПРОВАЛЕН");
        }

        // Тест 3.2: Неудачное снятие (недостаточно средств)
        var atm2 = new ATM(testDbHelper);
        atm2.Authenticate("8600123456781234", "0000"); // переаутентифицируем для сброса
        bool result2 = atm2.Withdraw(5000000.00m); // Сумма больше баланса
        decimal balanceAfterFailedWithdraw = atm2.CheckBalance();

        if (!result2 && balanceAfterFailedWithdraw == initialBalance)
        {
            Console.WriteLine(" Неудачное снятие (недостаточно средств) - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Неудачное снятие (недостаточно средств) - ПРОВАЛЕН");
        }

        // Тест 3.3: Неудачное снятие (отрицательная сумма)
        bool result3 = atm2.Withdraw(-100.00m);
        if (!result3)
        {
            Console.WriteLine(" Неудачное снятие (отрицательная сумма) - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Неудачное снятие (отрицательная сумма) - ПРОВАЛЕН");
        }
    }

    public void TestDeposit()
    {
        Console.WriteLine("\n Тест 4: Пополнение счета");

        var testDbHelper = new TestDatabaseHelper();
        var atm = new ATM(testDbHelper);

        // Аутентифицируемся
        atm.Authenticate("8600123456781234", "0000");
        decimal initialBalance = atm.CheckBalance();

        // Тест 4.1: Успешное пополнение
        bool result1 = atm.Deposit(50000.00m);
        decimal balanceAfterDeposit = atm.CheckBalance();

        if (result1 && balanceAfterDeposit == initialBalance + 50000.00m)
        {
            Console.WriteLine(" Успешное пополнение - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Успешное пополнение - ПРОВАЛЕН");
        }

        // Тест 4.2: Неудачное пополнение (отрицательная сумма)
        var atm2 = new ATM(testDbHelper);
        atm2.Authenticate("8600123456781234", "0000");
        bool result2 = atm2.Deposit(-100.00m);
        if (!result2)
        {
            Console.WriteLine(" Неудачное пополнение (отрицательная сумма) - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Неудачное пополнение (отрицательная сумма) - ПРОВАЛЕН");
        }

        // Тест 4.3: Неудачное пополнение (нулевая сумма)
        bool result3 = atm2.Deposit(0);
        if (!result3)
        {
            Console.WriteLine(" Неудачное пополнение (нулевая сумма) - ПРОЙДЕН");
        }
        else
        {
            Console.WriteLine(" Неудачное пополнение (нулевая сумма) - ПРОВАЛЕН");
        }
    }
}

// Тестовый DatabaseHelper, реализующий IDatabaseHelper
public class TestDatabaseHelper : IDatabaseHelper
{
    private Dictionary<string, (Card, Account, Client)> _testData;

    public TestDatabaseHelper()
    {
        InitializeTestData();
    }

    private void InitializeTestData()
    {
        _testData = new Dictionary<string, (Card, Account, Client)>();

        var account1 = new Account
        {
            Id = 1,
            Number = "8600123456789012",
            Currency = "UZS",
            Balance = 1500000.00m
        };

        var card1 = new Card
        {
            Id = 1,
            AccountId = 1,
            CardNumber = "8600123456781234",
            Expiry = DateTime.Now.AddYears(1),
            PinHash = "hash1",
            Status = 1
        };

        var client1 = new Client
        {
            Id = 1,
            FirstName = "Иван",
            LastName = "Иванов",
            PassportNo = "AB1234567",
            Phone = "+998901234567"
        };

        _testData["8600123456781234"] = (card1, account1, client1);
    }

    public (Card?, Account?, Client?) Authenticate(string cardNumber, string pin)
    {
        // Универсальный PIN для тестирования
        if (pin != "0000")
            return (null, null, null);

        if (_testData.ContainsKey(cardNumber))
        {
            var data = _testData[cardNumber];
            if (data.Item1.Expiry > DateTime.Now && data.Item1.Status == 1)
            {
                return data;
            }
        }

        return (null, null, null);
    }

    public bool Withdraw(int cardId, int atmId, decimal amount, decimal newBalance)
    {
        foreach (var data in _testData.Values)
        {
            if (data.Item1.Id == cardId)
            {
                var account = data.Item2;
                account.Balance = newBalance;
                return true;
            }
        }
        return false;
    }

    public bool Deposit(int cardId, int atmId, decimal amount, decimal newBalance)
    {
        foreach (var data in _testData.Values)
        {
            if (data.Item1.Id == cardId)
            {
                var account = data.Item2;
                account.Balance = newBalance;
                return true;
            }
        }
        return false;
    }

    public List<string> GetTransactionHistory(int cardId, bool sortAscending = false)
    {
        return new List<string>
        {
            "01.12.2023 10:30 | Снятие | 200,000.00 UZS | Баланс: 1,300,000.00 | Банкомат: Главный",
            "15.11.2023 14:15 | Пополнение | 500,000.00 UZS | Баланс: 1,500,000.00 | Банкомат: Филиал"
        };
    }

    public List<Client> GetAllClients()
    {
        throw new NotImplementedException();
    }

    public List<Client> SearchClients(string searchTerm)
    {
        throw new NotImplementedException();
    }

    public bool AddClient(string firstName, string lastName, string passportNo, string phone)
    {
        throw new NotImplementedException();
    }

    public bool AddAccountAndCard(int clientId, string accountNumber, string cardNumber, decimal initialBalance = 0)
    {
        throw new NotImplementedException();
    }
}