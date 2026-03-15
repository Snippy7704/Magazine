namespace Magazine.Core.Models;

public class Product
{
    public Guid Id { get; set; }
    public string Definition { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Image { get; set; }
}