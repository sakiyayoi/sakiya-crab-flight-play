# New loop interrupted-run audit (before further implementation)

Base `2d0d849120b0bc0002173f49b2db0fd7f6f05987`, branch `new-loop`. Original brief and 10 resume resolutions are authoritative. No earlier implementation commit or report exists. Byte snapshot: `../evidence/new-loop/interrupted/`; hash receipt: `interrupted-audit.json`.

| File | Decision | Evidence / required work |
|---|---|---|
| SpinCharge.cs | keep same stock and OFF branch; change ON policy | 1.2s, Energy clamp without decay and unbounded dizzy are obsolete; use 1.8s, tiers, latched per-throw penalty and hold escape. |
| CrabFlightGame.cs | keep gated integration points; finish and correct | calls missing NewLoopState/controller methods; release still feeds charge to launch spin/rotor; clock/camera/result/reset implementation absent. |
| PortRoles.cs | keep segment filtering; replace ON wave handling | pendingBreaks manufactured an impulse and directly broke static panels; implement actual Rigidbody impulse/contact thresholds and role-specific ignition. |
| FlightEventStream.cs | keep | nullable cause carried from WorldBreakEvent through ordinary destruction event; no duplicate score counter. |
| PrepareFramesR18.cs | keep ON retry shortcut; validate | avoid full world reconstruction in new-loop; OFF unchanged. |
| RestaurantPhysics.cs | keep first-breakable gate and shared accounting; correct wave/beam logic | first release only through crab QueueBreak; add direct beam bypass and real contact-only wave cause; filter secondary breaks to segment. |
| FlightField.unity | exclude entirely | SHA256 matches preexisting-worktree.json exactly; file ID rewrite/tuning component removal all predate interrupted job. No new-loop scene edits found. |
| tasks.md | keep task row, update scope/status | no completed validation or report yet. |
| NewLoopRules.cs (new) | keep 697–757m S5 segment; change timings/policies | all 54 S5 targets incl. complete 34-item yard-main and 18-item yard-sub; old 1s beam, parked delay to 4.8s and 5.2s global cap conflict. |
| NewLoopPhysics.cs (new) | keep release eligibility, actor-only rebuild intent, shared beam accounting; revise | ordinary charged contact seeds all chains; wave directly queues breaks; beam threshold-dependent; park clamps/teleports and simulates after stop; no rest snapshot. |
| docs/new-loop/preexisting-worktree.json | keep unchanged | 25 FBX metas plus scene, all 26 currently hash-identical. |

Missing before resume: controller and camera implementation, host input/HUD/result wiring, three-tier audio, segment-clear judgement, snapshot/timing instrumentation, new-loop tests, report, commits. Previous run could not typecheck as-is because its new-loop integration definitions were missing.

Charge baseline map from HEAD: Begin starts 10s timer; Add computes normalized swipe Work; Energy clamps to 0–1; ContinuousWork uses unbounded Work for sqrt spin; enhanced/body soft cap limits physical angular speed. Release passes spin into body angular velocity/inertia cap and ChargeRotor/FundLaunchSpinR16, and charge into ChargePose/neon/color/effects/audio/telemetry. CoreLaunchPower in core mode returns flick unchanged; non-core multiplies flick by .18+.82*energy. Thus charge does not directly increase core linear launch power, but indirectly affects flight through real angular momentum/rotor bursts, and affects non-core speed directly. No separate charge score exists.
