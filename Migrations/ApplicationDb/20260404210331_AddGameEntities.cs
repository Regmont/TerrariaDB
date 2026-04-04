using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerrariaCompendium.Migrations.ApplicationDb
{
    /// <inheritdoc />
    public partial class AddGameEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CraftingStations",
                columns: table => new
                {
                    CraftingStationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CraftingStationName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CraftingStationSprite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CraftingStations", x => x.CraftingStationId);
                });

            migrationBuilder.CreateTable(
                name: "Entities",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InternalNpcId = table.Column<short>(type: "smallint", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entities", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CraftingStationId = table.Column<int>(type: "int", nullable: true),
                    InternalItemId = table.Column<short>(type: "smallint", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuyPrice = table.Column<int>(type: "int", nullable: false),
                    SellPrice = table.Column<int>(type: "int", nullable: false),
                    BuyPriceCurrency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                    table.CheckConstraint("CK_Item_BuyPrice", "BuyPrice >= 0");
                    table.CheckConstraint("CK_Item_SellPrice", "SellPrice >= 0");
                    table.ForeignKey(
                        name: "FK_Items_CraftingStations_CraftingStationId",
                        column: x => x.CraftingStationId,
                        principalTable: "CraftingStations",
                        principalColumn: "CraftingStationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Enemies",
                columns: table => new
                {
                    EnemyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    ContactDamage = table.Column<short>(type: "smallint", nullable: false),
                    Hp = table.Column<short>(type: "smallint", nullable: false),
                    Defense = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enemies", x => x.EnemyId);
                    table.CheckConstraint("CK_Enemy_ContactDamage", "ContactDamage >= 0");
                    table.CheckConstraint("CK_Enemy_Defense", "Defense >= 0");
                    table.CheckConstraint("CK_Enemy_Hp", "Hp >= 0");
                    table.ForeignKey(
                        name: "FK_Enemies_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TownNpc",
                columns: table => new
                {
                    TownNpcId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Hp = table.Column<short>(type: "smallint", nullable: false),
                    Defense = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TownNpc", x => x.TownNpcId);
                    table.CheckConstraint("CK_TownNpc_Defense", "Defense >= 0");
                    table.CheckConstraint("CK_TownNpc_Hp", "Hp >= 0");
                    table.ForeignKey(
                        name: "FK_TownNpc_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bosses",
                columns: table => new
                {
                    BossId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SummonItemId = table.Column<int>(type: "int", nullable: true),
                    BossName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BossSprite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bosses", x => x.BossId);
                    table.ForeignKey(
                        name: "FK_Bosses_Items_SummonItemId",
                        column: x => x.SummonItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EntityDrops",
                columns: table => new
                {
                    EntityDropId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityDrops", x => x.EntityDropId);
                    table.CheckConstraint("CK_EntityDrop_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_EntityDrops_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityDrops_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemForms",
                columns: table => new
                {
                    ItemFormId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemSprite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ItemFormOrderId = table.Column<short>(type: "smallint", nullable: false),
                    Tooltip = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemForms", x => x.ItemFormId);
                    table.CheckConstraint("CK_ItemForm_ItemFormOrderId", "ItemFormOrderId >= 0 AND ItemFormOrderId <= 4");
                    table.ForeignKey(
                        name: "FK_ItemForms_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResultItemId = table.Column<int>(type: "int", nullable: false),
                    CraftingStationId = table.Column<int>(type: "int", nullable: true),
                    ResultItemQuantity = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.RecipeId);
                    table.CheckConstraint("CK_Recipe_ResultItemQuantity", "ResultItemQuantity > 0");
                    table.ForeignKey(
                        name: "FK_Recipes_CraftingStations_CraftingStationId",
                        column: x => x.CraftingStationId,
                        principalTable: "CraftingStations",
                        principalColumn: "CraftingStationId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Recipes_Items_ResultItemId",
                        column: x => x.ResultItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TownNpcForms",
                columns: table => new
                {
                    TownNpcFormId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TownNpcId = table.Column<int>(type: "int", nullable: false),
                    TownNpcFormSprite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TownNpcFormOrderId = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TownNpcForms", x => x.TownNpcFormId);
                    table.CheckConstraint("CK_TownNpcForm_TownNpcFormOrderId", "TownNpcFormOrderId >= 0 AND TownNpcFormOrderId <= 4");
                    table.ForeignKey(
                        name: "FK_TownNpcForms_TownNpc_TownNpcId",
                        column: x => x.TownNpcId,
                        principalTable: "TownNpc",
                        principalColumn: "TownNpcId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeOffers",
                columns: table => new
                {
                    TradeOfferId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TownNpcId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false),
                    TradeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOffers", x => x.TradeOfferId);
                    table.CheckConstraint("CK_TradeOffer_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_TradeOffers_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeOffers_TownNpc_TownNpcId",
                        column: x => x.TownNpcId,
                        principalTable: "TownNpc",
                        principalColumn: "TownNpcId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BossDrops",
                columns: table => new
                {
                    BossDropId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BossId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossDrops", x => x.BossDropId);
                    table.CheckConstraint("CK_BossDrop_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_BossDrops_Bosses_BossId",
                        column: x => x.BossId,
                        principalTable: "Bosses",
                        principalColumn: "BossId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BossDrops_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BossParts",
                columns: table => new
                {
                    BossPartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BossId = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    BossPartOrderId = table.Column<short>(type: "smallint", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossParts", x => x.BossPartId);
                    table.CheckConstraint("CK_BossPart_BossPartOrderId", "BossPartOrderId >= 0 AND BossPartOrderId <= 5");
                    table.CheckConstraint("CK_BossPart_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_BossParts_Bosses_BossId",
                        column: x => x.BossId,
                        principalTable: "Bosses",
                        principalColumn: "BossId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BossParts_Entities_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entities",
                        principalColumn: "EntityId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecipeItems",
                columns: table => new
                {
                    RecipeItemsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeItems", x => x.RecipeItemsId);
                    table.CheckConstraint("CK_RecipeItems_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_RecipeItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecipeItems_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "RecipeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BossPartStages",
                columns: table => new
                {
                    BossPartStageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BossPartId = table.Column<int>(type: "int", nullable: false),
                    BossPartStageSprite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BossPartStageOrderId = table.Column<short>(type: "smallint", nullable: false),
                    ContactDamage = table.Column<short>(type: "smallint", nullable: false),
                    Hp = table.Column<short>(type: "smallint", nullable: false),
                    Defense = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossPartStages", x => x.BossPartStageId);
                    table.CheckConstraint("CK_BossPartStage_BossPartStageOrderId", "BossPartStageOrderId >= 0 AND BossPartStageOrderId <= 3");
                    table.CheckConstraint("CK_BossPartStage_ContactDamage", "ContactDamage >= 0");
                    table.CheckConstraint("CK_BossPartStage_Defense", "Defense >= 0");
                    table.CheckConstraint("CK_BossPartStage_Hp", "Hp >= 0");
                    table.ForeignKey(
                        name: "FK_BossPartStages_BossParts_BossPartId",
                        column: x => x.BossPartId,
                        principalTable: "BossParts",
                        principalColumn: "BossPartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BossPartStageEnemies",
                columns: table => new
                {
                    BossPartStageEnemiesId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BossPartStageId = table.Column<int>(type: "int", nullable: false),
                    EnemyId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossPartStageEnemies", x => x.BossPartStageEnemiesId);
                    table.CheckConstraint("CK_BossPartStageEnemies_Quantity", "Quantity > 0");
                    table.ForeignKey(
                        name: "FK_BossPartStageEnemies_BossPartStages_BossPartStageId",
                        column: x => x.BossPartStageId,
                        principalTable: "BossPartStages",
                        principalColumn: "BossPartStageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BossPartStageEnemies_Enemies_EnemyId",
                        column: x => x.EnemyId,
                        principalTable: "Enemies",
                        principalColumn: "EnemyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BossDrops_BossId",
                table: "BossDrops",
                column: "BossId");

            migrationBuilder.CreateIndex(
                name: "IX_BossDrops_ItemId",
                table: "BossDrops",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Bosses_BossName",
                table: "Bosses",
                column: "BossName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bosses_BossSprite",
                table: "Bosses",
                column: "BossSprite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bosses_SummonItemId",
                table: "Bosses",
                column: "SummonItemId",
                unique: true,
                filter: "[SummonItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BossParts_BossId",
                table: "BossParts",
                column: "BossId");

            migrationBuilder.CreateIndex(
                name: "IX_BossParts_BossPartOrderId",
                table: "BossParts",
                column: "BossPartOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BossParts_EntityId",
                table: "BossParts",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_BossPartStageEnemies_BossPartStageId",
                table: "BossPartStageEnemies",
                column: "BossPartStageId");

            migrationBuilder.CreateIndex(
                name: "IX_BossPartStageEnemies_EnemyId",
                table: "BossPartStageEnemies",
                column: "EnemyId");

            migrationBuilder.CreateIndex(
                name: "IX_BossPartStages_BossPartId",
                table: "BossPartStages",
                column: "BossPartId");

            migrationBuilder.CreateIndex(
                name: "IX_BossPartStages_BossPartStageOrderId",
                table: "BossPartStages",
                column: "BossPartStageOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CraftingStations_CraftingStationName",
                table: "CraftingStations",
                column: "CraftingStationName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CraftingStations_CraftingStationSprite",
                table: "CraftingStations",
                column: "CraftingStationSprite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enemies_EntityId",
                table: "Enemies",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Entities_InternalNpcId",
                table: "Entities",
                column: "InternalNpcId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntityDrops_EntityId",
                table: "EntityDrops",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityDrops_ItemId",
                table: "EntityDrops",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemForms_ItemFormOrderId",
                table: "ItemForms",
                column: "ItemFormOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemForms_ItemId",
                table: "ItemForms",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemForms_ItemSprite",
                table: "ItemForms",
                column: "ItemSprite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_CraftingStationId",
                table: "Items",
                column: "CraftingStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InternalItemId",
                table: "Items",
                column: "InternalItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemName",
                table: "Items",
                column: "ItemName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeItems_ItemId",
                table: "RecipeItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeItems_RecipeId",
                table: "RecipeItems",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_CraftingStationId",
                table: "Recipes",
                column: "CraftingStationId");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ResultItemId",
                table: "Recipes",
                column: "ResultItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TownNpc_EntityId",
                table: "TownNpc",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_TownNpcForms_TownNpcFormOrderId",
                table: "TownNpcForms",
                column: "TownNpcFormOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TownNpcForms_TownNpcFormSprite",
                table: "TownNpcForms",
                column: "TownNpcFormSprite",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TownNpcForms_TownNpcId",
                table: "TownNpcForms",
                column: "TownNpcId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_ItemId",
                table: "TradeOffers",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_TownNpcId",
                table: "TradeOffers",
                column: "TownNpcId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BossDrops");

            migrationBuilder.DropTable(
                name: "BossPartStageEnemies");

            migrationBuilder.DropTable(
                name: "EntityDrops");

            migrationBuilder.DropTable(
                name: "ItemForms");

            migrationBuilder.DropTable(
                name: "RecipeItems");

            migrationBuilder.DropTable(
                name: "TownNpcForms");

            migrationBuilder.DropTable(
                name: "TradeOffers");

            migrationBuilder.DropTable(
                name: "BossPartStages");

            migrationBuilder.DropTable(
                name: "Enemies");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "TownNpc");

            migrationBuilder.DropTable(
                name: "BossParts");

            migrationBuilder.DropTable(
                name: "Bosses");

            migrationBuilder.DropTable(
                name: "Entities");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "CraftingStations");
        }
    }
}
