using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Services.Modding.Custom;

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
    public Dictionary<string, SemanticVersioning.Range>?ModDependencies { get; init; }
    public string? Url { get; init; } = "";
    public bool HasPrepatcher { get; init; } = false;
    public string License { get; init; } = "MIT";
}

// registers the hidden animation item the client spawns in memory to drive the mag-loading
// animation. used to go through WTT-ServerCommonLib, which has no SPT 5.0 build, so this
// talks to the server's own CustomItemService instead.
//
// the item is a straight clone of PortableRangeFinder under the rangefinder's own parent. the
// IL2CPP client cant spawn a custom Item subclass, so the client identifies it by template id
// instead of a custom parent + class.
[Injectable(TypePriority = OnLoadOrder.Preload + 2)]
public class LoadAmmoAnimServer(
    ISptLogger<LoadAmmoAnimServer> logger,
    CustomItemService customItemService,
    TemplateTable templateTable) : IOnLoad
{
    // must match LoadAmmoAnimDriver.BundleItemTplId on the client.
    private static readonly MongoId BundleItemTplId = new("69d69c70ed183ba9c882f7f7");

    // vanilla PortableRangeFinder (Vortex Ranger 1500).
    private static readonly MongoId RangeFinderTplId = new("5b80242286f77429445e0b47");

    private const string HandbookParentId = "5b47574386f77428ca22b338";
    private const string BundlePath = "manimal/stanags_container.bundle";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (!templateTable.Items.TryGetValue(RangeFinderTplId, out var rangeFinder))
        {
            logger.Error($"[LoadAmmoAnim] rangefinder template {RangeFinderTplId} is missing, cant create the animation item.");
            return Task.CompletedTask;
        }

        var result = customItemService.CreateItemFromClone(new NewItemFromCloneDetails
        {
            ItemTplToClone = RangeFinderTplId,
            ParentId = rangeFinder.Parent,
            NewId = BundleItemTplId,
            NewItemName = "LoadAmmoBundleItem",
            HandbookParentId = HandbookParentId,
            HandbookPriceRoubles = 0,
            AddToFleaPriceDb = false,
            AddToWeaponShelf = false,
            Locales = new Dictionary<string, LocaleDetails>
            {
                ["en"] = new()
                {
                    Name = "[LoadAmmoAnim] Animation Bundle",
                    ShortName = "AnimBundle",
                    Description = "An internal placeholder item used by LoadAmmoAnim for magazine loading animations. Do not remove.",
                },
            },
        });

        if (!result.Success)
        {
            logger.Error($"[LoadAmmoAnim] failed to create the animation item: {string.Join(", ", result.Errors)}");
            return Task.CompletedTask;
        }

        // TemplateItemProperties has non-nullable members, so passing it as OverrideProperties
        // would reset everything we dont set. edit the clone directly instead.
        var properties = templateTable.Items[BundleItemTplId].Properties;
        if (properties is not null)
        {
            properties.Weight = 0;
            properties.ExaminedByDefault = true;
            properties.QuestItem = true;
            properties.CanSellOnRagfair = false;
            properties.CanRequireOnRagfair = false;
            properties.Unlootable = true;
            properties.InsuranceDisabled = true;
            properties.UsePrefab = new Prefab { Path = BundlePath, Rcid = "" };
        }

        return Task.CompletedTask;
    }
}
