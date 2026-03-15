using Magazine.Core.Models;
using Magazine.Core.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Magazine.WebApi.Services;

public class ProductService : IProductService
{
    private readonly List<Product> _products = new();
    private readonly string _filePath;
    private readonly Mutex _mutex = new(); // Мьютекс для потокобезопасной записи

    public ProductService(IConfiguration config)
    {
        _filePath = config["DataBaseFilePath"];
        InitFromFile(); // Загружаем данные при создании сервиса
    }

    // Пункт 4: Метод загрузки из файла
    private void InitFromFile()
    {
        if (File.Exists(_filePath))
        {
            var text = File.ReadAllText(_filePath);
            var productsFromFile = JsonSerializer.Deserialize<List<Product>>(text);
            if (productsFromFile != null)
            {
                _products.AddRange(productsFromFile);
            }
        }
    }

    // Пункт 7: Метод записи в файл
    private void WriteToFile()
    {
        _mutex.WaitOne(); // Захватываем мьютекс
        try
        {
            var text = JsonSerializer.Serialize(_products);
            File.WriteAllText(_filePath, text);
        }
        finally
        {
            _mutex.ReleaseMutex(); // Освобождаем мьютекс
        }
    }

    // Пункт 6: Реализация методов
    public Task<Product> Add(Product product)
    {
        _mutex.WaitOne();
        try
        {
            product.Id = Guid.NewGuid();
            _products.Add(product);
            WriteToFile(); // Сохраняем изменения на диск
            return Task.FromResult(product);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public Task<Product> Remove(Guid id)
    {
        _mutex.WaitOne();
        try
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
                WriteToFile(); // Сохраняем изменения на диск
            }
            return Task.FromResult(product);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public Task<Product> Edit(Product product)
    {
        _mutex.WaitOne();
        try
        {
            var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
            if (existingProduct != null)
            {
                existingProduct.Definition = product.Definition;
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Image = product.Image;
                WriteToFile(); // Сохраняем изменения на диск
            }
            return Task.FromResult(existingProduct);
        }
        finally
        {
            _mutex.ReleaseMutex();
        }
    }

    public Task<Product?> Search(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult<Product?>(product);
    }

    public Task<IEnumerable<Product>> GetAll()
    {
        return Task.FromResult<IEnumerable<Product>>(_products.AsEnumerable());
    }
}