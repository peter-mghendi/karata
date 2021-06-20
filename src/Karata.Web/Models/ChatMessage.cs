using System;
namespace Karata.Web.Models
{
    public class ChatMessage
    {
        public string Text { get; set; }
        public string Sender { get; set; }
        public DateTime Sent { get; set; }
    }
}