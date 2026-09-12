# CLAUDE.md

Guidance for Claude Code when working in this repository.

## What this is

**Terra Invicta Augmenter** (`TI_Augmenter`) — a Unity Mod Manager (UMM) mod for the game *Terra Invicta* (Pavonis Interactive). It is a single C# class library that uses **Harmony** to patch game methods in `Assembly-CSharp.dll` and adds a handful of gameplay features (resource depletion, mission follow-up modifiers, randomized councilor stats, nuclear damage multipliers, etc.). See [README.md](README.md) for the player-facing feature list.

- Published on Nexus: https://www.nexusmods.com/terrainvicta/mods/14 (version in `ModInfo.json` / `AssemblyInfo.cs`, currently 0.3.5)
- Also on Steam Workshop (`WorkshopItemInfo.xml`, id 2878051407)
- Author handle: ModMaster9000

## Git workflow

- **Commit and push directly to `master`.** Do not create branches or PRs for this project — it is a game mod and the owner does not want branch overhead.
- Keep `TI_Augmenter/TI_Augmenter.dll`, `.pdb` and `TI_Augmenter.zip` checked in and in sync with the source when a release-worthy change is made (they are the distributable).

## Build

- Target: .NET Framework 4.8, C# 10 (`LangVersion` 10 in the old-style `.csproj`). **New `.cs` files must be added to `<Compile Include=...>` in `TI_Augmenter.csproj`** — there is no glob.
- References are pinned to `import/*.dll` (game assemblies copied from `TerraInvicta_Data/Managed/`, plus `0Harmony.dll`, `UnityModManager.dll`). When the game updates, refresh `import/Assembly-CSharp.dll` and rebuild (see commit "update using latest game version main dll").
- No `dotnet` SDK on this machine; build with Rider's MSBuild (or any .NET Framework MSBuild):

```bash
"/c/Program Files/JetBrains/JetBrains Rider 2022.2/tools/MSBuild/Current/Bin/MSBuild.exe" TI_Augmenter.csproj -p:Configuration=Debug -v:minimal -nologo
```

- Output: `bin/Debug/TI_Augmenter.dll` (gitignored). Copy the dll + pdb into `TI_Augmenter/` and re-zip that folder as `TI_Augmenter.zip` for release.
- There are no automated tests; verification is done in-game with `enable_debug_mode=true` in `TI_Augmenter/TI_Augmenter_Config.txt` (logs go to the console / `Player.log` prefixed `TI_Augmenter:`).

## Layout

```
Main.cs                       UMM entry point (Main.Load) — reads config, runs ExtendedInstall if needed,
                              applies every Harmony patch behind its config flag; resource-pool save/load helpers
Config.cs                     key=value config loader (TI_Augmenter_Config.txt + Extended_Install_Configurations.txt)
augmentations/
  EventManager.cs             Pushes the "hab resources depleted" notification into the game's notification queue
  harmonypatches/
    LoadGameOrStartGame_Patch.cs   Lifecycle hooks: game start/load flags, save/load of resource-pool sidecar file
    councilorstate/           RandomizeStats (stat range multipliers), HireRecruitCost (attribute-based influence cost)
    factionstate/             AddToCurrentResource (depletion bookkeeping), MissionControlContributionFromHabs (x4 MC)
    habsitestate/             RandomizeSiteMiningData — generates per-site finite resource totals (ResourceSiteTotalInfo)
    habitatsscreencontroller/ Postfix on HabitatsScreenController.PreviewBase — shows "EMPTY" for depleted resources
    missionrelated/           Follow-up success/failure modifiers, Protect-mission ops slider, DefenseMissionCostModifier
    nationstate/              Permanently remove control points on "Abandon Nation" (auto-renew) for the human player
    regionstate/              ApplyDamageToRegion — full reimplementation of nuclear damage with config multipliers
    resourcecost/, factionperiodicupdate/, notifications/   Unused / stubs for the planned "debt" feature
TI_Augmenter/                 The shipped mod folder (what goes into the zip / UMM install)
  ModInfo.json                UMM manifest (Id, Version, EntryMethod = TI_Augmenter.Main.Load)
  TI_Augmenter_Config.txt     Player-facing config, key=value, one per line, no spaces
  Extended_Install_Configurations.txt   JSON written by the extended installer (completed flag + game dll size)
  EXTENDED_INSTALL.bat + ExtendedInstaller/   Mono.Cecil-based tool that rewrites the game's Assembly-CSharp.dll
  TIGlobalConfig.json / TINotificationTemplate.json   Game data-file overrides (recruit pool 12, notification template)
  Localization/<lang>/UI_Main.<lang>   Strings for keys `UI.TI_Augmenter.*` (9 languages)
import/                       Reference assemblies (game + Harmony + UMM); not shipped
mod_*_example/                Reference JSON snippets of game data formats; not used by the build
how_to_update/Capture.JPG     Screenshot of Discord advice on updating a Steam Workshop upload
```

## Key conventions & gotchas

- **Patches are applied manually in `Main.Load`** via `harmony.Patch(original, prefix, postfix)` — no `[HarmonyPatch]` attributes. Each feature is gated by a boolean config key (`*_enabled`). Add new features the same way: config key with a default in `Config.LoadValues()`, a line in `TI_Augmenter_Config.txt`, and a guarded `harmony.Patch` block in `Main.Load`.
- **Read config directly at the point of use** (`Config.GetValueAsFloat/Int/Bool(key)`). The getters unbox already-typed values (no parsing, no allocation), so there is no need to cache them in statics — the old `setConfigVariables()` pattern was removed because several patches forgot to call it and silently ran with default values.
- `Config` types values at load time: integer-looking → `int`, has `.` → `float`, `true/false` → `bool`, else string. `enable_debug_mode` is special-cased into `Config.isDebugModeActive`. For optional keys use `Config.GetValueAsFloat(key, defaultValue)` or `Config.isKeySet`; a missing required key throws `KeyNotFoundException`.
- **Extended installer:** on first run (or when `TerraInvicta_Data/Managed/Assembly-CSharp.dll` size differs from `extended_installation_main_dll_file_size`), `Main.Load` launches `EXTENDED_INSTALL.bat`, which runs `ExtendedInstaller.exe` against the game dll for methods `GetAllModifiers,StartMissionPhase,RandomizeStats,RandomizeSiteMiningData` and fields `targetID,councilorID`, then **quits and relaunches the game**. This exposes otherwise inaccessible game members the patches rely on. The installer's source is not in this repo — only the compiled exe. Don't add patches that depend on private members without either using `AccessTools` reflection or adding the member to the bat's argument list.
- **Resource depletion state lives outside the save file:** `TIHabSiteStateRandomizeSiteMiningDataPatch.HabSiteToTotalResources` is serialized to `<savefile>_resource_pool_data.txt` next to the save (`Main.SaveResourcePoolData` / `LoadResourcePoolData`, hooked on `GameStateManager.SaveAllGameStates/LoadAllGameStates`). Keys are `parentBody.displayName + habSite.displayName + FactionResource`. If the sidecar is missing, totals are regenerated on `GameControl.Initialize`.
- Follow-up modifier state (`FollowUpSuccessModifier` / `FollowUpFailureModifier` static dictionaries keyed `councilorID_missionName_targetID`) is **in-memory only** and resets on game restart; counters also reset after 56 in-game days.
- `ApplyDamageToRegionPatch.Prefix` copies the game's whole `ApplyDamageToRegion` body and returns `false`. When the game updates, this method is the most likely to drift from vanilla — re-diff it against the decompiled game code.
- Game types come from `PavonisInteractive.TerraInvicta` namespace in `Assembly-CSharp.dll`; use a decompiler (dnSpy/ILSpy) on `import/Assembly-CSharp.dll` to inspect signatures before patching.
- Some files carry stale namespaces (`TI_General_Adjustments_Alterations...`, `augmentations.core.missionrelated`) from before the rename to TI_Augmenter; harmless but don't propagate them.
- Localization keys added by the mod must be added to **all nine** `UI_Main.*` files under `TI_Augmenter/Localization/`.

## Releasing

1. Bump `Version` in `TI_Augmenter/ModInfo.json` and `AssemblyVersion`/`AssemblyFileVersion` in `Properties/AssemblyInfo.cs`.
2. Build, copy `bin/Debug/TI_Augmenter.dll` and `.pdb` into `TI_Augmenter/`.
3. Zip the `TI_Augmenter/` folder to `TI_Augmenter.zip` (folder must be the zip root, as UMM expects).
4. Commit to `master`, push, upload the zip to Nexus / update the Workshop item (see `how_to_update/`).
