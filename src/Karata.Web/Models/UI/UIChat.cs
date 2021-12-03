namespace Karata.Web.Models.UI;

public class UIChat
{
    public string Text { get; set; }
    public UIUser Sender { get; set; }
    public DateTime Sent { get; set; }
}