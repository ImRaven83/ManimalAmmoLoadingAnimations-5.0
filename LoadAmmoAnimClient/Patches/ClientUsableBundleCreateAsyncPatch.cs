using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPTushonka.Reflection.Patching;

namespace Manimal.LoadAmmoAnim.Patches
{
    // patches ClientUsableItemController.CreateAsync, the static async factory the
    // engine uses when building a controller from an item id during a hand swap.
    // our item is a PortableRangeFinder clone; we route it through CreateControllerAsync
    // explicitly so the controller always gets our Item.
    internal sealed class ClientUsableBundleCreateAsyncPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(ClientUsableItemController).GetMethod(
                nameof(ClientUsableItemController.CreateAsync),
                BindingFlags.Public | BindingFlags.Static);

        [PatchPrefix]
        private static bool Prefix(
            ref Il2CppSystem.Threading.Tasks.Task<ClientUsableItemController> __result,
            ClientPlayer player,
            string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return true;

            var item = player.InventoryController.FindItem<PortableRangeFinder>(itemId);
            if (LoadAmmoBundle.IsBundleItem(item))
            {
                __result = Player.UsableItemController.CreateControllerAsync<ClientUsableItemController>(player, item);
                return false;
            }
            return true;
        }
    }
}
