using System.Collections.Generic;
namespace Karata.Web.Models
{
    public class Game
    {
        public List<User> Players { get; set; } = new();
        public int CurrentTurn { get; set; } = 0;
    }
}