using System.Collections.Concurrent;

namespace Karata.Server.Services;

// TODO: Look into Microsoft.Extensions.Caching.Hybrid
public class PresenceService
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _presence = new();
    
    public void AddPresence(string user, string room)
    {
        _presence.AddOrUpdate(user, [room], (_, set) =>
        {
            set.Add(room);
            return set;
        });
    }
    
    public void RemovePresence(string user, string room)
    {
        _presence.AddOrUpdate(user, [room], (_, set) =>
        {
            set.Remove(room);
            return set;
        });
    }
    
    public bool TryGetPresence(string user, out HashSet<string>? rooms)
    {
        return _presence.TryGetValue(user, out rooms);
    }

    public HashSet<string> this[string key]
    {
        get => _presence[key];
        set => _presence[key] = value;
    }
}