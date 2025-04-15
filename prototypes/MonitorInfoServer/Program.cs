using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => {
    return Results.Redirect("/monitors");
});

app.MapGet("/monitors", () =>
{
    var physicalMonitors = Helpers.GetPhysicalMonitors();
    var virtualMonitors = Helpers.GetVirtualMonitors();

    var unmatchedVirtualMonitors = virtualMonitors.ToList();

    // Map virtual monitors to physical by comparing EDID or model name
    foreach (var phys in physicalMonitors)
    {
        var matchedVirtualMonitors = virtualMonitors
            .Where(vm => vm.EDID != null && vm.EDID == phys.EDID)
            .ToList();
        
        phys.VirtualMonitors = matchedVirtualMonitors;

        foreach(var virtualMonitor in matchedVirtualMonitors)
            unmatchedVirtualMonitors.Remove(virtualMonitor);
    }

    var unmatchedPhyiscalMonitors =
        physicalMonitors
        .Where(monitor => monitor.VirtualMonitors.Count == 0)
        .ToList();

    if (unmatchedPhyiscalMonitors.Count > 0) {
        Console.WriteLine("Unmatched physical monitors:");

        foreach(var virtualMonitor in unmatchedPhyiscalMonitors)
            Console.WriteLine($"    {JsonSerializer.Serialize(virtualMonitor)}");        
    }

    if (unmatchedVirtualMonitors.Count > 0) {
        Console.WriteLine("Unmatched virtual monitors:");

        foreach(var virtualMonitor in unmatchedVirtualMonitors)
            Console.WriteLine($"    {JsonSerializer.Serialize(virtualMonitor)}");
    }

    return Results.Json(physicalMonitors);
});

app.MapGet("/windows", () =>
{
    var windows = Helpers.GetWindows();

    return Results.Json(windows);
});

app.Run();