namespace SemmieRemote;

internal static class Program {
    public const ushort Port = 6854;

    private static async Task Main(string[] args) {
        WebSocketManager.Start();
        await OscManager.Start();

        while (true) {
            await Task.Yield();
        }
    }
}