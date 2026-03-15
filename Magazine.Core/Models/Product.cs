using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Magazine.Core.Models;

public class Product
{
   
    [Key]
    public Guid Id { get; set; }
    
    public string? Definition { get; set; }
    
    public string? Name { get; set; }
    
    public decimal Price { get; set; }
    
    public string? Image { get; set; }

    public string? Category { get; set; }
}