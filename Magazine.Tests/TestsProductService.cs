using Magazine.Core.Models;
using Magazine.Core.Services;
using Magazine.WebApi.Database;
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
        private DataBase _dataBase;
        private string _testDbPath;

        [SetUp]
        public void Setup()
        {
            // Создаем временную БД для тестов
            _testDbPath = Path.Combine(Path.GetTempPath(), $"test_magazine_{Guid.NewGuid()}.db");

            // Создаем конфигурацию с путем к тестовой БД
            var configValues = new Dictionary<string, string>
            {
                { "DataBasePath", _testDbPath }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            // Создаем базу данных
            _dataBase = new DataBase(configuration);

            // Создаем сервис с реальной базой данных
            _productService = new ProductService(_dataBase);
        }

        [TearDown]
        public void TearDown()
        {
            // Сначала освобождаем ресурсы базы данных
            _dataBase?.Dispose();

            // Принудительно запускаем сборку мусора
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Небольшая задержка для гарантии освобождения файла
            System.Threading.Thread.Sleep(100);

            // Удаляем временную БД с обработкой ошибок
            if (File.Exists(_testDbPath))
            {
                try
                {
                    File.Delete(_testDbPath);
                }
                catch (IOException)
                {
                    // Если файл всё ещё занят - игнорируем для тестов
                }
            }
        }

        [Test]
        public async Task Add_ShouldAddProductAndReturnItWithNewId()
        {
            var product = new Product
            {
                Definition = "Smartphone",
                Name = "iPhone 15",
                Price = 79999.99m,
                Image = "iphone15.jpg"
            };

            var result = await _productService.Add(product);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(result.Name, Is.EqualTo("iPhone 15"));

            var savedProduct = _dataBase.SelectById(result.Id);
            Assert.That(savedProduct, Is.Not.Null);
        }

        [Test]
        public async Task GetAll_ShouldReturnAllProducts()
        {
            var product1 = new Product { Definition = "Def1", Name = "Product1", Price = 100, Image = "img1.jpg" };
            var product2 = new Product { Definition = "Def2", Name = "Product2", Price = 200, Image = "img2.jpg" };

            await _productService.Add(product1);
            await _productService.Add(product2);

            var result = await _productService.GetAll();

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(2));
        }

        [Test]
        public async Task Search_WhenProductExists_ShouldReturnProduct()
        {
            var product = new Product { Definition = "Def", Name = "TestProduct", Price = 500, Image = "test.jpg" };
            var addedProduct = await _productService.Add(product);

            var result = await _productService.Search(addedProduct.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(addedProduct.Id));
            Assert.That(result.Name, Is.EqualTo("TestProduct"));
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
            var product = new Product { Definition = "Old Def", Name = "Old Name", Price = 100, Image = "old.jpg" };
            var addedProduct = await _productService.Add(product);

            addedProduct.Name = "New Name";
            addedProduct.Price = 999.99m;

            var result = await _productService.Edit(addedProduct);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("New Name"));
            Assert.That(result.Price, Is.EqualTo(999.99m));

            var searchedProduct = await _productService.Search(addedProduct.Id);
            Assert.That(searchedProduct.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task Remove_WhenProductExists_ShouldRemoveAndReturnIt()
        {
            var product = new Product { Definition = "Def", Name = "ToRemove", Price = 50, Image = "remove.jpg" };
            var addedProduct = await _productService.Add(product);

            var result = await _productService.Remove(addedProduct.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(addedProduct.Id));

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

        [Test]
        public async Task Add_ShouldSaveDataToDatabase()
        {
            var product = new Product { Definition = "Def", Name = "SaveTest", Price = 777, Image = "save.jpg" };

            await _productService.Add(product);

            var productsFromDb = _dataBase.SelectAll();
            Assert.That(productsFromDb, Is.Not.Null);
            Assert.That(productsFromDb.Count(), Is.EqualTo(1));
            Assert.That(productsFromDb.First().Name, Is.EqualTo("SaveTest"));
        }
    }
}