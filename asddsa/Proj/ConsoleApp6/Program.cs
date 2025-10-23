using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("система банкомата");
        Console.WriteLine("1. Запустить банкомат");
        Console.WriteLine("2. Запустить тесты");
        Console.WriteLine("3. Админ панель");
        Console.Write("Выберите режим: ");

        string? choice = Console.ReadLine();

        string connectionString = "Server=192.168.9.203\\sqlexpress;Database=AtmService;User Id=student1;Password=123456;Encrypt=false;";

        if (choice == "2")
        {
            var tests = new SimpleTests();
            tests.RunAllTests();
        }
        else if (choice == "3")
        {
            var adminPanel = new AdminPanel(connectionString);
            adminPanel.ShowAdminMenu();
        }
        else
        {
            ATM atm = new ATM(connectionString);

            Console.WriteLine("банкомат");
            Console.Write("Введите номер карты: ");
            string? cardNumber = Console.ReadLine();

            Console.Write("Введите PIN: ");
            string? pin = Console.ReadLine();

            if (atm.Authenticate(cardNumber, pin))
            {
                Console.WriteLine("Авторизация успешна!");
                atm.DisplayClientInfo();
                ShowMenu(atm);
            }
            else
            {
                Console.WriteLine("Неверные данные карты или карта заблокирована!");
            }
        }
    }

    static void ShowMenu(ATM atm)
    {
        while (true)
        {
            Console.WriteLine("\n=== МЕНЮ ===");
            Console.WriteLine("1. Проверить баланс");
            Console.WriteLine("2. Снять наличные");
            Console.WriteLine("3. Пополнить счет");
            Console.WriteLine("4. История операций");
            Console.WriteLine("5. Выход");
            Console.Write("Выберите действие: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine($"Ваш баланс: {atm.CheckBalance():N} UZS");
                    break;
                case "2":
                    Console.Write("Введите сумму для снятия: ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal withdrawAmount))
                    {
                        if (atm.Withdraw(withdrawAmount))
                            Console.WriteLine("Операция успешна!");
                        else
                            Console.WriteLine("Ошибка операции! Проверьте баланс и сумму.");
                    }
                    else
                    {
                        Console.WriteLine("Неверная сумма!");
                    }
                    break;
                case "3":
                    Console.Write("Введите сумму для пополнения: ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal depositAmount))
                    {
                        if (atm.Deposit(depositAmount))
                            Console.WriteLine("Операция успешна!");
                        else
                            Console.WriteLine("Ошибка операции!");
                    }
                    else
                    {
                        Console.WriteLine("Неверная сумма!");
                    }
                    break;
                case "4":
                    atm.ShowTransactionHistory();
                    break;
                case "5":
                    atm.Logout();
                    Console.WriteLine("До свидания! Заберите вашу карту.");
                    return;
                default:
                    Console.WriteLine("Неверный выбор!");
                    break;
            }
        }
    }
}