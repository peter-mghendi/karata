using System.Collections.Concurrent;
using System.Collections.Generic;
using Karata.Web.Models;

namespace Karata.Web.Services
{
    public interface IRoomService
    {
        Dictionary<string, Room> rooms { get; }
    }
}