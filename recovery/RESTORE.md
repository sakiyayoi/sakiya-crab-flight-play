# Partial reconstruction R1 — reproducible local candidate

This is **r32 source + one evidenced r49 ceiling sign change**, not recovered original r49 and not r49-equivalent. Original r49 host/index are separately archived with matching patch Git blob IDs. Missing original source/Assets are listed in source/recovery/RECONSTRUCTION-MANIFEST.json; none is synthesized. New king-crab/model direction is not applied to R1.

Source base: 49a260853a127e9e5be8184e9a1dbe1fcff493a7. Engine source: d5a72fae5e0071e24e24a8f404b92a0e206c7cf0. Engine identity SHA256: f9cb6db6320834a4f1e7ebe93fc69d9936c64a67972013c87ac49c52c71d29c3.

Restore: unzip to a writable folder. The source/games/flight/crab-flight directory is a complete Unity6000.3.23f1 project; keep its .meta files. The source/voice, source/licenses and source/recovery/runtime-assets contain the required existing credits/audio distribution assets.

Rebuild using existing Unity6000.3.23f1 with WebGL support:

    Unity -batchmode -quit -buildTarget WebGL -projectPath "<folder>/source/games/flight/crab-flight" -executeMethod ReconstructionBuildR1.Web -logFile "<folder>/rebuild.log"
    python3 "<folder>/source/recovery/post_package.py"

Run the bundled tested Player locally:

    python3 -m http.server 8765 --bind 127.0.0.1 --directory "<folder>/web-r1"

Open http://127.0.0.1:8765/ in a WebGL-capable browser. This is local only; do not publish. The compiled candidate revision is kani-recon-r32-ceiling49-r1.

Observed: Unity build succeeded/errors0/warnings15. Chrome headlessMetal/DPR1/1280x800 ordinary-host keyboard/pointer loop passed twice:168.438461m/24breaks and209.653687m/27breaks, normal settled, browsererrors0. Existing r49 under same requested angle/power/direction had195.235275m/22 and183.804932m/29; phase/spin differ. These are scoped loop checks, not strict10, full40, M14, physical iPhone, real listening or owner quality acceptance.
