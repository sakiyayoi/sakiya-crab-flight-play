"""Expand the current AddPanel call sites; NOT a substitute for the Native registry audit.
Retains source line, Scene gates and byte hashes. Regeneration never alters placements.
"""
from pathlib import Path
import re,json,hashlib,collections
HERE=Path(__file__).resolve().parent;SOURCE=HERE.parents[1];ASSETS=SOURCE/'games/flight/crab-flight/Assets'
scene=(ASSETS/'FlightField.unity').read_text()
flags={k:int(re.search(r'^  '+k+r': (\d+)',scene,re.M)[1]) for k in ['cityLayout','useOverdrive','useOverdriveRoute','useCoreDestruction']}
assert flags=={'cityLayout':2,'useOverdrive':1,'useOverdriveRoute':1,'useCoreDestruction':1},flags
sources=['RestaurantPhysics.cs','OverdriveRoute.cs','CoreDistrictContactsR7.cs'];targets=[];excluded=[];models={};entries=[]
pattern=re.compile(r'(AddPanel|AddOverdriveGroundPile)\(("[^"]+"(?:\+side)?(?:\+"-"\+row)?),"([^"]+)",new Vector3\(((?:[^()]|\([^()]*\))+)\),new Vector3\(((?:[^()]|\([^()]*\))+)\),new Color\(([^)]+)\),([.\d]+)f\)')
def num(expr,side,row):
 expr=re.sub(r'(?<=[\d.])f\b','',expr)
 assert re.fullmatch(r'[\d.\s*+()/\-a-z]+',expr)
 return eval(expr,{'__builtins__':{}},{'side':side,'row':row})
def vec(expr,side,row):return [num(e,side,row)for e in expr.split(',')]
for source in sources:
 text=(ASSETS/'Scripts'/source).read_text()
 for match in pattern.finditer(text):
  call,idexpr,kind,pos,size,color,threshold=match.groups()
  for side in [-1,1]:
   for row in (range(3)if 'row' in idexpr else[0]):
    id=idexpr.replace('+side',f'+"{side}"').replace('+row',f'+"{row}"');id=eval(id,{'__builtins__':{}})
    rec={'id':id,'type':kind,'system':'Restaurant'if source=='RestaurantPhysics.cs'else'Core'if source.startswith('Core')else'Overdrive','position':vec(pos,side,row),'size':vec(size,side,row),'color':vec(color,side,row),'threshold':float(threshold),'source':source,'line':text[:match.start()].count('\n')+1}
    if id.startswith(('wood-street-crate','metal-street-bin')):
     rec['reason']='ActiveCityLayout == 0 is false';excluded.append(rec);continue
    name=re.sub(r'--?1$','',id).replace('-','_')
    if id.startswith('outer-wall'):name='outer_wall_low'if row==0 else'outer_wall_upper'
    family='panel_'+name
    k='wall_low'if name=='outer_wall_low'else'wall'if name.startswith('outer_wall')else'ceiling'if name=='ceiling'else'chair'if name=='wood_chair'else'table'if kind=='wood'else'kitchen'if kind=='metal'else'dishes'if kind=='tableware'else kind
    w,h,d=rec['size'];lo=[-w/2,0,-d/2];hi=[w/2,h,d/2]
    if family not in models:models[family]={'kind':k,'min':lo,'max':hi,'shards':[{'id':0,'min':lo,'max':hi}],'ids':[]}
    assert models[family]['min']==lo and models[family]['max']==hi
    models[family]['ids'].append(id)
    # Legacy Dishes: first plate minimum=-.5h+.065625h. GroundPile's existing
    # later translation is intentionally left to OverdriveRoute, unchanged.
    bottom=-h*.5+(h*.065625 if kind in ('food','tableware')else 0)
    if kind=='glass':bottom-=min(.075,min(d,h)*.035)*.5
    entries.append({'id':id,'model':family,'offset':{'x':0,'y':bottom,'z':0}})
    rec['model']=family;rec['legacyBottomAfterGroundShift']=bottom-(h*.065625 if call=='AddOverdriveGroundPile'else 0);targets.append(rec)
for kind in ['glass','wall','tableware','food','wood','metal']:
 lo=[-.5,0,-.5];hi=[.5,1,.5];models['panel_fragment_'+kind]={'kind':'fragment_'+kind,'min':lo,'max':hi,'shards':[{'id':0,'min':lo,'max':hi}],'ids':[]}
result={'status':'SOURCE_ENUMERATION_NATIVE_PENDING','sceneFlags':flags,'sourceHashes':{p:hashlib.sha256((ASSETS/'Scripts'/p).read_bytes()).hexdigest()for p in sources},'sceneSha256':hashlib.sha256((ASSETS/'FlightField.unity').read_bytes()).hexdigest(),'total':len(targets),'counts':dict(collections.Counter(r['system']for r in targets)),'targets':sorted(targets,key=lambda r:r['id']),'inactiveLayoutObjects':excluded,'models':models}
(HERE/'panels-families.json').write_text(json.dumps(result,indent=2)+'\n')
(ASSETS/'BlenderModels/Resources/BlenderModels/panels-catalog.json').write_text(json.dumps({'entries':sorted(entries,key=lambda r:r['id'])},indent=2)+'\n')
print(result['status'],result['total'],result['counts'],'models',len(models))
