"""Offline source-geometry contact sheet. NOT a Unity/game screenshot.
Uses recorded production Build hierarchy. Blender lighting is diagnostic only.
"""
import bpy,json,math,sys
from pathlib import Path
from mathutils import Matrix,Vector
out=Path(__file__).resolve().parent
rows=json.loads((out/'shape-record.json').read_text())
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.render.engine='BLENDER_EEVEE_NEXT'
scene.render.resolution_x=390;scene.render.resolution_y=844;scene.render.resolution_percentage=100
scene.world.color=(.035,.035,.045)
scene.view_settings.view_transform='Standard';scene.view_settings.look='Medium High Contrast'
materials={}
def material(color,emission):
 key=tuple(color.values())+tuple(emission.values())
 if key not in materials:
  m=bpy.data.materials.new('source-colour');m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF')
  n.inputs['Base Color'].default_value=(*color.values(),1);n.inputs['Roughness'].default_value=.5
  n.inputs['Emission Color'].default_value=(*emission.values(),1);n.inputs['Emission Strength'].default_value=1
  materials[key]=m
 return materials[key]
def primitive(kind):
 if kind=='Cylinder':
  bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=.5,depth=2)
  o=bpy.context.object
  for v in o.data.vertices:v.co=Matrix.Rotation(math.pi/2,4,'X')@v.co
 elif kind=='Capsule':
  bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=16,radius=.5)
  o=bpy.context.object
  for v in o.data.vertices:v.co.y+=.5 if v.co.y>0 else -.5
 else:
  bpy.ops.mesh.primitive_uv_sphere_add(segments=24,ring_count=16,radius=.5);o=bpy.context.object
 for p in o.data.polygons:p.use_smooth=True
 return o
for i,row in enumerate(rows):
 transforms={};offset=Vector(((i%2-.5)*4.6,(1-i//2)*5.3,0))
 for n in row['nodes']:
  p,s,r=[n[k] for k in ['p','s','r']]
  local=Matrix.Translation(Vector((p['x'],p['y'],p['z'])))
  local=local@Matrix.Rotation(math.radians(r['y']),4,'Y')@Matrix.Rotation(math.radians(r['x']),4,'X')@Matrix.Rotation(math.radians(r['z']),4,'Z')@Matrix.Diagonal((s['x'],s['y'],s['z'],1))
  world=(transforms[n['parent']] if n['parent'] else Matrix.Translation(offset))@local
  transforms[n['name']]=world
  if n['shape']:
   o=primitive(n['shape']);o.name=row['kind']+'/'+n['name'];o.matrix_world=Matrix.Diagonal((1,1,-1,1))@world;o.data.materials.append(material(n['color'],n['emission']))
bpy.ops.object.camera_add(location=(0,6,28));cam=bpy.context.object;forward=(Vector((0,0,0))-cam.location).normalized();right=forward.cross(Vector((0,1,0))).normalized();up=right.cross(forward);cam.rotation_euler=Matrix((right,up,-forward)).transposed().to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=21.8;scene.camera=cam
bpy.ops.object.light_add(type='AREA',location=(0,5,10));light=bpy.context.object;light.data.energy=1100;light.data.shape='DISK';light.data.size=12;light.rotation_euler=(Vector((0,0,0))-light.location).to_track_quat('-Z','Y').to_euler()
scene.render.filepath=str(out/'shape-geometry.png');bpy.ops.render.render(write_still=True)
from bpy_extras.object_utils import world_to_camera_view
up=cam.rotation_euler.to_quaternion()@Vector((0,1,0))
positions=[]
for i,row in enumerate(rows):
 center=Vector(((i%2-.5)*4.6,(1-i//2)*5.3,0))
 top=world_to_camera_view(scene,cam,center+up*2.16)
 positions.append(dict(kind=row['kind'].lower(),side=1,visible=True,x=top.x,y=top.y))
(out/'preview-badges.json').write_text(json.dumps(positions,indent=2)+'\n')
