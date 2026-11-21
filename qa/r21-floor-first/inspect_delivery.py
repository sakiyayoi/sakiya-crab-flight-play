from pathlib import Path
import argparse,json,hashlib,struct,collections,math
p=argparse.ArgumentParser();p.add_argument('source');p.add_argument('captures');p.add_argument('output');args=p.parse_args()
source=Path(args.source);res=source/'games/flight/crab-flight/Assets/Resources';capture=Path(args.captures);out=Path(args.output);out.mkdir(parents=True,exist_ok=True)
sha=lambda b:hashlib.sha256(b).hexdigest()
route=json.loads((res/'WorldRouteR11.json').read_text());expected={x['id'] for x in route['sectors']}
# Frozen before inspecting the candidate. Repeated structural supports can be
# legitimate. Colour/palette/UV changes and uniform scaling are not new forms.
rules={'version':'r21-inspection-1','coverage':'Every declared section needs current-source native PNG at390x844 and844x390; provenance camera/section/source identity; all bytesSHA verified. Static fixtures do not prove natural reach.','repetition':'Fail4consecutive same normalized geometry families or3consecutive repeats of a3-object motif within one section/direction. Structural nonbreakable columns/pillars/rails/tiles/floors/ceilings are recorded exemptions. Colour, palette, UV and uniform size changes never count as variety. No thresholds adjusted after result.','scopeLimit':'Actual active-renderer mesh components and277 logical runtime targets supplement source instances. Automatic duplicate alerts are review requirements, not a universal beauty classifier; unresolved alerts fail delivery.'}
(out/'inspection-rules.json').write_text(json.dumps(rules,ensure_ascii=False,indent=2)+'\n')
manifest=capture/'photo-manifest.json';coverage={'pass':False,'expectedSections':sorted(expected),'missing':[],'errors':[],'checked':[]}
if manifest.exists():
 d=json.loads(manifest.read_text());seen=set()
 for row in d['photos']:
  f=capture/row['file'];b=f.read_bytes() if f.exists() else b'';errors=[]
  if not b.startswith(b'\x89PNG\r\n\x1a\n'):errors.append('not-png')
  else:
   w,h=struct.unpack('>II',b[16:24]);
   if (w,h)!=(row['width'],row['height']):errors.append('dimension')
  if sha(b)!=row['sha256']:errors.append('sha')
  if row['sourceIdentity']!=d['sourceIdentity']:errors.append('source-identity')
  if row['section'] not in expected or 'cameraPosition' not in row:errors.append('section/camera')
  if not errors:seen.add((row['section'],row['width'],row['height']))
  coverage['checked'].append({'file':row['file'],'bytes':len(b),'sha256':sha(b),'errors':errors});coverage['errors']+=errors
 coverage['missing']=[{'section':x,'width':w,'height':h} for x in sorted(expected) for w,h in [(390,844),(844,390)] if (x,w,h) not in seen]
 coverage['pass']=not coverage['missing'] and not coverage['errors']
else:coverage['missing']=['current photo-manifest absent']
assets={};geometryFiles=['CityDensityR2/CityGeometryR2Compact.json','FarWorldR10/Geometry.json','RouteR12/Geometry.json']
for name in geometryFiles:
 for a in json.loads((res/name).read_text())['assets']:assets[a['id']]=a
families={}
for id,a in assets.items():
 points=[]
 for piece in a['pieces']:
  v=piece['v'];points += [tuple(v[i:i+3]) for i in range(0,len(v),3)]
 lo=[min(v[k] for v in points) for k in range(3)];hi=[max(v[k] for v in points) for k in range(3)];scale=max(hi[k]-lo[k] for k in range(3)) or 1
 normalized=sorted({tuple(round((v[k]-lo[k])/scale,4) for k in range(3)) for v in points})
 families[id]=sha(json.dumps(normalized,separators=(',',':')).encode())
placements=[]
for name in ['CityDensityR2/CityPlacementAtrium.json','RouteR12/Placements.json']:
 for x in json.loads((res/name).read_text())['instances']:
  # The production Build excludes nonbreakable mid-layer when worldArt is ON.
  if not x['breakable'] and x.get('band') in ['mid','far']:continue #SectionArt disables DecorativeFar
  placements.append(x)
issues=[];exempt=[];groups={};unknown=[]
for x in placements:
 asset=x['asset'];sector=next((s['id'] for s in route['sectors'] if s['start']<=abs(x['p'][0])<s['end']),None)
 if sector is None:continue
 if asset not in families:unknown.append(x['id']);continue
 if not x['breakable'] and any(n in asset for n in ['column','pillar','rail','tile','floor','ceiling']):exempt.append({'id':x['id'],'reason':'nonbreakable structural rhythm'});continue
 groups.setdefault((sector,1 if x['p'][0]>=0 else -1,x['breakable']),[]).append(x)
for key,items in groups.items():
 items.sort(key=lambda x:abs(x['p'][0]));tokens=[families[x['asset']] for x in items]
 for start in range(len(items)-3):
  if len(set(tokens[start:start+4]))==1:issues.append({'type':'four-same-form','section':key[0],'direction':key[1],'ids':[x['id'] for x in items[start:start+4]],'assets':[x['asset'] for x in items[start:start+4]]})
 for start in range(len(items)-8):
  seq=tokens[start:start+9]
  if seq[:3]==seq[3:6]==seq[6:9]:issues.append({'type':'three-times-three-object-motif','section':key[0],'direction':key[1],'ids':[x['id'] for x in items[start:start+9]],'assets':[x['asset'] for x in items[start:start+9]]})
runtimeFile=capture/'world-mesh-snapshot.json';runtimeIssues=[];runtimeExempt=[];runtimeTargets=[];runtimeMissing=[]
if runtimeFile.exists():
 snapshot=json.loads(runtimeFile.read_text());runtimeTargets=snapshot.get('targets',[])
 runtimeGroups={}
 for x in runtimeTargets:
  position=x['position'];sid=next((s['id'] for s in route['sectors'] if s['start']<=abs(position['x'])<s['end']),None)
  if sid:runtimeGroups.setdefault((sid,1 if position['x']>=0 else -1),[]).append(x)
 for key,items in runtimeGroups.items():
  items.sort(key=lambda x:abs(x['position']['x']));tokens=[x['family'] for x in items]
  for start in range(len(items)-3):
   if len(set(tokens[start:start+4]))==1:runtimeIssues.append({'type':'runtime-four-same-rendered-form','section':key[0],'direction':key[1],'ids':[x['id'] for x in items[start:start+4]]})
  for start in range(len(items)-8):
   seq=tokens[start:start+9]
   if seq[:3]==seq[3:6]==seq[6:9]:runtimeIssues.append({'type':'runtime-three-times-three-object-motif','section':key[0],'direction':key[1],'ids':[x['id'] for x in items[start:start+9]]})
 # Batched background/restaurant mesh: weld coincident positions, find connected
 # triangle components, fingerprint each form independently of paint/UV/scale.
 for mesh in snapshot['meshes']:
  raw=mesh['points'];tri=mesh['triangles'];parent=[];vertices=[];index={};original=[]
  for v in raw:
   point=tuple(round(v[k],4) for k in ['x','y','z'])
   if point not in index:index[point]=len(vertices);vertices.append(point);parent.append(len(parent))
   original.append(index[point])
  def find(i):
   while parent[i]!=i:parent[i]=parent[parent[i]];i=parent[i]
   return i
  for start in range(0,len(tri),3):
   a,b,c=[original[i] for i in tri[start:start+3]];parent[find(b)]=find(a);parent[find(c)]=find(a)
  components={}
  for i,v in enumerate(vertices):components.setdefault(find(i),[]).append(v)
  groups={}
  for points in components.values():
   if len(points)<8:continue
   lo=[min(v[k] for v in points) for k in range(3)];hi=[max(v[k] for v in points) for k in range(3)];scale=max(hi[k]-lo[k] for k in range(3))
   if scale<.05:continue
   form=sorted(tuple(round((v[k]-lo[k])/scale,3) for k in range(3)) for v in points);family=sha(json.dumps(form,separators=(',',':')).encode())
   groups.setdefault(family,[]).append({'center':[(lo[k]+hi[k])/2 for k in range(3)],'size':[hi[k]-lo[k] for k in range(3)]})
  for family,components in groups.items():
   if len(components)<4:continue
   # Explicit structural rhythm, not blanket exemption of every background.
   structural=any(term in mesh['path'].lower() for term in ['wet tile and pipes','timber ribs','deep spans and bracing','floor','ground','ceiling','railings'])
   row={'type':'batched-repeated-component-review','renderer':mesh['path'],'family':family,'count':len(components),'components':components}
   if structural:row['reason']='named noninteractive structural rhythm; remains in report';runtimeExempt.append(row)
   else:runtimeIssues.append(row)
else:runtimeMissing.append('current active-renderer mesh snapshot absent')
# Source instances identify logical placements; runtime meshes catch changed
# regional models, batch repetition and inactive background exclusions.
# Source-generated whole-module recurrence supplements mesh-component alerts.
# This targets the actual Mall builder and retains source bytes for audit.
import re
artSource=source/'games/flight/crab-flight/Assets/Scripts/SectionWorldArtR16.cs';artText=artSource.read_text();authored=[]
mall=artText[artText.index(' static void Mall('):artText.index(' static void Office(')]
match=re.search(r'foreach\(float floor in new\[\]\{([^}]+)\}\)foreach\(float x in new\[\]\{([^}]+)\}\)',mall)
if match:
 floors=[float(x.strip().rstrip('f')) for x in match.group(1).split(',')];positions=[float(x.strip().rstrip('f')) for x in match.group(2).split(',')]
 body=mall[match.end():mall.index('// The front galleries')]
 count=len(floors)*len(positions)
 if count>=3 and not re.search(r'\bif\s*\(|\bswitch\s*\(',body):authored.append({'type':'whole-shop-template-recurrence','section':'mall','instancesPerDirection':count,'floors':floors,'positions':positions,'sourceSHA256':sha(artSource.read_bytes()),'layoutBodySHA256':sha(body.encode()),'evidence':'same nested loop body draws glass shopfront,plinth,threepedestals/products attranslations only; paint is ignored. Actual Native mall image andindependent review confirmthiscomposition.'})
status='FAIL_REVIEWED_TEMPLATE' if authored else 'FAIL_REVIEW_REQUIRED' if runtimeIssues else 'INCOMPLETE' if runtimeMissing else 'PASS'

repetition={'pass':status=='PASS','status':status,'authoredTemplateFindings':authored,'runtimeIssues':runtimeIssues,'runtimeExemptions':runtimeExempt,'sourcePlacementAlerts':issues,'sourceExemptions':exempt,'unknownAssetIDs':unknown,'placementCount':len(placements),'runtimeTargetCount':len(runtimeTargets),'geometryFamilies':families,'missing':runtimeMissing,'structuralPolicy':'Only named noninteractive tiling/ribs/deep bridge spans/floor/ceiling/railings are exempt. Other repeated components remain unresolved review alerts and block delivery; paint/UV/size do not remove a match.'}
result={'schema':'flight.delivery-inspection.r21.1','rules':rules,'coverage':coverage,'repetition':repetition,'mayDeliverExecutable':coverage['pass'] and repetition['pass'],'inputSHA':{n:sha((res/n).read_bytes()) for n in ['WorldRouteR11.json',*geometryFiles,'CityDensityR2/CityPlacementAtrium.json','RouteR12/Placements.json']}}
(out/'delivery-inspection.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n');readback=json.loads((out/'delivery-inspection.json').read_text());assert readback==result
print(json.dumps({'coveragePass':coverage['pass'],'missingPhotos':len(coverage['missing']),'repetition':repetition['status'],'repetitionIssues':len(runtimeIssues),'mayDeliverExecutable':result['mayDeliverExecutable']}))
