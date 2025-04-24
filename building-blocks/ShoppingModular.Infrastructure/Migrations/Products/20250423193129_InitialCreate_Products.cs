#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace ShoppingModular.Infrastructure.Migrations.Products;

/// <inheritdoc />
public partial class InitialCreate_Products : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Products",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                Name = table.Column<string>("character varying(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>("character varying(500)", maxLength: 500, nullable: false),
                Price = table.Column<decimal>("numeric(18,2)", nullable: false),
                Stock = table.Column<int>("integer", nullable: false),
                Category = table.Column<string>("character varying(100)", maxLength: 100, nullable: false),
                Tags = table.Column<string>("text", nullable: false),
                Images = table.Column<string>("text", nullable: false),
                IsActive = table.Column<bool>("boolean", nullable: false),
                Rating = table.Column<double>("numeric(3,2)", nullable: false),
                ReviewCount = table.Column<int>("integer", nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>("timestamp with time zone", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_Products", x => x.Id); });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Products");
    }
}