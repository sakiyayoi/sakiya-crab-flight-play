# NEON new-loop FIX ROUND — Codex source report

Date: 2026-11-24 JST. Branch: **new-loop-fix**, created from the exact `new-loop` tip **dad61373317489381ef3b71a3f99e244cfa5ebe6**.
Implementation commit: **911bc64f2df223219b74b40c7077ad8e59766e14**. Runtime policy revision: **new-loop-fix-1**.

Implemented the two requested corrections: **「壊れ方が地味・スカッとしない」** and **「回すのが地味なのもある」**. Resume resolution 5's physical-only wave rule is explicitly superseded by this FIX ROUND. Managed compilation, executed policy contracts and offline host Playwright checks pass. **This is a source submission, not a Unity-rendered feel/quality acceptance. No Unity Editor was run.**

The source patch is [fix/implementation.patch](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/implementation.patch), **91,988 bytes**, SHA256 **4462d4f818fd65179cf36176b960ee045acc237ee069d0b5d110254643fffc19**. It contains the implementation/test/task changes from dad6137; it is not a playable WebGL distribution. [fix/implementation-manifest.json](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/implementation-manifest.json) binds the 22 implementation/test/tool files to SHA256; the implementation commit also includes `tasks.md`.

## A. Destruction = 爆散

| Request | Implementation and concrete parameters |
|---|---|
| A1 — directly break through existing accounting | `QueueNewLoopWave` queues into `pendingBreaks`; `ApplyBreaks` alone disables intact objects, increments material counters / `DestructionEvent` / `BrokenObjects`, and produces `BreakEvents`. ON direct-wave targets join beam targets in the explicit threshold-bypass condition. **No authored threshold value is changed.** Reach stays **8 × (1 + energy) = 8–16m**. Ignition per-node capacity stays **ceil(max(1, baseline-neighbors-at-8m) × (1 + 2 × energy))**. Ordinary release stays at **ceil(3 × energy)** additional local targets, plus the first contact itself. Existing segment, same-chain and mass-suggestion ≤100 candidate filters remain. |
| A2 — travelling explosion | The runtime uses a deterministic breadth-first plan through the authored chain graph, distance then ordinal ID for ties. **0.040 real seconds per hop**, independent of slow-motion; hop 0 is the contacted object, hop 1 its eligible neighbors, then successive graph layers. Actual display/accounting occurs on the next available simulation/render frame. Boost/jackpot ignition plans the complete authored chain: S5 **yard-main 34**, **yard-sub 18**; S7 **warehouse 6**. Already-broken nodes remain valid graph bridges, while accounting skips their second break. Ordinary releases do not recursively enroll a chain or trigger unbounded support-wave propagation. Real debris contacts still run. |
| A3 — own-color shards, white smoke, lantern sparks | Every ON port destruction event takes the shared explosive `PortParticles` path: **40 marks = 24 own-family-color shards + 8 white round puffs + 8 lantern-orange sparks**. Shard radial component **12–21.6m/s**, size **0.22m**, lifetime **0.55s**; puff radial component **2.2m/s**, size **0.65m**, lifetime **0.65s**, alpha **0.72**; spark radial component **18m/s** for the spark slots, size **0.09m**, lifetime **0.32s**. All add **3m/s upward**. Sparks reuse `PortVisuals.Lantern` (**#FFB347**, visual tokens v2); shards use the actual object's `PortLayout.FamilyColor`. The existing **512-mark** cap is retained, retiring oldest marks to reserve all three layers at each burst. Physical fragments keep their existing models, colliders and masses; after the old scatter they receive a finite `ForceMode.Impulse` targeting **20–28m/s** outward/upward velocity, above the previous **18m/s** cap. The previous velocity is subtracted from that impulse so it does not stack indefinitely. Existing debris contact and secondary-break threshold rules remain. |
| A4 — release impact | New **64-point full radial ring** expands from the actual first-contact point, **0.3m → energy radius (8–16m)** over **0.45s**, quadratic ease-out; width starts **0.18m** and tapers to zero. Simulation slow/hitstop becomes **0.25× for 0.30 real seconds** (previously 0.22s). Side-camera size punches to **0.72×**, returns over **0.30s** (previously 0.78× / 0.28s). Existing **0.35m** decaying release shake is retained. Shake-off suppresses camera shake and punch; reduced-motion/effects-off suppress the added motion. Existing release flash still uses the shared flash limiter. |
| A5 — phase cap and dancing count | Hard destruction cap is **4.0 real seconds after release**, previously 3.0. Natural rest / segment exit still end immediately; no-contact timeout remains **5.2s**. Host `粉砕` readout advances **one accepted count every ≥40ms**, with **150ms** pop, peak scale **1.35×** and **−4°** tilt. Repeated snapshots do not add counts. Result immediately shows the authoritative finalized count; reduced-motion/effects-off use the exact count without animation. No extra score or synthetic break is created by the UI. |
| A6 — same beam explosion | Beam retains its existing touch/sweep/hold/release rules. Its accepted breaks produce the same destruction event and use the **same physical shard impulse and three-layer port presentation**. No parallel beam score path. |
| A7 — protected authoring and OFF | All new behavior is reached only through `useNewLoop` / `NewLoopActive`, except an inert cleanup call on destruction. No model, placement, authored mass, collision-shape or authored threshold edits. Source/asset protections and the OFF argument are below. |

Cause labels: **`wave-direct`** for the initial release contact and first-hop direct wave, **`chain`** for later graph hops and secondary debris/solver breaks, **`beam`** for beam touches (beam wins if both queues address one target). Later ordinary direct crab contacts retain **`direct-contact`**. Each accepted object still counts exactly once. The old ON `wave-contact` / `debris-contact` wording is consolidated into the requested `chain` label. OFF cause behavior is unchanged.

The new debris impulse is deliberately added explosion energy in ON mode. The old scatter-energy receipt describes the old scatter before this added impulse; it must not be presented as a conservation proof for the new blast. No historical physical acceptance result was reclassified as passing this change.

Full-chain contracts certify the actual authored graph plan, **not a measured native destruction count**. The retained immediate rest/exit rules can still terminate a throw before pending work; no early-end rule was silently relaxed.

## B. Charge = a show

Let **I(E) = (exp(3E) − 1) / (exp(3) − 1)** for clamped energy 0–1. Input, stock, fill time and launch physics are unchanged.

| Request | Implementation and concrete parameters |
|---|---|
| B1 — steep spin and suction | Separate render-only neon crab spins at **2 + 30I(E) rotations/s**: **2.00 at 0%**, **7.47 at 50%**, **32.00 at 100%**. Its angle comes from the integrated visual rotations. The added spin/scale never writes the collision root or model data. Suction uses a fixed **64-quad pool**, emission **14 + 82I(E) particles/s** (**14 → 96/s**), each traveling **3.6–5.2m → 0.12m** inward while spiralling **7 + 5E radians** over **0.55s**. Diameter starts **0.18 + 0.14E m**, shrinking toward the crab. No simulation RNG is consumed. |
| B2 — charge audio | Reuses only existing **charge_low_loop / charge_mid_loop / charge_high_loop**, **charge.work** variants and **charge_ready_clack**. Active-tier gain weight is **0.16 + 0.84I(E)**; the preceding tier remains at **16%** of that weight. Loop playback rate is **0.8 + 0.7I(E)**, smoothed with existing **0.0267s** time constant. Tier 2 immediately plays the existing work cue at gain **0.85**, pitch **0.9 → 1.35** in **0.16s**. Tier 3 immediately plays the ready cue at gain **1.0**, pitch **1.1 → 1.65** in **0.16s**. Existing audio enable, pause, volume, voice limits and asset licensing stay in place. No sound file added or regenerated. |
| B3 — micro shake | While held: tier 1 amplitude **0.012m**, tier 2 **0.055m**, tier 3 **0.12m**, Y component **0.65×** X, using **83 / 101 rad/s** sine components. Gated by existing shake setting, reduced motion and effects preset. |
| B4 — threshold / MAX punctuation | Each upward tier transition emits immediately. Its ring brightens toward white, radius pops up to **1.25×**, width up to **4×**, returns over **0.18s**; crab render scale peaks at **1.22×** over **0.18s**. Tier flashes share the existing rolling flash gate; no independent unlimited flash channel. At full: **0.38s gold aura**, size **4.5 → 6.4m**, and **0.38s scene dim** with sine envelope peaking at **15%**. A command buffer tints the scene then redraws only visible crab renderers with their existing materials, leaving the crab undimmed without changing layers, masses or colliders. Host **MAX** pops **0.65 → 1.45 → 1.0× over 260ms**, remains for **500ms**, keyed by a one-shot cue so the next-frame overspin state cannot erase the entrance. |
| B5 — 回転数 | Host adds **回転数** and an integer count from Unity's integrated visual rotations, counting only while held; retries reset to zero. No host-invented stock/count. Telemetry adds `spinCount`, `visualSpinRps`, `maxCue`. At reduced/effects-off the existing source rotation is used and the counter follows that rotation while held. |
| B6 — overspin unchanged | `SpinCharge.cs` is byte-identical to dad6137. Fill **1.8s**; thresholds **50% / 100%**; further holding latches overspin, drains **0.5 energy/s**, disarms beam, preserves the **±6°**, approximately **1s** dizzy sway and release escape. No refill/re-arm until retry. The new MAX cue is presentation only. |

Comfort changes are also applied immediately through `SetFeelR27`, `SetReducedMotion` and pause handling, because ordinary frame ticking stops while paused. Transient aura/dim/motes/pop are cleared there; gameplay stock and counters are not reset by an accessibility setting.

## Preserved scope

- Segments: S5 **697–757m, 54 objects**, S7 **781–848m, 41 objects**; clear **≥70%** (**38/54**, **29/41**); progression/history, result contents and immediate retry remain.
- Fixed ON launch **22m/s**, **2rps**, canonical neutral pose/phase; no in-flight steering/kick. Charge is release energy only. Beam **2.5s**, single activation, auto sweep, hold/release, saved crab pose and return to side camera remain.
- Rest snapshot reset, actor-only rebuild and no reset settling simulation remain. New queues/visual counters clear with that reset. Reset-duration instrumentation remains; no native timing is invented.
- Existing scene and 25 FBX meta changes: **26/26 SHA256-identical to the inherited snapshot**, excluded from commits. **467 other tracked C#/shader/JSON/FBX inputs** match dad6137 Git blobs; see [fix/protected-inputs.json](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/protected-inputs.json). Existing authoring data, models, assets and previous-round evidence are preserved.

## Verification run

Run from `source/`:

```sh
python3 tools/new-loop/typecheck.py
python3 tools/new-loop/contracts.py
python3 tools/port-prototype/contracts.py
env NODE_PATH=/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules npx --offline --prefix .git/placement-fix-qa/runtime --no-install playwright test --config qa/new-loop/playwright.config.cjs
```

| Check | Result | What it proves / boundary |
|---|---|---|
| Unity-installed Roslyn runtime compilation | **125 sources, 0 errors, 20 warnings** | Compiles against installed Unity runtime assemblies. Warning count unchanged. No Editor, shader import or player run. [compile.json](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/compile.json), [typecheck.log](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/typecheck.log). |
| Actual C# NewLoopRules / SpinCharge contracts | **71 passed / 0 failed** | 4s cap, .04s hop ordering, actual S5/S7 authored ignition coverage at 0/.5/1 energy, ordinary radius/count bounds, spin/intensity, existing clear/beam/rest/exit/overspin rules. Includes **9600 OFF charge steps bit-identical** to actual legacy SpinCharge from 2d0d849. [contracts.log](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/contracts.log). |
| Existing PortRules C# contracts | **30 passed / 0 failed** | Existing role and camera formulas. [port-contracts.log](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/port-contracts.log). |
| Final offline Playwright suite | **14 passed / 0 failed / 0 skipped**, **15.8s** | Existing 10 plus charge HUD, per-break counter, protected authoring / gates, existing-asset gain/pitch/cue tests. Actual host adapter runs in Chrome; Unity is explicitly stubbed. Audio graph calls are executed against test doubles, not listened to. [playwright.json](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/playwright.json). |
| UI fixture images | **390×844 / 320×568 / 844×390** | Result layout checks retained; added charge HUD/counter/MAX screenshots. Portrait 390 and landscape 844 charge images visually inspected. Blank canvas and synthetic state: **not Unity/game screenshots**. |
| Protected files, source receipts, JS parse, final whitespace | **PASS** | Current compile hashes, unchanged asset/model blobs and inherited scene/meta hashes checked. New meta trailing whitespace was removed before commit; final staged check passed. |
| Native PhysX, real explosion chain timing/counts, shader import, crab/particle/dim render, beam visual, listening, iPhone/FPS, fixed-input replay, game-playtest | **NOT_RUN** | Explicit instruction: no Unity Editor runs. No native/WebGL build or playable deployment produced. |

Relevant UI-only images: [charge 390×844](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/ui-charge-fixture-390x844.png), [charge 844×390](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/ui-charge-fixture-844x390.png), [result 390×844](/Users/konomichiharu/Documents/Codex/2026-10-21/task-4/kani-neon-proto-20261118/source/docs/new-loop/fix/ui-fixture-390x844.png). The fixture's existing instruction wrapping is retained; no unrelated copy/layout redesign was made.

## OFF fixed-input identity argument

Set **`useNewLoop=false` before Initialize**. New code runs through ON controller calls or `NewLoopActive` checks:

1. Wave planning/clock/direct bypass/extra shard impulses are unreachable. `QueuePortWaves` selects the original `Time` clock and original OFF branch; the legacy threshold, support, roles, body mass, friction, shape, placement and RNG paths are unchanged.
2. The added charge rotation and pops write only the separate neon render tree on ON. OFF does not create suction, dim commands, aura or shockwave. Physical launch, source pose, original charge calculations and legacy reset remain unchanged. Cleanup is inert when these resources were never created.
3. `PortParticles(..., explosive=false)` executes the original particle path. New audio logic requires `s.newLoop`; OFF keeps the exact old charge blend. Host additions are hidden and do not intercept OFF inputs or write its destruction count. Shader resources are not attached to any OFF renderer.
4. **467 protected inputs** are unchanged, all **26 inherited dirty files** are unchanged and excluded, and actual OFF SpinCharge passes **9600 bitwise scalar comparisons**. Existing PortRules' **30** contracts also pass.

Therefore the guarded source changes preserve the OFF fixed-input computation. This remains an **identity argument, not a freshly executed PhysX replay**. Historical supplied reference: seed **42043**, accepted input → **3220.05712890625m / 118 breaks / 3738 ticks**. That number was **not rerun or remeasured** here. JSON telemetry gains ON presentation fields; no claim of identical full telemetry serialization is made.

## Native follow-through and submission boundary

Use a clean checkout of the implementation commit (or the later report-only tip), excluding the inherited 26 dirty files. Copy the complete current `games/flight/web-template/` when building; include the new `PortPrototype/NewLoopDim` resource shader. No new audio assets are needed.

Chloe's next native check should exercise weak/strong ordinary release vs boost/jackpot; inspect ordered `wave-direct` / `chain` / `beam` events and one count per object; watch object-color shards/white smoke/orange sparks; test beam release and early rest/exit; confirm 4s cap and retry reset. Inspect fast charge, inward spirals, ring/body tier pops, undimmed crab at MAX, and shake-off/reduced/effects-off including settings while paused. Listen to the actual existing loops/cues and replay OFF against the fixed-input reference. Those are remaining **unverified acceptance checks**, not observed native failures. Existing floor intrusion / collider approximation / scenery repetition issues are not marked fixed.

Likelihood + impact (1–3 each) for native follow-through: wave/physical debris and retained early-end interaction **2+3=5**; MAX shader/crab redraw and performance **2+2=4**; OFF numerical replay **1+3=4**. No arbitrary gameplay gate was added or lowered.

The report is mirrored byte-for-byte at proto root `NEON-REPORT-NEWLOOPFIX-by-codex.md`. Existing `tasks.md` and `../../HANDOFF.md` carry the local handoff. Studio's requested entry skills were not present in the current native catalog; TEAM/KB remained unread under this project's explicit KB prohibition. No delegation, external message, Linear mutation, push, upload/deployment, purchase or authentication change was performed.
