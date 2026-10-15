using System.Reflection;
using EFT.InventoryLogic;
using EFT.NextObservedPlayer;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPTushonka.Reflection.Patching;

namespace Manimal.LoadAmmoAnim.Patches
{
    // patches ObservedPlayerUsableItemController.GetObservedUsableItem, the dispatcher
    // that returns the IObservedUsableItem implementation for a given item type during
    // the controller swap chain. vanilla returns specific classes for radio transmitter
    // and rangefinder, otherwise null — and since our item is a PortableRangeFinder clone
    // it would hand us the rangefinder one, which expects rangefinder parts our bundle
    // doesnt have. return null like vanilla does for every other usable item. (4.x
    // returned a managed no-op stub, IL2CPP would need it injected as an interface impl.)
    internal sealed class UsableBundleInterfaceDispatchPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(ObservedPlayerUsableItemController).GetMethod(
                nameof(ObservedPlayerUsableItemController.GetObservedUsableItem),
                BindingFlags.Public | BindingFlags.Static);

        [PatchPrefix]
        private static bool Prefix(ref IObservedUsableItem __result, Item item)
        {
            if (LoadAmmoBundle.IsBundleItem(item))
            {
                __result = null;
                return false;
            }
            return true;
        }
    }
}
