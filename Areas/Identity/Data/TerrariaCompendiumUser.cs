using Microsoft.AspNetCore.Identity;

namespace TerrariaCompendium.Areas.Identity.Data;

public class TerrariaCompendiumUser : IdentityUser
{
    public required string Name { get; set; }
    public string PlainPassword { get; set; } = string.Empty;
}
