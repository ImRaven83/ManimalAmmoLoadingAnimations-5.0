using System.Reflection;
using EFT;
using HarmonyLib;
using Manimal.LoadAmmoAnim.CustomEFTData;
using SPTushonka.Reflection.Patching;
using UnityEngine;

namespace Manimal.LoadAmmoAnim.Patches
{
    // binds the bundle's animator when the engine attaches a UsableItemController to the
    // spawned WeaponPrefab. replaces the InitializeController override the old
    // LoadAmmoBundleController subclass had, IL2CPP cant spawn that subclass.
    internal sealed class BundleControllerInitializePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(Player.UsableItemController), "InitializeController",
                new[] { typeof(Player), typeof(WeaponPrefab) });

        [PatchPostfix]
        private static void Postfix(Player.UsableItemController __instance, Player player, WeaponPrefab weaponPrefab)
        {
            if (weaponPrefab == null || !LoadAmmoBundle.IsBundleItem(__instance.Item)) return;

            var session = LoadAmmoAnimState.Get(player);
            if (session == null) return;

            session.BundleAnimator = weaponPrefab.GetComponentInChildren<Animator>();
            LoadAmmoAnimDriver.Trace($"bundle InitializeController: animator={(session.BundleAnimator != null ? session.BundleAnimator.name : "missing")}");
            LoadAmmoBundleAnimator.Initialize(session.BundleAnimator);
        }
    }
}
