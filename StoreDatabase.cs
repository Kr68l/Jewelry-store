using Microsoft.Data.Sqlite;
using System.Data;
using System.Collections.Generic;
using System;
using System.IO;

namespace JewelryStore;

internal static class StoreDatabase
{
    private static readonly string DbPath =
        Path.Combine(AppContext.BaseDirectory, "jewelry_store.db");

    private static string ConnectionString =>
        $"Data Source={DbPath}";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Purchases
            (
                PurchaseId INTEGER PRIMARY KEY AUTOINCREMENT,
                ClientId TEXT NOT NULL,
                ClientName TEXT NOT NULL,
                ProductId TEXT NOT NULL,
                ProductName TEXT NOT NULL,
                City TEXT NOT NULL,
                StoreNumber INTEGER NOT NULL,
                Phone TEXT NOT NULL,
                Email TEXT,
                PurchaseDate TEXT NOT NULL,
                AdminName TEXT NOT NULL,
                Price REAL NOT NULL,
                RepairIncluded INTEGER NOT NULL DEFAULT 0
            );
            """;

        command.ExecuteNonQuery();

        command.CommandText = "SELECT COUNT(*) FROM Purchases;";

        if (Convert.ToInt32(command.ExecuteScalar()) == 0)
        {
            AddPurchase(
                "CL-1001", "Анна Коваленко",
                "PR-2001", "Золота каблучка 585°",
                "Харків", 1,
                "+380 67 111 22 33", "anna@example.com",
                "2026-10-21 12:35",
                "Адміністратор 1", 12500, true);

            AddPurchase(
                "CL-1002", "Олександр Мельник",
                "PR-2002", "Срібний ланцюжок",
                "Харків", 2,
                "+380 50 222 33 44", "",
                "2026-10-21 14:10",
                "Адміністратор 2", 3200, false);

            AddPurchase(
                "CL-1003", "Марія Шевченко",
                "PR-2003", "Сережки з діамантами",
                "Київ", 3,
                "+380 93 333 44 55", "maria@example.com",
                "2026-10-22 09:05",
                "Адміністратор 1", 28700, true);

            AddPurchase(
                "CL-1004", "Ірина Бондар",
                "PR-2004", "Золота підвіска",
                "Харків", 1,
                "+380 63 444 55 66", "iryna@example.com",
                "2026-10-22 16:42",
                "Адміністратор 3", 8400, false);

            AddPurchase(
                "CL-1005", "Дмитро Ткаченко",
                "PR-2005", "Срібний браслет",
                "Київ", 3,
                "+380 97 555 66 77", "",
                "2026-10-23 11:20",
                "Адміністратор 2", 1900, true);

            AddPurchase(
                "CL-1006", "Софія Поліщук",
                "PR-2006", "Золота каблучка з каменем",
                "Харків", 2,
                "+380 66 666 77 88", "sofia@example.com",
                "2026-10-23 18:15",
                "Адміністратор 1", 16400, true);

            AddPurchase(
                "CL-1007", "Максим Савченко",
                "PR-2007", "Срібні сережки",
                "Київ", 3,
                "+380 68 777 88 99", "max@example.com",
                "2026-10-24 10:00",
                "Адміністратор 3", 2600, false);
        }
    }

    private static void AddPurchase(
        string clientId,
        string clientName,
        string productId,
        string productName,
        string city,
        int storeNumber,
        string phone,
        string email,
        string purchaseDate,
        string adminName,
        decimal price,
        bool repairIncluded)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO Purchases
            (
                ClientId, ClientName,
                ProductId, ProductName,
                City, StoreNumber,
                Phone, Email,
                PurchaseDate, AdminName,
                Price, RepairIncluded
            )
            VALUES
            (
                $clientId, $clientName,
                $productId, $productName,
                $city, $storeNumber,
                $phone, $email,
                $purchaseDate, $adminName,
                $price, $repairIncluded
            );
            """;

        command.Parameters.AddWithValue("$clientId", clientId);
        command.Parameters.AddWithValue("$clientName", clientName);
        command.Parameters.AddWithValue("$productId", productId);
        command.Parameters.AddWithValue("$productName", productName);
        command.Parameters.AddWithValue("$city", city);
        command.Parameters.AddWithValue("$storeNumber", storeNumber);
        command.Parameters.AddWithValue("$phone", phone);
        command.Parameters.AddWithValue("$email", email);
        command.Parameters.AddWithValue("$purchaseDate", purchaseDate);
        command.Parameters.AddWithValue("$adminName", adminName);
        command.Parameters.AddWithValue("$price", price);
        command.Parameters.AddWithValue(
            "$repairIncluded", repairIncluded ? 1 : 0);

        command.ExecuteNonQuery();
    }

    public static DataTable GetPurchases(
        string city,
        string store,
        string search,
        string repair)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        var conditions = new List<string>();

        if (city != "Усі міста")
        {
            conditions.Add("City = $city");
            command.Parameters.AddWithValue("$city", city);
        }

        if (store != "Усі точки" &&
            int.TryParse(store, out int storeNumber))
        {
            conditions.Add("StoreNumber = $store");
            command.Parameters.AddWithValue("$store", storeNumber);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add("""
                (
                    ClientName LIKE $search
                    OR ClientId LIKE $search
                    OR ProductName LIKE $search
                    OR ProductId LIKE $search
                    OR Phone LIKE $search
                    OR Email LIKE $search
                    OR AdminName LIKE $search
                )
                """);

            command.Parameters.AddWithValue(
                "$search", $"%{search.Trim()}%");
        }

        if (repair == "Так")
            conditions.Add("RepairIncluded = 1");

        if (repair == "Ні")
            conditions.Add("RepairIncluded = 0");

        string where = conditions.Count > 0
            ? "WHERE " + string.Join(" AND ", conditions)
            : "";

        command.CommandText = $"""
            SELECT
                PurchaseId AS 'ID покупки',
                ClientId AS 'ID клиента',
                ClientName AS 'ПІБ клієнта',
                ProductId AS 'ID виробу',
                ProductName AS 'Виріб',
                City AS 'Місто',
                '№ ' || StoreNumber AS 'Точка продажу',
                Phone AS 'Телефон',
                COALESCE(NULLIF(Email, ''), 'Не вказано') AS 'Email',
                PurchaseDate AS 'Дата та час покупки',
                AdminName AS 'Адміністратор',
                Price AS 'Ціна на момент покупки (₴)',
                CASE
                    WHEN RepairIncluded = 1 THEN 'Так'
                    ELSE 'Ні'
                END AS 'Ремонт включено'
            FROM Purchases
            {where}
            ORDER BY PurchaseDate DESC;
            """;

        using var reader = command.ExecuteReader();

        var table = new DataTable();
        table.Load(reader);

        return table;
    }

    public static int CountPurchases()
    {
        return ExecuteInt(
            "SELECT COUNT(*) FROM Purchases;");
    }

    public static int CountRepairPurchases()
    {
        return ExecuteInt(
            "SELECT COUNT(*) FROM Purchases WHERE RepairIncluded = 1;");
    }

    public static decimal TotalRevenue()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
            "SELECT COALESCE(SUM(Price), 0) FROM Purchases;";

        return Convert.ToDecimal(command.ExecuteScalar());
    }

    private static int ExecuteInt(string sql)
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt32(command.ExecuteScalar());
    }
}