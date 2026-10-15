using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPTushonka.Reflection.Patching;

namespace Manimal.LoadAmmoAnim.Patches
{
    // routes our item through a plain UsableItemController when something calls
    // Player.SetInHandsUsableItem on it. vanilla would treat it as a rangefinder.
    // BundleControllerInitializePatch then binds the bundle's animator.
    internal sealed class SetInHandsBundlePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(Player).GetMethod(
                nameof(Player.SetInHandsUsableItem),
                BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        private static bool Prefix(Player __instance, Item item, Callback<IUsableItemController> callback)
        {
            if (LoadAmmoBundle.IsBundleItem(item))
            {
                __instance.Proceed<Player.UsableItemController>(item, callback, true);
                return false;
            }
            return true;
        }
    }
}
