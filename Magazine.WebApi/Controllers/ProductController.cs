using Magazine.Core.Models;
using Magazine.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Magazine.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Add(Product product)
    {
        var createdProduct = await _productService.Add(product);
        return CreatedAtAction(nameof(Get), new { id = createdProduct.Id }, createdProduct);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Product>> Remove(Guid id)
    {
        var removedProduct = await _productService.Remove(id);
        if (removedProduct == null)
        {
            return NotFound();
        }
        return removedProduct;
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Product>> Edit(Guid id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        var editedProduct = await _productService.Edit(product);
        if (editedProduct == null)
        {
            return NotFound();
        }

        return Ok(editedProduct);
    }

    [HttpGet]  
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _productService.GetAll();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> Get(Guid id)
    {
        var product = await _productService.Search(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }
}