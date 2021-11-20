namespace Karata.Web.Models;

public class Chat
{
    public int Id { get; set; }
    public string Text { get; set; }
    public virtual ApplicationUser Sender { get; set; }
    public DateTime Sent { get; } = DateTime.Now;
}