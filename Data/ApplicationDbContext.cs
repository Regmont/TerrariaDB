using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TerrariaDB.Models.Terraria;

namespace TerrariaDB.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<Boss> Boss { get; set; } = default!;
        public DbSet<BossPart> BossPart { get; set; } = default!;
        public DbSet<Enemy> Enemy { get; set; } = default!;
        public DbSet<HostileEntity> HostileEntity { get; set; } = default!;
        public DbSet<Entity> Entity { get; set; } = default!;
        public DbSet<TownNpc> TownNpc { get; set; } = default!;
        public DbSet<GameObject> GameObject { get; set; } = default!;
        public DbSet<Item> Item { get; set; } = default!;
        public DbSet<CraftingStation> CraftingStation { get; set; } = default!;
        public DbSet<Recipe> Recipe { get; set; } = default!;
        public DbSet<BossDrop> BossDrop { get; set; } = default!;
        public DbSet<BossPartEnemies> BossPartEnemies { get; set; } = default!;
        public DbSet<TradeOffer> TradeOffer { get; set; } = default!;
        public DbSet<EntityDrop> EntityDrop { get; set; } = default!;
        public DbSet<RecipeItems> RecipeItems { get; set; } = default!;
        public DbSet<TradeType> TradeType { get; set; } = default!;
        public DbSet<CurrencyType> CurrencyType { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Boss>(entity =>
            {
                entity.HasKey(e => e.BossName);
                entity.Property(e => e.BossName).HasColumnType("VARCHAR(50)").HasMaxLength(50);
                entity.Property(e => e.SummonItemId).HasColumnType("SMALLINT").IsRequired(false);
                entity.HasOne(e => e.SummonItem).WithOne(i => i.SummonedBoss).HasForeignKey<Boss>(e => e.SummonItemId).OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(e => e.BossParts).WithOne(bp => bp.Boss).HasForeignKey(bp => bp.BossName);
                entity.HasMany(e => e.BossDrops).WithOne(bd => bd.Boss).HasForeignKey(bd => bd.BossName);
            });

            builder.Entity<BossPart>(entity =>
            {
                entity.HasKey(e => e.BossPartId);
                entity.Property(e => e.BossPartId).HasColumnType("SMALLINT").ValueGeneratedOnAdd();
                entity.Property(e => e.BossName).HasColumnType("VARCHAR(50)").HasMaxLength(50);
                entity.Property(e => e.HostileEntityId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.Quantity).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_BossPart_Quantity", "Quantity > 0"));
                entity.HasOne(e => e.Boss).WithMany(b => b.BossParts).HasForeignKey(e => e.BossName).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasOne(e => e.HostileEntity).WithOne(he => he.BossPart).HasForeignKey<BossPart>(e => e.HostileEntityId).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasMany(e => e.BossPartEnemies).WithOne(bpe => bpe.BossPart).HasForeignKey(bpe => bpe.BossPartId);
            });

            builder.Entity<Enemy>(entity =>
            {
                entity.HasKey(e => e.EnemyId);
                entity.Property(e => e.EnemyId).HasColumnType("SMALLINT").ValueGeneratedOnAdd();
                entity.Property(e => e.HostileEntityId).HasColumnType("SMALLINT").IsRequired();
                entity.HasOne(e => e.HostileEntity).WithOne(he => he.Enemy).HasForeignKey<Enemy>(e => e.HostileEntityId).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasMany(e => e.BossPartEnemies).WithOne(bpe => bpe.Enemy).HasForeignKey(bpe => bpe.EnemyId);
            });

            builder.Entity<HostileEntity>(entity =>
            {
                entity.HasKey(e => e.HostileEntityId);
                entity.Property(e => e.HostileEntityId).HasColumnType("SMALLINT").ValueGeneratedOnAdd();
                entity.Property(e => e.EntityId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.ContactDamage).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_HostileEntity_ContactDamage", "ContactDamage >= 0"));
                entity.HasOne(e => e.Entity).WithOne(en => en.HostileEntity).HasForeignKey<HostileEntity>(e => e.EntityId).OnDelete(DeleteBehavior.Cascade).IsRequired();
            });

            builder.Entity<Entity>(entity =>
            {
                entity.HasKey(e => e.EntityId);
                entity.Property(e => e.EntityId).HasColumnType("SMALLINT").ValueGeneratedNever();
                entity.Property(e => e.GameObjectName).HasColumnType("VARCHAR(50)").HasMaxLength(50);
                entity.Property(e => e.Hp).HasColumnType("INT").IsRequired(false);
                entity.Property(e => e.Defense).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => {
                    tb.HasCheckConstraint("CK_Entity_Hp", "Hp >= 0");
                    tb.HasCheckConstraint("CK_Entity_Defense", "Defense >= 0");
                });
                entity.HasOne(e => e.GameObject).WithOne(go => go.Entity).HasForeignKey<Entity>(e => e.GameObjectName).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasMany(e => e.EntityDrops).WithOne(ed => ed.Entity).HasForeignKey(ed => ed.EntityId);
            });

            builder.Entity<TownNpc>(entity =>
            {
                entity.HasKey(e => e.TownNpcId);
                entity.Property(e => e.TownNpcId).HasColumnType("TINYINT").ValueGeneratedOnAdd();
                entity.Property(e => e.EntityId).HasColumnType("SMALLINT").IsRequired();
                entity.HasOne(e => e.Entity).WithOne(en => en.TownNpc).HasForeignKey<TownNpc>(e => e.EntityId).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasMany(e => e.TradeOffers).WithOne(to => to.TownNpc).HasForeignKey(to => to.TownNpcId);
            });

            builder.Entity<GameObject>(entity =>
            {
                entity.HasKey(e => e.GameObjectName);
                entity.Property(e => e.GameObjectName).HasColumnType("VARCHAR(50)").HasMaxLength(50);
                entity.Property(e => e.TransformName).HasColumnType("VARCHAR(50)").HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.Description).HasColumnType("VARCHAR(200)").HasMaxLength(200).IsRequired(false);
                entity.Property(e => e.Sprite).HasColumnType("VARCHAR(500)").HasMaxLength(500).IsRequired();
                entity.HasIndex(e => e.Sprite).IsUnique();
                entity.HasOne(e => e.Transform).WithOne(e => e.TransformedFrom).HasForeignKey<GameObject>(e => e.TransformName).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Entity).WithOne(en => en.GameObject).HasForeignKey<Entity>(e => e.GameObjectName);
                entity.HasOne(e => e.Item).WithOne(i => i.GameObject).HasForeignKey<Item>(e => e.GameObjectName);
            });

            builder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.ItemId);
                entity.Property(e => e.ItemId).HasColumnType("SMALLINT").ValueGeneratedNever();
                entity.Property(e => e.GameObjectName).HasColumnType("VARCHAR(50)").HasMaxLength(50);
                entity.Property(e => e.CraftingStationName).HasColumnType("VARCHAR(50)").HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.BasePrice).HasColumnType("INT").IsRequired();
                entity.Property(e => e.CurrencyName).HasColumnType("VARCHAR(50)").HasMaxLength(50).IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_Item_BasePrice", "BasePrice >= 0"));
                entity.HasOne(e => e.GameObject).WithOne(go => go.Item).HasForeignKey<Item>(e => e.GameObjectName).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasOne(e => e.CraftingStation).WithMany(cs => cs.Items).HasForeignKey(e => e.CraftingStationName).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.CurrencyType).WithMany(cs => cs.Items).HasForeignKey(e => e.CurrencyName).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(e => e.BossDrops).WithOne(bd => bd.Item).HasForeignKey(bd => bd.ItemId);
                entity.HasMany(e => e.EntityDrops).WithOne(bpd => bpd.Item).HasForeignKey(bpd => bpd.ItemId);
                entity.HasMany(e => e.TradeOffers).WithOne(to => to.Item).HasForeignKey(to => to.ItemId);
                entity.HasMany(e => e.ResultRecipes).WithOne(r => r.ResultItem).HasForeignKey(r => r.ResultItemId);
                entity.HasMany(e => e.RecipeItems).WithOne(ri => ri.Item).HasForeignKey(ri => ri.ItemId);
            });

            builder.Entity<CraftingStation>(entity =>
            {
                entity.HasKey(e => e.CraftingStationName);
                entity.Property(e => e.CraftingStationName).HasColumnType("VARCHAR(50)").HasMaxLength(50);
                entity.HasMany(e => e.Items).WithOne(i => i.CraftingStation).HasForeignKey(i => i.CraftingStationName);
                entity.HasMany(e => e.Recipes).WithOne(r => r.CraftingStation).HasForeignKey(r => r.CraftingStationName);
            });

            builder.Entity<Recipe>(entity =>
            {
                entity.HasKey(e => e.RecipeId);
                entity.Property(e => e.RecipeId).HasColumnType("SMALLINT").ValueGeneratedOnAdd();
                entity.Property(e => e.ResultItemId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.CraftingStationName).HasColumnType("VARCHAR(50)").HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.ResultItemQuantity).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_Recipe_ResultItemQuantity", "ResultItemQuantity > 0"));
                entity.HasOne(e => e.ResultItem).WithMany(i => i.ResultRecipes).HasForeignKey(e => e.ResultItemId).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasOne(e => e.CraftingStation).WithMany(cs => cs.Recipes).HasForeignKey(e => e.CraftingStationName).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(e => e.RecipeItems).WithOne(ri => ri.Recipe).HasForeignKey(ri => ri.RecipeId);
            });

            builder.Entity<BossDrop>(entity =>
            {
                entity.HasKey(e => new { e.BossName, e.ItemId });
                entity.Property(e => e.BossName).HasColumnType("VARCHAR(50)").HasMaxLength(50).IsRequired();
                entity.Property(e => e.ItemId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.Quantity).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_BossDrop_Quantity", "Quantity > 0"));
                entity.HasOne(e => e.Boss).WithMany(b => b.BossDrops).HasForeignKey(e => e.BossName).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasOne(e => e.Item).WithMany(i => i.BossDrops).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict).IsRequired();
            });

            builder.Entity<BossPartEnemies>(entity =>
            {
                entity.HasKey(e => new { e.BossPartId, e.EnemyId });
                entity.Property(e => e.BossPartId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.EnemyId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.Quantity).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_BossPartEnemies_Quantity", "Quantity > 0"));
                entity.HasOne(e => e.BossPart).WithMany(bp => bp.BossPartEnemies).HasForeignKey(e => e.BossPartId).OnDelete(DeleteBehavior.Restrict).IsRequired();
                entity.HasOne(e => e.Enemy).WithMany(e => e.BossPartEnemies).HasForeignKey(e => e.EnemyId).OnDelete(DeleteBehavior.Cascade).IsRequired();
            });

            builder.Entity<TradeOffer>(entity =>
            {
                entity.HasKey(e => new { e.ItemId, e.TownNpcId });
                entity.Property(e => e.ItemId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.TownNpcId).HasColumnType("TINYINT").IsRequired();
                entity.Property(e => e.TradeTypeName).HasColumnType("VARCHAR(50)").HasMaxLength(50).IsRequired();
                entity.Property(e => e.Quantity).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_TradeOffer_Quantity", "Quantity > 0"));
                entity.HasOne(e => e.TownNpc).WithMany(tn => tn.TradeOffers).HasForeignKey(e => e.TownNpcId).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasOne(e => e.Item).WithMany(i => i.TradeOffers).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict).IsRequired();
                entity.HasOne(e => e.TradeType).WithMany(tt => tt.TradeOffers).HasForeignKey(e => e.TradeTypeName).OnDelete(DeleteBehavior.Restrict).IsRequired();
            });

            builder.Entity<EntityDrop>(entity =>
            {
                entity.HasKey(e => new { e.EntityId, e.ItemId });
                entity.Property(e => e.EntityId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.ItemId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.Quantity).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_EntityDrop_Quantity", "Quantity > 0"));
                entity.HasOne(e => e.Entity).WithMany(i => i.EntityDrops).HasForeignKey(e => e.EntityId).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasOne(e => e.Item).WithMany(i => i.EntityDrops).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict).IsRequired();
            });

            builder.Entity<RecipeItems>(entity =>
            {
                entity.HasKey(e => new { e.RecipeId, e.ItemId });
                entity.Property(e => e.RecipeId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.ItemId).HasColumnType("SMALLINT").IsRequired();
                entity.Property(e => e.Quantity).HasColumnType("SMALLINT").IsRequired();
                entity.ToTable(tb => tb.HasCheckConstraint("CK_RecipeItems_Quantity", "Quantity > 0"));
                entity.HasOne(e => e.Recipe).WithMany(r => r.RecipeItems).HasForeignKey(e => e.RecipeId).OnDelete(DeleteBehavior.Cascade).IsRequired();
                entity.HasOne(e => e.Item).WithMany(i => i.RecipeItems).HasForeignKey(e => e.ItemId).OnDelete(DeleteBehavior.Restrict).IsRequired();
            });

            builder.Entity<TradeType>(entity =>
            {
                entity.HasKey(e => e.TradeTypeName);
                entity.Property(e => e.TradeTypeName).HasColumnType("VARCHAR(50)").HasMaxLength(50);
                entity.HasMany(e => e.TradeOffers).WithOne(to => to.TradeType).HasForeignKey(to => to.TradeTypeName);
            });

            builder.Entity<CurrencyType>(entity =>
            {
                entity.HasKey(e => e.CurrencyName);
                entity.Property(e => e.CurrencyName).HasColumnType("VARCHAR(50)").HasMaxLength(50);
                entity.HasMany(e => e.Items).WithOne(to => to.CurrencyType).HasForeignKey(to => to.CurrencyName);
            });
        }
    }
}
