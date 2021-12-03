#nullable enable

using Karata.Web.Models.UI;
using Microsoft.AspNetCore.Identity;

namespace Karata.Web.Models;

public class User : IdentityUser {
    public virtual List<Hand> Hands { get; set; } = new();
    public virtual List<Turn> Turns { get; set; } = new();

    public UIUser ToUI() => new () 
    {
        Id = Id,
        Email = Email,
    };
}