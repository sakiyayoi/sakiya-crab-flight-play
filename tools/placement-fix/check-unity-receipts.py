#!/usr/bin/env python3
"""After Chloe runs NeonStage6Review.Candidate, enforce this brief's extra gates.
Does not start Unity. It rejects old-layout receipts and cannot certify visual QA.
"""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def check(directory):
    def load(name):
        return json.loads((directory/name).read_text())
    receipt = load("stage6-receipt.json")
    audit = load("stage6-density-audit.json")
    pixels = load("stage6-pixel-assertions.json")
    placements_file = ROOT/"games/flight/crab-flight/Assets/NeonDensity/Resources/NeonDensity/Placements.json"
    placements = json.loads(placements_file.read_text())["instances"]
    before = json.loads((ROOT/"tools/placement-fix/baseline-targets.json").read_text())["world"]["targets"]
    actual = {t["id"]: t for t in audit["after"]["targets"]}
    errors = []
    if receipt.get("error") or not receipt["replayPass"]:
        errors.append("Native candidate replay/assertion failed")
    state = receipt["state"]
    if state["distance"] < 2500 or receipt["breaks"] < 96 or state.get("physicsFault") or state["phase"] != "result":
        errors.append("Requires result, distance >=2500m, breaks >=96, empty physicsFault")
    if not audit["originalTargetsUnchanged"] or not audit["stage5OriginalSignaturesUnchanged"]:
        errors.append("Baseline signatures changed")
    if any(t["id"] not in actual or actual[t["id"]]["signature"] != t["signature"] for t in before):
        errors.append("Not all 348 baseline signatures match the supplied original")
    counts = receipt["counts"]
    if [counts[k] for k in ("mallTargets", "ground", "mid", "high")] != [74, 54, 14, 6]:
        errors.append("Mall must remain 74 (54/14/6)")
    mismatch = []
    for p in placements:
        t = actual.get(p["id"])
        if not t or t["asset"] != p["asset"] or any(abs(t["position"][k]-p["p"][i]) > .001
                                                     or abs(t["scale"][k]-p["s"][i]) > .00001
                                                     for i, k in enumerate("xyz")):
            mismatch.append(p["id"])
    if mismatch:
        errors.append(f"Receipt is not current layout: {len(mismatch)} position/scale/asset mismatches")
    if audit["after"]["totalBreakTargets"] != 1481:
        errors.append("Expected 1481 registered targets")
    sign = next((s for s in pixels["signs"] if s["path"].endswith("/Neon sign orbit 0")), None)
    if not sign or not sign["visible"] or sign["pixelContribution"] <= .01:
        errors.append("Orbit sign0 visibility/pixel contribution failed (original threshold .01)")
    return dict(pass_=not errors, errors=errors, distance=state["distance"], breaks=receipt["breaks"],
                currentPlacementsSha256=hashlib.sha256(placements_file.read_bytes()).hexdigest(),
                scope="Native receipt gates only. Inspect fresh sector/portrait/landscape screenshots separately.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path)
    args = parser.parse_args()
    result = check(args.directory)
    print(json.dumps(result, ensure_ascii=False, indent=2))
    raise SystemExit(0 if result["pass_"] else 1)
