using Microsoft.Data.Sqlite;
using Magazine.Core.Models;
using System.Data;

namespace Magazine.WebApi.Database;

public class DataBase : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;

    // Пункт 1: SQL запросы как константы
    private const string CREATE_TABLE_QUERY =
        "CREATE TABLE IF NOT EXISTS Products (Id TEXT PRIMARY KEY, Definition TEXT, Name TEXT, Price REAL, Image TEXT)";

    private const string CREATE_INDEX_QUERY =
        "CREATE INDEX IF NOT EXISTS IX_Products_Id ON Products(Id)";

    private const string SELECT_ALL_QUERY =
        "SELECT Id, Definition, Name, Price, Image FROM Products";

    private const string SELECT_BY_ID_QUERY =
        "SELECT Id, Definition, Name, Price, Image FROM Products WHERE Id = @Id";

    private const string INSERT_QUERY =
        "INSERT INTO Products (Id, Definition, Name, Price, Image) VALUES (@Id, @Definition, @Name, @Price, @Image)";

    private const string UPDATE_QUERY =
        "UPDATE Products SET Definition = @Definition, Name = @Name, Price = @Price, Image = @Image WHERE Id = @Id";

    private const string DELETE_QUERY =
        "DELETE FROM Products WHERE Id = @Id";

    // Конструктор принимает IConfiguration
    public DataBase(IConfiguration config)
    {
        // Получаем путь к БД из конфигурации
        var dbPath = config["DataBasePath"] ?? "magazine.db";
        _connectionString = $"Data Source={dbPath}";
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();

        // Инициализируем таблицу при создании
        InitializeDatabase();
    }

    // Пункт 2.a и 2.b: Создание таблицы и индекса
    private void InitializeDatabase()
    {
        using var command = _connection.CreateCommand();
        command.CommandText = CREATE_TABLE_QUERY;
        command.ExecuteNonQuery();

        command.CommandText = CREATE_INDEX_QUERY;
        command.ExecuteNonQuery();
    }

    // Пункт 2.c: Select - получить все продукты
    public IEnumerable<Product> SelectAll()
    {
        var products = new List<Product>();

        using var command = _connection.CreateCommand();
        command.CommandText = SELECT_ALL_QUERY;

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            products.Add(new Product
            {
                Id = Guid.Parse(reader.GetString(0)),
                Definition = reader.GetString(1),
                Name = reader.GetString(2),
                Price = reader.GetDecimal(3),
                Image = reader.GetString(4)
            });
        }

        return products;
    }

    // Пункт 2.c: Select by Id - получить продукт по ID
    public Product? SelectById(Guid id)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = SELECT_BY_ID_QUERY;
        command.Parameters.AddWithValue("@Id", id.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Product
            {
                Id = Guid.Parse(reader.GetString(0)),
                Definition = reader.GetString(1),
                Name = reader.GetString(2),
                Price = reader.GetDecimal(3),
                Image = reader.GetString(4)
            };
        }

        return null;
    }

    // Пункт 2.d: Insert - вставить новый продукт (с связыванием переменных)
    public void Insert(Product product)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = INSERT_QUERY;
        command.Parameters.AddWithValue("@Id", product.Id.ToString());
        command.Parameters.AddWithValue("@Definition", product.Definition ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Name", product.Name ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Image", product.Image ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }

    // Пункт 2.e: Update - обновить существующий продукт
    public void Update(Product product)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = UPDATE_QUERY;
        command.Parameters.AddWithValue("@Id", product.Id.ToString());
        command.Parameters.AddWithValue("@Definition", product.Definition ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Name", product.Name ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Price", product.Price);
        command.Parameters.AddWithValue("@Image", product.Image ?? (object)DBNull.Value);

        command.ExecuteNonQuery();
    }

    // Пункт 2.f: Delete - удалить продукт по ID
    public void Delete(Guid id)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = DELETE_QUERY;
        command.Parameters.AddWithValue("@Id", id.ToString());

        command.ExecuteNonQuery();
    }

    // Реализация IDisposable для закрытия соединения
    public void Dispose()
    {
        if (_connection?.State == System.Data.ConnectionState.Open)
        {
            _connection.Close();
        }
        _connection?.Dispose();

        // Принудительно освобождаем ресурсы
        GC.SuppressFinalize(this);
    }
}