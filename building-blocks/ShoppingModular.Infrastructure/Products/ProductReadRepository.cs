using MongoDB.Driver;
using ShoppingModular.Domain.Products;
using ShoppingModular.Infrastructure.Common.Mongo;

namespace ShoppingModular.Infrastructure.Products;

/// <summary>
///     Repositório de leitura de produtos baseado em MongoDB.
/// </summary>
public class ProductReadRepository(IMongoDatabase database)
    : BaseMongoReadRepository<ProductReadModel>(database, "products");