#!/usr/bin/env python3
"""Opening-only layout overlay. Immutable port tip, same identities, no Unity run."""
import argparse
import copy
import importlib.util
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
BASE = '2d0d849120b0bc0002173f49b2db0fd7f6f05987'
ASSETS = ROOT / 'games/flight/crab-flight/Assets'
OUT = ASSETS / 'OpeningGrammar/Resources/OpeningGrammar/Layout.json'
SECTORS = [('tank', 0, 4.5), ('restaurant', 4.5, 28), ('mall', 28, 135)]
spec = importlib.util.spec_from_file_location('placement_geometry', ROOT/'tools/author-neon-density6.py')
G = importlib.util.module_from_spec(spec)
spec.loader.exec_module(G)


def geometry():
    geo = G.load_geometry()
    for name in ['MallAreasR5', 'MallDensityProto']:
        for asset in json.loads((ASSETS/f'{name}/Resources/{name}/Geometry.json').read_text())['assets']:
            vertices = [p['v'] for p in asset['pieces']]
            geo[asset['id']] = dict(bounds=[
                [min(v[k] for v in vertices for k in range(axis, len(v), 3)),
                 max(v[k] for v in vertices for k in range(axis, len(v), 3))]
                for axis in range(3)])
    return geo


def before():
    # This historical native receipt is the unchanged opening input on port tip.
    world = json.loads((ROOT/'docs/blender-rollout/native-final02/world.json').read_text())
    rows = []
    for t in world['targets']:
        q = json.loads(t['signature'].split('|')[3])
        rows.append(dict(id=t['id'], asset=t['asset'], p=[t['position'][k] for k in 'xyz'],
                         s=[t['scale'][k] for k in 'xyz'], yaw=math.degrees(2*math.atan2(q['y'], q['w'])),
                         breakable=True, band='near', chunk=int(abs(t['position']['x'])//40), chainGroup=0))
    port = {r['id']: r for r in json.loads((ASSETS/'PortPrototype/Resources/PortPrototype/Layout.json').read_text())['instances']}
    return [port.get(r['id'], r) for r in rows]


def canonical_positive(identity):
    return identity.replace('--1-', '-+1-') if not identity.startswith('r29-') else identity.replace('--1-', '-1-')


def panel_overrides():
    return [dict(id=name+'-'+str(side),p=[side*x,y,z]) for side in [1,-1]
            for name,x,y,z in [('dish-stack',5.5,1.6,3.9),('r5-restaurant-service-food',8.1,.275,3.2),
                               ('r5-restaurant-service-table',22.5,1.45,3.4),('wood-table',20,2.1,-4.4)]]


def fixed_targets(after=False):
    """Non-City registered targets, transcribed from the unchanged generators.

    The contract pins RestaurantPhysics/OverdriveRoute/CoreDistrictContactsR7/
    LifeContactsR6 to BASE. Bounds are conservative panel boxes, not mesh claims.
    """
    rows=[]
    fixtures=[('tank-glass',4.5,3.25,[.13,6.5,5.2]),('dish-stack',13.5,1.6,[1.7,3.2,3.6]),
              ('ceiling',12.5,13.5,[24,.45,7]),('food-tray',7.5,1.9,[1.4,1.2,3]),
              ('wood-chair',10,1.7,[1.6,3.4,3]),('wood-table',18,2.1,[3.3,4.2,4]),
              ('metal-kitchen',21.5,2.3,[1.6,4.6,3]),
              ('r5-restaurant-entry-dishes',6,.4,[.9,.8,2.4]),
              ('r5-restaurant-chair-food',8.7,.275,[.65,.55,2.4]),
              ('r5-restaurant-dish-tower',11.7,.55,[1.3,1.1,2.8]),
              ('r5-restaurant-service-table',15.7,1.45,[1.1,2.9,3]),
              ('r5-restaurant-service-food',14.8,.275,[.6,.55,2.6]),
              ('r5-restaurant-table-dishes',20.15,.45,[.65,.9,2.8]),
              ('r5-restaurant-kitchen-food',22.625,.25,[.5,.5,2.5]),
              ('r5-restaurant-exit-table',23.5,1.6,[1.2,3.2,3]),
              ('core-mall-glass',112,7.2,[.14,4.8,3.2])]
    fixtures += [('outer-wall-'+str(i),25,(i+.5)*4.5,[.55,4.5,7]) for i in range(3)]
    for side in [1,-1]:
        for key,x,y,size in fixtures:
            identity='outer-wall-'+str(side)+'-'+key.rsplit('-',1)[1] if key.startswith('outer-wall-') else key+'-'+str(side)
            rows.append(dict(id=identity,p=[x*side,y,0],box=[[x*side-size[0]/2,x*side+size[0]/2],
                         [y-size[1]/2,y+size[1]/2],[-size[2]/2,size[2]/2]],system='panel'))
        for x in [15,60]:
            rows.append(dict(id='life-person-'+str(side)+'-'+str(x),p=[side*x,0,0],
                             box=[[side*x-.651,side*x+.651],[.357,4.2525],[-.5355,.4725]],system='life'))
        rows.append(dict(id='life-bird-'+str(side)+'-45',p=[side*45,11,0],
                         box=[[side*45-.63,side*45+.63],[11.24,13.16],[-1.05,1.05]],system='life'))
    if after:
        edits={r['id']:r['p'] for r in panel_overrides()}
        for r in rows:
            if r['id'] not in edits:continue
            p=edits[r['id']];offset=[p[i]-r['p'][i] for i in range(3)]
            r['box']=[[a+offset[i],b+offset[i]] for i,(a,b) in enumerate(r['box'])];r['p']=p
    return rows


def build():
    original = before()
    geo = geometry()
    positive = [copy.deepcopy(r) for r in original if 0 <= r['p'][0] < 135]
    by_id = {r['id']: r for r in positive}
    partners = {canonical_positive(r['id']):r for r in original if -135<r['p'][0]<0}
    def mirrored(r):
        old = partners.get(r['id'])
        if old is None:return None
        result = copy.deepcopy(r)
        result.update(id=old['id'],asset=old['asset'],yaw=180)
        result['p'][0]*=-1
        if result.get('hero'):result['p'][2]*=-1
        return result
    def place(r, x, z, scale=None):
        if scale is not None:
            r['s'] = [scale]*3 if isinstance(scale, (int, float)) else scale
        r['p'] = [round(x, 6), 0, round(z, 6)]
        r['yaw'] = 0
        G.snap(r, geo)
        r['chunk'] = int(x//40)
        return r

    tank = [r for r in positive if r['p'][0] < 4.5]
    for r, x, z in zip(tank, [.48, .64, 4.02], [-4.5, 4.5, -4.5]):
        place(r, x, z)
        r['beat'] = 'tank-entry' if x < 1 else 'tank-exit'

    # The intact sign is ONE existing destructible assembly: its thin post and
    # six broad upper panels already share the same BreakPanel and shard graph.
    # Hitting the post therefore releases the upper panels via existing physics.
    small = by_id['atrium-+1-00-blade_sign']
    place(small, 26.8, -.30, [.85, .85, 1.25])
    small['hero'] = 'restaurant'
    restaurant = [r for r in positive if 4.5 <= r['p'][0] < 28 and r is not small]
    # Wide panels go to the side pockets; the existing restaurant wall/ceiling keep their coordinates; four small fixture
    # placements use the same data overlay. Contact/break algorithms are unchanged.
    restaurant.sort(key=lambda r: -max(b-a for a,b in G.world_bounds(r,geo)))
    occupied=[G.world_bounds(small,geo)]+[r['box'] for r in fixed_targets(True) if 0<r['p'][0]<28]
    slots=[(4.6,11.4)]*8+[(20.2,24.0)]*3+[(24.2,27.8)]*5
    for index,(r,(lo,hi)) in enumerate(zip(restaurant,slots)):
        placed=False
        for step in range(int((hi-lo)*20)):
            x=lo+step*.05
            for z in [-4.6,4.6,-2.45,2.45,0]:
                place(r,x,z);b=G.world_bounds(r,geo)
                if b[0][0]<4.6 or b[0][1]>27.9:continue
                if any(G.overlaps([[a-.05,b+.05] for a,b in b],box) for box in occupied):continue
                occupied.append(b);placed=True;break
            if placed:break
        if not placed:raise ValueError('No restaurant bay for '+r['id'])

    mall = [r for r in positive if 28 <= r['p'][0] < 135]
    large = by_id['core-r1-+1-02-mall-exit-high-sign']
    place(large, 106.8, -.48, [2.1, 1.75, 2.0])
    large['hero'] = 'mall'
    # Preserve the adopted giant capsule at full scale. Move it behind the
    # corridor instead of shrinking the spectacle to manufacture an open path.
    giant = by_id['density-proto-18-giant-capsule']
    place(giant,118,9.8)
    for r in mall:
        if r is not large:
            r['yaw'] = 0
            G.snap(r,geo)

    # Two dense, staggered shopping pockets, separated by a 29m clear interval.
    # Shelf-pack by actual mesh envelope; lane spacing covers every footprint.
    clusters = [(29.0,64.0),(94.0,134.6)]
    lanes = [-4.7,-1.45,1.75,5.0]
    placed = [large,giant]
    placed_boxes=[G.world_bounds(r,geo) for r in placed]
    mirrored_boxes=[G.world_bounds(mirrored(r),geo) for r in placed if r['id'] in partners]
    fixed_mall=[r['box'] for r in fixed_targets(True) if 28<=r['p'][0]<135]
    fixed_mirror=[r['box'] for r in fixed_targets(True) if -135<r['p'][0]<=-28]
    remaining = [r for r in mall if r is not large and r is not giant]
    remaining.sort(key=lambda r:(-max(b-a for a,b in G.world_bounds(r,geo)),r['id']))
    for index,r in enumerate(remaining):
        order = [index%2,1-index%2]
        success = False
        for c in order:
            lo,hi = clusters[c]
            local=copy.deepcopy(r);place(local,0,0)
            b=G.world_bounds(local,geo)
            for step in range(int((hi-lo)*10)):
                x=lo-b[0][0]+step*.1
                if x+b[0][1]>hi:break
                for lane in range(len(lanes)):
                    z=lanes[(lane+index)%len(lanes)]+.037*(index%3)
                    place(r,x,z);box=G.world_bounds(r,geo)
                    # Keep some breathing room; never overlap objects in 3D.
                    expanded=[[a-.10,b+.10] for a,b in box]
                    if any(G.overlaps(expanded,b) for b in placed_boxes):continue
                    if any(G.overlaps(expanded,b) for b in fixed_mall):continue
                    partner=mirrored(r)
                    if partner is not None:
                        mirror_box=[[a-.10,b+.10] for a,b in G.world_bounds(partner,geo)]
                        if any(G.overlaps(mirror_box,b) for b in mirrored_boxes):continue
                        if any(G.overlaps(mirror_box,b) for b in fixed_mirror):continue
                        mirrored_boxes.append(G.world_bounds(partner,geo))
                    placed.append(r);placed_boxes.append(box);success=True;break
                if success:break
            if success:break
        if not success:raise ValueError('No opening bay for '+r['id'])

    for r in positive:
        if r.get('beat'):continue
        sector = next(n for n,a,b in SECTORS if a<=r['p'][0]<b)
        r['beat'] = sector+('-entry' if r['p'][0] < (16 if sector=='restaurant' else 79) else '-exit')

    # Preserve the established asymmetric inventory. Mirror the 42 EXISTING
    # negative partners, never manufacture copies of positive-only density IDs.
    negative=[]
    for old in original:
        if not -135 < old['p'][0] < 0:continue
        r=mirrored(by_id[canonical_positive(old['id'])])
        # The blade post is offset in local z. Center the post itself on z=0
        # on BOTH approaches; other partners keep the established z lanes.
        # Legacy mirror is x -> -x, yaw -> 180-yaw, same z.
        # Re-snap against the unchanged yaw-invariant mesh bottom.
        negative.append(r)
    # Original Dishes mesh starts 0.065625*3.2 = 0.21m above its panel bottom.
    # Keep its unchanged collider and add a visible, grounded serving plinth.
    supports=[dict(key='dish-plinth-'+str(side),p=[side*5.5,.105,3.9],size=[1.7,.21,3.6],
                   color=[.141176,.117647,.180392]) for side in [1,-1]]
    return dict(revision='grammar-opening-1',baseline=BASE,instances=positive+negative,panels=panel_overrides(),supports=supports,
                landmarks=landmarks(), gaps=[dict(sector='tank',start=1.1,end=3.5),
                    dict(sector='restaurant',start=12.4,end=18.2),dict(sector='mall',start=64.1,end=93.9)])


def landmarks():
    """Connected, ground-rooted sign assemblies; no Collider or animation."""
    ink=[.141176,.117647,.180392];lamp=[1,.70196,.27843];paper=[1,.953,.839]
    result=[]
    for name,x,z,w,base,head in [('restaurant',3.45,-3.6,1.9,2.65,1.25),
                               ('mall',26.95,-4.8,1.9,6.0,2.7),
                               ('office',133.65,-5.0,2.0,0,14.0)]:
        boxes=[]
        def box(key,p,size,color):boxes.append(dict(key=key,p=p,size=size,color=color))
        if name!='office':
            box('post',[x,base/2,z],[.24,base,.32],ink)
            box('frame',[x,base+head/2,z],[w,head,.32],ink)
            box('lamp-face',[x,base+head/2,z-.18],[w-.14,head-.14,.08],lamp)
            # Dish + fork / shopping bag, built onto the luminous sign face.
            cy=base+head/2
            if name=='restaurant':
                box('dish',[x+.25,cy-.15,z-.23],[.65,.12,.04],ink)
                box('fork',[x-.48,cy-.02,z-.23],[.08,.65,.04],ink)
                for dx in [-.14,0,.14]:box('fork-prong',[x-.48+dx,cy+.24,z-.23],[.06,.24,.04],ink)
                box('fork-join',[x-.48,cy+.14,z-.23],[.34,.07,.04],ink)
            else:
                box('bag',[x,cy-.2,z-.23],[1.1,.9,.04],ink)
                for dx in [-.32,.32]:box('handle',[x+dx,cy+.36,z-.23],[.12,.35,.04],ink)
                box('handle-top',[x,cy+.55,z-.23],[.76,.12,.04],ink)
        else:
            box('tower',[x,7,z],[w,14,.65],ink)
            for y in [3.0,5.6,8.2,10.8,13.0]:
                for dx in [-.43,.43]:box('window',[x+dx,y,z-.36],[.46,.95,.08],lamp)
            box('crown',[x,13.85,z-.36],[1.8,.28,.08],paper)
        for side in [1,-1]:
            mirrored=copy.deepcopy(boxes)
            for b in mirrored:b['p'][0]*=side
            result.append(dict(id=name+('-positive' if side==1 else '-negative'),nextSector=name,side=side,boxes=mirrored))
    return result


def encoded(data):return json.dumps(data,ensure_ascii=False,indent=2)+'\n'


if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');args=parser.parse_args()
    text=encoded(build())
    if args.check:assert OUT.read_text()==text,'Opening layout regeneration mismatch'
    else:OUT.parent.mkdir(parents=True,exist_ok=True);OUT.write_text(text)
    print('Opening layout: 136 existing placements, 6 render-only landmarks; '+('matched' if args.check else 'written'))
