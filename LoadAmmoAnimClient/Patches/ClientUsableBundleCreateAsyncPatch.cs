using System.Reflection;
using System.Threading.Tasks;
using EFT;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPT.Reflection.Patching;

namespace Manimal.LoadAmmoAnim.Patches
{
    // patches ClientUsableItemController.CreateAsync, the static async factory the
    // engine uses when building a controller from an item id during a hand swap.
    // vanilla only looks up PortableRangeFinder; we route our type through
    // CreateControllerAsync explicitly so the controller always gets our Item.
    internal sealed class ClientUsableBundleCreateAsyncPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(ClientUsableItemController).GetMethod(
                nameof(ClientUsableItemController.CreateAsync),
                BindingFlags.Public | BindingFlags.Static);

        [PatchPrefix]
        private static bool Prefix(
            ref Task<ClientUsableItemController> __result,
            ClientPlayer player,
            string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return true;

            var item = player.InventoryController.FindItem<LoadAmmoBundleItem>(itemId);
            if (item != null)
            {
                __result = Player.UsableItemController.CreateControllerAsync<ClientUsableItemController>(player, item);
                return false;
            }
            return true;
        }
    }
}
