using System.Reflection;
using EFT;
using EFT.NextObservedPlayer;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPT.Reflection.Patching;

namespace Manimal.LoadAmmoAnim.Patches
{
    // patches ObservedPlayerHandsController.GetWeaponAnimationType, the function that
    // decides which PlayerAnimator.EWeaponAnimationType the held item uses. force
    // Pistol for our item — one-handed object held in front of the camera is the
    // closest fit for a magazine.
    internal sealed class HandsControllerAnimationTypeBundlePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(ObservedPlayerHandsController).GetMethod(
                nameof(ObservedPlayerHandsController.GetWeaponAnimationType),
                BindingFlags.Public | BindingFlags.Instance);

        [PatchPrefix]
        private static bool Prefix(
            ref PlayerAnimator.EWeaponAnimationType __result,
            ObservedPlayerHandsController __instance)
        {
            if (__instance.ItemInHands is LoadAmmoBundleItem)
            {
                __result = PlayerAnimator.EWeaponAnimationType.Pistol;
                return false;
            }
            return true;
        }
    }
}
