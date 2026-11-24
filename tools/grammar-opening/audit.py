#!/usr/bin/env python3
"""Geometry/source contracts only. Never starts Unity, a player, or PhysX."""
import collections
import copy
import hashlib
import json
import math
import statistics
import subprocess
from pathlib import Path
import author as A

OUT=A.ROOT/'docs/grammar-opening'


def sha(value):return hashlib.sha256(value).hexdigest()


def metrics(rows,lo,hi):
    selected=[r for r in rows if lo<=r['p'][0]<hi]
    width=(hi-lo)/6
    bins=[sum(lo+i*width<=r['p'][0]<lo+(i+1)*width for r in selected) for i in range(6)]
    return dict(count=len(selected),binWidthMetres=width,bins=bins,
                densityCV=statistics.pstdev(bins)/statistics.mean(bins) if selected else None)


def boxes_touch(a,b):
    return all(min(a[i][1],b[i][1])-max(a[i][0],b[i][0])>=-1e-5 for i in range(3))


def scenery_audit(sign):
    boxes=[[[p-s/2,p+s/2] for p,s in zip(b['p'],b['size'])] for b in sign['boxes']]
    grounded={i for i,b in enumerate(boxes) if abs(b[1][0])<=1e-5}
    reached=set(grounded)
    while True:
        more={i for i,b in enumerate(boxes) if i not in reached and any(boxes_touch(b,boxes[j]) for j in reached)}
        if not more:break
        reached.update(more)
    return dict(id=sign['id'],boxes=len(boxes),groundedParts=len(grounded),
                disconnectedParts=[sign['boxes'][i]['key'] for i in range(len(boxes)) if i not in reached],
                minY=min(b[1][0] for b in boxes),bounds=[[min(b[k][0] for b in boxes),max(b[k][1] for b in boxes)] for k in range(3)])


def hero_contract(row,geo):
    assets=json.loads((A.ASSETS/'Resources/CityDensityR2/CityGeometryR2Compact.json').read_text())['assets']
    asset=next(a for a in assets if a['id']==row['asset'])
    proposals=json.loads((A.ASSETS/'Resources/CityDensityR2/CityDestructionProposals.json').read_text())['assets']
    prop=next(p for p in proposals if p['asset']==row['asset'])
    post=next(p for p in asset['pieces'] if p['shard']==-1 and min(p['v'][1::3])==0)
    groups=sorted({p['shard'] for p in asset['pieces'] if p['shard']>=0})
    local=[[min(post['v'][k::3]),max(post['v'][k::3])] for k in range(3)]
    p=copy.deepcopy(row)
    post_box=copy.deepcopy(geo);post_box[row['asset']]={'bounds':local}
    base_box=A.G.world_bounds(p,post_box)
    upper_vertices=[p['v'] for p in asset['pieces'] if p['shard']>=0]
    top_bounds=[[min(v[k] for v in upper_vertices for k in range(i,len(v),3)),
                 max(v[k] for v in upper_vertices for k in range(i,len(v),3))] for i in range(3)]
    upper_geo={row['asset']:dict(bounds=top_bounds)}
    upper=A.G.world_bounds(row,upper_geo)
    assert groups==list(range(6))
    assert abs(base_box[1][0])<1e-5
    assert base_box[2][0]<0<base_box[2][1],('post misses flight plane',row['id'])
    assert upper[0][1]-upper[0][0] > 5*(base_box[0][1]-base_box[0][0])
    return dict(id=row['id'],sector=row['hero'],position=row['p'],scale=row['s'],yaw=row['yaw'],
                base=dict(posts=1,bounds=base_box),upper=dict(shardGroups=groups,bounds=upper),
                massKgSuggestion=prop['massKgSuggestion'],threshold=prop['normalizedImpulseThresholdSuggestion'],
                contactPath='existing single BreakPanel: base hit -> complete assembly fracture -> existing debris contacts',
                note='6 upper groups; existing physical fragment cap may leave one group decorative; no new wave/score/impulse')


def source_contract():
    hooks={
      'games/flight/crab-flight/Assets/Scripts/City2Data.cs':'allInstances=OpeningLayout.Apply(allInstances);',
      'games/flight/crab-flight/Assets/NeonProto/NeonSectionArt.cs':'\n  OpeningLandmarks.Build(Root);',
      'games/flight/crab-flight/Assets/Scripts/RestaurantPhysics.cs':'  position=OpeningLayout.PanelPosition(id,position); // Placement input only; contact/break rules below are unchanged.\n',
    }
    for name,hook in hooks.items():
        source=(A.ROOT/name).read_text()
        assert source.count(hook)==1,name
        base=subprocess.check_output(['git','show',A.BASE+':'+name],cwd=A.ROOT).decode()
        assert source.replace(hook,'')==base,'Non-placement source changed: '+name
    changes=subprocess.check_output(['git','diff','--name-only',A.BASE,'--'],cwd=A.ROOT,text=True).splitlines()
    allowed=lambda p:p in hooks or p=='tasks.md' or any(p.startswith(d) for d in
          ['tools/grammar-opening/','docs/grammar-opening/','qa/grammar-opening/','games/flight/crab-flight/Assets/OpeningGrammar'])
    assert all(allowed(p) for p in changes),changes
    # Native intact targets have no moving body; the destruction/contact code is
    # BASE byte-identical after removing only the coordinate lookup above.
    contacts=(A.ASSETS/'Scripts/City2Contacts.cs').read_text()
    intact=contacts.split('public IEnumerable<object> City2ContactWorkR18(){',1)[1].split('void PrepareCityDebris()',1)[0]
    assert 'AddComponent<Rigidbody>' not in intact
    assert 'AddPanelShape(p,' in intact and 'panels.Add(p)' in intact
    assert 'SpawnCity2Shards' in contacts and 'fixedNodes' in contacts and 'var rb=obj.AddComponent<Rigidbody>()' in contacts
    restaurant=(A.ASSETS/'Scripts/RestaurantPhysics.cs').read_text()
    shape=restaurant.split('void AddPanelShape(',1)[1].split('void QueueBreak(',1)[0]
    assert 'AddComponent<Rigidbody>' not in shape and 'AddComponent<BoxCollider>' in shape
    opening=(A.ASSETS/'OpeningGrammar/OpeningLandmarks.cs').read_text()
    assert 'AddComponent<BoxCollider>' not in opening and 'AddComponent<Rigidbody>' not in opening
    return dict(changedExistingFiles=changes,placementHooks=hooks,otherPhysicsByteExact=True,
                staticTargetContract='No Rigidbody on intact target; body creation occurs only after accepted break')


def audit(data,check_source=True):
    geo=A.geometry();rows=data['instances'];before=A.before();by_id={r['id']:r for r in before}
    assert len(rows)==len({r['id'] for r in rows})==136
    assert {r['id'] for r in rows}=={r['id'] for r in before if abs(r['p'][0])<135}
    assert all(abs(r['p'][0])<135 for r in rows)
    for r in rows:
        assert r['asset']==by_id[r['id']]['asset'],('family changed',r['id'])
        assert next(n for n,a,b in A.SECTORS if a<=abs(r['p'][0])<b)==next(n for n,a,b in A.SECTORS if a<=abs(by_id[r['id']]['p'][0])<b)
    floats=[r['id'] for r in rows if abs(A.G.bottom(r,geo))>1e-5]
    assert not floats,('floating',floats)
    isolated=[r['id'] for r in rows if A.G.nearest(r,rows)>A.G.CHAIN]
    assert not isolated,('isolated',isolated)
    # Preserve the 10m side-to-corridor rule separately for each direction.
    outside=[]
    for sign in [1,-1]:
        side=[r for r in rows if r['p'][0]*sign>0]
        anchors=[r for r in side+A.fixed_targets(True) if abs(r['p'][2])<=2 and r['p'][0]*sign>0 and r['p'][1]<20]
        outside += [r['id'] for r in side if abs(r['p'][2])>2 and min(math.dist(r['p'],s['p']) for s in anchors)>10]
    assert not outside,('sideOutsideChain',outside)
    overlaps=[];fixture_overlaps=[]
    boxes={r['id']:A.G.world_bounds(r,geo) for r in rows}
    for i,r in enumerate(rows):
        for s in rows[i+1:]:
            if A.G.overlaps(boxes[r['id']],boxes[s['id']]):overlaps.append([r['id'],s['id']])
        for s in A.fixed_targets(True):
            if A.G.overlaps(boxes[r['id']],s['box']):fixture_overlaps.append([r['id'],s['id']])
    assert not overlaps,overlaps
    assert not fixture_overlaps,fixture_overlaps
    assert all(max(abs(v) for v in b[0])<135 for b in boxes.values())
    landmarks=[scenery_audit(s) for s in data['landmarks']]
    assert all(not s['disconnectedParts'] and abs(s['minY'])<1e-5 for s in landmarks),landmarks
    assert all(max(abs(v) for v in s['bounds'][0])<135 for s in landmarks)
    for support in data['supports']:
        assert abs(support['p'][1]-support['size'][1]/2)<1e-5
        assert abs(support['p'][1]+support['size'][1]/2-3.2*.065625)<1e-5
        assert support['size'][0]>=1.7 and support['size'][2]>=3.6
    old_other=[r for r in before if abs(r['p'][0])>=135]
    replacement={r['id']:r for r in rows}
    after=[replacement.get(r['id'],r) for r in before]
    encode=lambda r:json.dumps(r,sort_keys=True).encode()
    assert sha(encode(old_other))==sha(encode([r for r in after if abs(r['p'][0])>=135]))
    metrics_by_sector={}
    for name,lo,hi in A.SECTORS:
        old=metrics(before+A.fixed_targets(),lo,hi);new=metrics(rows+A.fixed_targets(True),lo,hi)
        assert new['count']==old['count']
        assert new['densityCV']>=.4,(name,new)
        metrics_by_sector[name]=dict(before=old,after=new,cityBefore=metrics(before,lo,hi),cityAfter=metrics(rows,lo,hi))
    gaps=[]
    for gap in data['gaps']:
        members=[r['id'] for r in rows if gap['start']<r['p'][0]<gap['end']]
        fixed=[r['id'] for r in A.fixed_targets(True) if gap['start']<r['p'][0]<gap['end']]
        assert not members,members
        gaps.append(dict(**gap,cityTargets=members,retainedOtherTargets=fixed))
    # Sample an uninterrupted low-flight strip through the mall. A 0.6m radius
    # center at y=11.5 and z=0 clears all new physical extents, including the hero.
    # This is geometric feasibility, NOT a measured ballistic replay.
    blocked=[]
    for step in range(1071):
        x=28+step*.1;swept=[[x-.6,x+.6],[10.9,12.1],[-.6,.6]]
        if any(A.G.overlaps(swept,b) for k,b in boxes.items() if 28<=replacement[k]['p'][0]<135):blocked.append(x)
    assert not blocked,('corridor',blocked[:10])
    heroes=[hero_contract(r,geo) for r in rows if r.get('hero')]
    # Reduced offline static model: intact targets have no dynamic body and no
    # unsolicited break queue. Keep every transform and target intact for 10s.
    start=encode(rows);broken=set();steps=2400
    for _ in range(steps):
        for h in heroes:
            impulse=0.0
            if impulse>h['threshold']:broken.add(h['id'])
    assert not broken and start==encode(rows)
    positive={r['id']:r for r in rows if r['p'][0]>0}
    for r in rows:
        if r['p'][0]>=0:continue
        p=positive[A.canonical_positive(r['id'])]
        assert r['p']==[-p['p'][0],p['p'][1],-p['p'][2] if p.get('hero') else p['p'][2]] and r['yaw']==180
    return dict(baseline=A.BASE,scope='offline placement/source contracts; no Unity/PhysX/native rendering',
       densityMethod='six equal-width bins per sector; population standard deviation / mean (port: 300/6=50m)',
       sectors=metrics_by_sector,cityWorldBefore=len(before),cityWorldAfter=len(after),worldTotalBefore=1481,worldTotalAfter=1481,
       cityChangedPositive=94,cityMirroredExistingNegative=42,panelMoves=len(data['panels']),floating=len(floats),isolated=len(isolated),
       sideOutsideChain=outside,aabbOverlaps=overlaps,fixtureOverlaps=fixture_overlaps,groundTolerance=1e-5,chainDistance=10,
       landmarks=landmarks,servingPlinths=data['supports'],heroes=heroes,gaps=gaps,protectedOutsideOpeningCount=len(old_other),protectedOutsideOpeningSHA256=sha(encode(old_other)),
       staticSimulation=dict(model='intact static targets; zero external impulses; not PhysX',seconds=10,step=1/240,steps=steps,
                             selfCollapse=0,selfBreaks=0,transformDrift=0),
       corridor=dict(mallSweptRadius=.6,mallCenterHeight=11.5,sampledPositions=1071,blocked=blocked,
                     limits='geometric open strip; actual launch trajectories and collisions remain Native validation'),
       source=source_contract() if check_source else None)


if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True)
    data=json.loads(A.OUT.read_text());assert A.encoded(data)==A.encoded(A.build())
    result=audit(data);(OUT/'audit.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
    print(json.dumps({k:v for k,v in result.items() if k in ['sectors','floating','isolated','staticSimulation','worldTotalAfter']},ensure_ascii=False,indent=2))
