"""Blender 4.2 LTS. Render-only rollout; reuse the pilot export/shard/origin pipeline.
Run with -- <family> to regenerate one family. Existing pilot models are not regenerated.
Authoring helpers take Unity XYZ metres; saved .blend uses Z up and front -Y.
"""
import sys, pathlib, json, math, os
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import build_mall as pilot
from build_mall import box, cyl, ring, tube, ell, panel, B
import bpy
from mathutils import Vector

HERE=pathlib.Path(__file__).resolve().parent
DATA=json.loads((HERE/'rollout-families.json').read_text())['families']
pilot.EVIDENCE=pilot.ROOT/'evidence/blender-rollout/models'
pilot.EVIDENCE.mkdir(parents=True,exist_ok=True)
if os.environ.get('BLENDER_ROLLOUT_STAGING'):
    pilot.OUT=pathlib.Path(os.environ['BLENDER_ROLLOUT_STAGING']);pilot.OUT.mkdir(parents=True,exist_ok=True)
# Straw is the audit's ochre; muted rust is reserved for hazard/nautical markings,
# not the protagonist's #FF3131. Other roles retain the pilot's shared palette.
pilot.PALETTE.update(straw='D6B568',rust='C66B4E')
OLD_BUILD=pilot.build

def cone(name,p,r1,r2,h,c,n=24):
    bpy.ops.mesh.primitive_cone_add(vertices=n,radius1=r1,radius2=r2,depth=h,location=B(p))
    return pilot.finish(bpy.context.object,name,c)

def lathe(name,profile,p,c,axis='Y',n=32):
    """Closed faceted profile; (radius, axial position)."""
    verts=[];faces=[]
    for r,y in profile:
        for k in range(n):
            a=k*math.tau/n;v=(r*math.cos(a),y,r*math.sin(a))
            if axis=='Z':v=(v[0],v[2],v[1])
            verts.append(B(tuple(v[i]+p[i] for i in range(3))))
    for j in range(len(profile)-1):
        for k in range(n):faces.append((j*n+k,j*n+(k+1)%n,(j+1)*n+(k+1)%n,(j+1)*n+k))
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);return pilot.finish(o,name,c)

def transformed_group(fn,scale=(1,1,1),offset=(0,0,0)):
    start=len(pilot.OBJECTS);fn()
    # Apply only authored geometry; never a placement transform.
    for o in pilot.OBJECTS[start:]:
        bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o
        bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
        for v in o.data.vertices:v.co=B((v.co.x*scale[0]+offset[0],v.co.z*scale[1]+offset[1],v.co.y*scale[2]+offset[2]))

def solar_panel(x,y,z,w,h,d):
    box('Panel metal frame',(x,y,z),(w,h,d),'cream',.025)
    # Bifacial cells remain readable on the negative route's existing 180-degree yaw.
    for side in (-1,1):
        box('Blue photovoltaic face',(x,y,z+side*d*.53),(w*.94,h*.91,.025),'navy')
        for i in range(6):
            for j in range(3):
                xx=x+w*(-.39+i*.156);yy=y+h*(-.29+j*.29)
                box('Individual PV cell',(xx,yy,z+side*d*.56),(w*.143,h*.264,.02),'glass')
                box('Cell conductor',(xx,yy,z+side*d*.58),(w*.006,h*.24,.01),'cream')

def valve(x,y,z,s):
    cyl('Valve stem',(x,y,z),s*.11,s*.4,'gold')
    ring('Valve hand wheel',(x,y+s*.24,z),s*.30,s*.055,'rust')
    for a in (0,math.pi/2):
        tube('Valve spoke',[(x-math.cos(a)*s*.3,y+s*.24,z-math.sin(a)*s*.3),(x+math.cos(a)*s*.3,y+s*.24,z+math.sin(a)*s*.3)],s*.035,'rust')

def build(f,w,h,d):
    if f in ('delivery_van','market_stall'):
        OLD_BUILD(f,w,h,d)
        if f=='delivery_van':
            box('Opposite cab side window',(w*.34,h*.68,d*.448),(w*.23,h*.25,.04),'ink',.045)
            box('Opposite door handle',(w*.28,h*.47,d*.46),(w*.075,.045,.025),'ink')
            box('Opposite cargo door seam',(-w*.13,h*.55,d*.46),(.035,h*.63,.02),'wood')
            pilot.cup(-w*.21,h*.60,d*.47,h*.4,'ink')
    elif f=='crate_stack':
        for x in (-w*.25,w*.25):
            transformed_group(lambda:OLD_BUILD('market_crate',w*.48,h*.46,d*.95),offset=(x,0,0))
        transformed_group(lambda:OLD_BUILD('market_crate',w*.55,h*.46,d*.81),offset=(0,h*.51,0))
    elif f=='cargo_container_neon6':
        box('Container shell',(0,h*.5,0),(w,h,d*.92),'glass')
        for z in (-.47,.47):
            for k in range(13):box('Raised corrugation rib',(w*(-.46+k*.077),h*.51,d*z),(w*.035,h*.88,d*.055),'mint')
        for x in (-.48,.48):
            for z in (-.48,.48):box('Heavy corner casting',(w*x,h*.5,d*z),(w*.045,h,d*.045),'cream')
        # End double doors are visible from the 3/4 game view, plus bold front seam.
        for z in (-.23,.23):
            box('Separate cargo door',(w*.506,h*.5,d*z),(.025,h*.88,d*.43),'navy')
            tube('Vertical door locking bar',[(w*.525,h*.14,d*z),(w*.525,h*.86,d*z)],.035,'cream')
            box('Door handle',(w*.55,h*.43,d*(z+.06)),(.05,h*.035,d*.17),'gold')
        for y in (.06,.94):box('Container edge rail',(0,h*y,0),(w,h*.065,d),'cream')
        box('Freight label',(-w*.29,h*.68,-d*.51),(w*.15,h*.13,.015),'cream')
    elif f=='farm_fence_neon6':
        for x in (-.44,.44):
            box('Square fence post',(w*x,h*.49,0),(w*.095,h*.98,d*.8),'wood')
            cone('Pointed post cap',(w*x,h*.96,0),w*.073,0,h*.09,'cream',4)
        for y in (.32,.70):box('Fence cross rail',(0,h*y,-d*.09),(w,h*.13,d*.60),'straw')
        tube('Diagonal timber brace',[(-w*.40,h*.24,-d*.43),(w*.40,h*.78,-d*.43)],h*.055,'cream')
    elif f=='hay_roll_neon6':
        r=min(w,h)*.48;cyl('Golden straw bale',(0,h*.5,0),r,d*.96,'straw','Z',32)
        for z in (-.49,.49):
            cyl('Rolled straw end',(0,h*.5,d*z),r*.97,.02,'cream','Z',32)
            pts=[]
            for k in range(120):
                a=k/119*math.tau*3.7;rr=r*.91*k/119
                pts.append((math.cos(a)*rr,h*.5+math.sin(a)*rr,d*z*1.025))
            tube('Visible end-grain spiral',pts,r*.033,'wood')
        for z in (-d*.28,d*.28):ring('Two binding straps',(0,h*.5,z),r*1.005,r*.038,'navy','Z')
        for k in range(20):
            a=k*math.tau/20;tube('Long straw striation',[(r*math.cos(a),h*.5+r*math.sin(a),-d*.41),(r*math.cos(a+.02),h*.5+r*math.sin(a+.02),d*.41)],r*.009,'gold')
    elif f=='dish_antenna_neon6':
        box('Antenna foundation',(0,h*.07,.12*d),(w*.60,h*.14,d*.65),'navy')
        box('Dish pedestal',(0,h*.37,.17*d),(w*.16,h*.62,d*.24),'cream')
        r=w*.48;cy=h*.65
        # An actual concave dish, not radial ribs on a disc. Mouth faces -Z.
        lathe('Parabolic receiving bowl',[(0,d*.25),(r*.23,d*.21),(r*.5,d*.08),(r*.75,-d*.12),(r,-d*.38),(r,-d*.32),(r*.75,-d*.05),(r*.5,d*.14),(r*.23,d*.27),(0,d*.31)],(0,cy,0),'cream','Z')
        ring('Dish rolled rim',(0,cy,-d*.35),r,r*.035,'glass','Z')
        tube('Single receiver arm',[(0,cy-r*.76,-d*.19),(0,cy-r*.16,-d*.53),(0,cy,-d*.57)],r*.048,'navy')
        cyl('Feed receiver',(0,cy,-d*.59),r*.11,d*.17,'gold','Z')
        cyl('Tilt hinge',(0,cy-r*.35,d*.28),r*.13,w*.45,'gold','X')
    elif f=='harbor_buoy_neon6':
        r=min(w,d)*.46
        lathe('Buoyant flotation body',[(0,0),(r*.72,0),(r,h*.13),(r,h*.25),(r*.72,h*.35),(0,h*.35)],(0,0,0),'rust')
        ring('White waterline band',(0,h*.23,0),r*.99,r*.055,'cream')
        cone('White tapered beacon',(0,h*.49,0),r*.68,r*.26,h*.34,'cream')
        cone('Rust nautical stripe',(0,h*.49,0),r*.51,r*.40,h*.09,'rust')
        for x in (-.19,.19):tube('Beacon cage',[(r*x,h*.63,0),(r*x,h*.89,0)],r*.045,'navy')
        cyl('Beacon light',(0,h*.81,0),r*.19,h*.15,'gold')
        cone('Lantern roof',(0,h*.92,0),r*.32,0,h*.12,'navy')
        ring('Mooring eye',(r*.70,h*.34,0),r*.15,r*.04,'navy','Z')
    elif f=='irrigation_tank_r12':
        r=min(w*.36,d*.43)
        cyl('Water storage cylinder',(-w*.08,h*.48,0),r,h*.73,'mint')
        ell('Domed tank roof',(-w*.08,h*.84,0),(r*2,h*.26,r*2),'cream')
        for yy in (.17,.73):ring('Tank reinforcement hoop',(-w*.08,h*yy,0),r*1.01,r*.045,'glass')
        for xx in (-.28,.15):box('Tank concrete foot',(w*xx,h*.06,0),(w*.18,h*.12,d*.75),'navy')
        cyl('Filling inlet',(-w*.08,h*.96,0),r*.20,h*.10,'gold')
        box('Pump cabinet',(w*.35,h*.22,-d*.04),(w*.23,h*.35,d*.48),'cream',.04)
        tube('Outlet with hanging hose',[(w*.08,h*.35,-d*.40),(w*.29,h*.35,-d*.45),(w*.40,h*.23,-d*.47),(w*.38,h*.06,-d*.48),(w*.24,h*.055,-d*.48),(w*.20,h*.16,-d*.47)],r*.055,'navy')
        valve(w*.20,h*.38,-d*.43,w*.18)
        box('Water level gauge',(-w*.18,h*.48,-r*1.015),(w*.035,h*.42,.03),'cream')
    elif f=='propellant_tank_neon6':
        r=min(w,d)*.43
        cyl('Pressure vessel',(0,h*.41,0),r,h*.68,'cream')
        ell('Rounded pressure shoulder',(0,h*.74,0),(2*r,h*.29,2*r),'glass')
        for y in (.24,.62):cyl('Hazard band',(0,h*y,0),r*1.015,h*.085,'gold')
        for y in (.24,.62):
            for x in (-.19,0,.19):panel('Hazard slash',[(w*(x-.05),h*(y-.04),-r*1.025),(w*(x+.01),h*(y-.04),-r*1.025),(w*(x+.10),h*(y+.04),-r*1.025),(w*(x+.04),h*(y+.04),-r*1.025)],'navy')
        ring('Base skid',(0,h*.06,0),r*1.02,r*.09,'navy');valve(0,h*.92,0,w*.37)
        cyl('Pressure gauge body',(w*.24,h*.88,-d*.07),w*.10,.09,'navy','Z')
        cyl('Pressure gauge dial',(w*.24,h*.88,-d*.12),w*.08,.015,'cream','Z')
        tube('Pressure gauge pointer',[(w*.24,h*.88,-d*.135),(w*.27,h*.92,-d*.135)],.012,'ink')
    elif f=='oxygen_tank_neon6':
        box('Cylinder rack foot',(0,h*.035,0),(w,h*.07,d),'navy')
        for x in (-w*.24,w*.24):
            r=w*.21;cyl('Oxygen bottle',(x,h*.40,0),r,h*.70,'mint')
            ell('Bottle shoulder',(x,h*.74,0),(r*2,h*.18,r*2),'mint')
            cyl('Bottle neck',(x,h*.84,0),r*.33,h*.13,'cream');valve(x,h*.92,0,r*.85)
            box('Oxygen label',(x,h*.48,-r*1.02),(r*1.1,h*.16,.018),'cream')
            ring('Oxygen O marking',(x,h*.48,-r*1.05),r*.24,r*.045,'navy','Z')
        for y in (.23,.62):box('Bottle harness',(0,h*y,-d*.40),(w*.94,h*.055,d*.10),'navy')
    elif f=='junction_cabinet':
        box('Concrete plinth',(0,h*.055,0),(w,h*.11,d),'navy')
        box('Weatherproof electrical cabinet',(0,h*.55,0),(w*.94,h*.88,d*.89),'glass',.035)
        box('Separate hinged door',(0,h*.55,-d*.465),(w*.84,h*.76,.04),'cream',.015)
        for k in range(5):box('Vent slot',(0,h*(.31+k*.052),-d*.491),(w*.60,h*.023,.018),'navy')
        box('Cabinet handle',(w*.29,h*.65,-d*.50),(w*.035,h*.11,.045),'navy')
        for y in (.25,.8):box('Visible door hinge',(-w*.43,h*y,-d*.48),(w*.045,h*.06,.04),'gold')
        panel('Electrical hazard triangle',[(-w*.17,h*.66,-d*.5),(w*.17,h*.66,-d*.5),(0,h*.86,-d*.5)],'gold')
        tube('Lightning bolt',[(w*.04,h*.81,-d*.515),(-w*.04,h*.75,-d*.515),(w*.04,h*.75,-d*.515),(-w*.03,h*.70,-d*.515)],.018,'navy')
        box('Rear service panel',(0,h*.55,d*.465),(w*.84,h*.76,.04),'cream',.015)
        for k in range(5):box('Rear ventilation slot',(0,h*(.31+k*.052),d*.491),(w*.60,h*.023,.018),'navy')
        box('Rear service latch',(w*.29,h*.65,d*.50),(w*.035,h*.11,.045),'navy')
        panel('Rear hazard triangle',[(-w*.17,h*.66,d*.5),(w*.17,h*.86,d*.5),(w*.17,h*.66,d*.5)],'gold')
    elif f=='flywheel_accumulator_r12':
        r=min(w*.46,h*.29)
        box('Accumulator plinth',(0,h*.045,0),(w,h*.09,d*.95),'navy')
        for x in (-.39,.39):box('Bearing upright',(w*x,h*.44,d*.15),(w*.11,h*.80,d*.37),'cream')
        # Axle, open hub and heavy wheel make the spring accumulator unlike plates.
        cy=h*.52
        cyl('Spring drum',(0,cy,d*.03),r*.73,d*.70,'wood','Z')
        for z in [d*(-.28+i*.075) for i in range(9)]:ring('Coiled energy spring',(0,cy,z),r*.76,r*.047,'gold','Z')
        for z in (-d*.40,d*.40):
            ring('Heavy flywheel rim',(0,cy,z),r,r*.11,'gold','Z')
            ring('Open axle bearing',(0,cy,z),r*.22,r*.065,'cream','Z')
            for k in range(6):
                a=k*math.tau/6;tube('Flywheel spoke',[(r*.26*math.cos(a),cy+r*.26*math.sin(a),z),(r*.89*math.cos(a),cy+r*.89*math.sin(a),z)],r*.055,'navy')
        box('Accumulator head',(0,h*.94,d*.16),(w*.76,h*.12,d*.66),'cream',.04)
        tube('Charge arrow',[(-w*.20,h*.94,-d*.19),(w*.19,h*.94,-d*.19),(w*.10,h*.98,-d*.19)],.035,'navy')
    elif f=='road_sign_neon6':
        box('Sign foundation',(0,h*.035,0),(w*.35,h*.07,d),'navy')
        box('Sign support',(0,h*.45,d*.14),(w*.07,h*.9,d*.20),'cream')
        box('Road sign border',(0,h*.78,0),(w,h*.37,d*.45),'cream',.045)
        box('Road sign blue face',(0,h*.78,-d*.25),(w*.93,h*.31,.025),'navy')
        z=-d*.28
        panel('Single direction arrow',[(-w*.36,h*.74,z),(w*.14,h*.74,z),(w*.14,h*.69,z),(w*.37,h*.78,z),(w*.14,h*.87,z),(w*.14,h*.82,z),(-w*.36,h*.82,z)],'cream')
    elif f=='traffic_signal_neon6':
        box('Traffic signal base',(0,h*.035,0),(w*.82,h*.07,d*.80),'navy')
        cyl('Signal pole',(0,h*.35,d*.15),w*.09,h*.70,'cream')
        box('Three lamp housing',(0,h*.76,0),(w*.92,h*.47,d*.61),'navy',.05)
        for y,c in [(.9,'rust'),(.76,'gold'),(.62,'mint')]:
            r=w*.31;cyl('Colour signal lens',(0,h*y,-d*.33),r,d*.045,c,'Z')
            # Open dark visors frame each circular lens without covering its colour.
            tube('Lamp sun visor',[(r*math.cos(k*math.pi/16),h*y+r*math.sin(k*math.pi/16),-d*.44)for k in range(17)],r*.14,'ink')
        box('Housing cap',(0,h*.995,0),(w,h*.035,d*.69),'cream')
    elif f in ('solar_array_r12','satellite_neon6'):
        satellite=f=='satellite_neon6'
        cy=h*(.52 if satellite else .69)
        if not satellite:
            box('Solar support foot',(0,h*.035,0),(w*.23,h*.07,d),'navy')
            box('Solar mast',(0,h*.38,d*.16),(w*.055,h*.76,d*.25),'cream')
        box('Central instrument bus',(0,cy,0),(w*.23,h*.43,d*.75),'gold',.04)
        for x in (-w*.32,w*.32):solar_panel(x,cy,0,w*.37,h*(.42 if satellite else .52),d*.15)
        box('Instrument face',(0,cy,-d*.40),(w*.18,h*.29,.03),'cream')
        ring('Central instrument aperture',(0,cy,-d*.43),min(w*.06,h*.09),.025,'navy','Z')
        if satellite:
            tube('Satellite telemetry mast',[(0,cy+h*.21,0),(0,h*.98,0)],.035,'cream')
            ell('Telemetry tip',(0,h*.99,0),(.10,.10,.10),'gold')
    elif f=='port_lattice_loader_r10':
        for x in (-w*.43,w*.43):
            box('Loader footplate',(x,h*.035,0),(w*.20,h*.07,d),'navy')
            for dx in (-w*.025,w*.025):box('Lattice tower chord',(x+dx,h*.5,0),(w*.018,h*.94,d*.24),'gold')
            for k in range(6):
                y=h*(.10+k*.14)
                tube('Cross-braced tower',[(x-w*.04,y,-d*.18),(x+w*.04,y+h*.13,-d*.18)],.038,'cream')
                tube('Tower cross brace',[(x+w*.04,y,-d*.18),(x-w*.04,y+h*.13,-d*.18)],.038,'cream')
        for y in (.87,.97):box('Gantry truss beam',(0,h*y,0),(w,h*.035,d*.40),'gold')
        for k in range(8):tube('Gantry diagonal',[(-w*.45+k*w*.112,h*.88,-d*.22),(-w*.34+k*w*.112,h*.96,-d*.22)],.04,'cream')
        box('Hoist trolley',(w*.10,h*.86,0),(w*.18,h*.10,d*.70),'navy')
        tube('Lifting cable',[(w*.10,h*.82,0),(w*.10,h*.49,0)],.035,'cream')
        tube('Cargo hook',[(w*.10,h*.49,0),(w*.10,h*.42,0),(w*.14,h*.39,0),(w*.18,h*.42,0),(w*.18,h*.46,0)],.065,'gold')
    elif f=='rail_glass_planter_display_r10':
        box('Display plinth',(0,h*.04,0),(w,h*.08,d),'navy')
        for x in (-w*.47,w*.47):
            for z in (-d*.44,d*.44):box('Glazed case frame',(x,h*.51,z),(w*.027,h*.98,d*.055),'cream')
        for y in (.13,.51,.98):box('Display shelf',(0,h*y,0),(w,h*.035,d),'cream')
        for y in (.16,.54):
            for x in (-w*.27,w*.27):
                r=w*.10;cone('Flower pot',(x,h*(y+.095),0),r*.75,r,h*.18,'wood')
                cyl('Pot rim',(x,h*(y+.18),0),r*1.06,h*.04,'gold')
                cyl('Visible soil',(x,h*(y+.205),0),r*.9,.015,'navy')
                for k in range(5):
                    a=k*math.tau/5;xx=x+math.cos(a)*r*1.05;zz=math.sin(a)*r*.8
                    tube('Plant stem',[(x,h*(y+.2),0),(xx,h*(y+.36),zz)],.02,'mint')
                    o=ell('Large readable leaf',(xx,h*(y+.34),zz),(r*1.15,h*.20,r*.45),'mint');o.rotation_euler[1]=a
        # Empty interior and sparse diagonal highlights communicate glass without alpha.
        for x in (-w*.27,w*.27):
            tube('Glass reflection stroke',[(x-w*.08,h*.87,-d*.48),(x+w*.03,h*.70,-d*.48)],.027,'white')
    else:
        raise RuntimeError('No rollout model for '+f)

FEATURES={
 'cargo_container_neon6':'波板の縦リブ、両開き扉、扉線、ロック棒と取っ手、角金具',
 'crate_stack':'独立した木箱3個、板の隙間、斜め補強、上下の積み重なり',
 'delivery_van':'荷室と運転席、窓、4輪、ドア取手、カップ印（pilotと共通）',
 'dish_antenna_neon6':'本物の凹面のお椀、縁、中心受信棒と受信部、傾斜ヒンジ',
 'farm_fence_neon6':'尖った支柱、横木2段、斜めの木の補強',
 'flywheel_accumulator_r12':'開いた軸穴、スポーク付き重い輪、巻きばね、軸受支柱（蓄力機の家族意味を保持）',
 'harbor_buoy_neon6':'浮体、錆朱と白の横縞、上部の小ライトとケージ、係留環',
 'hay_roll_neon6':'両端面の太い渦巻き、黄土の藁色、結束バンド2本、藁の筋',
 'irrigation_tank_r12':'給水口、給水タンクのドーム、ポンプ、垂れたホース、水位計',
 'junction_cabinet':'扉とヒンジ、通気口、取手、稲妻入り電気警告表示',
 'market_stall':'縞の日除け、柱、商品トレイ、カウンター（pilotと共通）',
 'oxygen_tank_neon6':'2本の肩付きボンベ、細い首、手回しバルブ、固定帯、O表示',
 'port_lattice_loader_r10':'トラスの門形クレーン、斜材、巻上げ台車、ケーブルとフック',
 'propellant_tank_neon6':'丸い肩、手回しバルブ、圧力計、黄と藍の警告帯',
 'rail_glass_planter_display_r10':'ガラス枠、斜めの反射線、鉢、土、大きな葉、2段棚',
 'road_sign_neon6':'道路標識の枠と支柱、意味のある大きな方向矢印1個',
 'satellite_neon6':'金色の機器本体、両翼の太陽電池格子、通信アンテナ',
 'solar_array_r12':'支柱、左右のパネル、太陽電池セルと導体線',
 'traffic_signal_neon6':'3色の丸い灯器、各灯の庇、縦長の筐体と支柱'
}
pilot.build=build

if __name__=='__main__':
    selected=sys.argv[sys.argv.index('--')+1:]if'--'in sys.argv else []
    new=[f for f in DATA if not DATA[f]['pilotModel']]
    if selected and any(f not in new for f in selected):raise ValueError('Select a new rollout family; pilot originals are protected')
    stats=[]
    for f in new:
        if selected and f not in selected:continue
        print('MODELLING',f,flush=True)
        info=pilot.export_family(f,DATA[f]);info['features']=FEATURES[f];stats.append(info)
    manifest=pilot.EVIDENCE.parent/'blender-manifest.json'
    if selected and manifest.exists():
        stats=[r for r in json.loads(manifest.read_text())['models']if r['family']not in selected]+stats
    manifest.write_text(json.dumps(dict(blender=bpy.app.version_string,models=stats,palette=pilot.PALETTE),ensure_ascii=False,indent=2)+'\n')
    print('BLENDER_ROLLOUT_COMPLETE',len(stats),flush=True)
