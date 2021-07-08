using System;
namespace Karata.Web.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ApplicationUser Sender { get; set; }
        public Room Room { get; set; }
        public DateTime Sent { get; } = DateTime.Now;
    }
}