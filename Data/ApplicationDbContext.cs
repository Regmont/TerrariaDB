using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using TerrariaCompendium.Models.Terraria;

namespace TerrariaCompendium.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Boss> Bosses { get; set; } = default!;
        public DbSet<BossPart> BossParts { get; set; } = default!;
        public DbSet<BossPartStage> BossPartStages { get; set; } = default!;
        public DbSet<Enemy> Enemies { get; set; } = default!;
        public DbSet<Entity> Entities { get; set; } = default!;
        public DbSet<TownNpc> TownNpc { get; set; } = default!;
        public DbSet<TownNpcForm> TownNpcForms { get; set; } = default!;
        public DbSet<Item> Items { get; set; } = default!;
        public DbSet<ItemForm> ItemForms { get; set; } = default!;
        public DbSet<CraftingStation> CraftingStations { get; set; } = default!;
        public DbSet<Recipe> Recipes { get; set; } = default!;
        public DbSet<BossDrop> BossDrops { get; set; } = default!;
        public DbSet<BossPartStageEnemies> BossPartStageEnemies { get; set; } = default!;
        public DbSet<EntityDrop> EntityDrops { get; set; } = default!;
        public DbSet<TradeOffer> TradeOffers { get; set; } = default!;
        public DbSet<RecipeItems> RecipeItems { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Ignore<IdentityRole>();
            builder.Ignore<IdentityUser>();
            builder.Ignore<IdentityRoleClaim<string>>();
            builder.Ignore<IdentityUserClaim<string>>();
            builder.Ignore<IdentityUserLogin<string>>();
            builder.Ignore<IdentityUserRole<string>>();
            builder.Ignore<IdentityUserToken<string>>();

            builder.Entity<Boss>(entity =>
            {
                entity.HasKey(e => e.BossId);

                entity.Property(e => e.BossName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.BossSprite).HasMaxLength(500).IsRequired();

                entity.HasIndex(e => e.BossName).IsUnique();
                entity.HasIndex(e => e.BossSprite).IsUnique();

                entity.HasOne(e => e.SummonItem).WithOne(i => i.SummonedBoss).HasForeignKey<Boss>(e => e.SummonItemId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<BossPart>(entity =>
            {
                entity.HasKey(e => e.BossPartId);

                entity.HasOne(e => e.Boss).WithMany(b => b.BossParts).HasForeignKey(e => e.BossId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Entity).WithMany(e => e.BossParts).HasForeignKey(e => e.EntityId).OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.BossPartOrderId).IsUnique();

                entity.ToTable(tt => tt.HasCheckConstraint("CK_BossPart_BossPartOrderId", "BossPartOrderId >= 0 AND BossPartOrderId <= 5"));
                entity.ToTable(tt => tt.HasCheckConstraint("CK_BossPart_Quantity", "Quantity > 0"));
            });

            builder.Entity<BossPartStage>(entity =>
            {
                entity.HasKey(e => e.BossPartStageId);

                entity.Property(e => e.BossPartStageSprite).HasMaxLength(500).IsRequired();

                entity.HasOne(e => e.BossPart).WithMany(bp => bp.BossPartStages).HasForeignKey(e => e.BossPartId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.BossPartStageOrderId).IsUnique();

                entity.ToTable(tt => tt.HasCheckConstraint("CK_BossPartStage_BossPartStageOrderId", "BossPartStageOrderId >= 0 AND BossPartStageOrderId <= 3"));
                entity.ToTable(tt => tt.HasCheckConstraint("CK_BossPartStage_ContactDamage", "ContactDamage >= 0"));
                entity.ToTable(tt => tt.HasCheckConstraint("CK_BossPartStage_Hp", "Hp >= 0"));
                entity.ToTable(tt => tt.HasCheckConstraint("CK_BossPartStage_Defense", "Defense >= 0"));
            });

            builder.Entity<Enemy>(entity =>
            {
                entity.HasKey(e => e.EnemyId);

                entity.HasOne(e => e.Entity).WithMany(e => e.Enemies).HasForeignKey(e => e.EntityId).OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(tt => tt.HasCheckConstraint("CK_Enemy_ContactDamage", "ContactDamage >= 0"));
                entity.ToTable(tt => tt.HasCheckConstraint("CK_Enemy_Hp", "Hp >= 0"));
                entity.ToTable(tt => tt.HasCheckConstraint("CK_Enemy_Defense", "Defense >= 0"));
            });

            builder.Entity<Entity>(entity =>
            {
                entity.HasKey(e => e.EntityId);

                entity.Property(e => e.EntityName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.HasIndex(e => e.InternalNpcId).IsUnique();
            });

            builder.Entity<TownNpc>(entity =>
            {
                entity.HasKey(e => e.TownNpcId);

                entity.HasOne(e => e.Entity).WithMany(e => e.TownNpcs).HasForeignKey(e => e.EntityId).OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(tt => tt.HasCheckConstraint("CK_TownNpc_Hp", "Hp >= 0"));
                entity.ToTable(tt => tt.HasCheckConstraint("CK_TownNpc_Defense", "Defense >= 0"));
            });

            builder.Entity<TownNpcForm>(entity =>
            {
                entity.HasKey(e => e.TownNpcFormId);

                entity.Property(e => e.TownNpcFormSprite).HasMaxLength(500).IsRequired();

                entity.HasOne(e => e.TownNpc).WithMany(tn => tn.TownNpcForms).HasForeignKey(e => e.TownNpcId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.TownNpcFormSprite).IsUnique();
                entity.HasIndex(e => e.TownNpcFormOrderId).IsUnique();

                entity.ToTable(tt => tt.HasCheckConstraint("CK_TownNpcForm_TownNpcFormOrderId", "TownNpcFormOrderId >= 0 AND TownNpcFormOrderId <= 4"));
            });

            builder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.ItemId);

                entity.Property(e => e.ItemName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.BuyPriceCurrency).HasConversion<string>().HasMaxLength(50);

                entity.HasOne(e => e.CraftingStation).WithMany(cs => cs.Items).HasForeignKey(e => e.CraftingStationId).OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.InternalItemId).IsUnique();
                entity.HasIndex(e => e.ItemName).IsUnique();

                entity.ToTable(tt => tt.HasCheckConstraint("CK_Item_BuyPrice", "BuyPrice >= 0"));
                entity.ToTable(tt => tt.HasCheckConstraint("CK_Item_SellPrice", "SellPrice >= 0"));
            });

            builder.Entity<ItemForm>(entity =>
            {
                entity.HasKey(e => e.ItemFormId);

                entity.Property(e => e.ItemSprite).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Tooltip).HasMaxLength(200);

                entity.HasOne(e => e.Item).WithMany(i => i.ItemForms).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ItemSprite).IsUnique();
                entity.HasIndex(e => e.ItemFormOrderId).IsUnique();

                entity.ToTable(tt => tt.HasCheckConstraint("CK_ItemForm_ItemFormOrderId", "ItemFormOrderId >= 0 AND ItemFormOrderId <= 4"));
            });

            builder.Entity<CraftingStation>(entity =>
            {
                entity.HasKey(e => e.CraftingStationId);

                entity.Property(e => e.CraftingStationName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CraftingStationSprite).HasMaxLength(500).IsRequired();

                entity.HasIndex(e => e.CraftingStationName).IsUnique();
                entity.HasIndex(e => e.CraftingStationSprite).IsUnique();
            });

            builder.Entity<Recipe>(entity =>
            {
                entity.HasKey(e => e.RecipeId);

                entity.HasOne(e => e.ResultItem).WithMany(i => i.Recipes).HasForeignKey(e => e.ResultItemId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CraftingStation).WithMany(cs => cs.Recipes).HasForeignKey(e => e.CraftingStationId).OnDelete(DeleteBehavior.SetNull);

                entity.ToTable(tt => tt.HasCheckConstraint("CK_Recipe_ResultItemQuantity", "ResultItemQuantity > 0"));
            });

            builder.Entity<BossDrop>(entity =>
            {
                entity.HasKey(e => e.BossDropId);

                entity.HasOne(e => e.Boss).WithMany(b => b.BossDrops).HasForeignKey(e => e.BossId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Item).WithMany(i => i.BossDrops).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(tt => tt.HasCheckConstraint("CK_BossDrop_Quantity", "Quantity > 0"));
            });

            builder.Entity<BossPartStageEnemies>(entity =>
            {
                entity.HasKey(e => e.BossPartStageEnemiesId);

                entity.HasOne(e => e.BossPartStage).WithMany(bps => bps.BossPartStageEnemies).HasForeignKey(e => e.BossPartStageId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Enemy).WithMany(e => e.BossPartStageEnemies).HasForeignKey(e => e.EnemyId).OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(tt => tt.HasCheckConstraint("CK_BossPartStageEnemies_Quantity", "Quantity > 0"));
            });

            builder.Entity<EntityDrop>(entity =>
            {
                entity.HasKey(e => e.EntityDropId);

                entity.HasOne(e => e.Entity).WithMany(e => e.EntityDrops).HasForeignKey(e => e.EntityId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Item).WithMany(i => i.EntityDrops).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(tt => tt.HasCheckConstraint("CK_EntityDrop_Quantity", "Quantity > 0"));
            });

            builder.Entity<TradeOffer>(entity =>
            {
                entity.HasKey(e => e.TradeOfferId);

                entity.Property(e => e.TradeType).HasConversion<string>().HasMaxLength(50);

                entity.HasOne(e => e.TownNpc).WithMany(tn => tn.TradeOffers).HasForeignKey(e => e.TownNpcId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Item).WithMany(i => i.TradeOffers).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(tt => tt.HasCheckConstraint("CK_TradeOffer_Quantity", "Quantity > 0"));
            });

            builder.Entity<RecipeItems>(entity =>
            {
                entity.HasKey(e => e.RecipeItemsId);

                entity.HasOne(e => e.Recipe).WithMany(r => r.RecipeItems).HasForeignKey(e => e.RecipeId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Item).WithMany(i => i.RecipeItems).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(tt => tt.HasCheckConstraint("CK_RecipeItems_Quantity", "Quantity > 0"));
            });
        }
    }
}
