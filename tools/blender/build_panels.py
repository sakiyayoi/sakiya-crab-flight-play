"""Original restaurant/contact-panel models. Blender 4.2 LTS, metres, bottom-centre.
Reads panels-families.json extracted from registered runtime objects. No physics data is written.
Uses the proven pilot export, colour and axis convention; never regenerates City models.
"""
import sys, pathlib, json, math
sys.path.insert(0,str(pathlib.Path(__file__).resolve().parent))
import build_mall as m
from mathutils import Vector
HERE=pathlib.Path(__file__).resolve().parent
DATA=json.loads((HERE/'panels-families.json').read_text())
m.EVIDENCE=HERE.parents[2]/'evidence/blender-panels/models';m.EVIDENCE.mkdir(parents=True,exist_ok=True)
m.PALETTE.update(clay='C66B4E',ceramic='E2DAC6',steel='8BADA8',rice='FFF8E3',seaweed='173942',salmon='DAA06C')
def plate(name,y,w,d,h,color='ceramic'):
 # Closed spun cross-section, visible raised rim and recessed centre.
 profile=[(0,0),(.30,0),(.46,.23),(.50,.75),(.48,1),(.42,.78),(.28,.28),(0,.28)]
 vertices=[];faces=[];n=32
 for r,z in profile:
  for i in range(n):
   a=2*math.pi*i/n;vertices.append(m.B((math.cos(a)*r*w,y+z*h,math.sin(a)*r*d)))
 for j in range(len(profile)-1):
  for i in range(n):faces.append((j*n+i,j*n+(i+1)%n,(j+1)*n+(i+1)%n,(j+1)*n+i))
 mesh=m.bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update();o=m.bpy.data.objects.new(name,mesh);m.bpy.context.collection.objects.link(o);m.finish(o,name,color)
def dishes(w,h,d):
 count=6 if h>1.4 else 4
 step=h/(count+.4)
 for k in range(count):plate('Raised ceramic rim %s'%k,k*step,w,d,step*.66,'mint' if k==count//2 else 'ceramic')
 # Top plate's broad dark inner ring makes the hollow dish readable from above.
 for side in (-1,1):m.tube('Blue glaze mark',[(w*.25*side,h*.91,-d*.19),(w*.30*side,h*.91,0),(w*.25*side,h*.91,d*.19)],min(w,d)*.02,'glass')
def sushi(w,h,d):
 plate('Serving tray',0,w,d,h*.16,'ink')
 for k in range(3):
  z=d*(-.31+.31*k);top=h*(.78 if k==1 else .62)
  m.box('Rice pillow',(0,h*.39,z),(w*.66,h*.40,d*.245),'rice',min(w,h)*.055)
  m.box('Salmon topping',(0,top,z),(w*.72,h*.16,d*.26),'salmon',min(w,h)*.04)
  m.box('Nori wrap',(0,top+.085*h,z),(w*.74,h*.018,d*.055),'seaweed')
  for sign in (-1,1):m.box('Nori side',(sign*w*.35,h*.50,z),(w*.032,h*.40,d*.055),'seaweed')
  for dx in (-.17,.17):m.tube('Fish cut stripe',[(w*dx,top+h*.085,z-d*.075),(w*(dx+.08),top+h*.085,z+d*.075)],min(w,h)*.018,'cream')
 m.ell('Garnish',(w*.26,h*.88,d*.32),(w*.26,h*.24,d*.11),'mint')
def timber(w,h,d,chair=False):
 for x in (-.31,.31):
  for z in (-.29,.29):m.box('Wooden leg',(w*x,h*.38,d*z),(w*.14,h*.76,d*.16),'wood',.015)
 m.box('Seat' if chair else 'Table top',(0,h*(.77 if chair else .94),0),(w,h*(.14 if chair else .12),d),'wood',.025)
 if chair:
  for x in (-.38,.38):m.box('Back upright',(w*x,h*.85,d*.42),(w*.12,h*.30,d*.12),'wood')
  for k in range(3):m.box('Chair back slat',(w*(-.27+k*.27),h*.94,d*.43),(w*.21,h*.12,d*.10),'gold')
  m.box('Back top rail',(0,h*.98,d*.43),(w,.04*h,.10*d),'wood')
 else:
  for z in (-.40,.40):m.box('Table apron',(0,h*.83,d*z),(w*.83,h*.14,d*.10),'wood')
  # Separate top boards and end-grain, not a texture gradient.
  for z in (-.28,0,.28):m.box('Board joint',(0,h*.999,d*z),(w*.96,h*.005,d*.012),'ink')
  m.box('Cloth runner',(0,h*1.003,0),(w*.50,h*.006,d*.95),'cream')
def glass(w,h,d):
 # The thin axis is X for every current glass. Keep the opening clear.
 for z in (-.48,.48):m.box('Glazed stile',(0,h*.5,d*z),(w,h,d*.04),'steel')
 for y in (.015,.985):m.box('Glazed rail',(0,h*y,0),(w,h*.03,d),'steel')
 for side in (-1,1):
  m.tube('Glass reflection long',[(w*.51*side,h*.22,-d*.28),(w*.51*side,h*.64,d*.04)],min(h,d)*.012,'white')
  m.tube('Glass reflection short',[(w*.51*side,h*.55,d*.15),(w*.51*side,h*.78,d*.32)],min(h,d)*.009,'white')
  for z in (-.47,.47):m.box('Frame corner fitting',(w*.52*side,h*.035,d*z),(w*.12,h*.055,d*.06),'ink')
def wall(w,h,d,low=False):
 m.box('Terracotta wall',(0,h*.5,0),(w*.86,h,d),'clay')
 for side in (-1,1):
  x=w*.46*side
  for y in (.03,.97):m.box('Stone cap',(x,h*y,0),(w*.08,h*.06,d),'ceramic')
  for y in (.32,.64):m.box('Horizontal masonry joint',(x,h*y,0),(w*.02,h*.012,d*.97),'wood')
  for k in range(4):m.box('Vertical masonry joint',(x,h*.50,d*(-.36+k*.24)),(w*.02,h*.91,d*.008),'wood')
  if low:
   m.box('Timber wainscot',(x,h*.20,0),(w*.10,h*.32,d*.98),'wood')
   m.box('Wainscot rail',(x,h*.37,0),(w*.13,h*.035,d),'gold')
def ceiling(w,h,d):
 # Horizontal wood ceiling panel, underside beams remain inside the old envelope.
 m.box('Ceiling board',(0,h*.73,0),(w,h*.54,d),'ceramic')
 for z in (-.42,-.14,.14,.42):m.box('Ceiling joist',(0,h*.23,d*z),(w,h*.46,d*.05),'wood')
 for x in (-.4,-.2,0,.2,.4):m.box('Board seam',(w*x,h*.451,0),(w*.006,h*.012,d*.98),'wood')
def kitchen(w,h,d):
 m.box('Stainless kitchen oven',(0,h*.52,0),(w*.92,h*.88,d*.94),'steel',.05)
 for z in (-.34,.34):
  for x in (-.32,.32):m.box('Oven foot',(w*x,h*.05,d*z),(w*.18,h*.10,d*.15),'ink')
 m.box('Cooktop rim',(0,h*.98,0),(w,h*.04,d),'ceramic')
 for z in (-.23,.23):
  m.cyl('Burner',(0,h*1.005,d*z),w*.27,h*.015,'ink')
  m.ring('Burner ring',(0,h*1.017,d*z),w*.20,w*.025,'steel')
 for side in (-1,1):
  m.box('Oven dark door',(side*w*.47,h*.48,0),(w*.025,h*.46,d*.71),'ink',.02)
  m.box('Oven glass window',(side*w*.485,h*.47,0),(w*.016,h*.28,d*.49),'glass')
  m.box('Door handle',(side*w*.52,h*.71,0),(w*.08,h*.035,d*.65),'cream')
  for z in (-.29,0,.29):m.cyl('Cooker knob',(side*w*.49,h*.85,d*z),h*.025,w*.05,'ink','X')
def fragment(kind,w,h,d):
 if kind=='tableware':
  plate('Ceramic broken rim',0,w,d,h,'ceramic')
 elif kind=='food':sushi(w,h,d)
 elif kind=='glass':
  m.panel('Angular glass shard',[(-w/2,0,-d/2),(w/2,h*.2,-d*.3),(w*.30,h,d*.3),(-w*.4,h*.7,d/2)],'glass')
  m.tube('Shard edge',[(-w/2,0,-d/2),(w*.3,h,d*.3)],min(w,d)*.04,'white')
 else:
  m.box('Broken '+kind,(0,h*.5,0),(w,h,d),'wood' if kind=='wood' else 'steel' if kind=='metal' else 'clay',.04)
  if kind=='wood':m.box('Exposed wood',(0,h*.99,0),(w*.95,h*.02,d*.94),'gold')
def build(f,w,h,d):
 info=DATA['models'][f];kind=info['kind']
 if kind.startswith('fragment_'):fragment(kind[9:],w,h,d)
 elif kind=='glass':glass(w,h,d)
 elif kind=='dishes':dishes(w,h,d)
 elif kind=='food':sushi(w,h,d)
 elif kind in ('table','chair'):timber(w,h,d,kind=='chair')
 elif kind in ('wall','wall_low'):wall(w,h,d,kind=='wall_low')
 elif kind=='ceiling':ceiling(w,h,d)
 elif kind=='kitchen':kitchen(w,h,d)
 else:raise ValueError(kind)
m.build=build
if __name__=='__main__':
 selected=sys.argv[sys.argv.index('--')+1:]if'--'in sys.argv else[]
 stats=[]
 for f,info in DATA['models'].items():
  if selected and f not in selected:continue
  print('MODELLING',f,flush=True);stats.append(m.export_family(f,info))
 if selected and (m.EVIDENCE/'manifest.json').exists():
  old=json.loads((m.EVIDENCE/'manifest.json').read_text())['models'];stats=[r for r in old if r['family'] not in selected]+stats
 (m.EVIDENCE/'manifest.json').write_text(json.dumps({'blender':m.bpy.app.version_string,'models':stats,'palette':m.PALETTE},indent=2)+'\n')
 print('BLENDER_PANELS_COMPLETE',len(stats),flush=True)
