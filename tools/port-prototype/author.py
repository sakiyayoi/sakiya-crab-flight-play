#!/usr/bin/env python3
"""Port scene templates. No Unity process, no physics parameter authoring.
The 539acb1 native inventory is historical input, not a new runtime receipt.
"""
import argparse, collections, copy, hashlib, importlib.util, json, math, random, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('placement_geometry',ROOT/'tools/author-neon-density6.py')
G=importlib.util.module_from_spec(spec);spec.loader.exec_module(G)
BASE='539acb1264739e52dc236d0260bda105431910a6'
OUT=ROOT/'games/flight/crab-flight/Assets/PortPrototype/Resources/PortPrototype/Layout.json'
BEATS=[('S1',600,615,10),('S2',615,632,1),('S3',632,668,25),('S4',668,698,4),('S5',698,762,54),('S6',762,782,3),('S7',782,848,41),('S8',848,900,7)]
F={'container':'cargo_container_neon6','crate':'market_crate','drum':'steel_drum','buoy':'harbor_buoy_neon6','stall':'market_stall','shelf':'rail_glass_planter_display_r10','fence':'farm_fence_neon6','sign':'blade_sign','crane':'port_lattice_loader_r10','van':'delivery_van','road':'road_sign_neon6'}
def before():
    world=json.loads(subprocess.check_output(['git','show',f'{BASE}:docs/blender-rollout/native-final02/world.json'],cwd=ROOT))
    return world

def build():
    geo=G.load_geometry(); rows=[];rng=random.Random(42043)
    def add(beat,fam,x,z=0,scale=1,*,support='',role='',chain='',tier=0,yaw=0):
        asset=F.get(fam,fam); scales=[scale]*3 if isinstance(scale,(float,int)) else scale
        r=dict(key=f'{beat}-{fam}-{sum(t["asset"]==asset and t["beat"]==beat for t in rows):02}',asset=asset,beat=beat,p=[x,0,z],s=scales,yaw=yaw,band='near',breakable=True,chunk=int(x//40),chainGroup=0,support=support,role=role,chain=chain,tier=tier,variant=len(rows)%3)
        # Ground roots use actual mesh min, including legacy offsets.
        top=0
        if support:
            parent=next(t for t in rows if t['key']==support)
            top=G.world_bounds(parent,geo)[1][1]
        G.snap(r,geo,top);rows.append(r);return r
    # S1: the legacy crane's long axis is rotated across the corridor, feet on both sides.
    add('S1','crane',608.17,.013,[1.7,2.2,1.5],yaw=90,role='landmark')
    for x,z in [(611.35,-4.87),(611.81,.63),(612.72,4.61)]:add('S1','container',x,z,1.50)
    for i,(x,z) in enumerate([(600.94,-.31),(602.33,1.53),(603.58,-.47),(604.87,1.41)]):add('S1','crate',x,z,.72,role='boost45' if i==0 else '',chain='gate')
    for x,z in [(602.14,-2.66),(604.39,3.37)]:add('S1','drum',x,z,.9)
    add('S2','buoy',624.23,-5.17,.9)
    # S3: asymmetric market pockets, not mirrored lanes.
    for x,z in [(635.21,-3.43),(643.38,3.72),(652.13,-3.81),(661.49,3.46)]:add('S3','stall',x,z,.70)
    shelves=[add('S3','shelf',x,z,1.05) for x,z in [(640.17,-4.89),(650.31,4.93),(659.28,-4.43)]]
    for i in range(8):add('S3','crate',633.73+i*3.87+rng.uniform(-.37,.37),(-.81 if i%2 else .74)+rng.uniform(-.19,.19),.85,role='boost30' if i==0 else '',chain='market')
    for i,s in enumerate(shelves[:2]):add('S3','crate',s['p'][0]+.013*(i+1),s['p'][2]+.017,.68,support=s['key'],chain='market')
    for x,z in [(636.71,1.71),(642.29,-1.64),(647.66,1.81),(655.02,-1.63),(659.0,1.64),(665.12,-.53)]:add('S3','drum',x,z,.9,chain='market')
    for x,z in [(635.49,-4.72),(661.73,4.67)]:add('S3','sign',x,z,.95)
    for x,z in [(669.14,-5.42),(677.91,-4.72),(686.28,-5.68),(696.17,-4.61)]:add('S4','buoy',x,z,.9)
    # S5: 24 ground containers, 11 second tiers, 6 third tiers. The crack separates sets.
    bases=[]
    for i,(x,z) in enumerate([(701.21,-3.93),(704.74,1.38),(707.16,-3.71),(710.83,1.56),(712.81,-3.83),(716.37,1.71),(719.02,-3.61),(721.24,1.49)]):
        bases.append(add('S5','container',x,z,1.50,chain='yard-sub'))
    for col,x in enumerate([734.57,739.06,743.49,748.09]):
        for lane,z in enumerate([-4.57,-1.62,1.37,4.38]):
            bases.append(add('S5','container',x+[0,.093,.217,.361][lane]+col*lane*.019,z+col*.037+lane*.011,1.50,chain='yard-main'))
    second=[]
    for i,base in enumerate([bases[1],bases[3],bases[5],*bases[8:16]]):
        second.append(add('S5','container',base['p'][0]+(-.13+i*.024),base['p'][2]+.011,1.40,support=base['key'],chain=base['chain'],tier=1))
    for i,base in enumerate(second[3:9]):add('S5','container',base['p'][0]+(-.074+i*.027),base['p'][2]+.009,1.34,support=base['key'],chain=base['chain'],tier=2)
    for i,(x,z) in enumerate([(699.13,-.21),(706.13,-1.10),(714.24,-.44),(725.31,.71),(731.42,-.23),(738.21,6.28)]):
        add('S5','crate',x,z,.72,role='boost60' if i==0 else 'jackpot' if i==4 else '',chain='yard-sub' if i<4 else 'yard-main')
    for base in [bases[0],bases[19]]:add('S5','crate',base['p'][0]+.027,base['p'][2]+.021,.70,support=base['key'],chain=base['chain'])
    for x,z in [(702.43,-1.10),(718.67,-1.05),(731.71,1.42),(751.12,-.61)]:add('S5','drum',x,z,.9,chain='' if x==751.12 else 'yard-sub' if x<730 else 'yard-main',role='hazard' if x==751.12 else '')
    add('S5','crane',754.13,.047,[1.7,2.2,1.5],yaw=90,role='landmark')
    add('S6','buoy',769.31,-5.13,.9)
    for x,z in [(764.87,4.17),(779.19,4.73)]:add('S6','fence',x,z,1)
    # S7 includes the three registrations omitted by the brief's inventory (+container/crate/drum).
    s7bases=[]
    for x,z in [(784.71,-3.67),(791.13,3.43),(798.31,-4.01),(805.49,3.71),(812.17,-3.81),(819.57,3.39),(826.11,-4.13),(834.19,3.53),(842.41,-3.63)]:s7bases.append(add('S7','container',x,z,1.50))
    for i,base in enumerate(s7bases[3:5]):add('S7','container',base['p'][0]+(.083+i*.031),base['p'][2]+.012,1.40,support=base['key'],tier=1)
    for i,(x,z) in enumerate([(783.21,-.49),(786.73,.87),(790.11,-.37),(793.57,.73),(800.89,-.41),(820.21,-.59),(838.31,.89)]):add('S7','crate',x,z,.85,chain='warehouse' if i<4 else '',role='boost30' if i==0 else '')
    for x,z in [(787.93,-1.73),(795.20,.10),(803.91,-.81),(823.77,.71),(844.13,-.31)]:add('S7','drum',x,z,.9,chain='warehouse' if x<797 else '')
    for x,z in [(788.23,-4.51),(801.79,4.63),(821.91,-4.57),(839.37,4.39)]:add('S7','stall',x,z,.70)
    for x,z in [(795.37,4.31),(830.59,-4.43)]:add('S7','shelf',x,z,1.05)
    for x,z in [(789.01,-5.71),(839.71,5.63)]:add('S7','sign',x,z,.95)
    for x,z in [(782.93,5.43),(815.79,-5.71),(846.19,4.91)]:add('S7','fence',x,z,1)
    add('S7','van',809.13,-.17,1,role='redirect30')
    for fam,x,z in [('flywheel_accumulator_r12',816.31,.31),('flywheel_accumulator_r12',829.13,.53),('facade_break_panel',833.17,4.89),('street_awning',817.13,-3.41),('hvac_unit',836.79,-2.71),('street_bench',825.51,4.57)]:
        add('S7',fam,x,z,.55 if fam=='street_awning' else .9 if fam=='facade_break_panel' else 1,role='resource' if 'flywheel' in fam else '')
    for x,z in [(850.37,-3.39),(853.59,1.19)]:add('S8','container',x,z,1.50)
    for x,z in [(858.31,3.81),(880.73,4.19)]:add('S8','road',x,z,1,role='preview')
    for x,z in [(859.17,-5.33),(875.91,-4.87),(893.21,-5.61)]:add('S8','buoy',x,z,.9)
    old=[r for r in before()['targets'] if 600<=r['position']['x']<900]
    assert len(old)==len(rows)==145,(len(old),len(rows))
    # Keep matching family IDs first; preserve the two finite resource identities.
    pool=copy.deepcopy(old)
    for r in sorted(rows,key=lambda x:(x['asset']!='flywheel_accumulator_r12',x['key'])):
        candidates=[o for o in pool if o['asset']==r['asset']]
        o=next(iter(candidates),pool[0]);pool.remove(o);r['id']=o['id']
    return dict(revision='port-prototype-1',baseline=BASE,resourceReserve=[683,0,0],instances=rows)

def old_rows():
    result=[]
    for t in before()['targets']:
        q=json.loads(t['signature'].split('|')[3]);result.append(dict(id=t['id'],asset=t['asset'],p=[t['position'][k] for k in 'xyz'],s=[t['scale'][k] for k in 'xyz'],yaw=math.degrees(2*math.atan2(q['y'],q['w']))))
    return result

def metrics(rows):
    geo=G.load_geometry();ordered=sorted(rows,key=lambda r:r['p'][0]);bins=[sum(lo<=r['p'][0]<lo+50 for r in rows) for lo in range(600,900,50)];mean=sum(bins)/6
    gaps=[round(b['p'][0]-a['p'][0],2) for a,b in zip(ordered,ordered[1:])];counts=collections.Counter(gaps)
    return dict(count=len(rows),families=dict(collections.Counter(r['asset'] for r in rows)),bins50m=bins,densityCV=(sum((v-mean)**2 for v in bins)/6)**.5/mean,
        faceRate=sum(r['asset']==F['container'] for r in rows)/len(rows),partnerRate=sum(r['asset']==F['crate'] for r in rows)/len(rows),
        reaching3m=sum(G.world_bounds(r,geo)[1][1]>=3 for r in rows),reaching3mRate=sum(G.world_bounds(r,geo)[1][1]>=3 for r in rows)/len(rows),
        modeGapMetres=counts.most_common(1)[0][0],modeGapCount=counts.most_common(1)[0][1],modeGapRate=counts.most_common(1)[0][1]/len(gaps),
        consecutiveSameZ=sum(abs(a['p'][2]-b['p'][2])<1e-6 for a,b in zip(ordered,ordered[1:])))

def audit(data):
    rows=data['instances'];geo=G.load_geometry();bykey={r['key']:r for r in rows};old=old_rows();oldport=[r for r in old if 600<=r['p'][0]<900];support_errors=[];bands=[]
    for r in rows:
        box=G.world_bounds(r,geo);s=bykey.get(r['support']);base=G.world_bounds(s,geo) if s else None
        expected=base[1][1] if s else 0
        if abs(box[1][0]-expected)>1e-5:support_errors.append(r['key']+':height')
        if s:
            for axis in [0,2]:
                if box[axis][0]<base[axis][0]-1e-5 or box[axis][1]>base[axis][1]+1e-5:support_errors.append(r['key']+':footprint')
            if s['s'][1]<r['s'][1] and r['asset']==F['container']:support_errors.append(r['key']+':heavier-above')
        if r['asset']==F['drum'] and box[1][1]>=3:bands.append(r['key'])
        if r['asset']==F['buoy'] and (abs(r['p'][2])<=2 or box[1][1]>=3):bands.append(r['key'])
        if r['asset']==F['container'] and r['tier']==2 and not 8<=box[1][1]<15:bands.append(r['key'])
    chains={}
    for name in sorted({r['chain'] for r in rows if r['chain']}):
        group=[r for r in rows if r['chain']==name];start=next(r for r in group if r['role']);reached={start['key']};changed=True
        while changed:
            changed=False
            for r in group:
                if r['key'] not in reached and any(math.dist(r['p'],bykey[k]['p'])<=8 for k in reached):reached.add(r['key']);changed=True
        chains[name]=dict(count=len(group),reachableWithin8m=len(reached),trigger=start['key'])
    replacing={r['id']:r for r in rows};after=[replacing.get(r['id'],r) for r in old]
    protected=[r for r in old if not 600<=r['p'][0]<900]
    digest=lambda rs:hashlib.sha256(json.dumps(rs,sort_keys=True,separators=(',',':')).encode()).hexdigest()
    anchors=G.corridor(after);peak=max(sum(x['p'][0]<=r['p'][0]<x['p'][0]+100 for r in anchors) for x in anchors if x['p'][0]>=135)
    gaps={name:sum(a<=r['p'][0]<b and abs(r['p'][2])<=2 for r in rows) for name,a,b,n in BEATS if name in ['S2','S4','S6']}
    result=dict(baseline=BASE,inventorySource='historical Native inventory + current unchanged source; not a new Unity receipt',before=metrics(oldport),after=metrics(rows),beatCounts=dict(collections.Counter(r['beat'] for r in rows)),groundingViolations=support_errors,altitudeViolations=bands,chains=chains,gapCorridorCounts=gaps,rolling100mCorridorPeak=peak,corridorLimit=63.55,worldCityBefore=len(old),worldCityAfter=len(after),worldTotalBefore=before()['totalBreakTargets'],worldTotalAfter=before()['totalBreakTargets']-len(oldport)+len(rows),protectedBeforeSHA256=digest(protected),protectedAfterSHA256=digest([r for r in after if not 600<=r['p'][0]<900]))
    return result

def validate(a):
    assert not a['groundingViolations'],a['groundingViolations']
    assert not a['altitudeViolations'],a['altitudeViolations']
    assert a['after']['count']==a['before']['count']==145
    assert a['worldTotalAfter']==1481
    assert a['protectedBeforeSHA256']==a['protectedAfterSHA256']
    assert a['beatCounts']=={k:n for k,_,_,n in BEATS},a['beatCounts']
    assert a['after']['densityCV']>=.5,a['after']
    assert .35<=a['after']['faceRate']<=.45
    assert .20<=a['after']['partnerRate']<=.30
    assert .15<=a['after']['reaching3mRate']<=.25,a['after']
    assert a['after']['modeGapRate']<.05,a['after']
    assert a['after']['consecutiveSameZ']==0
    assert all(v['count']==v['reachableWithin8m'] for v in a['chains'].values()),a['chains']
    assert not any(a['gapCorridorCounts'].values())
    assert a['rolling100mCorridorPeak']<=a['corridorLimit']

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--check',action='store_true');args=p.parse_args();d=build();a=audit(d)
    Path(ROOT/'docs/port-prototype/audit.json').write_text(json.dumps(a,ensure_ascii=False,indent=2)+'\n')
    validate(a)
    encoded=json.dumps(d,ensure_ascii=False,indent=2)+'\n'
    if args.check:assert OUT.read_text()==encoded,'Regeneration mismatch'
    else:OUT.write_text(encoded)
    print(json.dumps(a,ensure_ascii=False,indent=2))
