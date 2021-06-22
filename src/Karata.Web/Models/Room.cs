using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Karata.Web.Models
{
    public class Room
    {
        public string Link { get; set; }

        public User Creator { get; set; }

        public DateTimeOffset CreatedAt { get; } = DateTime.Now;

        public List<User> Members { get; set; } = new();
    }
}
