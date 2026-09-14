using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;

namespace LoadAmmoAnimMod;

public record ModMetadata : IModMetadata
{
    public string ModGuid { get; init; } = BuildInfo.ModGuid;
    public string Name { get; init; } = "LoadAmmoAnim";
    public string Author { get; init; } = "Manimal";
    public List<string>?Contributors { get; init; }
    public SemanticVersioning.Version Version { get; init; } = new(BuildInfo.Version);
    public SemanticVersioning.Range SptVersion { get; init; } = new(BuildInfo.SptVersion);
    public List<string>?Incompatibilities { get; init; }
    public Dictionary<string, SemanticVersioning.Range>?ModDependencies { get; init; } = new()
    {
        ["com.wtt.commonlib"] = new("~3.0.6")
    };
    public string? Url { get; init; } = "";
    public bool HasPrepatcher { get; init; } = false;
    public string License { get; init; } = "MIT";
}

[Injectable(TypePriority = OnLoadOrder.Preload + 2)]
public class LoadAmmoAnimServer(
    WTTServerCommonLib.WTTServerCommonLib wttCommon) : IOnLoad
{
    public async Task OnLoadAsync(CancellationToken cancellationToken)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        await wttCommon.CustomItemParentService.CreateCustomParents(assembly);
        cancellationToken.ThrowIfCancellationRequested();
        await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
    }
}
