using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Database;
using Microsoft.Extensions.Configuration;

namespace Magazine.WebApi.Services;

public class ProductService : IProductService
{
    private readonly DataBase _dataBase;

    public ProductService(DataBase dataBase)
    {
        _dataBase = dataBase;
    }

    public Task<Product> Add(Product product)
    {
        product.Id = Guid.NewGuid();
        _dataBase.Insert(product);
        return Task.FromResult(product);
    }

    public Task<Product> Remove(Guid id)
    {
        var product = _dataBase.SelectById(id);
        if (product != null)
        {
            _dataBase.Delete(id);
        }
        return Task.FromResult(product);
    }

    public Task<Product> Edit(Product product)
    {
        var existingProduct = _dataBase.SelectById(product.Id);
        if (existingProduct != null)
        {
            existingProduct.Definition = product.Definition;
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Image = product.Image;
            _dataBase.Update(existingProduct);
        }
        return Task.FromResult(existingProduct);
    }

    public Task<Product?> Search(Guid id)
    {
        var product = _dataBase.SelectById(id);
        return Task.FromResult<Product?>(product);
    }

    public Task<IEnumerable<Product>> GetAll()
    {
        var products = _dataBase.SelectAll();
        return Task.FromResult(products);
    }
}