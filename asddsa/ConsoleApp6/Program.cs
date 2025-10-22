using System;

class Program
{
    static void Main(string[] args)
    {
        // Ваша строка подключения
        string connectionString = "Server=192.168.9.203\\sqlexpress;Database=AtmService;User Id=student1;Password=123456;Encrypt=false;";

        Console.Clear(); // ОЧИСТКА ПЕРЕД ЗАПУСКОМ
        Console.WriteLine("=== БАНКОМАТ ===");

        ATM atm = new ATM(connectionString);
        Console.Write("Введите номер карты: ");
        string? cardNumber = Console.ReadLine();

        Console.Write("Введите PIN: ");
        string? pin = Console.ReadLine();

        if (atm.Authenticate(cardNumber, pin))
        {
            Console.Clear(); // ОЧИСТКА ПОСЛЕ АУТЕНТИФИКАЦИИ
            Console.WriteLine("Авторизация успешна!");
            atm.DisplayClientInfo();
            ShowMenu(atm);
        }
        else
        {
            Console.WriteLine("Неверные данные карты или карта заблокирована!");
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
                    Console.Clear(); // ОЧИСТКА ПЕРЕД БАЛАНСОМ
                    Console.WriteLine($"Ваш баланс: {atm.CheckBalance():N} UZS");
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear(); // ОЧИСТКА ПЕРЕД ВОЗВРАТОМ В МЕНЮ
                    break;

                case "2":
                    Console.Clear(); // ОЧИСТКА ПЕРЕД СНЯТИЕМ
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
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear(); // ОЧИСТКА ПЕРЕД ВОЗВРАТОМ В МЕНЮ
                    break;

                case "3":
                    Console.Clear(); // ОЧИСТКА ПЕРЕД ПОПОЛНЕНИЕМ
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
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear(); // ОЧИСТКА ПЕРЕД ВОЗВРАТОМ В МЕНЮ
                    break;

                case "4":
                    Console.Clear(); // ОЧИСТКА ПЕРЕД ИСТОРИЕЙ
                    atm.ShowTransactionHistory();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear(); // ОЧИСТКА ПЕРЕД ВОЗВРАТОМ В МЕНЮ
                    break;

                case "5":
                    Console.Clear(); // ОЧИСТКА ПЕРЕД ВЫХОДОМ
                    atm.Logout();
                    Console.WriteLine("До свидания! Заберите вашу карту.");
                    return;

                default:
                    Console.WriteLine("Неверный выбор!");
                    Console.WriteLine("Нажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    Console.Clear(); // ОЧИСТКА ПЕРЕД ВОЗВРАТОМ В МЕНЮ
                    break;
            }
            
            // Показываем информацию о клиенте после очистки
            Console.WriteLine("Авторизация успешна!");
            atm.DisplayClientInfo();
        }
    }
}