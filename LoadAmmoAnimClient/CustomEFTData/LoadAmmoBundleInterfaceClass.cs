using EFT.NextObservedPlayer;
using UnityEngine;

namespace Manimal.LoadAmmoAnim.CustomEFTData
{
    // stub IObservedUsableItem the dispatch patch hands back from
    // ObservedPlayerUsableItemController.GetObservedUsableItem for our item. vanilla
    // would hand back ObservedRangeFinder (we inherit PortableRangeFinder), which
    // expects rangefinder parts our bundle doesnt have.
    public class LoadAmmoBundleInterfaceClass : IObservedUsableItem
    {
        public void Initialize(GameObject gameObject) { }
        public void UpdateData(ObservedUsableItemUpdatedData observedUsableItemUpdatedData) { }
        public void Disable() { }
    }
}
