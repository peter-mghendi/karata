#nullable enable

using Microsoft.AspNetCore.Identity;

namespace Karata.Web.Models;

public class ApplicationUser : IdentityUser {
    public bool IsLastCard { get; set; } = false;
    public List<Card> Hand { get; set; } = new();
    public virtual List<Turn> Turns { get; set; } = new();
}