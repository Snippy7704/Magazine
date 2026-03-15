using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace Magazine.Tests
{
    [TestFixture]
    public class TestsProductController
    {
        private ProductController _controller;
        private Mock<IProductService> _mockService;

        [SetUp]
        public void Setup()
        {
            _mockService = new Mock<IProductService>();
            _controller = new ProductController(_mockService.Object);
        }

        [Test]
        public async Task GetAll_ShouldReturnOkResultWithAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = Guid.NewGuid(), Name = "Product1", Price = 100, Definition = "Def1", Image = "img1.jpg", Category = "Cat1" },
                new Product { Id = Guid.NewGuid(), Name = "Product2", Price = 200, Definition = "Def2", Image = "img2.jpg", Category = "Cat2" }
            };

            _mockService.Setup(x => x.GetAll()).ReturnsAsync(products);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            var okResult = result.Result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Add_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            var product = new Product { Name = "New Product", Price = 500, Definition = "Def", Image = "img.jpg", Category = "Electronics" };
            var createdProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = product.Name,
                Price = product.Price,
                Definition = product.Definition,
                Image = product.Image,
                Category = product.Category
            };

            _mockService.Setup(x => x.Add(It.IsAny<Product>())).ReturnsAsync(createdProduct);

            // Act
            var result = await _controller.Add(product);

            // Assert
            Assert.That(result, Is.Not.Null);
            var createdAtResult = result.Result as CreatedAtActionResult;
            Assert.That(createdAtResult, Is.Not.Null);
            Assert.That(createdAtResult.StatusCode, Is.EqualTo(201));
        }

        [Test]
        public async Task Get_WhenProductExists_ShouldReturnOkResult()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Product", Price = 100, Definition = "Def", Image = "img.jpg", Category = "TestCat" };

            _mockService.Setup(x => x.Search(productId)).ReturnsAsync(product);

            // Act
            var result = await _controller.Get(productId);

            // Assert
            Assert.That(result, Is.Not.Null);

            var okResult = result.Result as OkObjectResult;
            if (okResult == null && result.Value != null)
            {
                okResult = new OkObjectResult(result.Value);
            }

            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Get_WhenProductDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _mockService.Setup(x => x.Search(productId)).ReturnsAsync((Product)null);

            // Act
            var result = await _controller.Get(productId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Edit_ShouldReturnOkResult()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Updated Product", Price = 999, Definition = "Def", Image = "img.jpg", Category = "UpdatedCat" };

            _mockService.Setup(x => x.Edit(It.IsAny<Product>())).ReturnsAsync(product);

            // Act
            var result = await _controller.Edit(productId, product);

            // Assert
            Assert.That(result, Is.Not.Null);

            var okResult = result.Result as OkObjectResult;
            if (okResult == null && result.Value != null)
            {
                okResult = new OkObjectResult(result.Value);
            }

            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.StatusCode, Is.EqualTo(200));
        }

        [Test]
        public async Task Edit_WhenIdDoesNotMatch_ShouldReturnBadRequest()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = Guid.NewGuid(), Name = "Product", Price = 100, Definition = "Def", Image = "img.jpg", Category = "Cat" };

            // Act
            var result = await _controller.Edit(productId, product);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Edit_WhenProductDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Product", Price = 100, Definition = "Def", Image = "img.jpg", Category = "Cat" };

            _mockService.Setup(x => x.Edit(It.IsAny<Product>())).ReturnsAsync((Product)null);

            // Act
            var result = await _controller.Edit(productId, product);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Remove_WhenProductExists_ShouldReturnOkResult()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Product", Price = 100, Definition = "Def", Image = "img.jpg", Category = "Cat" };

            _mockService.Setup(x => x.Remove(It.IsAny<Guid>())).ReturnsAsync(product);

            // Act
            var result = await _controller.Remove(productId);

            // Assert
            Assert.That(result, Is.Not.Null);

            var okResult = result.Result as OkObjectResult;
            if (okResult == null && result.Value != null)
            {
                okResult = new OkObjectResult(result.Value);
            }

            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult.Value, Is.EqualTo(product));
        }

        [Test]
        public async Task Remove_WhenProductDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var productId = Guid.NewGuid();
            _mockService.Setup(x => x.Remove(It.IsAny<Guid>())).ReturnsAsync((Product)null);

            // Act
            var result = await _controller.Remove(productId);

            // Assert
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }
    }
}