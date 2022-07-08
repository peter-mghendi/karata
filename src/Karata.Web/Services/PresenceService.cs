using System.Collections.Concurrent;
using System.Text.Json;

namespace Karata.Web.Services;

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
        Debug();
    }
    
    public void RemovePresence(string user, string room)
    {
        _presence.AddOrUpdate(user, new HashSet<string> { room }, (_, set) =>
        {
            set.Remove(room);
            return set;
        });
        Debug();
    }
    
    public bool TryGetPresence(string user, out HashSet<string> rooms)
    {
        return _presence.TryGetValue(user, out rooms);
    }

    public HashSet<string> this[string key]
    {
        get => _presence[key];
        set => _presence[key] = value;
    }

    private void Debug() => Console.WriteLine(JsonSerializer.Serialize(_presence));
}