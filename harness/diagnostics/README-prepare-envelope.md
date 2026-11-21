# Private Prepare envelope diagnosis — unbuilt

This is a local instrumentation candidate, not a performance repair or adopted game version. Original R18 source7065e442 / engine571e1e1a and the public build are unchanged. It derives from the preserved 11/4 diagnostic source c8e6f80; b51 and Hull input sharing are not included. No Unity, build, browser, new throw, foreground operation or process query was run in this stage.

## New information and reused information

- Reuse PrepareCostProbeR15 active/yield separation, PrepareUnitTimelineR18 Advance +315 City2 units and existing supporting definition/creation counters, and AttachPhaseSplitR18. No second unit/physics timer or new generation step is added.
- InputReceiptR23: synchronous Receive entry, JSON parse, Initialize, ACK object construction, receipt completion/audit JSON. The request ID is attached after parsing, including spans preceding parse completion via a stable receive-call serial.
- BeginPrepareFrameR18 / State / Report: parent-child scopes distinguish pre-preparing State from report State, report serialization, and synchronous FlightReport call. No state/JSON field or report callback order is changed.
- Only the first actual MoveNext of each Prepare gets a direct new interval. Later MoveNexts use existing unit/Advance timers. The old3ms outer budget, iterator Current, generation order and forced yields remain.
- QueueDrain / QueueEncode and Configure identify queued processing. An externally queued request causes two ReceiveInput calls with the same input ID; these are two receive-call serials, NOT two user commands. Bridge reentry restores the outer command context.

Every span is elapsed Stopwatch wall time inside a synchronous non-yield call, not CPU cycles or utilization. Parents include children: do not sum all rows. Span.frame/endFrame mismatch, invalid nesting, open intervals or dropped rows invalidate a complete accounting. Scheduler waits between synchronous calls are not charged to span duration. They remain in the full window/rAF record; gaps are not automatically CPU idle, GC or GPU work.

The enabled recorder is thread-affine. Buffers (2048 spans,128 receive calls,32 nesting) are allocated/reset by Set before the window; no per-span JSON, console or growing list. Export copies/JSON happen after Freeze. Probe clock reads, native frame reads, fixed-buffer bookkeeping and the2 anchor bridge calls still perturb execution. Actual overhead/packing/clock alignment are NOT measured; do not claim zero overhead.

## Boundary synchronization / future use only

Controls are loopback-only through existing LocalPrepareProbeR15. Do not enable or read in the middle of a C# call or rearm an open capture. Future execution requires a separately permitted build and measurement slot.

1. Use the extended `prepare-envelope-observer.js` INSTEAD of the old timeline setup, once. It reuses the same continuous raw-rAF sampler and previous-rAF edge. It retains existing kind60/61 markers and additionally keeps kind70/71 and existing report kind2/3. Sync capacity4096 with explicit overflow. This file is not loaded into the game or host automatically.
2. Before the ordinary Retry, preallocate/arm existing unit/active recorders as needed, then `SetPrepareEnvelopeR18("1")`. Keep original normal Prepare input, physical input/seed/body and raw-rAF measurement windows. RetryLaunch is a different path, not a replacement for the canonical "もう一投".
3. kind70/71 bracket the first C# Receive and Freeze via the unchanged FlightSyncStampR8: C# before/after ticks, JS performance.now, frame and epoch. Reuse the existing source-derived clock inverse model and validate actual new binary/frequency/anchors before using this join. No new tolerance, guessed offset or different Mono formula. kind2/3's fourth argument is simulation time, NOT the Stopwatch clock. Their JS timestamps can be matched to containing mapped ReportBridge intervals; if matching is ambiguous, report it as unresolved.
4. After normal ready ACK and queued inputs settle, defer read until the synchronous call unwinds. `ReadPrepareEnvelopeR18` freezes before serialization/reporting; retain raw rAF and distinguish the post-window diagnostic export. If necessary also read existing unit/Advance/Attach outputs. Do not stop the acceptance window early or call this a strict10 result. The envelope is for the original fixed-input ordinary result→Prepare diagnosis, not an extra warmup or a time0-only substitute for the historical failure.

The new C# build target label is kani-r18-prepare-envelope-20261111. Existing generated Scene/SourceIdentity/ProjectSettings in this copy are inherited from c8e6f80 and NOT a new binary identity; only a future permitted Unity build will regenerate them. Never publish or claim that this source already has a new executable.

## Checks completed and not completed

- Standalone mcs/mono recorder tests11 PASS, fake clock/stub Unity: nesting/parent containment, waits excluded from synchronous spans, full integer IDs, bridge reentry, same-ID queued calls, first-MoveNext count, capacity/stack overflow, freeze/incomplete/snapshot immutability, invalid frame/order and unparsed input visibility. The earlier10-test log is preserved.
- Node VM observer tests3 PASS: added markers+old markers retained, explicit buffer overflow, stopped capture does not collect exporter markers. No browser ran.
- Recorder WebGL conditional branch compiles with mcs/stubs; no IL2CPP build/link/extern execution. This is NOT a full game compile.
- Static hook-removal restores all3 modified control files byte-exact to c8e6f80. Physics/generation/timers/JS bridges/assets unchanged; editor builder changes version strings only.
- NOT_RUN: full Unity/game compile, gameplay/shape/physics/drawing equality, real rAF/clock join, overhead calibration, original383.2 reproduction, formal10, M14, physical iPhone and listening. Prior Native equality for older diagnostics is historical evidence only, not a new candidate PASS.

Old383.2ms and old616.6ms remain unresolved. Source code for the new measurement is implemented; no causal finding or performance improvement has been demonstrated. Do not start diff-reset, pooling/merging, prefetch, new optimization or adopt rejected candidates from this result. Return actual measurements to the owner only when separately allowed; this stage has no external writes or publication.
