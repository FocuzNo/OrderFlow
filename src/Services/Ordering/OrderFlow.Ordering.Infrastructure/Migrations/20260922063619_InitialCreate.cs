using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Ordering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_email = table.Column<string>(
                        type: "character varying(320)",
                        maxLength: 320,
                        nullable: false
                    ),
                    shipping_line1 = table.Column<string>(
                        type: "character varying(300)",
                        maxLength: 300,
                        nullable: false
                    ),
                    shipping_city = table.Column<string>(
                        type: "character varying(120)",
                        maxLength: 120,
                        nullable: false
                    ),
                    shipping_postal_code = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    shipping_country = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    updated_at = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    unit_price = table.Column<decimal>(
                        type: "numeric(18,2)",
                        precision: 18,
                        scale: 2,
                        nullable: false
                    ),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id_created_at",
                table: "orders",
                columns: new[] { "customer_id", "created_at" }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "order_items");

            migrationBuilder.DropTable(name: "orders");
        }
    }
}
