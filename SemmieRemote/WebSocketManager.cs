using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Fleck;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace SemmieRemote;

public static class WebSocketManager {
    private static readonly ILogger Logger = Logging.GetLogger("WS Server");

    public static void Start() {
        FleckLog.LogAction = (level, message, ex) => Logger.Log(level switch {
            Fleck.LogLevel.Debug => LogLevel.Debug,
            Fleck.LogLevel.Info => LogLevel.Information,
            Fleck.LogLevel.Warn => LogLevel.Warning,
            Fleck.LogLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        }, ex, "{}", message);

        Logger.LogInformation("Starting server...");

        var server = new WebSocketServer("wss://0.0.0.0:" + Program.Port);

        server.Certificate = X509CertificateLoader.LoadPkcs12FromFile("cert.pfx", File.ReadAllText("cert.pass"));
        server.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        server.ListenerSocket.NoDelay = true;
        server.RestartAfterListenError = true;

        server.Start(socket => {
            socket.OnOpen = () => {
                Logger.LogInformation("New connection from {Ip}:{Port}", socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort);
            };

            socket.OnClose = () => {
                Logger.LogInformation("Connection closed from {Ip}:{Port}", socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort);
            };

            socket.OnMessage = message => {
                var json = JObject.Parse(message);

                switch (json["type"]?.ToString()) {
                    case "key": {
                        HandleKey(json);
                        break;
                    }
                    case "rotation": {
                        OscManager.Send("Buddy_Cam/Move/Pitch", json["pitch"]?.Value<float>() ?? 0);
                        OscManager.Send("Buddy_Cam/Move/Yaw", json["yaw"]?.Value<float>() ?? 0);
                        break;
                    }
                    case "speed": {
                        OscManager.Send("Buddy_Cam/Speed", (float) Math.Clamp(json["speed"]?.Value<float>() ?? 0, 0.00001, 1));
                        break;
                    }
                    default: {
                        Logger.LogWarning("Ignoring unknown message type {}", json["type"]);
                        break;
                    }
                }
            };
        });
    }

    private static void HandleKey(JObject json) {
        var isDown = json["isDown"]?.Value<bool>() ?? false;

        switch (json["key"]?.ToString()) {
            case "KeyW": {
                OscManager.Send("Buddy_Cam/Move/Forward", isDown);
                break;
            }
            case "KeyA": {
                OscManager.Send("Buddy_Cam/Move/Left", isDown);
                break;
            }
            case "KeyS": {
                OscManager.Send("Buddy_Cam/Move/Back", isDown);
                break;
            }
            case "KeyD": {
                OscManager.Send("Buddy_Cam/Move/Right", isDown);
                break;
            }
            case "KeyE": {
                OscManager.Send("Buddy_Cam/Move/Up", isDown);
                break;
            }
            case "KeyQ": {
                OscManager.Send("Buddy_Cam/Move/Down", isDown);
                break;
            }
            case "KeyR": {
                OscManager.Send("Buddy_Cam/Move/Reset", isDown);
                break;
            }
        }
    }
}