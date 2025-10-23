public class AdminPanel
{
    private IDatabaseHelper dbHelper;

    public AdminPanel(string connectionString)
    {
        dbHelper = new DatabaseHelper(connectionString);
    }

    public void ShowAdminMenu()
    {
        while (true)
        {
            Console.WriteLine("\n=== АДМИН ПАНЕЛЬ ===");
            Console.WriteLine("1. Показать всех клиентов");
            Console.WriteLine("2. Поиск клиентов");
            Console.WriteLine("3. Добавить клиента");
            Console.WriteLine("4. Добавить счет и карту клиенту");
            Console.WriteLine("5. Выход в главное меню");
            Console.Write("Выберите действие: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAllClients();
                    break;
                case "2":
                    SearchClients();
                    break;
                case "3":
                    AddNewClient();
                    break;
                case "4":
                    AddAccountToClient();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Неверный выбор!");
                    break;
            }
        }
    }

    private void ShowAllClients()
    {
        var clients = dbHelper.GetAllClients();

        Console.WriteLine("\n=== ВСЕ КЛИЕНТЫ ===");
        if (clients.Count == 0)
        {
            Console.WriteLine("Клиенты не найдены");
            return;
        }

        Console.WriteLine("ID | Имя | Фамилия | Паспорт | Телефон");
        Console.WriteLine(new string('-', 60));

        foreach (var client in clients)
        {
            Console.WriteLine($"{client.Id} | {client.FirstName} | {client.LastName} | {client.PassportNo} | {client.Phone}");
        }
        Console.WriteLine($"\nВсего клиентов: {clients.Count}");
    }

    private void SearchClients()
    {
        Console.Write("\nВведите поисковый запрос (имя, фамилия или паспорт): ");
        string? searchTerm = Console.ReadLine();

        if (string.IsNullOrEmpty(searchTerm))
        {
            Console.WriteLine("Поисковый запрос не может быть пустым");
            return;
        }

        var clients = dbHelper.SearchClients(searchTerm);

        Console.WriteLine($"\n=== РЕЗУЛЬТАТЫ ПОИСКА: '{searchTerm}' ===");
        if (clients.Count == 0)
        {
            Console.WriteLine("Клиенты не найдены");
            return;
        }

        Console.WriteLine("ID | Имя | Фамилия | Паспорт | Телефон");
        Console.WriteLine(new string('-', 60));

        foreach (var client in clients)
        {
            Console.WriteLine($"{client.Id} | {client.FirstName} | {client.LastName} | {client.PassportNo} | {client.Phone}");
        }
        Console.WriteLine($"\nНайдено клиентов: {clients.Count}");
    }

    private void AddNewClient()
    {
        Console.WriteLine("\n=== ДОБАВЛЕНИЕ НОВОГО КЛИЕНТА ===");

        Console.Write("Имя: ");
        string? firstName = Console.ReadLine();

        Console.Write("Фамилия: ");
        string? lastName = Console.ReadLine();

        Console.Write("Номер паспорта: ");
        string? passportNo = Console.ReadLine();

        Console.Write("Телефон: ");
        string? phone = Console.ReadLine();

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
            string.IsNullOrEmpty(passportNo) || string.IsNullOrEmpty(phone))
        {
            Console.WriteLine(" Все поля обязательны для заполнения!");
            return;
        }

        bool success = dbHelper.AddClient(firstName, lastName, passportNo, phone);

        if (success)
        {
            Console.WriteLine(" Клиент успешно добавлен!");
        }
        else
        {
            Console.WriteLine(" Ошибка при добавлении клиента");
        }
    }

    private void AddAccountToClient()
    {
        Console.WriteLine("\n=== ДОБАВЛЕНИЕ СЧЕТА И КАРТЫ ===");

        var clients = dbHelper.GetAllClients();
        if (clients.Count == 0)
        {
            Console.WriteLine("Нет клиентов для добавления счета");
            return;
        }

        Console.WriteLine("Доступные клиенты:");
        foreach (var client in clients)
        {
            Console.WriteLine($"{client.Id}. {client.FirstName} {client.LastName} ({client.PassportNo})");
        }

        Console.Write("\nВведите ID клиента: ");
        if (!int.TryParse(Console.ReadLine(), out int clientId))
        {
            Console.WriteLine(" Неверный ID клиента");
            return;
        }

        var clientExists = clients.Any(c => c.Id == clientId);
        if (!clientExists)
        {
            Console.WriteLine(" Клиент с таким ID не найден");
            return;
        }

        Console.Write("Номер счета (16 цифр): ");
        string? accountNumber = Console.ReadLine();

        Console.Write("Номер карты (16 цифр): ");
        string? cardNumber = Console.ReadLine();

        Console.Write("Начальный баланс: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal balance))
        {
            balance = 0;
        }

        if (string.IsNullOrEmpty(accountNumber) || accountNumber.Length != 16 ||
            string.IsNullOrEmpty(cardNumber) || cardNumber.Length != 16)
        {
            Console.WriteLine(" Номер счета и карты должны содержать 16 цифр!");
            return;
        }

        bool success = dbHelper.AddAccountAndCard(clientId, accountNumber, cardNumber, balance);

        if (success)
        {
            Console.WriteLine(" Счет и карта успешно созданы!");
            Console.WriteLine($" Номер карты: {cardNumber}");
            Console.WriteLine($" Номер счета: {accountNumber}");
            Console.WriteLine($" Начальный баланс: {balance:N} UZS");
            Console.WriteLine($" PIN для карты: 0000");
        }
        else
        {
            Console.WriteLine(" Ошибка при создании счета и карты");
        }
    }
}