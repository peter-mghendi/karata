using System;
namespace Karata.Web.Models
{
    public class SystemMessage
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public Room Room { get; set; }
        public DateTime Sent { get; set; } = DateTime.Now;
    }
}