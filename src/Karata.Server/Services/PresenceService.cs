using System.Collections.Concurrent;

namespace Karata.Server.Services;

// TODO: Look into Microsoft.Extensions.Caching.Memory
public class PresenceService
{
    private readonly ConcurrentDictionary<string, HashSet<string>> _presence = new();
    
    public void AddPresence(string user, string room)
    {
        _presence.AddOrUpdate(user, new HashSet<string> { room }, (_, set) =>
        {
            set.Add(room);
            return set;
        });
    }
    
    public void RemovePresence(string user, string room)
    {
        _presence.AddOrUpdate(user, new HashSet<string> { room }, (_, set) =>
        {
            set.Remove(room);
            return set;
        });
    }
    
    public bool TryGetPresence(string user, out HashSet<string> rooms)
    {
        // TODO: Weird null check
        return _presence.TryGetValue(user, out rooms!);
    }

    public HashSet<string> this[string key]
    {
        get => _presence[key];
        set => _presence[key] = value;
    }
}