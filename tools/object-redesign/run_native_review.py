#!/usr/bin/env python3
"""Run the archived review helper and require its action log before accepting output.

Use this script from the same complete checkout that is being reviewed. It never
injects a /tmp helper, changes game settings, builds, serves, or publishes anything.
PORT_REVIEW_INPUT and PORT_REVIEW_OUTPUT retain the existing review contract.
"""
import argparse
import hashlib
import json
import math
import os
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[2]
UNITY = "/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity"
HELPER = Path("Assets/PortPrototype/Editor/PortNativeReview.cs")


def verify_output(output):
    """An empty log is valid; a missing log or malformed event is a failed review."""
    path = output / "camon/action-events.jsonl"
    lines = path.read_text().splitlines()
    previous = -math.inf
    for line in lines:
        event = json.loads(line)
        if set(event) != {"kind", "position", "time"}:
            raise ValueError("Action event must contain kind, position, time")
        if event["kind"] not in {"boost", "bounce", "spin", "brake", "blast", "death"}:
            raise ValueError("Unknown action kind")
        position = event["position"]
        if not isinstance(position, dict) or set(position) != {"x", "y", "z"}:
            raise ValueError("Action position must contain x, y, z")
        values = [event["time"], *position.values()]
        if any(type(v) not in (int, float) or not math.isfinite(v) for v in values):
            raise ValueError("Action coordinates/time must be finite numbers")
        if event["time"] < 0 or event["time"] < previous:
            raise ValueError("Action times must be nonnegative and ordered")
        previous = event["time"]
    receipts = json.loads((output / "receipts.json").read_text())
    runs = [r for r in receipts["runs"] if r["cameraOn"]]
    if len(runs) != 1 or not runs[0]["retryReady"]:
        raise ValueError("Expected one completed camon review with retry evidence")
    return {"path": str(path), "events": len(lines),
            "sha256": hashlib.sha256(path.read_bytes()).hexdigest()}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--unity", default=UNITY)
    parser.add_argument("--log-file", required=True)
    args = parser.parse_args()
    project = ROOT / "games/flight/crab-flight"
    helper = project / HELPER
    # Refuse the legacy helper before starting Unity. Post-run output verification
    # is authoritative; this also gives a clear diagnosis for a stale build copy.
    source = helper.read_text()
    if "WriteActionEvents(dir, sim.ObjectHits);" not in source:
        raise ValueError("Stale PortNativeReview: restore the helper from this source commit; do not inject /tmp/PortNativeReviewOpening.cs")
    helper_hash = hashlib.sha256(helper.read_bytes()).hexdigest()
    output_value = os.environ.get("PORT_REVIEW_OUTPUT", "")
    input_value = os.environ.get("PORT_REVIEW_INPUT", "")
    if not output_value or Path(output_value).exists():
        raise ValueError("Use a fresh PORT_REVIEW_OUTPUT directory")
    if not input_value or not Path(input_value).is_file():
        raise ValueError("PORT_REVIEW_INPUT must point at accepted-input.json")
    output = Path(output_value).resolve()
    env = dict(os.environ, PORT_REVIEW_OUTPUT=str(output),
               PORT_REVIEW_INPUT=str(Path(input_value).resolve()))
    subprocess.run([args.unity, "-batchmode", "-quit", "-forgetProjectPath",
                    "-projectPath", str(project), "-executeMethod", "PortNativeReview.Run",
                    "-logFile", str(Path(args.log_file).resolve())], env=env, check=True)
    if hashlib.sha256(helper.read_bytes()).hexdigest() != helper_hash:
        raise ValueError("Review helper changed during execution")
    result = verify_output(output)
    result["helperSha256"] = helper_hash
    print(json.dumps(result, ensure_ascii=False))


if __name__ == "__main__":
    main()
