using System;
namespace Karata.Web.Models
{
    public class SystemMessage
    {
        public string Text { get; set; }
        public DateTime Sent { get; set; } = DateTime.Now;
        
        public SystemMessage(string text) => Text = text;
    }
}