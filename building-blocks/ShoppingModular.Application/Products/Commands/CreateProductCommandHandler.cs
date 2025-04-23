using KafkaProducerService;
using MediatR;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Interfaces;
using MongoDB.Driver;
using ShoppingModular.Infrastructure.Interfaces.Products;

namespace ShoppingModular.Application.Products.Commands;

/// <summary>
/// Handler responsável por criar produtos, projetar leitura e publicar no Kafka.
/// </summary>
public class CreateProductCommandHandler(
    IProductWriteRepository writeRepo,
    IMongoDatabase mongoDb,
    ICacheService<ProductReadModel> cache,
    IKafkaProducerService kafka
) : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        #region 1. Criação do domínio

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category,
            Tags = request.Tags,
            Images = request.Images,
            CreatedAt = DateTime.UtcNow
        };

        #endregion

        #region 2. Persiste no PostgreSQL

        await writeRepo.AddAsync(product, cancellationToken);

        #endregion

        #region 3. Projeção de leitura (ReadModel)

        var readModel = new ProductReadModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            Category = product.Category,
            Tags = product.Tags,
            Images = product.Images,
            CreatedAt = product.CreatedAt
        };

        await mongoDb
            .GetCollection<ProductReadModel>("products")
            .InsertOneAsync(readModel, cancellationToken: cancellationToken);

        #endregion

        #region 4. Cache no Redis

        await cache.SetAsync($"product:{product.Id}", readModel, TimeSpan.FromMinutes(10), cancellationToken);

        #endregion

        #region 5. Publicação no Kafka

        await kafka.PublishAsync("products.created", readModel, cancellationToken);

        #endregion

        return product.Id;
    }
}