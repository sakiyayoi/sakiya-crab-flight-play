# NEON aquarium water fix — Codex

Date: 2026-11-23 JST. Branch: `port-prototype`.
Base: `5be0ca45214150e58981d0e947ec56c6a7708011`.
Implementation and verification commit: `6286e3236c4ab73efc08591bd045303ec69403db`.

The reported water-following bug is fixed in source with a render-only change. Managed C# compilation and two offline Playwright evidence checks passed. Unity/Native/WebGL playback and the fixed-input replay were **not run**; this report does not claim observed visual approval or a newly measured replay result.

## Confirmed cause

The failure is a repeatable call-chain error in the code, rather than an assumed phase spelling or a missing normal-flight Sample call:

1. `SpinCharge.Begin()` sets `Started=true`. `SpinCharge.Reset()` clears it; the ordinary reset is in `CrabFlightGame.PrepareWorkR18()`.
2. `CrabFlightGame.ReleaseLaunch()` and `Launch()` clear `aiming`. `Launch()` sets the actual phase to `"flight"`, but does **not** reset `spinCharge.Started`. The retained charge state is intentional existing gameplay state and was left alone.
3. At the end of each normal `CrabFlightGame.Tick()`, `TickNeon(dt)` calls `neonCrab.Sample(...)` and then `neonCrab.SampleSpinColor(..., aiming || spinCharge.Started)`.
4. Both methods call `SamplePort()`, which calls `PortHeroVisual.Sample()`. The first call supplies false and hides water in flight. The second supplies true after a charged launch because `Started` is still true. The old `tank = phase=="ready" || phase=="charge" || aiming` expression then re-enables water even though the phase is `flight`.
5. The old Sample also rewrote the card's position to `body.position + camera.forward * 4`, its rotation to the camera rotation, and its scale to the full-screen orthographic dimensions. The second call therefore both re-enabled the aquarium and moved it with the crab/camera every normal flight frame, in any district.

The parameter named `aiming` in `PortHeroVisual.Sample` actually receives combined aim/charge eligibility through that second path. The real `CrabFlightGame.aiming` flag is not stuck. Phase names match. The constructor uses the game owner passed by `AttachNeon()`, not the crab render root, so a moving-parent theory is unnecessary to explain this bug. These findings are from the current source and call sites, not a fresh playback of the shipped trial.

Relevant unchanged call sites: `Assets/Scripts/SpinCharge.cs` (`Begin`, `Reset`), `Assets/Scripts/CrabFlightGame.cs` (`ReleaseLaunch`, `Launch`, `PrepareWorkR18`, `Tick`), `Assets/NeonProto/NeonGameHook.cs` (`TickNeon`), and `Assets/NeonProto/NeonCrabVisual.cs` (`Sample`, `SampleSpinColor`, `SamplePort`). All Assets paths are relative to `games/flight/crab-flight`.

## Minimal structural fix

Only three runtime files changed:

- `PortHeroVisual.cs`: centralize the water and underwater-shadow visibility in `SyncTankVisibility(phase)`. The only enabled states are `PortLayout.Enabled && (phase == "ready" || phase == "charge")`. Initialize the cards hidden before sampling. Apply the same phase gate on every Sample, regardless of the aim/charge argument. Update their transforms only while the water is enabled.
- `NeonCrabVisual.cs`: add a one-line forwarding method for that tank visibility operation.
- `CrabFlightGame.cs`: add `neonCrab?.SyncTankVisibility(phase)` to its existing unconditional `LateUpdate`, after the existing synchronization stamp. This reads the authoritative current phase independently of `TickNeon`, event consumption, and both Sample calls. It also runs when Tick returns early for pause or preparation.

This is a separate per-frame rendering guard, not merely a changed eligibility flag. On the first rendered flight frame, LateUpdate hides both tank cards even if Sample has not run since the aim view. A subsequent Sample with `aiming=true` and `phase="flight"` cannot re-enable them. `result`, setup/unknown/null phases are also hidden. The guard is independent of district/position and PortLayout being switched off. Retry into ready can show the backdrop again.

The aim-view shader, geometry, placement formula, camera rotation, scale multiplier `2.1`, shadow formula, and crab eye/material eligibility are preserved. In flight the inactive card retains its last tank pose instead of being repositioned along the route. The static `PortAtmosphere` inlet strips are untouched.

These lifecycle conclusions are **code-inspection results**, not runtime assertions or screenshot comparisons.

## Verification actually performed

| Check | Result | Limit |
|---|---|---|
| Unity-installed .NET/Roslyn compiler, 121 current runtime C# files | exit 0, errors 0, warnings 18 | Managed compile; no Unity Editor import, shader compilation, or PlayMode |
| Playwright compiler/source receipt check | PASS | Confirms compiled bytes match the current runtime sources |
| Playwright protected-input comparison | PASS | All 364 other tracked C#/shader/JSON files match the base commit; controller matches base after removing the one render call; accepted input matches |
| Changed-file whitespace check | PASS | Pre-existing imported meta whitespace is outside this change |
| Existing dirty files | 26/26 byte-identical to snapshot | 25 FBX meta files and `FlightField.unity` preserved, excluded from commits |
| Native review / optional batchmode compile | NOT_RUN | Isolated project copy could not be completed; no Unity process was launched |
| Fixed-input replay / WebGL rebuild / actual screen inspection / game-playtest | NOT_RUN | No new distance measurement, screenshots, visual QA, or browser gameplay claim |

The 18 compiler warnings concern existing obsolete contact APIs, unreachable code and unused fields; see `waterfix/typecheck.log`. No C# error was ignored.

Commands from the source root:

```sh
(cd games/flight/crab-flight && "/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting/NetCoreRuntime/dotnet" "/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting/DotNetSdkRoslyn/csc.dll" @../../../docs/port-prototype/waterfix/Assembly-CSharp.rsp -nologo)

env NODE_PATH=/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules npx --offline --prefix .git/placement-fix-qa/runtime --no-install playwright test --config qa/port-prototype/waterfix.config.cjs
```

Final Playwright result: **2 passed / 0 failed / 0 skipped**, 33.5 seconds. It launches no game browser. The first run was 1 passed / 1 failed because Node's `execFileSync` buffer overflowed on a large authored JSON (`ENOBUFS`); the comparison was changed to stream Git blob hashes, and the final run passed. Both results are retained. The full legacy gameplay suite was not rerun for this bounded render-only change.

The compiler response file is retained in `waterfix/Assembly-CSharp.rsp`; it uses existing Unity references and writes products to `../evidence/waterfix/`. Its source SHA256 manifest and result are in `waterfix/compile.json`.

For optional native validation, an owned isolated copy first stalled reading `Library/Artifacts/ab/abe0fab1bf33270f71556ab48be0ce43`. A second attempt omitted Library, but stalled reading `Assets/ThirdPartyNotices/RuntimeInspector-MIT.txt`. The precise filesystem cause was not established. Both copy processes were stopped; the incomplete temporary copies were not used as evidence. No Unity license failure was observed or asserted. Chloe's `PortNativeReview` was not copied from or run in `port-build01`. The temporary unrun regression-harness draft was removed.

## Fixed-input identity argument

Required reference: **seed 42043, accepted-input.json, 3220.05712890625 m / 118 breaks / 3738 ticks**.

This replay was not rerun. Those numbers remain the supplied acceptance reference, not a result measured in this task. They are expected to remain unchanged because:

- Simulation integration, contact/collider generation, launch input, spin-charge lifecycle, role formulas, random seed use, placement, timestep and feel/camera parameters are unchanged. The protection test compares 364 tracked code/shader/JSON files with the base.
- The sole controller difference calls a render-object visibility method in LateUpdate. Removing that call yields the base controller byte-for-byte; no launch/Tick calculation or order changed.
- The other two runtime changes affect only render cards without colliders. They write no physics state and consume no random values. Camera transform and orthographicSize are only read.
- Both the proto-root and tracked `tools/blender/accepted-input.json` have SHA256 `c952b51c82b3ceb7838471da2d5f2bc93f4aa007f1e8cc560c046bf1036b4125`.

This establishes a render-only diff argument for replay identity, as allowed by the request; it is not a fresh numerical proof. No FPS or timing-performance claim is made.

## Housekeeping and saved state

Deleted **55 untracked space-2 duplicates**, including `PortHeroVisual 2.cs`, `PortAtmosphere 2.cs`, the other six PortPrototype ` 2.cs` copies, associated duplicate meta files, and duplicate docs/tools/QA/web files. Each remaining duplicate selected for deletion had a tracked unsuffixed counterpart. No tracked space-2 C#/meta copies were found or deleted. The eight actual PortPrototype scripts, including `PortAtmosphere.cs`, remain tracked and intact except for the intended `PortHeroVisual.cs` fix.

The complete deletion list is `waterfix/removed-untracked.json`. The 18 Assets duplicates were also preserved outside the working tree under `../evidence/waterfix/removed-untracked/` before deletion. The pre-existing 26 modified tracked files are listed with SHA256 in `waterfix/existing-dirty-before.json`; they were not reverted or included in this commit.

The existing `tasks.md` and `../../HANDOFF.md` were updated. Studio's named entry skills/game-playtest were absent from the current skill catalog; TEAM was not read because the project explicitly prohibits KB access. No subagents, external messages, Linear changes, push, deployment, purchases, or credential changes were made.

## Remaining verification

Source repair and its requested compile/inspection submission are complete. Actual aim-view appearance and first-flight-frame disappearance are **unverified in Unity/WebGL**. The next runtime check is ready/charge → ordinary charged release → first flight → port and non-port districts → result → retry, including a frame with Sample omitted. Rebuild the trial from this commit before judging the fix in a browser; existing WebGL output has not been changed.

Reported bug priority: likelihood 3 + impact 2 = 5. This fix does not claim resolution of historical floor intrusion, collider approximation, repeated scenery, or any other gameplay issue.

Report locations: `source/docs/port-prototype/NEON-REPORT-WATERFIX-by-codex.md` (tracked) and proto-root `NEON-REPORT-WATERFIX-by-codex.md` (identical copy outside the source Git root). The report is committed separately on the same branch.
