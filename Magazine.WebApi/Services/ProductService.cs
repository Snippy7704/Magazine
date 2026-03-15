using Magazine.Core.Models;
using Magazine.Core.Services;

namespace Magazine.WebApi.Services;

public class ProductService : IProductService
{
    public Task<IEnumerable<Product>> GetAll()
    {
        return Task.FromResult<IEnumerable<Product>>(_products);
    }

    private readonly List<Product> _products = new();

    public Task<Product> Add(Product product)
    {
        product.Id = Guid.NewGuid();
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task<Product> Remove(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            _products.Remove(product);
        }
        return Task.FromResult(product);
    }

    public Task<Product> Edit(Product product)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existingProduct != null)
        {
            existingProduct.Definition = product.Definition;
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Image = product.Image;
        }
        return Task.FromResult(existingProduct);
    }

    public Task<Product?> Search(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(product);
    }
}