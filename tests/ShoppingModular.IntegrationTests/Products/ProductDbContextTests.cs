using Microsoft.EntityFrameworkCore;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Products;

namespace ShoppingModular.IntegrationTests.Products;

[TestFixture]
public class ProductDbContextTests
{
    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=shoppingdb")
            .Options;

        _dbContext = new ProductDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    private ProductDbContext? _dbContext;

    [Test]
    public async Task Should_Connect_To_Postgres()
    {
        var canConnect = await _dbContext!.Database.CanConnectAsync();
        Assert.That(canConnect, Is.True);
    }

    [Test]
    public async Task Should_Insert_And_Read_Product()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 9.99m,
            Stock = 5,
            Category = "Tests",
            Tags = new List<string> { "unit" },
            Images = new List<string> { "img.jpg" },
            CreatedAt = DateTime.UtcNow
        };

        _dbContext!.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        var result = await _dbContext.Products.FindAsync(product.Id);
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo(product.Name));
            Assert.That(result.Stock, Is.EqualTo(product.Stock));
        });
    }

    [Test]
    public async Task Should_Update_Product()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "ToUpdate",
            Description = "Before Update",
            Price = 5,
            Stock = 3,
            Category = "PreUpdate",
            Tags = new List<string> { "update" },
            Images = new List<string> { "img1.png" },
            CreatedAt = DateTime.UtcNow
        };

        _dbContext!.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        product.Description = "After Update";
        _dbContext.Products.Update(product);
        await _dbContext.SaveChangesAsync();

        var updated = await _dbContext.Products.FindAsync(product.Id);
        Assert.That(updated!.Description, Is.EqualTo("After Update"));
    }

    [Test]
    public async Task Should_Delete_Product()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "ToDelete",
            Description = "Delete Me",
            Price = 10,
            Stock = 2,
            Category = "Test",
            Tags = new List<string> { "del" },
            Images = new List<string> { "del.jpg" },
            CreatedAt = DateTime.UtcNow
        };

        _dbContext!.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync();

        var result = await _dbContext.Products.FindAsync(product.Id);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Should_List_All_Products()
    {
        var products = await _dbContext!.Products.ToListAsync();
        Assert.That(products, Is.Not.Null);
    }
}