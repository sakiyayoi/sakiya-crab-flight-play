"""Blender 4.2 LTS: original mall models, FBX exports, native render proofs.
Run: Blender --background --factory-startup --python source/tools/blender/build_mall.py
Coordinates in author helpers: Unity metres (X/right, Y/up, Z/back).
The saved Blender scene uses Z/up, front=-Y, bottom-centre origin and real metre units.
Unity import was verified to map Blender (x,y,z) to Unity (x,z,y).
"""
import bpy, math, json, pathlib, sys
from mathutils import Vector
HERE=pathlib.Path(__file__).resolve().parent
SOURCE=HERE.parents[1]; ROOT=SOURCE.parent
OUT=SOURCE/'games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels'
EVIDENCE=ROOT/'evidence/blender-pilot'; OUT.mkdir(parents=True,exist_ok=True);EVIDENCE.mkdir(parents=True,exist_ok=True)
DATA=json.loads((HERE/'families.json').read_text())['families']
# Existing R5 cream/mint/lemon, existing dark recess, and the brief's night indigo.
PALETTE={'ink':'173942','navy':'2A1650','cream':'FAE6B5','mint':'66C4A3','gold':'FABA29','wood':'955C36','glass':'60959E','white':'FFF8E3'}
MATS={};OBJECTS=[]
def linear(v):return v/12.92 if v<=.04045 else ((v+.055)/1.055)**2.4
def mat(name):
 if name in MATS:return MATS[name]
 c=tuple(linear(int(PALETTE[name][i:i+2],16)/255)for i in (0,2,4))+(1,)
 m=bpy.data.materials.new(name);m.diffuse_color=c;m.use_nodes=True;n=m.node_tree.nodes;n.clear();l=m.node_tree.links
 out=n.new('ShaderNodeOutputMaterial');em=n.new('ShaderNodeEmission');geo=n.new('ShaderNodeNewGeometry');dot=n.new('ShaderNodeVectorMath');dot.operation='DOT_PRODUCT';dot.inputs[1].default_value=(-.37,-.56,.74)
 step=n.new('ShaderNodeMath');step.operation='GREATER_THAN';step.inputs[1].default_value=.05
 remap=n.new('ShaderNodeMath');remap.operation='MULTIPLY_ADD';remap.inputs[1].default_value=.35;remap.inputs[2].default_value=.65
 rgb=n.new('ShaderNodeRGB');rgb.outputs[0].default_value=c;mul=n.new('ShaderNodeMixRGB');mul.blend_type='MULTIPLY';mul.inputs[0].default_value=1
 l.new(geo.outputs['Normal'],dot.inputs[0]);l.new(dot.outputs['Value'],step.inputs[0]);l.new(step.outputs[0],remap.inputs[0]);l.new(rgb.outputs[0],mul.inputs[1]);l.new(remap.outputs[0],mul.inputs[2]);l.new(mul.outputs[0],em.inputs['Color']);l.new(em.outputs[0],out.inputs[0]);MATS[name]=m;return m
def B(p):return Vector((p[0],p[2],p[1]))
def finish(o,name,color):
 o.name=name;o.data.materials.append(mat(color));OBJECTS.append(o);return o
def box(name,p,size,c,bevel=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=B(p));o=bpy.context.object;o.scale=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new('Small crafted edges','BEVEL');mod.width=bevel;mod.segments=1;bpy.ops.object.modifier_apply(modifier=mod.name)
 return finish(o,name,c)
def ell(name,p,size,c):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=10,radius=1,location=B(p));o=bpy.context.object;o.scale=(size[0]/2,size[2]/2,size[1]/2);return finish(o,name,c)
def cyl(name,p,r,h,c,axis='Y',n=24):
 bpy.ops.mesh.primitive_cylinder_add(vertices=n,radius=r,depth=h,location=B(p));o=bpy.context.object
 if axis=='Z':o.rotation_euler[0]=math.pi/2
 if axis=='X':o.rotation_euler[1]=math.pi/2
 return finish(o,name,c)
def ring(name,p,r,t,c,axis='Y'):
 bpy.ops.mesh.primitive_torus_add(major_segments=24,minor_segments=6,location=B(p),major_radius=r,minor_radius=t);o=bpy.context.object
 if axis=='Z':o.rotation_euler[0]=math.pi/2
 if axis=='X':o.rotation_euler[1]=math.pi/2
 return finish(o,name,c)
def tube(name,points,r,c):
 curve=bpy.data.curves.new(name,'CURVE');curve.dimensions='3D';curve.resolution_u=1;curve.bevel_depth=r;curve.bevel_resolution=1
 sp=curve.splines.new('POLY');sp.points.add(len(points)-1)
 for a,p in zip(sp.points,points):a.co=(*B(p),1)
 o=bpy.data.objects.new(name,curve);bpy.context.collection.objects.link(o);bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o=bpy.context.object;return finish(o,name,c)
def panel(name,points,c):
 mesh=bpy.data.meshes.new(name);mesh.from_pydata([B(p)for p in points],[],[list(range(len(points)))]);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);return finish(o,name,c)
def cup(x,y,z,s,c='cream'):
 box('Cup silhouette',(x,y,z),(s*.65,s*.55,.025),c,s*.06)
 ring('Cup handle',(x+s*.37,y+s*.05,z),s*.19,s*.055,c,'Z')
 box('Saucer',(x,y-s*.34,z),(s,.07*s,.03),'gold')
 tube('Steam',[(x-.12*s,y+.35*s,z),(x-.18*s,y+.52*s,z),(x-.08*s,y+.68*s,z)],.035*s,c)
def bear(x,y,z,s,c='mint'):
 ell('Soft belly',(x,y+.44*s,z),(.65*s,.82*s,.49*s),c);ell('Round head',(x,y+.98*s,z-.04*s),(.68*s,.6*s,.52*s),c)
 for side in (-1,1):
  ell('Round ear',(x+side*.26*s,y+1.24*s,z),(.26*s,.26*s,.22*s),c)
  ell('Inner ear',(x+side*.26*s,y+1.24*s,z-.12*s),(.13*s,.13*s,.025*s),'cream')
  ell('Paw',(x+side*.21*s,y+.12*s,z-.16*s),(.3*s,.24*s,.4*s),'cream')
  ell('Arm',(x+side*.39*s,y+.51*s,z),(.25*s,.47*s,.27*s),c)
  ell('Eye',(x+side*.14*s,y+1.01*s,z-.294*s),(.065*s,.075*s,.035*s),'ink')
 ell('Muzzle',(x,y+.86*s,z-.30*s),(.3*s,.21*s,.10*s),'cream');ell('Nose',(x,y+.92*s,z-.36*s),(.09*s,.07*s,.05*s),'ink')
 box('Neck ribbon',(x,y+.7*s,z-.21*s),(.38*s,.12*s,.13*s),'gold')
def pop(x,y,z,s,c='gold'):
 cyl('Candy stick',(x,y+.42*s,z),.035*s,.84*s,'cream')
 cyl('Candy disc',(x,y+1.02*s,z),.33*s,.17*s,c,'Z',32)
 pts=[]
 for k in range(89):
  t=k/88;ang=t*math.pi*4.7;r=.28*s*t;pts.append((x+math.cos(ang)*r,y+1.02*s+math.sin(ang)*r,z-.098*s))
 tube('Raised candy spiral',pts,.033*s,'cream')
def capsule_machine(x,y,z,w,h,d):
 box('Machine body',(x,y+h*.47,z),(w,h*.94,d),'mint',w*.06)
 box('Glazed display recess',(x,y+h*.66,z-d*.51),(w*.84,h*.42,.055),'ink',w*.07)
 for ix in range(3):
  for iy in range(2):
   xx=x+(ix-1)*w*.245;yy=y+h*(.56+iy*.17);zz=z-d*.55
   ell('Two-tone capsule',(xx,yy,zz),(w*.22,h*.14,d*.16),'gold'if(ix+iy)%2 else'cream')
   box('Capsule seam',(xx,yy,zz-d*.084),(w*.21,h*.018,.018),'navy')
 tube('Glass reflection',[(x-w*.32,y+h*.81,z-d*.65),(x-w*.21,y+h*.72,z-d*.65)],w*.022,'white')
 cyl('Turn knob disc',(x,y+h*.31,z-d*.55),w*.14,d*.12,'cream','Z');box('Turn knob grip',(x,y+h*.31,z-d*.63),(w*.22,h*.035,.055),'ink',.015)
 box('Coin slot',(x+w*.28,y+h*.32,z-d*.53),(w*.07,h*.1,.03),'ink')
 box('Prize hatch',(x,y+h*.12,z-d*.53),(w*.62,h*.14,.06),'ink',.035)
 box('Roof cap',(x,y+h*.97,z),(w*1.03,h*.06,d*1.02),'cream',.025)
def build(f,w,h,d):
 if f in ('steel_drum','mall_r5_density_can'):
  r=min(w,d)*.47;cyl('Rolled steel body',(0,h*.5,0),r,h*.94,'glass')
  for yy in (.08,.32,.72,.95):ring('Contrasting rolled rib',(0,h*yy,0),r,.035*min(w,d),'cream'if yy in(.32,.72)else'ink')
  cyl('Inset lid',(0,h*.97,0),r*.88,.025*h,'navy');ring('Bung lip',(.2*w,h*.987,.12*d),w*.09,w*.025,'gold')
 elif 'crate' in f:
  for xx in (-.43,.43):
   for zz in (-.42,.42):box('Corner timber',(w*xx,h*.5,d*zz),(w*.11,h,d*.13),'wood')
  for k in range(4):
   yy=h*(.13+k*.24)
   for zz in (-.45,.45):box('Separate side plank',(0,yy,d*zz),(w,h*.19,d*.09),'gold'if k==3 else'wood')
   for xx in (-.45,.45):box('End plank',(w*xx,yy,0),(w*.09,h*.19,d),'wood')
  for k in range(5):box('Base slat',(w*(-.4+k*.2),h*.04,0),(w*.18,h*.08,d),'wood')
  tube('Diagonal shipping brace',[(-w*.42,h*.12,-d*.51),(w*.42,h*.87,-d*.51)],min(w,h)*.055,'cream')
  box('Shipping label',(w*.2,h*.69,-d*.565),(w*.2,h*.15,.015),'cream')
 elif 'vending' in f:
  box('Vending cabinet',(0,h*.5,0),(w,h,d),'navy',w*.035);box('Glass display',(w*-.08,h*.64,-d*.51),(w*.74,h*.51,.03),'ink')
  for y in (.47,.63,.79):
   box('Product shelf',(-w*.08,h*(y-.055),-d*.55),(w*.69,.035*h,.12*d),'cream')
   for x in (-.31,-.09,.13):
    cyl('Bottle',(w*x,h*y,-d*.56),w*.063,h*.105,'gold'if x==-.09 else'mint');cyl('Bottle cap',(w*x,h*(y+.06),-d*.56),w*.035,h*.02,'cream')
    cyl('Selection button',(w*x,h*(y-.09),-d*.57),w*.025,.02,'cream','Z')
  box('Dispensing hatch',(0,h*.17,-d*.52),(w*.7,h*.15,.035),'ink',.04);box('Return lip',(0,h*.09,-d*.56),(w*.73,h*.035,.05),'cream');box('Payment column',(w*.36,h*.55,-d*.53),(w*.16,h*.34,.06),'gold');box('Coin aperture',(w*.36,h*.61,-d*.57),(.025,h*.07,.02),'ink')
  tube('Glass reflection',[(-w*.39,h*.84,-d*.59),(-w*.27,h*.76,-d*.59)],.025,'white')
 elif f=='street_bench':
  for xx in (-.36,.36):
   box('Cast foot',(w*xx,h*.21,0),(w*.10,h*.42,d*.78),'ink');box('Back support',(w*xx,h*.63,d*.32),(w*.06,h*.74,d*.09),'ink')
  for z in (-.3,0,.3):box('Seat plank',(0,h*.45,d*z),(w,h*.10,d*.23),'wood',.018)
  for y in (.68,.86):box('Back plank',(0,h*y,d*.35),(w,h*.15,d*.14),'cream',.015)
  for xx in (-.46,.46):tube('Arm rest',[(w*xx,h*.48,-d*.3),(w*xx,h*.67,-d*.3),(w*xx,h*.67,d*.27)],.045,'ink')
 elif f=='hvac_unit':
  box('Condenser case',(0,h*.55,0),(w,h*.83,d*.9),'cream',.045)
  for xx in (-.35,.35):box('Floor mounting foot',(w*xx,h*.05,0),(w*.22,h*.1,d),'ink')
  r=h*.31;ring('Circular fan guard',(-w*.13,h*.56,-d*.47),r,.045,'ink','Z');cyl('Fan dark recess',(-w*.13,h*.56,-d*.46),r*.96,.025,'navy','Z')
  for k in range(5):
   a=k*math.tau/5;pts=[(-w*.13,h*.56,-d*.49),(-w*.13+math.cos(a)*r*.85,h*.56+math.sin(a)*r*.85,-d*.49),(-w*.13+math.cos(a+.6)*r*.6,h*.56+math.sin(a+.6)*r*.6,-d*.49)];panel('Fan blade',pts,'glass')
  for k in range(5):box('Vent grille',(w*.33,h*(.3+k*.12),-d*.47),(w*.19,h*.035,.03),'ink')
 elif f=='road_barrier':
  for xx in (-.39,.39):box('Broad foot',(w*xx,h*.05,0),(w*.22,h*.1,d),'ink');box('Upright',(w*xx,h*.48,0),(w*.07,h*.87,d*.15),'cream')
  box('Barrier beam',(0,h*.67,0),(w,h*.42,d*.24),'gold')
  for k in range(5):
   x=w*(-.47+k*.19);panel('Hazard stripe',[(x,h*.46,-d*.126),(x+w*.09,h*.46,-d*.126),(x+w*.21,h*.88,-d*.126),(x+w*.12,h*.88,-d*.126)],'ink')
 elif f=='facade_break_panel':
  box('Ceramic sign backing',(0,h*.5,0),(w,h,d*.8),'navy')
  for x in (-.33,0,.33):
   for y in (.25,.75):box('Separated ceramic tile',(w*x,h*y,-d*.43),(w*.31,h*.46,d*.08),'cream',.02)
  cup(0,h*.49,-d*.49,h*.55,'ink')
 elif 'sign' in f:
  vertical='vertical' in f;hanging='hanging' in f
  if not hanging:
   box('Sign post',(-w*.37,h*.5,d*.24),(w*.10,h,d*.25),'ink')
   for yy in (.43,.9):box('Sign bracket',(-w*.06,h*yy,d*.2),(w*.72,h*.045,d*.18),'gold')
  else:
   for xx in (-.36,.36):box('Hanger',(w*xx,h*.88,d*.2),(w*.035,h*.24,d*.12),'gold')
  sy=h*.76 if vertical else h*(.68 if not hanging else .72);cy=h*.55 if not hanging else h*.4
  box('Sign rim',(w*.06,cy,0),(w*.84,sy,d*.76),'gold',min(w,h)*.04);box('Indigo sign face',(w*.06,cy,-d*.4),(w*.73,sy*.91,d*.04),'navy')
  for y in ([cy-sy*.3,cy,cy+sy*.3]if vertical else[cy]):cup(w*.04,y,-d*.44,min(w*.66,sy*.62))
 elif f=='street_awning' or 'stall' in f:
  stall='stall'in f
  for xx in (-.44,.44):
   for zz in (-.36,.36):box('Canopy post',(w*xx,h*.46,d*zz),(w*.055,h*.92,d*.06),'ink')
  for k in range(7):
   x=w*(-.428+k*.143);roof=box('Striped canopy',(x,h*.92,0),(w*.145,h*.10,d),'cream'if k%2 else'mint');roof.rotation_euler[0]=-.13
   box('Scalloped valance',(x,h*.82,-d*.47),(w*.145,h*.12,d*.06),'cream'if k%2 else'mint')
  if stall:
   box('Counter',(0,h*.32,0),(w*.95,h*.53,d*.85),'wood');box('Countertop',(0,h*.61,0),(w,h*.055,d),'cream')
   for k in range(3):box('Produce tray',(w*(-.3+k*.3),h*.65,0),(w*.25,h*.06,d*.6),'navy');ell('Produce',(w*(-.3+k*.3),h*.72,0),(w*.20,h*.14,d*.27),'gold')
   cup(0,h*.32,-d*.44,h*.36)
 elif 'van' in f:
  box('Cargo body',(-w*.12,h*.58,0),(w*.72,h*.77,d*.9),'cream',.10);box('Cab',(w*.34,h*.51,0),(w*.31,h*.62,d*.87),'mint',.10)
  box('Cab side window',(w*.34,h*.68,-d*.448),(w*.23,h*.25,.04),'ink',.045);box('Windscreen',(w*.497,h*.68,0),(.035,h*.25,d*.68),'ink')
  for xx in (-.30,.32):
   for zz in (-.44,.44):cyl('Rubber wheel',(w*xx,h*.18,d*zz),h*.18,d*.12,'ink','Z');cyl('Wheel hub',(w*xx,h*.18,d*zz*1.15),h*.09,d*.02,'gold','Z')
  box('Door handle',(w*.28,h*.47,-d*.46),(w*.075,.045,.025),'ink');box('Cargo door seam',(-w*.13,h*.55,-d*.46),(.035,h*.63,.02),'wood');cup(-w*.21,h*.60,-d*.47,h*.4,'ink')
 elif f=='mall_r5_gift_small':
  box('Gift plinth',(0,h*.06,0),(w,h*.12,d),'navy',.04);bear(0,h*.12,0,min(w/1.02,h*.63,d/0.75),'mint')
 elif f=='mall_r5_gift_large':
  box('Display plinth',(0,h*.05,0),(w,h*.10,d),'navy',.04)
  for xx in (-.47,.47):box('Shelf side',(w*xx,h*.5,d*.25),(w*.06,h,d*.5),'navy')
  box('Shelf backing',(0,h*.51,d*.44),(w*.91,h*.94,d*.10),'glass')
  for yy in (.14,.52):
   box('Shelf',(0,h*yy,0),(w,h*.035,d),'cream');bear(-w*.25,h*yy,0,h*.24,'mint');bear(w*.04,h*yy,-d*.03,h*.21,'cream')
   box('Wrapped present',(w*.32,h*(yy+.12),0),(w*.20,h*.24,d*.6),'gold',.025);box('Present ribbon',(w*.32,h*(yy+.12),-d*.31),(w*.04,h*.24,.02),'navy')
  box('Gift header',(0,h*.95,0),(w*.9,h*.1,d*.65),'gold');bear(0,h*.88,-d*.4,h*.075,'ink')
 elif f=='mall_r5_sweets_small':
  box('Candy stand',(0,h*.045,0),(w,h*.09,d*.75),'mint',.04);pop(0,h*.09,0,min(h*.65,w/0.7),'gold')
 elif f=='mall_r5_sweets_large':
  box('Pastry counter',(0,h*.10,0),(w,h*.20,d),'mint',.05)
  for xx in (-.47,.47):box('Glass case frame',(w*xx,h*.40,d*.38),(w*.035,h*.42,d*.04),'cream')
  box('Display shelf',(0,h*.3,0),(w*.96,h*.04,d*.95),'cream');box('Case lid',(0,h*.57,d*.05),(w,h*.04,d*.9),'cream')
  for k in range(6):
   x=w*(-.39+k*.155);y=h*.39;r=w*.066
   cyl('Macaron thick cream filling',(x,y,0),r,.07*h,'cream');ell('Macaron top shell',(x,y+.055*h,0),(r*2,.10*h,r*2),'gold'if k%2 else'mint');ell('Macaron bottom shell',(x,y-.055*h,0),(r*2,.10*h,r*2),'gold'if k%2 else'mint')
  for xx in (-.30,.30):pop(w*xx,h*.59,d*.05,h*.27,'gold'if xx<0 else'mint')
  tube('Glass diagonal',[(w*.28,h*.51,-d*.48),(w*.39,h*.39,-d*.48)],.028,'white')
 elif f=='mall_r5_capsule_small':capsule_machine(0,0,0,w*.95,h*.98,d*.84)
 elif f=='mall_r5_capsule_large':
  box('Twin machine plinth',(0,h*.045,0),(w,h*.09,d),'navy')
  for xx in (-.25,.25):capsule_machine(w*xx,h*.09,0,w*.47,h*.89,d*.88)
 else:raise RuntimeError('Unmodelled family '+f)

def export_family(f,info):
 global OBJECTS,MATS
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);OBJECTS=[];MATS={}
 scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
 lo=info['min'];hi=info['max'];dims=[hi[i]-lo[i]for i in range(3)];build(f,*dims)
 # Apply transforms and normalize the complete authored shape to the original metre envelope.
 for o in OBJECTS:
  bpy.ops.object.select_all(action='DESELECT');o.select_set(True);bpy.context.view_layer.objects.active=o;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 coords=[v.co.copy()for o in OBJECTS for v in o.data.vertices];a=Vector([min(v[i]for v in coords)for i in range(3)]);b=Vector([max(v[i]for v in coords)for i in range(3)])
 desired=Vector((dims[0],dims[2],dims[1]));center=(a+b)/2;center.z=a.z
 for o in OBJECTS:
  for v in o.data.vertices:v.co=Vector([(v.co[i]-center[i])*desired[i]/(b[i]-a[i])for i in range(3)])
 # Partition the authored surfaces by nearest LEGACY shard boxes. No exported collider.
 # The host keeps its existing shard pivots, IDs, masses and fixed-piece boxes.
 shards=info['shards'];buckets={s['id']:[]for s in shards};triangles=[]
 for o in OBJECTS:
  o.data.calc_loop_triangles()
  for t in o.data.loop_triangles:
   vs=[o.data.vertices[i].co.copy()for i in t.vertices];c=sum(vs,Vector())/3
   u=[c.x+(lo[0]+hi[0])/2,c.z+lo[1],c.y+(lo[2]+hi[2])/2]
   def score(s):
    outside=sum(max(s['min'][i]-u[i],0,u[i]-s['max'][i])**2 for i in range(3))
    centre=sum(((s['min'][i]+s['max'][i])/2-u[i])**2 for i in range(3))
    return outside+centre*.008
   chosen=min(shards,key=score)['id'];triangles.append([chosen,vs,o.data.materials[t.material_index]])
 for s in shards:
  if not any(t[0]==s['id']for t in triangles):
   candidates=[t for t in triangles if sum(x[0]==t[0]for x in triangles)>1]
   c=B(((s['min'][0]+s['max'][0]-lo[0]-hi[0])/2,(s['min'][1]+s['max'][1])/2-lo[1],(s['min'][2]+s['max'][2]-lo[2]-hi[2])/2))
   min(candidates,key=lambda t:(sum(t[1],Vector())/3-c).length_squared)[0]=s['id']
 for t in triangles:buckets[t[0]].append(t)
 for o in OBJECTS:bpy.data.objects.remove(o,do_unlink=True)
 parts=[]
 for sid,ts in buckets.items():
  vertices=[];faces=[];colors=[];materials=[];mis=[]
  for _,vs,m in ts:
   j=len(vertices);vertices.extend(vs);faces.append((j,j+1,j+2));colors.extend([m.diffuse_color[:]]*3)
   if m not in materials:materials.append(m)
   mis.append(materials.index(m))
  mesh=bpy.data.meshes.new(f+'_part_'+str(sid));mesh.from_pydata(vertices,[],faces);mesh.update()
  for m in materials:mesh.materials.append(m)
  for p,mi in zip(mesh.polygons,mis):p.material_index=mi
  col=mesh.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='POINT')
  for dst,c in zip(col.data,colors):dst.color=c
  o=bpy.data.objects.new('Part_'+str(sid),mesh);bpy.context.collection.objects.link(o);parts.append(o)
 # One material per part and one combined intact mesh keep the play draw count bounded.
 flat=mat('white').copy();flat.name='Pilot vertex colour';nodes=flat.node_tree.nodes
 rgb=next(n for n in nodes if n.type=='RGB');mul=next(n for n in nodes if n.type=='MIX_RGB')
 attr=nodes.new('ShaderNodeVertexColor');attr.layer_name='Color';flat.node_tree.links.new(attr.outputs['Color'],mul.inputs[1]);nodes.remove(rgb)
 for o in parts:
  o.data.materials.clear();o.data.materials.append(flat)
  for poly in o.data.polygons:poly.material_index=0
 # Joined copy is used only while intact. Fracture parts keep legacy part identities.
 bpy.ops.object.select_all(action='DESELECT')
 copies=[]
 for o in parts:
  copy=o.copy();copy.data=o.data.copy();bpy.context.collection.objects.link(copy);copy.select_set(True);copies.append(copy)
 bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join();intact=bpy.context.object;intact.name='Intact'
 for o in parts:o.hide_render=True;o.hide_set(True)
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.hide_set(False);o.select_set(True)
 intact.select_set(True)
 bpy.context.view_layer.objects.active=parts[0]
 bpy.ops.export_scene.fbx(filepath=str(OUT/(f+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',use_space_transform=True,bake_space_transform=True,bake_anim=False,mesh_smooth_type='FACE',use_mesh_modifiers=True,colors_type='LINEAR',path_mode='AUTO')
 # Save a complete reopenable studio scene. Camera looks at the authored front (-Unity Z).
 scene.render.engine='BLENDER_EEVEE_NEXT';scene.render.resolution_x=512;scene.render.resolution_y=512;scene.render.resolution_percentage=100
 scene.world.color=(.02,.02,.02);scene.render.image_settings.file_format='PNG';scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast' if False else 'None';scene.view_settings.exposure=0;scene.view_settings.gamma=1
 target=Vector((0,0,dims[1]*.5));camera_data=bpy.data.cameras.new('Contact camera');camera=bpy.data.objects.new('Contact camera',camera_data);scene.collection.objects.link(camera);camera.location=target+Vector((dims[0]*.85,-max(dims[0],dims[2])*2.5,dims[1]*.7));camera.rotation_euler=(target-camera.location).to_track_quat('-Z','Y').to_euler();camera_data.type='ORTHO';camera_data.ortho_scale=max(dims[1]*1.32,dims[0]*1.5,dims[2]*1.6);scene.camera=camera
 scene.render.filepath=str(EVIDENCE/(f+'.png'));scene.render.film_transparent=True;scene.render.image_settings.color_mode='RGBA'
 bpy.ops.wm.save_as_mainfile(filepath=str(HERE/(f+'.blend')))
 bpy.ops.render.render(write_still=True)
 return dict(family=f,triangles=len(triangles),vertices=len(triangles)*3,parts=len(parts),dimensions=dims,origin='bottom centre',units='metres',fbx=str(OUT/(f+'.fbx')))

if __name__=='__main__':
 selected=sys.argv[sys.argv.index('--')+1:]if'--'in sys.argv else []
 stats=[]
 for f,info in DATA.items():
  if selected and f not in selected:continue
  print('MODELLING',f,flush=True);stats.append(export_family(f,info))
 if selected and (EVIDENCE/'blender-manifest.json').exists():
  old=json.loads((EVIDENCE/'blender-manifest.json').read_text())['models'];stats=[r for r in old if r['family'] not in selected]+stats
 (EVIDENCE/'blender-manifest.json').write_text(json.dumps(dict(blender=bpy.app.version_string,models=stats,palette=PALETTE),indent=2)+'\n')
 print('BLENDER_PILOT_COMPLETE',len(stats),flush=True)
