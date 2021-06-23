using System.Collections.Concurrent;
using System.Collections.Generic;
using Karata.Web.Models;

namespace Karata.Web.Services
{

    public class RoomService : IRoomService
    {
        public Dictionary<string, Room> Rooms { get; } = new();
    }
}