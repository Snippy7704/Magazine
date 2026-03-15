using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Database;
using Microsoft.EntityFrameworkCore;

namespace Magazine.WebApi.Services;

public class DataBaseProductService : IProductService
{
    private readonly ApplicationContext _context;

    public DataBaseProductService(ApplicationContext context)
    {
        _context = context;
    }

    // Пункт 6: Получение всех продуктов
    public async Task<IEnumerable<Product>> GetAll()
    {
        return await _context.Products.ToListAsync();
    }

    // Пункт 6: Поиск продукта по ID
    public async Task<Product?> Search(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    // Пункт 6: Добавление нового продукта
    public async Task<Product> Add(Product product)
    {
        product.Id = Guid.NewGuid();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    // Пункт 6: Редактирование продукта
    public async Task<Product> Edit(Product product)
    {
        var existingProduct = await _context.Products.FindAsync(product.Id);
        if (existingProduct != null)
        {
            existingProduct.Definition = product.Definition;
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Image = product.Image;
            existingProduct.Category = product.Category; // Новое поле из Пункта 8

            await _context.SaveChangesAsync();
        }
        return existingProduct;
    }

    // Пункт 6: Удаление продукта
    public async Task<Product> Remove(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return product;
    }
}