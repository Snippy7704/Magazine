using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Text.Json;

namespace Magazine.Tests
{
    [TestFixture]
    public class TestsProductService
    {
        private ProductService _productService;
        private IConfiguration _configuration;
        private string _testFilePath;

        [SetUp]
        public void Setup()
        {
            // Создаем временный файл для тестов
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_database_{Guid.NewGuid()}.txt");

            // Создаем конфигурацию с путем к тестовому файлу
            var configValues = new Dictionary<string, string>
            {
                { "DataBaseFilePath", _testFilePath }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            // Создаем экземпляр сервиса
            _productService = new ProductService(_configuration);
        }

        [TearDown]
        public void TearDown()
        {
            // Удаляем временный файл после каждого теста
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }

        // Тест метода Add
        [Test]
        public async Task Add_ShouldAddProductAndReturnItWithNewId()
        {
            // Arrange
            var product = new Product
            {
                Definition = "Smartphone",
                Name = "iPhone 15",
                Price = 79999.99m,
                Image = "iphone15.jpg"
            };

            // Act
            var result = await _productService.Add(product);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Name, Is.EqualTo("iPhone 15"));

            // Проверяем, что товар сохранился в файле
            Assert.That(File.Exists(_testFilePath), Is.True);
        }

        // Тест метода GetAll
        [Test]
        public async Task GetAll_ShouldReturnAllProducts()
        {
            // Arrange
            var product1 = new Product { Definition = "Def1", Name = "Product1", Price = 100, Image = "img1.jpg" };
            var product2 = new Product { Definition = "Def2", Name = "Product2", Price = 200, Image = "img2.jpg" };

            await _productService.Add(product1);
            await _productService.Add(product2);

            // Act
            var result = await _productService.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        // Тест метода Search
        [Test]
        public async Task Search_WhenProductExists_ShouldReturnProduct()
        {
            // Arrange
            var product = new Product { Definition = "Def", Name = "TestProduct", Price = 500, Image = "test.jpg" };
            var addedProduct = await _productService.Add(product);

            // Act
            var result = await _productService.Search(addedProduct.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(addedProduct.Id));
            Assert.That(result.Name, Is.EqualTo("TestProduct"));
        }

        [Test]
        public async Task Search_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _productService.Search(nonExistentId);

            // Assert
            Assert.That(result, Is.Null);
        }

        // Тест метода Edit
        [Test]
        public async Task Edit_ShouldUpdateProduct()
        {
            // Arrange
            var product = new Product { Definition = "Old Def", Name = "Old Name", Price = 100, Image = "old.jpg" };
            var addedProduct = await _productService.Add(product);

            addedProduct.Name = "New Name";
            addedProduct.Price = 999.99m;

            // Act
            var result = await _productService.Edit(addedProduct);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Name"));
            Assert.That(result.Price, Is.EqualTo(999.99m));

            // Проверяем, что изменения сохранились
            var searchedProduct = await _productService.Search(addedProduct.Id);
            Assert.That(searchedProduct.Name, Is.EqualTo("New Name"));
        }

        // Тест метода Remove
        [Test]
        public async Task Remove_WhenProductExists_ShouldRemoveAndReturnIt()
        {
            // Arrange
            var product = new Product { Definition = "Def", Name = "ToRemove", Price = 50, Image = "remove.jpg" };
            var addedProduct = await _productService.Add(product);

            // Act
            var result = await _productService.Remove(addedProduct.Id);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(addedProduct.Id));

            // Проверяем, что товар удален
            var searchedProduct = await _productService.Search(addedProduct.Id);
            Assert.That(searchedProduct, Is.Null);
        }

        [Test]
        public async Task Remove_WhenProductDoesNotExist_ShouldReturnNull()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _productService.Remove(nonExistentId);

            // Assert
            Assert.That(result, Is.Null);
        }

        // Тест сохранения данных на диск (интеграционный)
        [Test]
        public async Task Add_ShouldSaveDataToFile()
        {
            // Arrange
            var product = new Product { Definition = "Def", Name = "SaveTest", Price = 777, Image = "save.jpg" };

            // Act
            await _productService.Add(product);

            // Assert
            Assert.That(File.Exists(_testFilePath), Is.True);

            var fileContent = await File.ReadAllTextAsync(_testFilePath);
            Assert.That(string.IsNullOrEmpty(fileContent), Is.False);

            var productsFromFile = JsonSerializer.Deserialize<List<Product>>(fileContent);
            Assert.That(productsFromFile, Is.Not.Null);
            Assert.That(productsFromFile.Count, Is.EqualTo(1));
            Assert.That(productsFromFile[0].Name, Is.EqualTo("SaveTest"));
        }
    }
}