using System.Reflection;
using EFT.InventoryLogic;
using EFT.NextObservedPlayer;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPT.Reflection.Patching;

namespace Manimal.LoadAmmoAnim.Patches
{
    // patches ObservedPlayerUsableItemController.GetObservedUsableItem, the dispatcher
    // that returns the IObservedUsableItem implementation for a given item type during
    // the controller swap chain. vanilla returns specific classes for radio transmitter
    // and rangefinder, otherwise null — and since we inherit PortableRangeFinder it would
    // hand us the rangefinder one. swap in a no-op stub instead.
    internal sealed class UsableBundleInterfaceDispatchPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(ObservedPlayerUsableItemController).GetMethod(
                nameof(ObservedPlayerUsableItemController.GetObservedUsableItem),
                BindingFlags.Public | BindingFlags.Static);

        [PatchPrefix]
        private static bool Prefix(ref IObservedUsableItem __result, Item item)
        {
            if (item is LoadAmmoBundleItem)
            {
                __result = new LoadAmmoBundleInterfaceClass();
                return false;
            }
            return true;
        }
    }
}
