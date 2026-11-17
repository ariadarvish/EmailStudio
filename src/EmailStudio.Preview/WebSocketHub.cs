using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EmailStudio.Preview;

/// <summary>
/// Manages connected WebSocket clients and broadcasts hot-reload events.
/// </summary>
public sealed class WebSocketHub
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();

    public void AddClient(Guid id, WebSocket socket)
    {
        _sockets[id] = socket;
    }

    public void RemoveClient(Guid id)
    {
        _sockets.TryRemove(id, out _);
    }

    public int ActiveConnections => _sockets.Count;

    /// <summary>
    /// Broadcasts a reload event to all connected preview clients.
    /// </summary>
    /// <param name="templateName">Name of the changed template, or null for global reload.</param>
    public async Task BroadcastReloadAsync(string? templateName = null)
    {
        var payload = JsonSerializer.Serialize(new
        {
            type = "reload",
            template = templateName,
            timestamp = DateTime.UtcNow
        });

        byte[] bytes = Encoding.UTF8.GetBytes(payload);
        var buffer = new ArraySegment<byte>(bytes);

        foreach (var kvp in _sockets)
        {
            var socket = kvp.Value;
            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    // Socket error, remove client
                    RemoveClient(kvp.Key);
                }
            }
            else
            {
                RemoveClient(kvp.Key);
            }
        }
    }
}

