using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TerrariaCompendium.Areas.Identity.Data;

namespace TerrariaCompendium.Data;

public class TerrariaCompendiumContext : IdentityDbContext<TerrariaCompendiumUser>
{
    public TerrariaCompendiumContext(DbContextOptions<TerrariaCompendiumContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TerrariaCompendiumUser>(entity =>
        {
            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.PlainPassword)
                .IsRequired()
                .HasMaxLength(100);
        });
    }
}
