# NEON new-loop prototype — Codex resume report

Date: 2026-11-24 JST. Branch: `new-loop`. Base: `port-prototype` **2d0d849120b0bc0002173f49b2db0fd7f6f05987** (includes waterfix 6286e32).
Implementation / evidence commit: **a6eb4a57d7a9ecb973b6a4edc790b8dbc365b31c**.

The interrupted source implementation has been completed against `../NEWLOOP-PROMPT.txt` and the ten resume resolutions. Managed compilation and offline policy/host-adapter checks pass. **Unity Editor, PhysX gameplay, native screenshots, WebGL build, listening, actual reset duration, median cycle time and numerical OFF replay were NOT_RUN**, as instructed. This is a source submission for Chloe's native verification, not a claim that the playable game has passed native review or been adopted/published.

## 1. Audit before additional code

Read the original brief first, then applicable AGENTS, the requirement sheet, existing HANDOFF's new-loop/waterfix entries and the working changes. HEAD was still 2d0d849; no implementation commit/report existed. Saved the interrupted bytes and full patch outside Git in `../evidence/new-loop/interrupted/`. `interrupted-audit.json` and `AUDIT.md` were written before implementation resumed.

| Interrupted file | Kept / changed / discarded, and why |
|---|---|
| `SpinCharge.cs` | Kept the same Work/Energy stock and OFF path. Replaced 1.2s fill, clamped-energy-without-decay, and Work-overflow dizziness with 1.8s fill, tiers, irreversible per-throw overspin latch, decay and bounded sway. |
| `CrabFlightGame.cs` | Kept gated integration points. They referred to missing controller/state methods and could not form a complete build. Added those definitions; changed charge-fed physical spin/power, stop handling, canonical launch pose, reset and real-time timing. |
| `PortRoles.cs` | Kept segment filters and existing role/chain definitions. Discarded the ON path that manufactured `pendingBreaks` impulses; ON waves now apply actual forces and wait for solver contacts. Added post-release guards. |
| `FlightEventStream.cs` | Kept optional cause field and forwarding into the existing destruction event stream. |
| `PrepareFramesR18.cs` | Kept ON bypass of full-world asynchronous rebuild, with snapshot/actor reset now implemented. OFF retains the original path. |
| `RestaurantPhysics.cs` | Kept the first eligible crab-contact hook and common accounting. Changed beam threshold handling and wave cause attribution; added dynamic-panel stop at its real break. |
| `FlightField.unity` | **Excluded entirely.** It exactly matches the SHA256 recorded before the interrupted job. Its object-ID rewrites and removal of the tuning component predate this job. There are **zero identified new-loop changes in this scene** to separate/stage. |
| `tasks.md` | Kept task entry; updated acceptance/status with the resumed scope and actual verification. Original first through-play row preserved. |
| New `NewLoopRules.cs` | Kept S5's 697–757m window. Replaced 1s beam and parked-until-4.8s/global-5.2s-after-release policy. Added clear threshold, tiers, second segment and bounded ordinary wave policy. |
| New `NewLoopPhysics.cs` | Kept eligible-contact release, same break accounting, capsule/box sweep intent and fresh-actor retry intent. Replaced indiscriminate charged-contact chain seeding, synthetic wave breaks, threshold-dependent beam, segment-edge teleport and continued simulation after crab stop. Added rest snapshot and timing. |
| `docs/new-loop/preexisting-worktree.json` | Kept byte-for-byte. All **26/26** protected files match: 25 panel FBX metas plus the scene. None are staged or committed. |

The interrupted run had no `NewLoopGame.cs`, host inputs/UI, 70% judgement, snapshot restoration, tests or final report. These are completed here. Additional bounded integration files: `RigidCrabFlight.cs`, `FeelDirectorR27.cs`, `DestructionProgressionR12.cs`, host HTML/JS/CSS and three new Unity script metas. No placement, model, collider-shape, authored threshold, material-mass, shader or other district-content edits.

## 2. SpinCharge current-behavior map — **before** this work

This map is from base 2d0d849, not the interrupted interpretation:

- `Begin()` sets Started. `Tick()` advances a ten-second elapsed timer, capped at 0.1s per call; Ready means Started and elapsed ≥10. Waiting advances readiness but does not add work.
- `Add(distance, seconds)` validates active/not-ready/finite input; clamps seconds to 1/240–0.25. Ordinary speed caps at 6; enhanced speed uses a logarithmic tail above 6. Added work is `travel * .09 * (.35 + .65 * clamp01(speed/5))`.
- Work is unbounded. Energy is clamped 0–1; ContinuousWork computes it from Work. Requested spin is `PowerScale * (.4 + 21.6 * sqrt(Work))` in continuous mode, otherwise from `Energy^.7`. Body spin uses the existing limit/soft knee. Current overdrive PowerScale is 28/22.
- **Linear power:** `CoreLaunchPower` returns the flick power unchanged in core mode. In non-core mode it multiplies flick by `.18 + .82 * Energy`. Thus “charge directly raises core launch power” would be inaccurate.
- **Indirect travel:** `ReleaseLaunch` passes charge-derived spin into physical angular velocity, the launch inertia cap, `ChargeRotor` and enhanced `FundLaunchSpinR16`. Rotor energy can later accelerate the crab on real breaks/automatic ground bounces. Charge therefore still affects physical travel in core mode through these paths.
- **Presentation/state:** Energy feeds `ChargePose`, neon crouch/color-related presentation, launch effects, launch receipts and telemetry. Requested/body spin feeds the visible spin, vortex/core-color gate and audio. Host sound blends low/mid/high loops from Energy; the old ready cue uses timer readiness.
- **Score:** no independent charge score. Existing material counters, BrokenObjects, DestructionEvent and BreakEvents count actual accepted breaks.

**ON rerouting now:** Energy is retained for the release layer (reach/capacity/full-charge beam) and its display. The accepted aim angle remains the player's choice, 10–45°, with overspin sway applied at release. Normalize power so the initial linear speed is **22m/s**, regardless of charge, saved power or upgrade. Set physical spin **2rps**, phase **0**, and neutral `ChargePose(0)` before freezing the collision compound. Use a canonical safe launch height computed from the neutral rest pose, independent of charge. Clear legacy rotor/funding stock in `BeginNewLoop`. The original flight flags, masses, colliders and thresholds are unchanged; additional environmental resource energy is confined to the current destruction phase. A charged throw cannot gain extra range from the charge; crossing the selected segment edge ends it immediately.

## 3. Per-beat behavior

### 回す

Same `SpinCharge`, no parallel energy system. Hold the aim area; move vertically to choose angle; release to throw. Keyboard: focus the area, Up/Down adjusts angle, hold Enter to charge, release Enter to launch. Zero-energy/short-hold throws are legal. Flight ignores kick input at both controller and physics layers.

Fill target **1.8 seconds** of held time. Tiers: **0–<50% cyan**, **50–<100% pink**, **100% gold**. Ring, gauge, percentage and existing low/mid/high sound loops reflect these tiers. Full charge arms the beam and uses the ready cue; tier/armed transitions report immediately, rather than waiting for the 25Hz state report. Actual frame quantization still applies.

Continuing to hold after full charge latches OverSpun, drains **50 percentage points/s**, disarms the beam, and adds a **±6°**, approximately **1s** aim sway (`sin(age*19)`). Releasing stops the sway. The penalty stays latched and the stock cannot refill/re-arm during that throw, including after cancel/re-hold; only retry clears it. Gameplay sway persists as a rule under reduced motion, while optional camera movement is disabled. Audio assets/licenses are reused; actual listening was not performed.

### 飛ぶ / segment scope

`useNewLoop=true` defaults this branch to the trial. Set it **false before Initialize** for the original full-flight mode; hot switching an initialized physics world is not supported.

| Segment | Bounds | Authored breakables | Chain integrity | Single-throw clear |
|---|---|---:|---|---:|
| S5 container yard (default) | **697–757m, 60m** | **54** | All **34 yard-main**, all **18 yard-sub**, hazard + landmark retained | **38/54**, ≥70% |
| S7 warehouse (unlocks after S5 clear) | **781–848m, 67m** | **41** | All **6 warehouse** members included | **29/41**, ≥70% |

No object was moved to fit these windows. Membership uses authored beat IDs; bounds contain every member, and offline checks reject any chain split. Other districts remain present and unchanged. Ordinary flight stays orthographic side view; there is no flight steering/push.

### 最初の接触 / 解放

`QueueBreak` identifies contact by the crab with a registered, unbroken, selected-segment breakable. That **first contact**, even below the break threshold, releases once. Ground, sea/floor, debris and out-of-segment contacts cannot release. They do not change charge/armed state; the energy ring stays until eligible release or result.

Release emits a distinct event, routes its flash/ring through the existing feel flash gate, slows simulation to **0.25× for 0.22s of real time**, punches side-camera size to **0.78× for 0.28s**, and uses a decaying **0.35m** shake. Prior port maxima were **0.88× for 0.25s** and **0.18m over 0.10–0.16s**. Shake-off/reduced-motion/effects-off gates are respected; flash uses the existing shared rate limiter. Legacy hitstop does not stretch the new-loop deadline. No rendered flash-safety pass is claimed.

### Wave / destruction

Energy changes reach **8→16m (×1→×2)** and max targets per scheduled wave **max(1, baseline)→3×max(1, baseline)**, rounded up. At zero energy this preserves baseline reach/candidate policy; the old synthetic direct-break timing is deliberately replaced by physical contact under resolution 5. The baseline is the count of eligible, unbroken same-chain neighbors within 8m; the old implementation had no separate fixed cap. Eligibility retains authored mass suggestion ≤100. Support-child releases remain finite authored support effects, now physical too.

**ON waves never enqueue breaks directly.** They activate a Rigidbody on the existing panel root, use that panel's unchanged authored mass suggestion and existing compound boxes, and apply `mass * 2 * (horizontal-away-unit + up)` as a physical impulse. They do not change mass data, collider geometry or any break threshold. Actual contact solver impulses subsequently enter `ApplyBreaks`, using its existing normalized threshold. Separate debris-scene proxies follow the moved root. Normal debris contacts retain the existing secondary threshold policy.

Aim matters through the authored role/chain graph: boost/jackpot ignition enrolls and propagates its own chain. An ordinary contact receives only **ceil(3×energy)** immediate nearby same-chain impulses, **zero at zero energy**, with no chain enrollment. Its incidental support/physical contacts can still break objects naturally. Offline topology confirms all 34 main-yard members are connected at the existing 8m baseline. **That is candidate reachability, not 34 measured breaks. The required large destruction-count difference between ignition and ordinary hits remains a native acceptance measurement.**

### Full-charge beam

At 100%, a flight button/Space hold can be armed before contact or started during the destruction phase. First breakable contact still releases automatically regardless of beam input. Hold fires an auto-aimed sweep for at most **2.5s**; release ends it, with no second activation that throw. Starting late may shorten it at the hard destruction cap. The player does not aim in flight.

The crab's position/rotation/velocity/angular velocity are saved, its body held kinematic during the beam, and its own renderers hidden to prevent self-occlusion. Targets ahead are ordered once; the beam interpolates between their positions, and the camera looks from the crab's beam origin toward that same target. Hit testing uses a finite beam against each actual oriented/scaled registered box expanded by 0.7m per axis, with sub-samples across skipped render frames. This is a conservative thick-beam box test (including corner allowance), not a triangle-perfect mesh raycast.

Touched targets use the **existing `pendingBreaks` / `ApplyBreaks` accounting**, bypassing only the break threshold for this explicit direct-destruction ability. No independent score increment exists. Material counters, BrokenObjects, DestructionEvent, shards and WorldBreakEvent occur once through the common path; `cause="beam"` reaches FlightGameEvent. Physical wave breaks use `cause="wave-contact"`; normal/secondary breaks retain distinct labels. On release/end, restore the crab's saved pose and velocity and return to orthographic side view; **no segment-edge teleport**. Beam-induced kinematic pose holding is not treated as natural rest. Real rest/exit before activation ends immediately.

### 結果 / progression / retry

The destruction phase ends no later than **3 real seconds after release**, independent of slow-motion; no further simulation/counting runs after finalization. Natural rest or segment exit ends immediately, including before release. A throw that never reaches a breakable times out at **5.2s** to avoid indefinite flight.

Each throw starts with restored objects; no cumulative damage. Result displays **粉砕数**, segment rate/count, ≥70% clear judgement, previous-throw count and difference for the same segment, and the single primary **もう一投** button together. Clearing S5 unlocks S7; S7 has a return-to-S5 control. Prototype unlock/history are session-local. New-loop results do not write old distance records or award/spend the legacy persistent upgrade currency.

Retry keeps the constructed world, restores the authored rest snapshot (all port transforms/rotations/scales, intact visuals/contacts, zero body velocities, cleared waves/debris/counters), and rebuilds only the crab because its frozen compound cannot safely be reused. **No reset settling simulation is executed.** Charge, overspin and resource stock reset to zero.

**Reset measurement: NOT_RUN in Unity.** Instrumented fields in `State().newLoop`: `restoreMs` (snapshot/reset work), `resetMs` (complete Prepare including actor rebuild), `resultToReadyMs` (wall time including the user's wait before retry). Browser separately records result-to-ready in `data-new-loop-result-to-ready-ms`. The retry button has no timed lockout and was immediately usable in the adapter tests. This does **not** establish native reset <3s; Chloe must record `resetMs` and result→ready after an immediate click. No synthetic timing is reported as actual reset performance.

## 4. Treatment of every existing port role

| Existing role | New-loop treatment |
|---|---|
| blank / ordinary | Eligible first release contact; bounded local impulse without chain enrollment. Ordinary physical breaks still count. |
| `boost30`, `boost45`, `boost60` | Ignition for the authored chain. Existing +5 XY-speed / angle role can apply on a real direct break **only after release**, within this throw. No player flight input. |
| `jackpot` | Main-set ignition; existing +9 / 45° direct-break role within destruction phase. Full 34-item S5 main set retained. |
| `hazard` | Existing ordinary ×0.35 slowdown, or airborne boost reversal (+20) after prior boost without ground contact. Only relevant inside the current destruction phase; reset clears state. |
| `redirect30` | Existing angle-only redirect, preserving XY speed; only post-release direct breaks. No next-throw carry-over. |
| `resource` | Existing r12 accumulator grants **9000J environmental stock** only after release. ON initializes a finite 9000J pulse allowance so this stock can actually feed later direct breaks after starting rotor stock was cleared. Entire stock clears on retry. No charge refill or persistent accumulation. |
| `landmark` | Breakable/visible authored target, no additional velocity effect. New-loop framing uses the release moment; legacy distant-landmark HUD is suppressed. |
| `preview` | Kept in layout, no physical modifier. S8 is outside the selected prototype segments; it does not participate in their release/count/clear judgement. |

These classifications use actual `PortLayout`, `PortRules`, `PortRoles` and `DestructionProgressionR12` source, not newly invented roles.

## 5. Which resume resolutions changed already-written code?

| Resolution | Impact on interrupted code |
|---|---|
| 1 — breakable-only automatic release | Existing eligibility hook was conforming and retained; added ended-phase guard. Ground/outside were never promoted to triggers. |
| 2 — no flight input / aim matters | Replaced all-neighbor charged-contact seeding with role/chain-specific ignition versus capped ordinary impulses; added kick rejection and hold-only beam UI. |
| 3 — charge→release energy only | Replaced old release's charge-derived spin and power path with canonical fixed-speed/spin/pose launch and cleared rotor stock. |
| 4 — per-throw ≥70% clear | Missing in interrupted implementation. Added result judgement, session unlock/history, reset counts and host presentation. |
| 5 — physical-only waves / whole chain boundaries | Replaced synthetic pending-break waves with Rigidbody impulses/real contacts. S5's old window was already correct and kept. |
| 6 — 3s cap / immediate rest-exit | Replaced parked waiting until 4.8s, world stepping after rest and edge clamping. 5.2s now applies only to a no-release throw. |
| 7 — tiers / 1.5–2s / decay / bounded wobble | Replaced 1.2s and overflow-based non-decaying energy logic. |
| 8 — hold beam 2–3s / direct destruction | Replaced 1s threshold-dependent beam, added 2.5s hold/release, snapshot pose hold and explicit beam threshold bypass. Retained common accounting intent. |
| 9 — role lifetime | Added post-release/ended-state guards and meaningful throw-local resource pulse. |
| 10 — rest snapshot / timed retry / one result screen | Kept actor-only rebuild intent; replaced the legacy settling Reset path for ON and added snapshot/stopwatch/UI. Actual reset performance remains NOT_RUN. |

## 6. Verification and timing argument

Commands run from `source/`:

```sh
python3 tools/new-loop/typecheck.py
python3 tools/new-loop/contracts.py
env NODE_PATH=/Users/konomichiharu/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules npx --offline --prefix .git/placement-fix-qa/runtime --no-install playwright test --config qa/new-loop/playwright.config.cjs
```

| Evidence | Actual result | Scope/limit |
|---|---|---|
| Roslyn using Unity-installed runtime references | **124 sources; errors 0; warnings 20** | Runtime managed compile only. 18 inherited warnings + 2 existing-style obsolete contact-ID API warnings in the new contact callback. No Editor/import/shader run. |
| Executed C# policy/SpinCharge contracts | **40 passed / 0 failed** | Uses actual current policy/charge source with a scalar Mathf shim, no simulated Unity physics. Includes 9600 OFF charge steps compared bit-for-bit against base SpinCharge, across eight configurations. |
| Offline Playwright, final pass | **10 passed / 0 failed / 0 skipped**, 10.2s | Compiler/source receipts, protected files, chain integrity, wave/beam wiring, JS parse/template integration, actual adapter inputs and result layout. Unity bridge is explicitly stubbed in UI tests. |
| Protected inputs | **464** other tracked C#/shader/JSON/FBX inputs match base Git blobs | Manifest `protected-inputs.json`; excludes deliberately modified runtime files. |
| Inherited dirty tree | **26/26 SHA256-identical** | Scene/meta changes excluded from commits. |
| Result adapter screenshots | 390×844, 844×390, 320×568 saved; portrait/landscape images visually inspected | Files prefixed `ui-fixture-`; synthetic 38/54 state, blank game canvas. They are **not game screenshots** or native evidence. |
| Changed-file whitespace and JS parsing | PASS | Relevant paths only; inherited importer-meta whitespace excluded. |
| Unity / fixed-input / actual reset / actual chain contrast / actual camera & sound / FPS / game-playtest | **NOT_RUN** | Latest instruction assigns native review/build to Chloe; no Unity Editor launched. |

The earlier intermediate Playwright pass was 9/9; after final source review, resource lifetime/camera renderer handling and template verification were added, then the final compile and 10-test pass were run. No failing gameplay test has been hidden or converted into a pass.

**Cycle-time estimate, not measurement:** speed 22m/s gives roughly 0.1–2.7s horizontal travel across S5's possible first contacts before collisions/aim effects; the main ignition point is ~34m from launch, about 1.6–2.2s for modest angles. Adding at most 3s destruction puts a main-set contact around **4.6–5.2s**. Earlier rest/exit shortens it, and a miss ends at 5.2s. This is why the target is plausible, **not proof of a ~5s median**; immediate-exit behavior deliberately takes precedence over artificially padding to five seconds.

## 7. OFF replay identity argument

Reference (not rerun): seed 42043 / accepted-input: **3220.05712890625m / 118 breaks / 3738 ticks**.

Set `game.useNewLoop=false` before its first Initialize in the native fixture or comparison build. New-loop world activation, force application, pose/speed normalization, new time/deadline logic, resource adjustment, camera, retry shortcut and break filtering are all behind that switch / the unactivated `NewLoopActive` flag. Legacy `Step`, launch power/spin, resets, role formulas, thresholds, authored geometry, random input, timing branch and waterfix remain in place. The actual OFF SpinCharge matches the original source for all 9600 tested scalar steps. `PortRules` and layout bytes are unchanged. Cause/newLoop telemetry fields extend the schema; an exact JSON-byte replay transcript is not claimed.

This is a guarded-diff and charge-contract identity argument, not a fresh PhysX numerical proof. Chloe must rerun the fixed-input reference in the OFF configuration. Other historical floor/collider/scenery issues are not marked fixed by this task.

## 8. Native handoff / remaining acceptance

Use an isolated copy of the implementation commit; do not include the 26 inherited scene/meta changes. Trial mode is ON by initializer, so no scene edit is required here. Build with the existing project pipeline and copy the current complete `games/flight/web-template/`, including `new-loop.js` and `new-loop.css`, plus existing sound/feel assets. No WebGL distribution was generated in this run.

1. Exercise three ordinary input cycles and compare 0/50/100% at the same angle: initial speed/spin/pose must agree; only release energy/beam differs. Ground-first, miss, debris and outside contacts must not consume/release charge. Record the exact first breakable ID and release timing.
2. Compare ordinary S5 hits with boost/jackpot ignition at equal sub-full energy. Record all cause-labelled events and final counts; require a large count difference, not just the offline 34-node graph. Confirm impulses cause threshold-based breaks, not direct wave kills.
3. Hold full beam for 2.5s and release early; verify auto-aim, touched objects, counter/event equality, saved crab pose on camera return, and no extra input effect. Test overspin, cancel/re-hold, pause, reduced motion and effects/shake settings.
4. Confirm result no later than 3s after release; natural rest/exit immediately finalizes; no later count change. Check 37/54 vs 38/54 and 28/41 vs 29/41 boundaries, S7 unlock, same-segment previous comparison and no throw-to-throw destruction.
5. Record `restoreMs`, `resetMs`, `resultToReadyMs` for immediate retries; target complete reset/next ready **<3000ms**. Inspect restored poses for zero settling noise. Capture actual game portrait/landscape/action/result screenshots and measured median cycle time.
6. OFF fixed-input replay must match the supplied numerical reference. Reuse the existing legal-input physics diagnostics if needed; do not lower old acceptance thresholds or relabel old evidence.

Verification priorities (likelihood/impact, 1–3): actual chain/beam/physics behavior **2+3=5**, reset performance **2+2=4**, OFF replay **1+3=4**. These are **unverified acceptance items**, not observed native failures. Native image, sound, iPhone/FPS and user feel/adoption remain separate.

The local report is mirrored byte-for-byte at proto root `NEON-REPORT-NEWLOOP-by-codex.md`. Existing `tasks.md` and the single `../../HANDOFF.md` carry state. Studio's named entry skills were absent from the current native catalog; no alternate cached skill was presented as that entry. TEAM/KB was not read because this project explicitly forbids KB access. No delegation, external messages, Linear mutation, push, deployment, purchase or credential changes were performed.
