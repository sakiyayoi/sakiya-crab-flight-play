# Partial source reconstruction R1

Base: actual saved r32, commit 49a260853a127e9e5be8184e9a1dbe1fcff493a7. This is not the lost r49 tree and is not equivalent to r49.

The saved five-file patch is r48-to-r49 and fails direct application to r32. Only the identical ceiling redirect statement supports a precise incoming-sign backport. All other runtime and Unity Assets remain the real r32 baseline. The original r49 host/index were recovered from the executable distribution with Git blob prefix matches and are archived, not connected to an incompatible runtime. Read RECONSTRUCTION-MANIFEST.json for exact missing files and unknown source ownership.

No V4/model direction has been applied, no missing Assets or C# types synthesized, and no public site changed. Build and loop verification are a new reconstruction comparison only.
