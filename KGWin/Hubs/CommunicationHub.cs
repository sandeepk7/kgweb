using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Windows;
using KGWin;

public class CommunicationHub : Hub
{
    // Raised whenever any client invokes SendMessage on the hub
    public static event Action<string, string>? MessageReceived;
    // Raised for structured payloads so the WPF UI can reflect them in chat
    public static event Action<AssetLocation>? AssetLocationReceived;
    public static event Action<MapMarker>? MapMarkerReceived;
    private static List<AssetLocation> _assetLocations = new List<AssetLocation>();
    private static List<MapMarker> _mapMarkers = new List<MapMarker>();
    private static int _connectedClientsCount = 0;
    private static List<string> _connectedClientIds = new List<string>();
    private static Dictionary<string, string> _connectedClientNames = new Dictionary<string, string>();

    public async Task SendMessage(string user, string message)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending message from {user}: {message}");
        await Clients.All.SendAsync("ReceiveMessage", user, message);
        try
        {
            // Notify in-process listeners (WPF UI) that a message arrived
            MessageReceived?.Invoke(user, message);
        }
        catch { /* best-effort local notification */ }
    }

    public async Task RegisterClientName(string name)
    {
        var connectionId = Context.ConnectionId;
        _connectedClientNames[connectionId] = name;
        
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client {connectionId} registered name: {name}");
        
        // Notify all clients about the name update
        await Clients.All.SendAsync("ClientNameUpdated", connectionId, name);
    }

    // Asset Management Methods
    public async Task SendAssetLocation(AssetLocation asset)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending asset location: {asset.Name}");
        
        // Update or add asset
        var existingAsset = _assetLocations.FirstOrDefault(a => a.Id == asset.Id);
        if (existingAsset != null)
        {
            _assetLocations.Remove(existingAsset);
        }
        _assetLocations.Add(asset);

        // Notify all clients about the asset update
        await Clients.All.SendAsync("ReceiveAssetUpdate", asset);
        
        // Also send the complete list
        await Clients.All.SendAsync("ReceiveAssetLocations", _assetLocations);

        try { AssetLocationReceived?.Invoke(asset); } catch {}
    }

    public async Task SendAssetLocations(List<AssetLocation> assets)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending {assets.Count} asset locations");
        _assetLocations = assets;
        await Clients.All.SendAsync("ReceiveAssetLocations", _assetLocations);
    }

    public async Task RequestAssetLocations()
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Requesting asset locations");
        await Clients.Caller.SendAsync("ReceiveAssetLocations", _assetLocations);
    }

    // Map Marker Methods
    public async Task SendMapMarker(MapMarker marker)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending map marker: {marker.Title}");
        
        // Update or add marker
        var existingMarker = _mapMarkers.FirstOrDefault(m => m.Id == marker.Id);
        if (existingMarker != null)
        {
            _mapMarkers.Remove(existingMarker);
        }
        _mapMarkers.Add(marker);

        // Notify all clients about the marker update
        await Clients.All.SendAsync("ReceiveMapMarkers", _mapMarkers);

        try { MapMarkerReceived?.Invoke(marker); } catch {}
    }

    public async Task SendMapMarkers(List<MapMarker> markers)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending {markers.Count} map markers");
        _mapMarkers = markers;
        await Clients.All.SendAsync("ReceiveMapMarkers", _mapMarkers);
    }

    public async Task RequestMapMarkers()
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Requesting map markers");
        await Clients.Caller.SendAsync("ReceiveMapMarkers", _mapMarkers);
    }

    // Group Management
    public async Task JoinAssetGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserJoinedAssetGroup", Context.ConnectionId, groupName);
    }

    public async Task LeaveAssetGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("UserLeftAssetGroup", Context.ConnectionId, groupName);
    }

    // Connection Management
    public override async Task OnConnectedAsync()
    {
        _connectedClientsCount++;
        _connectedClientIds.Add(Context.ConnectionId);
        
        // Register this hub instance with the App
        if (Application.Current is App app)
        {
            app.SetHubInstance(this);
        }
        
        // Log connection
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client connected: {Context.ConnectionId} (Total: {_connectedClientsCount})");
        
        await Clients.All.SendAsync("ConnectionStatus", true);
        await Clients.All.SendAsync("ClientConnected", Context.ConnectionId, _connectedClientsCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectedClientsCount = Math.Max(0, _connectedClientsCount - 1);
        _connectedClientIds.Remove(Context.ConnectionId);
        
        // Remove client name
        _connectedClientNames.Remove(Context.ConnectionId);
        
        // Log disconnection
        var reason = exception != null ? $" (Error: {exception.Message})" : "";
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client disconnected: {Context.ConnectionId} (Total: {_connectedClientsCount}){reason}");
        
        await Clients.All.SendAsync("ConnectionStatus", false);
        await Clients.All.SendAsync("ClientDisconnected", Context.ConnectionId, _connectedClientsCount);
        await base.OnDisconnectedAsync(exception);
    }

    // Public methods for monitoring
    public int GetConnectedClientsCount()
    {
        return _connectedClientsCount;
    }

    public List<string> GetConnectedClientIds()
    {
        return _connectedClientIds.ToList();
    }

    public Dictionary<string, string> GetConnectedClientNames()
    {
        return new Dictionary<string, string>(_connectedClientNames);
    }

    public string GetClientName(string connectionId)
    {
        return _connectedClientNames.TryGetValue(connectionId, out var name) ? name : string.Empty;
    }

    public List<AssetLocation> GetAssetLocations()
    {
        return _assetLocations;
    }

    public List<MapMarker> GetMapMarkers()
    {
        return _mapMarkers;
    }
}

// Data Models
public class AssetLocation
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Status { get; set; } = "active";
    public string? Description { get; set; }
    public DateTime LastUpdated { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

public class MapMarker
{
    public string Id { get; set; } = string.Empty;
    public string AssetId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string MarkerType { get; set; } = "asset";
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public DateTime Timestamp { get; set; }
}