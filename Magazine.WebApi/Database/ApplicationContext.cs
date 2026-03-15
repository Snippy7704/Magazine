using Microsoft.EntityFrameworkCore;
using Magazine.Core.Models;

namespace Magazine.WebApi.Database;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    // Пункт 2: Таблица Products
    public DbSet<Product> Products { get; set; }

    // Пункт 3 и 5: Создание БД и настройка ключа
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Пункт 5: Поле Id как первичный ключ с индексом
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Id);
        });
    }
}