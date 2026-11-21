# Integration comparison candidate

Based on tested flight charge2 source with surface-r1, city-density-r1, sound-r1 and VFX1.0.1. Native36 runs/27 configurations and Mac Chrome touch emulation were checked; this is not owner adoption or a completed full game.

The retained main commit corresponds to deployed private v4 flight and telegraph battle. No deployment automation is included. This branch has not been pushed.

Known issues: replay physics variance; impact point/normal/velocity fields are zero in this integration revision (corrected in a separate hitstop candidate); mall/country/space absent. Real audio listening and physical iPhone are unverified.

Local runtime package SHA256: 7e71832d1d1b400b736be0ee697565ae5426f5872e6b1f267ba84dc821c71a15. Source package SHA256: 0f79dd7e393ee324b9cb7139f10ff716c8d2c464dffe3cee1d3b5ce1ff29b750.

## Hitstop comparison follow-up

The branch now includes capped local impact stop (55ms maximum, 180ms cooldown, 120ms per 1s budget; reduced motion halves the hold) and actual impact point/normal/velocity assignment. Native9 off/on/reduced runs and Mac Chrome touch runs were checked. The underlying solver/replay variance remains unresolved. No global timeScale or rigid-body force change.

Runtime ZIP SHA256: 5b7a99bbbe401ca96a10505390efaf997eefbe6f4ca83bee16f10602a149cc21. Clean source ZIP SHA256: 746ccf2940f5b26580c9604c1dd46242b62456a611d78da2dc14d4b87368c44a. Local metadata-only FBX path sanitization preserves numeric geometry/animation and .meta GUIDs. No hosted deployment or GitHub push occurred.

Temporary Editor Scene comparisons now clear the actor review material cache before each Scene. This fixes stale-material pink proof frames; runtime code and WebGL ZIP are unchanged. Failed proof images are retained locally.
