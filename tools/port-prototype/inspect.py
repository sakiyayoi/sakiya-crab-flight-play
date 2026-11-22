#!/usr/bin/env python3
"""Source/geometry audit; never launches Unity. Historical registry is labelled as such."""
import hashlib, json, math, subprocess
from pathlib import Path
import author as A
ROOT=A.ROOT
def digest(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def inspect():
 data=A.build();audit=A.audit(data);A.validate(audit);rows=data['instances'];geo=A.G.load_geometry()
 boxes={r['key']:A.G.world_bounds(r,geo) for r in rows}
 overlaps=[]
 for i,r in enumerate(rows):
  for s in rows[i+1:]:
   a,b=boxes[r['key']],boxes[s['key']]
   if all(min(a[k][1],b[k][1])-max(a[k][0],b[k][0])>1e-5 for k in range(3)):overlaps.append([r['key'],s['key']])
 assert not overlaps,overlaps
 # Only fixed structural/placement inputs: the hooks and role-specific effects are reviewed separately.
 tracked=subprocess.check_output(['git','ls-files','games/flight/crab-flight/Assets'],cwd=ROOT,text=True).splitlines()
 paths=[p for p in tracked if p.endswith('.json') and any(s in p for s in ['Placements','Placement','Geometry','Proposals','NeonDensity','feel.json'])]
 paths+=['games/flight/crab-flight/Assets/Scripts/'+n for n in ['City2Contacts.cs','RigidCrabFlight.cs','SwipeFlightRules.cs','CrabModelR3.cs']]
 protected={}
 for name in paths:
  path=ROOT/name
  if not path.exists():continue
  before=subprocess.check_output(['git','show',A.BASE+':'+name],cwd=ROOT)
  actual=path.read_bytes();assert actual==before,name
  protected[name]=digest(path)
 scene=ROOT/'games/flight/crab-flight/Assets/FlightField.unity';start=json.loads((ROOT/'docs/port-prototype/start.json').read_text());assert digest(scene)==start['sceneSha256']
 proposals={}
 for path in sorted((ROOT/'games/flight/crab-flight/Assets').rglob('*Proposals*.json')):
  for prop in json.loads(path.read_text()).get('assets',[]):proposals.setdefault(prop['asset'],prop)
 chain_physics={}
 for asset in sorted({r['asset'] for r in rows if r['chain']}):
  prop=proposals[asset];mass=prop['massKgSuggestion'];threshold=prop['normalizedImpulseThresholdSuggestion']
  assert mass<=100 and threshold<=2,(asset,mass,threshold)
  chain_physics[asset]={'unchangedMassKg':mass,'unchangedFullThreshold':threshold,'authoredWaveImpulsePerCrabMass':2}
 # Empty physical crack between all members of the two authored sets, including their triggers.
 sub=max(boxes[r['key']][0][1] for r in rows if r['chain']=='yard-sub')
 main=min(boxes[r['key']][0][0] for r in rows if r['chain']=='yard-main')
 assert 3<=main-sub<=5,(sub,main)
 assert len({r['id'] for r in rows})==len(rows)
 internal_gaps={}
 for beat in ['S1','S3','S5','S7']:
  spans=sorted(boxes[r['key']][0] for r in rows if r['beat']==beat and abs(r['p'][2])<=2)
  end=spans[0][1];gap=0
  for left,right in spans[1:]:gap=max(gap,left-end);end=max(end,right)
  internal_gaps[beat]=gap
 assert max(internal_gaps.values())<8,internal_gaps
 standalone=[r['key'] for r in rows if abs(r['p'][2])>2 and r['beat'] in ['S1','S3','S5','S7'] and r['role'] not in ['landmark','resource','preview'] and not any(s['key']!=r['key'] and s['beat']==r['beat'] and math.dist(s['p'],r['p'])<=8 for s in rows)]
 assert not standalone,standalone
 assert sum(r['role']=='jackpot' for r in rows)==1
 assert sum(r['role']=='hazard' for r in rows)==1
 assert not next(r for r in rows if r['role']=='hazard')['chain']
 # Preserve the finite recovery node identities; the existing recovery code owns their values.
 old_resources=sorted(r['id'] for r in A.old_rows() if 600<=r['p'][0]<900 and r['asset']=='flywheel_accumulator_r12')
 assert old_resources==sorted(r['id'] for r in rows if r['role']=='resource')
 source=ROOT/'games/flight/crab-flight/Assets'
 resources=[p for p in (source/'PortPrototype').rglob('*') if p.is_file() and p.suffix!='.meta']
 for p in resources:assert Path(str(p)+'.meta').exists(),p
 result={'scope':'offline source geometry; no Native, shader import, contact solver or FPS evidence','aabbOverlapPairs':overlaps,'groundToleranceMetres':1e-5,'yardPhysicalCrackMetres':main-sub,'internalCorridorMaxGaps':internal_gaps,'standaloneSideObjects':standalone,'thirdTierMaxTopMetres':max(boxes[r['key']][1][1] for r in rows if r['tier']==2),'chainPhysics':chain_physics,'resourceIds':old_resources,'sceneSHA256':digest(scene),'protectedSourceCount':len(protected),'protectedSources':protected,'roles':[{'key':r['key'],'id':r['id'],'x':r['p'][0],'role':r['role'],'chain':r['chain']} for r in rows if r['role']]}
 (ROOT/'docs/port-prototype/source-audit.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n')
 print(json.dumps({k:v for k,v in result.items() if k not in ['protectedSources','roles']},ensure_ascii=False,indent=2))
if __name__=='__main__':inspect()
