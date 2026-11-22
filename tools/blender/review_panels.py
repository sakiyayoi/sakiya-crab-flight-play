"""Reopen each authored panel, verify geometry/colours/metres and frame native Blender previews.
These renders are asset evidence, not Unity Native gameplay screenshots.
"""
import bpy,json,pathlib,math
from mathutils import Vector
HERE=pathlib.Path(__file__).resolve().parent;ROOT=HERE.parents[2];EV=ROOT/'evidence/blender-panels'
data=json.loads((HERE/'panels-families.json').read_text());results=[]
for family,info in data['models'].items():
 bpy.ops.wm.open_mainfile(filepath=str(HERE/(family+'.blend')))
 bpy.context.preferences.filepaths.save_version=0
 scene=bpy.context.scene;obj=bpy.data.objects['Intact'];vs=[obj.matrix_world@v.co for v in obj.data.vertices]
 lo=[min(v[i]for v in vs)for i in range(3)];hi=[max(v[i]for v in vs)for i in range(3)]
 expectedLo=[info['min'][0],info['min'][2],info['min'][1]];expectedHi=[info['max'][0],info['max'][2],info['max'][1]]
 error=max(abs(x-y)for x,y in zip(lo+hi,expectedLo+expectedHi));assert error<.00001,(family,error)
 assert scene.unit_settings.system=='METRIC' and scene.unit_settings.scale_length==1
 assert obj.location.length<1e-6 and (obj.scale-Vector((1,1,1))).length<1e-6
 assert len(obj.data.color_attributes['Color'].data)==len(obj.data.vertices)
 dims=[info['max'][i]-info['min'][i]for i in range(3)];w,h,d=dims;target=Vector((0,0,h*.5));cam=scene.camera
 if info['kind'] in ('glass','wall','wall_low'):delta=Vector((max(w,d)*2,-max(w,d)*.65,h*.6))
 elif info['kind']=='ceiling':delta=Vector((w*.22,-w*.5,-w*.7))
 else:delta=Vector((max(w,d)*1.15,-max(w,d)*2,h*.9))
 cam.location=target+delta;cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();bpy.context.view_layer.update()
 points=[cam.matrix_world.inverted()@v for v in vs];xmin=min(v.x for v in points);xmax=max(v.x for v in points);ymin=min(v.y for v in points);ymax=max(v.y for v in points)
 cam.location+=cam.matrix_world.to_quaternion()@Vector(((xmin+xmax)/2,(ymin+ymax)/2,0));cam.data.ortho_scale=max(xmax-xmin,ymax-ymin)*1.15
 bpy.ops.wm.save_as_mainfile(filepath=str(HERE/(family+'.blend')))
 scene.render.filepath=str(EV/'models'/(family+'.png'));bpy.ops.render.render(write_still=True)
 results.append({'family':family,'vertices':len(obj.data.vertices),'triangles':len(obj.data.polygons),'boundsError':error,'metres':True,'bottomCentre':True,'vertexColours':True,'blendReopened':True})
(EV/'blender-readback.json').write_text(json.dumps({'models':results,'status':'PASS','nativeUnity':'NOT_RUN'},indent=2)+'\n')
print('PANEL_REOPEN_PASS',len(results),flush=True)
