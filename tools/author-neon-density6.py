#!/usr/bin/env python3
"""Regenerate the placement-fix candidate without Unity or changes to physics.

The immutable ui-simplify layout supplies identities, assets, scale and order.
Only positions/chunks and explicit, grounded scenery supports are authored here.
Run with --check to verify checked-in outputs without writing them.
"""
import argparse
from copy import deepcopy
import hashlib
import json
import math
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / "games/flight/crab-flight/Assets"
DATA = "games/flight/crab-flight/Assets/NeonDensity/Resources/NeonDensity/"
BASE = "d288db27c604ba716c2ef7af921ea0975e28efe1"
REVISION = "neon-placement-fix-1"
EPSILON = 1e-5  # metres: floating point/JSON rounding, not the old 0.5 m threshold
CHAIN = 10.0
CORRIDOR_LIMIT = 63.55
SECTORS = [("tank", 0, 4.5), ("restaurant", 4.5, 28), ("mall", 28, 135),
           ("office", 135, 600), ("port", 600, 900), ("viaduct", 900, 1300),
           ("country", 1300, 2050), ("launchrange", 2050, 2680), ("orbit", 2680, 3220)]
GEOMETRY = ["CityDensity/CityGeometry", "CityDensityR2/CityGeometryR2Compact",
            "FarWorldR10/Geometry", "RouteR12/Geometry"]


def require(condition, message):
    if not condition:
        raise ValueError(message)


def encode(value):
    return json.dumps(value, ensure_ascii=False, indent=2) + "\n"


def digest(raw):
    return hashlib.sha256(raw).hexdigest()


def base_layout():
    return json.loads(subprocess.check_output(
        ["git", "show", f"{BASE}:{DATA}Placements.json"], cwd=ROOT))


def load_geometry():
    geo = {}
    for name in GEOMETRY:
        for asset in json.loads((ASSETS / f"Resources/{name}.json").read_text())["assets"]:
            # All mesh vertices are already in asset-local coordinates (the runtime
            # loader uses these directly). Yaw does not change the bottom height.
            vertices = [p["v"] for p in asset["pieces"]]
            bounds = [[min(v[k] for v in vertices for k in range(axis, len(v), 3)),
                       max(v[k] for v in vertices for k in range(axis, len(v), 3))]
                      for axis in range(3)]
            geo[asset["id"]] = dict(bounds=bounds, source=f"Resources/{name}.json",
                                     envelope=asset["collisionEnvelopeSuggestion"])
    return geo


def baseline():
    data = json.loads((ROOT / "tools/placement-fix/baseline-targets.json").read_text())
    result = []
    for t in data["world"]["targets"]:
        q = json.loads(t["signature"].split("|")[3])
        require(abs(q["x"]) < EPSILON and abs(q["z"]) < EPSILON, "Baseline rotation is not yaw-only")
        result.append(dict(id=t["id"], asset=t["asset"], p=[t["position"][k] for k in "xyz"],
                           s=[t["scale"][k] for k in "xyz"], yaw=math.degrees(2*math.atan2(q["y"], q["w"]))))
    return result


def sector(row):
    return next((name for name, start, end in SECTORS if start <= row["p"][0] < end), None)


def natural_air(row):
    # Only the five already airborne orbit satellites are grandfathered, not a
    # generic flag permitting arbitrary objects or new high-altitude placements.
    return row["asset"] == "satellite_neon6" and sector(row) == "orbit" and row["p"][1] > 0


def bottom(row, geo):
    return row["p"][1] + geo[row["asset"]]["bounds"][1][0] * row["s"][1]


def footprint(row, geo):
    bounds = geo[row["asset"]]["bounds"]
    angle = math.radians(row["yaw"])
    cs, sn = math.cos(angle), math.sin(angle)
    corners = [(row["p"][0] + cs*x*row["s"][0] + sn*z*row["s"][2],
                row["p"][2] - sn*x*row["s"][0] + cs*z*row["s"][2])
               for x in bounds[0] for z in bounds[2]]
    return [min(x for x, z in corners), max(x for x, z in corners),
            min(z for x, z in corners), max(z for x, z in corners)]


def snap(row, geo, surface=0.0):
    require(row["asset"] in geo, f"Missing geometry: {row['asset']}")
    row["p"][1] = round(surface - geo[row["asset"]]["bounds"][1][0]*row["s"][1], 6)


def corridor(rows):
    return sorted((r for r in rows if r["p"][0] >= 0 and abs(r["p"][2]) <= 2
                   and r["p"][1] < 20), key=lambda r: (r["p"][0], r["id"]))


def nearest(row, rows):
    return min((math.dist(row["p"], r["p"]) for r in rows if r["id"] != row["id"]), default=math.inf)


def world_bounds(row, geo):
    x0, x1, z0, z1 = footprint(row, geo)
    return [[x0, x1], [bottom(row, geo), row["p"][1]+geo[row["asset"]]["bounds"][1][1]*row["s"][1]], [z0, z1]]


def overlaps(a, b):
    # Conservative rendered mesh AABBs, not a claim of exact Collider contacts.
    return all(min(x[1], y[1])-max(x[0], y[0]) > EPSILON for x, y in zip(a, b))


def overlap_pairs(rows, base, geo):
    known = [r for r in base if r["asset"] in geo]
    bounds = {r["id"]: world_bounds(r, geo) for r in rows+known}
    pairs = []
    for i, r in enumerate(rows):
        for other in rows[i+1:]+known:
            if overlaps(bounds[r["id"]], bounds[other["id"]]):
                pairs.append(tuple(sorted((r["id"], other["id"]))))
    return pairs


def separate_overlaps(rows, original, base, geo, fixed):
    previous = set(overlap_pairs(original, base, geo))
    known = [r for r in base if r["asset"] in geo]
    by_id = {r["id"]: r for r in rows}
    for iteration in range(5):
        conflicts = sorted(set(overlap_pairs(rows, base, geo))-previous)
        if not conflicts:
            return
        for pair in conflicts:
            a, b = pair
            all_by_id = {r["id"]: r for r in rows+known}
            if not overlaps(world_bounds(all_by_id[a], geo), world_bounds(all_by_id[b], geo)):
                continue
            choices = [by_id[i] for i in pair if i in by_id and i not in fixed]
            solved = False
            for r in choices:
                p = r["p"][:]
                others = [o for o in rows+known if o["id"] != r["id"]]
                other_bounds = [(o, world_bounds(o, geo)) for o in others]
                start, end = next((s, e) for n, s, e in SECTORS if n == sector(r))
                for amount in [i*.25 for i in range(1, 25)]:
                    for direction in (-1, 1):
                        r["p"][0] = round(p[0]+amount*direction, 4)
                        box = world_bounds(r, geo)
                        if box[0][0] <= start or box[0][1] >= end:
                            continue
                        if any(overlaps(box, bb) and tuple(sorted((r["id"], o["id"]))) not in previous
                               for o, bb in other_bounds):
                            continue
                        anchors = corridor(base+rows)
                        affected = [o for o in rows if abs(o["p"][2]) > 2 and
                                    min(abs(o["p"][0]-p[0]), abs(o["p"][0]-r["p"][0])) < 18]
                        if any(nearest(o, anchors) > CHAIN for o in affected):
                            continue
                        solved = True
                        break
                    if solved:
                        break
                if solved:
                    break
                r["p"] = p
            require(solved, f"Cannot separate overlap within 6 m without losing chain: {pair}")
    raise ValueError("Overlap separation failed to converge")


def add_tower(row, geo, x, top, z, supports):
    row["p"][0], row["p"][2] = x, z
    snap(row, geo, top)
    x0, x1, z0, z1 = footprint(row, geo)
    supports.append(dict(id="support-"+row["id"], target=row["id"],
                         p=[round((x0+x1)/2, 6), round(top/2, 6), round((z0+z1)/2, 6)],
                         size=[round(x1-x0+.3, 6), top, round(z1-z0+.3, 6)],
                         top=top, bottom=0.0, breakable=False, collision=False))


def author(original, base, geo):
    rows = deepcopy(original)
    satellites = {r["id"] for r in original if natural_air(r)}
    for r in rows:
        if r["id"] not in satellites:
            snap(r, geo)
    supports = []
    by_id = {r["id"]: r for r in rows}
    # Preserve the isolated satellite at [2950,21,4.5]. Two existing oxygen tanks
    # form the 0 -> 7 -> 14 -> 21 m chain, each on its own grounded tower.
    add_tower(by_id["neon-density-orbit-s6-1024"], geo, 2947, 7, 1.8, supports)
    add_tower(by_id["neon-density-orbit-s6-1026"], geo, 2951, 14, 1.8, supports)
    supported = {s["target"] for s in supports}
    moved = set(supported)
    small = {"steel_drum", "market_crate", "hay_roll_neon6", "farm_fence_neon6",
             "oxygen_tank_neon6", "harbor_buoy_neon6", "traffic_signal_neon6"}

    # Fill gaps with existing small side objects, never a whole line of heavy
    # containers/solar arrays. Orbit's first 120 m uses a 12 m gap;
    # elsewhere 14 m leaves <=7 m to an anchor, with lateral room within 10 m.
    while True:
        anchors = corridor(base+rows)
        gaps = [(a, b) for a, b in zip(anchors, anchors[1:])
                if a["p"][0] >= 135 and b["p"][0]-a["p"][0] >
                (12 if 2680 <= (a["p"][0]+b["p"][0])/2 < 2800 else 14)]
        if not gaps:
            break
        a, b = max(gaps, key=lambda pair: pair[1]["p"][0]-pair[0]["p"][0])
        midpoint = (a["p"][0]+b["p"][0])/2
        candidates = [r for r in rows if r["id"] not in moved and abs(r["p"][2]) >= 4
                      and r["asset"] in small and abs(r["p"][0]-midpoint) < 18
                      and r["p"][1] < 2]
        require(candidates, f"No small side object for corridor gap at {midpoint}")
        r = min(candidates, key=lambda r: (abs(r["p"][0]-midpoint), r["id"]))
        # Stable irregularity, independent of Python random/hash version.
        jitter = (int(digest(r["id"].encode())[:8], 16)/0xffffffff*2-1)*1.5
        name = sector(r)
        start, end = next((s, e) for n, s, e in SECTORS if n == name)
        half = (geo[r["asset"]]["bounds"][0][1]-geo[r["asset"]]["bounds"][0][0])*r["s"][0]/2
        x = max(start+half+.1, min(midpoint+jitter, end-half-.1))
        require(a["p"][0]+3 < x < b["p"][0]-3, "Insufficient gap for promotion")
        r["p"][0] = round(x, 4)
        r["p"][2] = 1.7 if r["p"][2] > 0 else -1.7
        moved.add(r["id"])

    # Balance side lanes per sector. Airborne exceptions remain byte-identical.
    for name, start, end in SECTORS:
        side = [r for r in rows if sector(r) == name and abs(r["p"][2]) >= 4]
        positive = sum(r["p"][2] > 0 for r in side)
        wanted = len(side)//2
        for r in sorted(side, key=lambda r: (r["p"][0], r["id"])):
            if r["id"] in satellites:
                continue
            if (positive > wanted and r["p"][2] > 0) or (positive < wanted and r["p"][2] < 0):
                positive += -1 if r["p"][2] > 0 else 1
                r["p"][2] *= -1

    # Remaining side/inner rows must have a *corridor* neighbor within 10 m.
    # A horizontal correction at the same lane avoids adding another direct hit.
    anchors = corridor(base+rows)
    for r in rows:
        if abs(r["p"][2]) <= 2 or nearest(r, anchors) <= CHAIN:
            continue
        require(r["id"] not in satellites, f"Airborne satellite unlinked: {r['id']}")
        local = [a for a in anchors if sector(a) == sector(r) and abs(a["p"][1]-r["p"][1]) < 6]
        require(local, f"No corridor anchor for {r['id']}")
        target = min(local, key=lambda a: math.dist(r["p"], a["p"]))
        r["p"][0] = round(target["p"][0] + (2 if r["p"][0] >= target["p"][0] else -2), 4)
    separate_overlaps(rows, original, base, geo, satellites | supported)
    for r in rows:
        r["chunk"] = math.floor(r["p"][0]/32)

    # Raised sign at the original x/z, above the obstructing foregound layer.
    # Its lower edge rests on a grounded mast; no baseline target is moved.
    supports.append(dict(id="support-orbit-sign0", target="Neon sign orbit 0",
                         p=[2725, 8.75, 6.5], size=[.6, 17.5, .6],
                         top=17.5, bottom=0.0, breakable=False, collision=False))
    return rows, dict(revision=REVISION, supports=supports,
                      orbitSign0=dict(p=[2725, 20, 6.5], size=[7, 5, 1], support="support-orbit-sign0"))


def support_issues(rows, geo, support_data):
    supports = {s["target"]: s for s in support_data["supports"]}
    issues = []
    for r in rows:
        if natural_air(r):
            continue
        s = supports.get(r["id"])
        surface = 0
        if s:
            surface = s["top"]
            require(abs(s["p"][1]-s["size"][1]/2) <= EPSILON, "Ungrounded support")
            require(abs(s["p"][1]+s["size"][1]/2-surface) <= EPSILON, "Incorrect support top")
            x0, x1, z0, z1 = footprint(r, geo)
            require(s["p"][0]-s["size"][0]/2 <= x0+EPSILON and x1 <= s["p"][0]+s["size"][0]/2+EPSILON
                    and s["p"][2]-s["size"][2]/2 <= z0+EPSILON and z1 <= s["p"][2]+s["size"][2]/2+EPSILON,
                    f"Object outside support footprint: {r['id']}")
        gap = bottom(r, geo)-surface
        if abs(gap) > EPSILON:
            issues.append(dict(id=r["id"], gap=gap))
    return issues


def metrics(rows, base, geo, support_data):
    all_rows = base+rows
    anchors = corridor(all_rows)
    floated = [r for r in rows if bottom(r, geo) > .5]
    isolated = [r["id"] for r in rows if nearest(r, all_rows) > CHAIN]
    side = [r for r in rows if abs(r["p"][2]) >= 4]
    unlinked = [r["id"] for r in side if nearest(r, anchors) > CHAIN]
    known_legacy = [r for r in floated if r["asset"] != "rail_glass_planter_display_r10"]
    gaps = [(b["p"][0]-a["p"][0], a["p"][0], b["p"][0]) for a, b in zip(anchors, anchors[1:])]
    rows_by_sector = []
    for name, start, end in SECTORS:
        addition = [r for r in rows if sector(r) == name]
        local = [r for r in all_rows if start <= r["p"][0] < end]
        center = [r for r in anchors if start <= r["p"][0] < end]
        rows_by_sector.append(dict(sector=name, additions=len(addition), corridorAdded=len(corridor(addition)),
                                   corridorTotal=len(center), corridorPer100m=len(center)*100/(end-start),
                                   flightBandPer100m=sum(r["p"][1] < 20 for r in local)*100/(end-start)))
    # Full rolling windows only outside protected mall/tank/restaurant, where
    # this candidate promotes rows. Sector averages alone can hide clumps.
    xs = [r["p"][0] for r in anchors if r["p"][0] >= 135]
    peak = max(sum(x <= y < x+100 for y in xs) for x in xs)
    added_center = sum(abs(r["p"][2]) <= 2 for r in rows)
    issues = support_issues(rows, geo, support_data)
    return dict(additions=len(rows), estimatedGlobalRegistered=422+len(rows),
                legacyFloatAboveGround=len(known_legacy), floatAboveGroundWithPlanter=len(floated),
                naturalAirborne=sum(natural_air(r) for r in rows),
                unsupported=len(issues), unsupportedIds=issues, corridorAdded=added_center,
                corridorAddedPercent=added_center*100/len(rows), corridorFlightPositiveRoute=len(anchors),
                sideCount=len(side), sidePositive=sum(r["p"][2] > 0 for r in side),
                sideNegative=sum(r["p"][2] < 0 for r in side), sideOutsideChain=unlinked,
                isolated=isolated, highIsolated=sum(r["p"][1] >= 20 and r["id"] in isolated for r in rows),
                maxCorridorGap=dict(metres=max(gaps)[0], start=max(gaps)[1], end=max(gaps)[2]),
                rolling100mPeakAfterMall=peak, sectors=rows_by_sector)


def validate(original, rows, base, geo, supports, after):
    require(len(rows) == len(original) == 1059, "Target count changed")
    require(len({r["id"] for r in rows}) == len(rows), "Duplicate placement ID")
    require(len({r["chainGroup"] for r in rows}) == len(rows), "Duplicate chain group")
    for old, new in zip(original, rows):
        require({k: v for k, v in old.items() if k not in ("p", "chunk")} ==
                {k: v for k, v in new.items() if k not in ("p", "chunk")}, "Non-position change")
        require(sector(old) == sector(new) and sector(new) != "mall", "Sector/mall changed")
        require(natural_air(old) == natural_air(new), "New airborne exception not authorized")
        if natural_air(old):
            require(old == new, f"Airborne exception changed: {old['id']}")
        require(all(math.isfinite(v) for v in new["p"]), "Nonfinite position")
        require(new["chunk"] == math.floor(new["p"][0]/32), "Stale chunk")
        start, end = next((s, e) for name, s, e in SECTORS if name == sector(new))
        x0, x1, _, _ = footprint(new, geo)
        require(start < x0 and x1 < end, f"Footprint crosses sector: {new['id']}")
    require(not after["unsupported"], "Unsupported objects remain")
    require(not after["isolated"], "Isolated additions remain")
    require(not after["sideOutsideChain"], "Side objects lack corridor chain neighbor")
    require(not set(overlap_pairs(rows, base, geo))-set(overlap_pairs(original, base, geo)), "New mesh AABB overlaps")
    require(after["rolling100mPeakAfterMall"] <= CORRIDOR_LIMIT, "Rolling corridor density too high")
    require(all(s["corridorPer100m"] <= CORRIDOR_LIMIT for s in after["sectors"]), "Sector corridor too dense")
    sequence = [s["flightBandPer100m"] for s in after["sectors"][2:]]
    require(all(a > b for a, b in zip(sequence, sequence[1:])), "Density no longer tapers from mall")
    for s in supports["supports"]:
        require(s["size"][0] > 0 and s["size"][1] > 0 and s["size"][2] > 0, "Empty support")
        require(abs(s["p"][1]-s["size"][1]/2) <= EPSILON, "Support floats")
    sign = supports["orbitSign0"]
    mast = next(s for s in supports["supports"] if s["id"] == sign["support"])
    require(abs(sign["p"][1]-sign["size"][1]/2-mast["top"]) <= EPSILON, "Sign not on mast")


def generate():
    original = base_layout()["instances"]
    base, geo = baseline(), load_geometry()
    rows, supports = author(original, base, geo)
    before = metrics(original, base, geo, dict(supports=[]))
    after = metrics(rows, base, geo, supports)
    require((before["legacyFloatAboveGround"], before["corridorAdded"], len(before["isolated"]),
             len(before["sideOutsideChain"]), before["corridorFlightPositiveRoute"]) == (229,254,39,64,385),
            "Baseline does not reproduce PLACEMENT-AUDIT.md")
    validate(original, rows, base, geo, supports, after)
    layout = dict(revision=REVISION, instances=rows)
    changes = [dict(id=a["id"], asset=a["asset"], before=a["p"], after=b["p"])
               for a, b in zip(original, rows) if a["p"] != b["p"]]
    report = dict(baseCommit=BASE, revision=REVISION,
                  method="Placement origins; Euclidean nearest-neighbor distance; +x corridor y<20, |z|<=2. "
                         "Original audit floating threshold 0.5m; support residual tolerance 0.00001m. "
                         "Baseline 348 is historical input, not fresh Unity signature verification.",
                  before=before, after=after,
                  geometry={k: v for k, v in sorted(geo.items()) if k in {r["asset"] for r in rows}},
                  changedPositions=len(changes), changes=changes,
                  overlapPairsBefore=overlap_pairs(original, base, geo), overlapPairsAfter=overlap_pairs(rows, base, geo),
                  placementsSha256=digest(encode(layout).encode()),
                  supportsSha256=digest(encode(supports).encode()),
                  runtimeValidation="NOT_RUN: Unity/build/replay/pixel checks delegated by user")
    return {DATA+"Placements.json": encode(layout), DATA+"Supports.json": encode(supports),
            "docs/placement-fix/audit.json": encode(report)}, report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check", action="store_true", help="verify byte-identical regeneration without writing")
    args = parser.parse_args()
    outputs, report = generate()  # Validate all files before writing any.
    for path, content in outputs.items():
        file = ROOT/path
        if args.check:
            require(file.exists() and file.read_text() == content, f"Regeneration mismatch: {path}")
        else:
            file.parent.mkdir(parents=True, exist_ok=True)
            file.write_text(content)
    for label in ("before", "after"):
        data = report[label]
        print(label, json.dumps({k: data[k] for k in ("legacyFloatAboveGround", "floatAboveGroundWithPlanter",
                    "unsupported", "corridorAdded", "corridorAddedPercent", "sideCount", "rolling100mPeakAfterMall")}, ensure_ascii=False))
        print(label, "isolated", len(data["isolated"]), "sideOutsideChain", len(data["sideOutsideChain"]))
    print("PASS", "read-only regeneration check" if args.check else "generated validated placement candidate")


if __name__ == "__main__":
    main()
