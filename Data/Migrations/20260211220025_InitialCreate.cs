using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariaDB.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CraftingStation",
                columns: table => new
                {
                    CraftingStationName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CraftingStation", x => x.CraftingStationName);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyType",
                columns: table => new
                {
                    CurrencyName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyType", x => x.CurrencyName);
                });

            migrationBuilder.CreateTable(
                name: "GameObject",
                columns: table => new
                {
                    GameObjectName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "VARCHAR(200)", maxLength: 200, nullable: true),
                    Sprite = table.Column<string>(type: "VARCHAR(500)", maxLength: 500, nullable: false),
                    TransformName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameObject", x => x.GameObjectName);
                    table.ForeignKey(
                        name: "FK_GameObject_GameObject_TransformName",
                        column: x => x.TransformName,
                        principalTable: "GameObject",
                        principalColumn: "GameObjectName");
                });

            migrationBuilder.CreateTable(
                name: "TradeType",
                columns: table => new
                {
                    TradeTypeName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeType", x => x.TradeTypeName);
                });

            migrationBuilder.CreateTable(
                name: "Entity",
                columns: table => new
                {
                    EntityId = table.Column<short>(type: "SMALLINT", nullable: false),
                    GameObjectName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    Hp = table.Column<int>(type: "INT", nullable: true),
                    Defense = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity", x => x.EntityId);
                    table.CheckConstraint("CK_Entity_Defense", "Defense >= 0");
                    table.CheckConstraint("CK_Entity_Hp", "Hp >= 0");
                    table.ForeignKey(
                        name: "FK_Entity_GameObject_GameObjectName",
                        column: x => x.GameObjectName,
                        principalTable: "GameObject",
                        principalColumn: "GameObjectName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemId = table.Column<short>(type: "SMALLINT", nullable: false),
                    GameObjectName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    CraftingStationName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true),
                    BasePrice = table.Column<int>(type: "INT", nullable: false),
                    CurrencyName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemId);
                    table.CheckConstraint("CK_Item_BasePrice", "BasePrice >= 0");
                    table.ForeignKey(
                        name: "FK_Item_CraftingStation_CraftingStationName",
                        column: x => x.CraftingStationName,
                        principalTable: "CraftingStation",
                        principalColumn: "CraftingStationName",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Item_CurrencyType_CurrencyName",
                        column: x => x.CurrencyName,
                        principalTable: "CurrencyType",
                        principalColumn: "CurrencyName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Item_GameObject_GameObjectName",
                        column: x => x.GameObjectName,
                        principalTable: "GameObject",
                        principalColumn: "GameObjectName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HostileEntity",
                columns: table => new
                {
                    HostileEntityId = table.Column<short>(type: "SMALLINT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<short>(type: "SMALLINT", nullable: false),
                    ContactDamage = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostileEntity", x => x.HostileEntityId);
                    table.CheckConstraint("CK_HostileEntity_ContactDamage", "ContactDamage >= 0");
                    table.ForeignKey(
                        name: "FK_HostileEntity_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TownNpc",
                columns: table => new
                {
                    TownNpcId = table.Column<byte>(type: "TINYINT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TownNpc", x => x.TownNpcId);
                    table.ForeignKey(
                        name: "FK_TownNpc_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Boss",
                columns: table => new
                {
                    BossName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    SummonItemId = table.Column<short>(type: "SMALLINT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Boss", x => x.BossName);
                    table.ForeignKey(
                        name: "FK_Boss_Item_SummonItemId",
                        column: x => x.SummonItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EntityDrop",
                columns: table => new
                {
                    EntityId = table.Column<short>(type: "SMALLINT", nullable: false),
                    ItemId = table.Column<short>(type: "SMALLINT", nullable: false),
                    Quantity = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityDrop", x => new { x.EntityId, x.ItemId });
                    table.CheckConstraint("CK_EntityDrop_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_EntityDrop_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityDrop_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recipe",
                columns: table => new
                {
                    RecipeId = table.Column<short>(type: "SMALLINT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResultItemId = table.Column<short>(type: "SMALLINT", nullable: false),
                    CraftingStationName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: true),
                    ResultItemQuantity = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipe", x => x.RecipeId);
                    table.CheckConstraint("CK_Recipe_ResultItemQuantity", "ResultItemQuantity > 0");
                    table.ForeignKey(
                        name: "FK_Recipe_CraftingStation_CraftingStationName",
                        column: x => x.CraftingStationName,
                        principalTable: "CraftingStation",
                        principalColumn: "CraftingStationName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recipe_Item_ResultItemId",
                        column: x => x.ResultItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enemy",
                columns: table => new
                {
                    EnemyId = table.Column<short>(type: "SMALLINT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostileEntityId = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enemy", x => x.EnemyId);
                    table.ForeignKey(
                        name: "FK_Enemy_HostileEntity_HostileEntityId",
                        column: x => x.HostileEntityId,
                        principalTable: "HostileEntity",
                        principalColumn: "HostileEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeOffer",
                columns: table => new
                {
                    TownNpcId = table.Column<byte>(type: "TINYINT", nullable: false),
                    ItemId = table.Column<short>(type: "SMALLINT", nullable: false),
                    TradeTypeName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOffer", x => new { x.ItemId, x.TownNpcId });
                    table.CheckConstraint("CK_TradeOffer_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_TradeOffer_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeOffer_TownNpc_TownNpcId",
                        column: x => x.TownNpcId,
                        principalTable: "TownNpc",
                        principalColumn: "TownNpcId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradeOffer_TradeType_TradeTypeName",
                        column: x => x.TradeTypeName,
                        principalTable: "TradeType",
                        principalColumn: "TradeTypeName",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BossDrop",
                columns: table => new
                {
                    BossName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    ItemId = table.Column<short>(type: "SMALLINT", nullable: false),
                    Quantity = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossDrop", x => new { x.BossName, x.ItemId });
                    table.CheckConstraint("CK_BossDrop_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_BossDrop_Boss_BossName",
                        column: x => x.BossName,
                        principalTable: "Boss",
                        principalColumn: "BossName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BossDrop_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BossPart",
                columns: table => new
                {
                    BossPartId = table.Column<short>(type: "SMALLINT", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BossName = table.Column<string>(type: "VARCHAR(50)", maxLength: 50, nullable: false),
                    HostileEntityId = table.Column<short>(type: "SMALLINT", nullable: false),
                    Quantity = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossPart", x => x.BossPartId);
                    table.CheckConstraint("CK_BossPart_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_BossPart_Boss_BossName",
                        column: x => x.BossName,
                        principalTable: "Boss",
                        principalColumn: "BossName",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BossPart_HostileEntity_HostileEntityId",
                        column: x => x.HostileEntityId,
                        principalTable: "HostileEntity",
                        principalColumn: "HostileEntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeItems",
                columns: table => new
                {
                    RecipeId = table.Column<short>(type: "SMALLINT", nullable: false),
                    ItemId = table.Column<short>(type: "SMALLINT", nullable: false),
                    Quantity = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeItems", x => new { x.RecipeId, x.ItemId });
                    table.CheckConstraint("CK_RecipeItems_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_RecipeItems_Item_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Item",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeItems_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BossPartEnemies",
                columns: table => new
                {
                    BossPartId = table.Column<short>(type: "SMALLINT", nullable: false),
                    EnemyId = table.Column<short>(type: "SMALLINT", nullable: false),
                    Quantity = table.Column<short>(type: "SMALLINT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossPartEnemies", x => new { x.BossPartId, x.EnemyId });
                    table.CheckConstraint("CK_BossPartEnemies_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_BossPartEnemies_BossPart_BossPartId",
                        column: x => x.BossPartId,
                        principalTable: "BossPart",
                        principalColumn: "BossPartId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BossPartEnemies_Enemy_EnemyId",
                        column: x => x.EnemyId,
                        principalTable: "Enemy",
                        principalColumn: "EnemyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Boss_SummonItemId",
                table: "Boss",
                column: "SummonItemId",
                unique: true,
                filter: "[SummonItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BossDrop_ItemId",
                table: "BossDrop",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BossPart_BossName",
                table: "BossPart",
                column: "BossName");

            migrationBuilder.CreateIndex(
                name: "IX_BossPart_HostileEntityId",
                table: "BossPart",
                column: "HostileEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BossPartEnemies_EnemyId",
                table: "BossPartEnemies",
                column: "EnemyId");

            migrationBuilder.CreateIndex(
                name: "IX_Enemy_HostileEntityId",
                table: "Enemy",
                column: "HostileEntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entity_GameObjectName",
                table: "Entity",
                column: "GameObjectName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityDrop_ItemId",
                table: "EntityDrop",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GameObject_Sprite",
                table: "GameObject",
                column: "Sprite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GameObject_TransformName",
                table: "GameObject",
                column: "TransformName",
                unique: true,
                filter: "[TransformName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HostileEntity_EntityId",
                table: "HostileEntity",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_CraftingStationName",
                table: "Item",
                column: "CraftingStationName");

            migrationBuilder.CreateIndex(
                name: "IX_Item_CurrencyName",
                table: "Item",
                column: "CurrencyName");

            migrationBuilder.CreateIndex(
                name: "IX_Item_GameObjectName",
                table: "Item",
                column: "GameObjectName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_CraftingStationName",
                table: "Recipe",
                column: "CraftingStationName");

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_ResultItemId",
                table: "Recipe",
                column: "ResultItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeItems_ItemId",
                table: "RecipeItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TownNpc_EntityId",
                table: "TownNpc",
                column: "EntityId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffer_TownNpcId",
                table: "TradeOffer",
                column: "TownNpcId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffer_TradeTypeName",
                table: "TradeOffer",
                column: "TradeTypeName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BossDrop");

            migrationBuilder.DropTable(
                name: "BossPartEnemies");

            migrationBuilder.DropTable(
                name: "EntityDrop");

            migrationBuilder.DropTable(
                name: "RecipeItems");

            migrationBuilder.DropTable(
                name: "TradeOffer");

            migrationBuilder.DropTable(
                name: "BossPart");

            migrationBuilder.DropTable(
                name: "Enemy");

            migrationBuilder.DropTable(
                name: "Recipe");

            migrationBuilder.DropTable(
                name: "TownNpc");

            migrationBuilder.DropTable(
                name: "TradeType");

            migrationBuilder.DropTable(
                name: "Boss");

            migrationBuilder.DropTable(
                name: "HostileEntity");

            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "Entity");

            migrationBuilder.DropTable(
                name: "CraftingStation");

            migrationBuilder.DropTable(
                name: "CurrencyType");

            migrationBuilder.DropTable(
                name: "GameObject");
        }
    }
}
