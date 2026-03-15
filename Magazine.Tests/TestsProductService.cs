using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Database;
using Magazine.WebApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Magazine.Tests
{
    [TestFixture]
    public class TestsProductService
    {
        private DataBaseProductService _productService;
        private ApplicationContext _context;
        private string _testDbPath;

        [SetUp]
        public async Task Setup()
        {
            // Создаем временную БД для тестов
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_ef_{Guid.NewGuid()}.db");

            // Создаем конфигурацию
            var configValues = new Dictionary<string, string>
            {
                { "ConnectionStrings:DefaultConnection", $"Data Source={_testDbPath}" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            // Настраиваем DbContext
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseSqlite($"Data Source={_testDbPath}")
                .Options;

            _context = new ApplicationContext(options);
            await _context.Database.EnsureCreatedAsync();

            // Создаем сервис
            _productService = new DataBaseProductService(_context);
        }

        [TearDown]
        public async Task TearDown()
        {
            // Очищаем ресурсы
            if (_context != null)
            {
                await _context.DisposeAsync();
            }

            // Удаляем временную БД
            if (File.Exists(_testDbPath))
            {
                try
                {
                    File.Delete(_testDbPath);
                }
                catch { /* Игнорируем ошибки удаления */ }
            }
        }

        [Test]
        public async Task Add_ShouldAddProductAndReturnItWithNewId()
        {
            // Arrange
            var product = new Product
            {
                Definition = "Smartphone",
                Name = "iPhone 15",
                Price = 79999.99m,
                Image = "iphone15.jpg",
                Category = "Electronics" // Новое поле
            };

            // Act
            var result = await _productService.Add(product);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Name, Is.EqualTo("iPhone 15"));
            Assert.That(result.Category, Is.EqualTo("Electronics"));
        }

        [Test]
        public async Task GetAll_ShouldReturnAllProducts()
        {
            // Arrange
            await _productService.Add(new Product { Name = "Product1", Price = 100, Category = "Cat1" });
            await _productService.Add(new Product { Name = "Product2", Price = 200, Category = "Cat2" });

            // Act
            var result = await _productService.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Search_WhenProductExists_ShouldReturnProduct()
        {
            // Arrange
            var product = new Product { Name = "TestProduct", Price = 500, Category = "TestCat" };
            var addedProduct = await _productService.Add(product);

            // Act
            var result = await _productService.Search(addedProduct.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(addedProduct.Id));
        }

        [Test]
        public async Task Search_WhenProductDoesNotExist_ShouldReturnNull()
        {
            var nonExistentId = Guid.NewGuid();
            var result = await _productService.Search(nonExistentId);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Edit_ShouldUpdateProduct()
        {
            // Arrange
            var product = new Product { Name = "Old Name", Price = 100, Category = "OldCat" };
            var addedProduct = await _productService.Add(product);

            addedProduct.Name = "New Name";
            addedProduct.Price = 999.99m;
            addedProduct.Category = "NewCat";

            // Act
            var result = await _productService.Edit(addedProduct);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Name"));
            Assert.That(result.Category, Is.EqualTo("NewCat"));
        }

        [Test]
        public async Task Remove_WhenProductExists_ShouldRemoveAndReturnIt()
        {
            // Arrange
            var product = new Product { Name = "ToRemove", Price = 50, Category = "RemoveCat" };
            var addedProduct = await _productService.Add(product);

            // Act
            var result = await _productService.Remove(addedProduct.Id);

            // Assert
            Assert.That(result, Is.Not.Null);

            var searchedProduct = await _productService.Search(addedProduct.Id);
            Assert.That(searchedProduct, Is.Null);
        }

        [Test]
        public async Task Remove_WhenProductDoesNotExist_ShouldReturnNull()
        {
            var nonExistentId = Guid.NewGuid();
            var result = await _productService.Remove(nonExistentId);
            Assert.That(result, Is.Null);
        }
    }
}