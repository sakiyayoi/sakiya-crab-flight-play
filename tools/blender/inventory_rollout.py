"""Count effective families from actual Native City targets, never raw placement totals."""
import collections, hashlib, json, pathlib, sys
HERE = pathlib.Path(__file__).resolve().parent
SOURCE = HERE.parents[1]
ROOT = SOURCE.parent
EV = ROOT / 'evidence/blender-rollout'

def digest(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def inventory(target_path):
    rows = json.loads(target_path.read_text())['targets']
    assert len({r['id'] for r in rows}) == len(rows), 'duplicate actual IDs'
    base = SOURCE / 'games/flight/crab-flight/Assets'
    geometry = {}
    for relative in ['Resources/CityDensityR2/CityGeometryR2Compact.json',
                     'Resources/FarWorldR10/Geometry.json', 'Resources/RouteR12/Geometry.json',
                     'MallAreasR5/Resources/MallAreasR5/Geometry.json',
                     'MallDensityProto/Resources/MallDensityProto/Geometry.json']:
        path = base / relative
        for asset in json.loads(path.read_text())['assets']:
            assert asset['id'] not in geometry, asset['id']
            geometry[asset['id']] = (path, asset)
    sectors = json.loads((base/'Resources/WorldRouteR11.json').read_text())['sectors']
    pilot = json.loads((HERE/'families.json').read_text())['families']
    counts = collections.Counter(r['asset'] for r in rows)
    families = {}
    for family, count in sorted(counts.items()):
        path, asset = geometry[family]
        points = [p['v'][i:i+3] for p in asset['pieces'] for i in range(0,len(p['v']),3)]
        shards = []
        for sid in sorted({p['shard'] for p in asset['pieces']}):
            pts = [p['v'][i:i+3] for p in asset['pieces'] if p['shard']==sid for i in range(0,len(p['v']),3)]
            shards.append(dict(id=sid,min=[min(p[i] for p in pts) for i in range(3)],max=[max(p[i] for p in pts) for i in range(3)]))
        families[family] = dict(count=count,min=[min(p[i] for p in points) for i in range(3)],max=[max(p[i] for p in points) for i in range(3)],shards=shards,geometry=str(path.relative_to(SOURCE)),geometrySha256=digest(path),pilotModel=family in pilot)
    groups = []
    for sector in sectors:
        for direction in [1,-1]:
            selected = [r for r in rows if (r['position']['x']>=0)==(direction==1) and
                        next((s['id'] for s in sectors if abs(r['position']['x'])<s['end']),'orbit')==sector['id']]
            grouped = collections.Counter(r['asset'] for r in selected)
            groups.append(dict(sector=sector['id'],label=sector['label'],direction=direction,count=len(selected),familyCount=len(grouped),families=dict(sorted(grouped.items())),unmodelledBefore=sorted(f for f in grouped if f not in pilot),unmodelledNow=sorted(f for f in grouped if not (base/'BlenderModels/Resources/BlenderModels'/f'{f}.fbx').exists()),ids=[r['id'] for r in selected]))
    doc = dict(sourceTargets=str(target_path),sourceSha256=digest(target_path),total=len(rows),familyCount=len(families),families=families,groups=groups)
    (HERE/'rollout-families.json').write_text(json.dumps(doc,ensure_ascii=False,indent=2)+'\n')
    EV.mkdir(exist_ok=True)
    (EV/'inventory.json').write_text(json.dumps(doc,ensure_ascii=False,indent=2)+'\n')
    print(json.dumps(dict(total=len(rows),families=len(families),newFamilies=[f for f in families if f not in pilot],groups=[{k:g[k] for k in ['sector','direction','count','familyCount','unmodelledNow']} for g in groups]),ensure_ascii=False,indent=2))

if __name__=='__main__':
    inventory(pathlib.Path(sys.argv[1]) if len(sys.argv)>1 else EV/'native-baseline/targets.json')
