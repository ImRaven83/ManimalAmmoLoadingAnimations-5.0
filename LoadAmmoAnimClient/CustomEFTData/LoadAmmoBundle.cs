using System;
using EFT;
using EFT.InventoryLogic;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;

namespace Manimal.LoadAmmoAnim.CustomEFTData
{
    // identifies the virtual mag-bundle item and the hands controller holding it.
    //
    // on SPT 4.x this was a LoadAmmoBundleItem : PortableRangeFinder class registered through
    // WTTClientCommonLib's [CustomParent], plus a LoadAmmoBundleController : UsableItemController
    // subclass. under IL2CPP the item factory and Player.Proceed<T> are native code and cant
    // construct managed subclasses, so the item is now a plain PortableRangeFinder clone
    // recognised by template id, and the controller is a vanilla UsableItemController.
    internal static class LoadAmmoBundle
    {
        // matches BundleItemTplId in the server mod.
        public const string BundleItemTplId = "69d69c70ed183ba9c882f7f7";

        public static bool IsBundleItem(Item item)
        {
            return item != null && string.Equals(item.TemplateId.ToString(), BundleItemTplId, StringComparison.OrdinalIgnoreCase);
        }

        // the UsableItemController currently holding our bundle item, or null. takes the base
        // wrapper type so it works for whatever hands controller type the caller has.
        public static Player.UsableItemController AsBundleController(Il2CppObjectBase handsController)
        {
            var controller = handsController?.TryCast<Player.UsableItemController>();
            return controller != null && IsBundleItem(controller.Item) ? controller : null;
        }
    }

    // drives the animator inside the spawned bundle prefab. used to live on
    // LoadAmmoBundleController, now bound by BundleControllerInitializePatch.
    internal static class LoadAmmoBundleAnimator
    {
        // layer 1 is the firearms-anim convention used everywhere FirearmsAnimator's
        // Play call sites live. our bundle borrows the IFAK animator graph.
        private const int HandsLayer = 1;

        // state names in the bundle's animator graph.
        private const string PutAwayStateName = "USE TO OUT S";
        private const string DrawStateName    = "OUT TO USE S";

        // pool-reuse reset. between sessions the bundle gameobject is just
        // deactivated, not destroyed, so the animator wakes up in whatever
        // state it was in last session. force-jump to the draw state and
        // SetActive(true) so the engine's use-loop machinery takes over.
        public static void Initialize(Animator animator)
        {
            if (animator == null) return;
            try
            {
                animator.Play(DrawStateName, HandsLayer, 0f);
                animator.Update(0f);
                animator.SetBool("Active", true);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError(
                    $"[LoadAmmoAnim] LoadAmmoBundleAnimator.Initialize threw: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // play the put-away clip. SetActive(false) handles the engine side; the
        // explicit Play covers the case where the animator graph has no transition
        // out of the put-away state once entered.
        public static void PlayPutAway(Animator animator)
        {
            if (animator == null) return;
            try
            {
                animator.SetBool("Active", false);
                animator.Play(PutAwayStateName, HandsLayer, 0f);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError(
                    $"[LoadAmmoAnim] PlayPutAway: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // play the draw clip. used by the chained-mag transition after the mesh
        // has been swapped off-screen.
        public static void PlayDraw(Animator animator)
        {
            if (animator == null) return;
            try
            {
                animator.Play(DrawStateName, HandsLayer, 0f);
                animator.SetBool("Active", true);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError(
                    $"[LoadAmmoAnim] PlayDraw: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // true once the put-away clip has played past 95% normalized time. AnimLoop
        // polls this so it knows when the mag is off-screen and its safe to swap.
        public static bool IsPutAwayNearlyDone(Animator animator)
        {
            if (animator == null) return true;
            try
            {
                var info = animator.GetCurrentAnimatorStateInfo(HandsLayer);
                return info.IsName(PutAwayStateName) && info.normalizedTime >= 0.95f;
            }
            catch { return true; }
        }
    }
}
