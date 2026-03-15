using Magazine.Core.Models;

namespace Magazine.Core.Services;

public interface IProductService
{
    Task<Product> Add(Product product);
    Task<Product> Remove(Guid id);
    Task<Product> Edit(Product product);
    Task<Product?> Search(Guid id);
    Task<IEnumerable<Product>> GetAll();
}