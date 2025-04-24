#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace ShoppingModular.Infrastructure.Migrations.Orders;

/// <inheritdoc />
public partial class InitialCreate_Orders : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            "Orders",
            table => new
            {
                Id = table.Column<Guid>("uuid", nullable: false),
                CustomerName = table.Column<string>("character varying(100)", maxLength: 100, nullable: false),
                CreatedAt = table.Column<DateTime>("timestamp with time zone", nullable: false),
                TotalAmount = table.Column<decimal>("numeric(18,2)", nullable: false)
            },
            constraints: table => { table.PrimaryKey("PK_Orders", x => x.Id); });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            "Orders");
    }
}