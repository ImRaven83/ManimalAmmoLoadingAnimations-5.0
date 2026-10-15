# SPT 5.0 port notes

SPT 5.0 (Single Player Tushonka) runs EFT 1.1 under **IL2CPP** with **BepInEx 6**, so this is a platform port, not a version bump. Mod version is now **3.0.0** (`Directory.Build.props`).

## Server (`LoadAmmoAnimServer`)

- Built against `SPTushonka.*` **5.0.0-pre.202610101655** (`SptPackageVersion` in `Directory.Build.props`). Bump it once 5.0 ships. It **compiles cleanly** against those packages.
- **WTT-ServerCommonLib dependency removed.** WTT has no SPT 5.0 build. The animation item is now created through the server's own `CustomItemService.CreateItemFromClone`, and its properties (weight, quest item, `UsePrefab` = `manimal/stanags_container.bundle`, ...) are set on the clone directly.
- The item is now a plain **PortableRangeFinder clone under the rangefinder node**, which the server mod finds by name (`PortableRangeFinder`) at startup. The 4.x jsonc cloned `5b80242286f77429445e0b47`, which is not a rangefinder. That only worked because the custom parent decided the client class; without the custom parent the client built a `Handguard`. The custom parent node (`8a3f72c1b9d5e60a4fe83b21`) and the `ServerModFiles/db` jsonc files are gone. The item template ID is unchanged (`69d69c70ed183ba9c882f7f7`).

## Client (`LoadAmmoAnimClient`)

- Targets `net10.0` against BepInEx 6, the interop assemblies in `BepInEx\interop` (launch the game once to generate them) and `SPTushonka.Reflection`. `BaseUnityPlugin`/`Awake` became `BasePlugin`/`Load`, and it depends on `com.sptushonka.core` for the main-thread dispatcher.
- **WTT-ClientCommonLib dependency removed, along with the custom subclasses.** Under IL2CPP the item factory and `Player.Proceed<T>` are native code and can't construct managed subclasses of EFT types, so:
  - `LoadAmmoBundleItem : PortableRangeFinder` is replaced by template-ID checks (`LoadAmmoBundle.IsBundleItem`).
  - `LoadAmmoBundleController : UsableItemController` is replaced by a vanilla `Player.UsableItemController`. The animator logic moved to `LoadAmmoBundleAnimator`, bound by the new `BundleControllerInitializePatch` (a postfix on `InitializeController`).
  - `LoadAmmoBundleInterfaceClass` is removed. `GetObservedUsableItem` now returns `null` for the bundle, as vanilla does for other usable items.
- IL2CPP idioms:
  - Sessions are keyed by native `Player.Pointer`, and magazine identity compares pointers.
  - Coroutines use BepInEx's managed `StartCoroutine` extension.
  - Delegates passed to the game go through `DelegateSupport.ConvertDelegate`. The prefab factory delegate wraps `ObjectsFactory.CreateItemUsablePrefab`.
  - Game `Task`s are bridged with `AsManaged()`, and `Il2CppSystem.Threading.Tasks.Task.Delay` replaces `__result` in the draw-delay patch.

## Fika compat (`LoadAmmoAnimClientFika`)

- Retargeted to `net10.0` and the interop/BepInEx 6 references. `FikaPlayer` checks use `TryCast`. The network API (`IFikaNetworkManager`, `RegisterPacket`, `SendData`, `INetSerializable`) matches Fika's `dev-5.0` branch, which is still early work. Rebuild against the released Fika 5.0.

## Verified against the 1.1 interop assemblies

The client now **compiles against the EFT 1.1 interop assemblies**, and every patch target was checked against the interop metadata (method exists, Harmony parameter names match).

- 1.1 is deobfuscated:
  - 4.x `smethod_1` is now `Player.ItemHandsController.CreateItemHandsController<T>`.
  - 4.x `Delegate8` is now `ItemObjectFactoryDelegate(Item, Player)`.
  - 4.x `LoadMagazineProcess.method_5` is the interop `Method_Private_Task_0`. The patch selects it by shape (the only parameterless method returning a plain `Task`) rather than by its generated name.
- `EasyAssetsExtensions.RetainSeparateTask` takes an IL2CPP `IEnumerable<string>`.

## Tested in-game

The full chain works on SPT 5.0 / EFT 1.2.0.0.47965: bundle item created as a `PortableRangeFinder`, controller swapped through `CreateItemHandsController<Player.UsableItemController>`, animator bound, mesh enabled, animation plays and restores the weapon. The asset bundle (Unity 2022.3.43f1) and its `dependencyKeys` load.

An **Animation Speed** setting (F12, 0.5-1.5) scales playback relative to the per-round load time. The per-round time itself comes from `BaseLoadTime` in globals, reduced by Mag Drills and the magazine.

## Still to do

- Fika compat could not be built (no `Fika.Core.dll`).
