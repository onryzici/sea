using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.PackageManager;

public static class InstallNetworkPackages
{
    public static async Task<object> Run()
    {
        // 2.7.0 failed on this Editor's EntityId API; UPM selected compatible 2.13.3.
        // Transport 2.6 request resolves to the Editor's built-in Transport 6.5.0.
        var req = Client.AddAndRemove(new[] { "com.unity.netcode.gameobjects@2.13.3", "com.unity.transport@2.6.0" });
        var deadline = DateTime.UtcNow.AddSeconds(45);
        while (!req.IsCompleted && DateTime.UtcNow < deadline) await Task.Delay(100);
        if (!req.IsCompleted || req.Status != StatusCode.Success)
            throw new Exception(req.Error?.message ?? "UPM request not completed; inspect resolution before retry");
        return req.Result.Where(p => p.name.Contains("netcode") || p.name.Contains("transport"))
            .Select(p => new { p.name, p.version, p.resolvedPath }).ToArray();
    }
}
