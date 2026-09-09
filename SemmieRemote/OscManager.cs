using System.Net;
using Microsoft.Extensions.Logging;
using Rug.Osc;
using VRC.OSCQuery;

namespace SemmieRemote;

public static class OscManager {
    private static readonly ILogger Logger = Logging.GetLogger("OSC Query");

    private static OSCQueryService oscQuery = null!;
    private static OscSender? oscSender;

    public static async Task Start() {
        Logger.LogInformation("Starting OSCQuery...");

        var oscQueryLogger = Logging.GetLogger<OSCQueryService>();
        oscQuery = new OSCQueryServiceBuilder()
            .WithServiceName("SemmieRemote")
            .WithLogger(oscQueryLogger)
            .WithDiscovery(new MeaModDiscovery(oscQueryLogger))
            .StartHttpServer()
            .AdvertiseOSCQuery()
            .Build();

        while (!await ConnectToVRChat()) {
            await Task.Delay(5000);
        }
    }

    public static void Send(string parameter, params object[] args) {
        oscSender?.Send(new OscMessage("/avatar/parameters/" + parameter, args));
    }

    private static async Task<bool> ConnectToVRChat() {
        Logger.LogInformation("Looking for VRChat's OSCQuery service...");

        oscQuery.RefreshServices();

        await Task.Delay(500); // Wait a bit for services to respond

        var profile = oscQuery.GetOSCQueryServices().FirstOrDefault(p => p.name.StartsWith("VRChat-Client"));

        if (profile == null) {
            Logger.LogError("Couldn't find VRChat's OSCQuery service");
            return false;
        }

        Logger.LogInformation("Found {Name} of type {ServiceType} on {IP}:{Port}", profile.name, profile.serviceType, profile.address, profile.port);

        var hostInfo = Extensions.GetHostInfo(profile.address, profile.port).Result;

        Logger.LogInformation("Connecting to {Name} on {IP}:{Port}...", hostInfo.name, hostInfo.oscIP, hostInfo.oscPort);

        oscSender = new OscSender(IPAddress.Parse(hostInfo.oscIP), 0, hostInfo.oscPort);
        oscSender.Connect();

        if (oscSender.State != OscSocketState.Connected) {
            Logger.LogError("Failed to connect to VRChat");
            return false;
        }

        Logger.LogInformation("Connected");

        return true;
    }
}