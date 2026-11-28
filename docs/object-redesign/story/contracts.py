#!/usr/bin/env python3
"""Real shape-builder recording contracts plus immutable Stage 1 source boundaries."""
from pathlib import Path
import hashlib,json,subprocess,os,sys
ROOT=Path(__file__).resolve().parents[3];INPUT=Path(__file__).resolve().parent
OUT=Path(os.environ.get('KANI_STORY_EVIDENCE_DIR',INPUT));OUT.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(INPUT.parent/'blastfix'))
from boundaries import restore_runtime
BASE='06df91585f52edefeb133cef041f64139edbf987'
ASSETS='games/flight/crab-flight/Assets/'
def git(p):return subprocess.check_output(['git','show',BASE+':'+p],cwd=ROOT)
def read(p):return (ROOT/p).read_text()
checks=[]
def check(ok,label):
 if not ok:raise AssertionError(label)
 checks.append(label);print('PASS',label,flush=True)
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
runtime=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir());products=Path('/tmp/kani-story-offline');products.mkdir(exist_ok=True)
cmd=[str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib','-target:exe','-out:'+str(products/'shapes.dll')]+['-r:'+str(p) for p in runtime.glob('*.dll')]+[str(ROOT/ASSETS/'ObjectRedesign/ObjectRules.cs'),str(ROOT/ASSETS/'ObjectRedesign/ObjectVisual.cs'),str(INPUT/'ShapeContracts.cs')]
r=subprocess.run(cmd,capture_output=True,text=True);(OUT/'shapes-compile.log').write_text(r.stdout+r.stderr);r.check_returncode()
(products/'shapes.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':runtime.name}}}))
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(products/'shapes.dll')],capture_output=True,text=True);(OUT/'shapes-contracts.log').write_text(r.stdout+r.stderr);print(r.stdout,r.stderr);r.check_returncode()
shape_result=r.stdout.strip().splitlines()[-1]
p=ASSETS+'ObjectRedesign/ObjectRules.cs';s=read(p)
for old,new in zip(['arrow','spring','propeller','cloud','barrel','spikes'],['industrial-fan','frying-pan','sushi-conveyor','mochi','firework-ball','crab-fisher']):s=s.replace('"'+new+'"','"'+old+'"')
s=s.replace('Kind.Brake,21,2.5f','Kind.Brake,18,12').replace('Kind.Brake,58,3','Kind.Brake,64,9').replace('Kind.Brake,108,3','Kind.Brake,108,9')
s=s.replace('Kind.Death,13,26','Kind.Death,22,26').replace('Kind.Death,95,3','Kind.Death,114,26')
s=s.replace(' public const float ObjectScale=1.6f;\n','').replace('HalfSize=1.15f*ObjectScale','HalfSize=1.15f')
check(s.encode()==git(p),'all Rules bytes unchanged except six shape names, three requested brake positions, two death positions and exact 1.6 scale')
p=ASSETS+'ObjectRedesign/ObjectRuntime.cs';s=restore_runtime(read(p))
s=s.replace('public ObjectVisual.Motion storyMotion;','').replace(',out var storyMotion','').replace('storyMotion=storyMotion,','').replace('   a.storyMotion.Pose(time,ObjectReducedMotion);\n','')
check(s.encode()==git(p),'entire Runtime unchanged except child visual motion and exact blastfix queue delta')
for name in ['Scripts/RigidCrabFlight.cs','Scripts/RestaurantPhysics.cs','Scripts/FlightEventStream.cs','Scripts/FeelDirectorR27.cs','Scripts/FeelConfig.cs']:
 p=ASSETS+name;check((ROOT/p).read_bytes()==git(p),'Stage 1 unchanged '+name)
presentation=read(ASSETS+'ObjectRedesign/ObjectPresentation.cs')
check('visible=FeelEffectsR27&&!ReducedMotion&&!feelSettingsR27.reducedMotion&&!paused' in presentation,'caption captures actual effect/reduced/pause flags at activation')
check('sequence=readObjectHits,kind=hit.kind.ToString().ToLowerInvariant()' in presentation,'caption reflects latest consumed hit only')
check('objectCaption=null;' in presentation,'caption snapshot cleared on existing reset')
game=read(ASSETS+'Scripts/CrabFlightGame.cs')
check('public ObjectCaptionState objectCaption;' in game and 'objectCaption=objectCaption,objectMode=' in game,'latest caption survives event ring in existing FlightState/Report')
before=json.loads((INPUT/'protected-before.json').read_text());after={'main':{},'branch':{}}
for where,base in [('main',ROOT.parent/'source'),('branch',ROOT)]:
 for p,expected in before[where].items():
  data=(base/p).read_bytes() if (base/p).is_file() else subprocess.check_output(['git','show','HEAD:'+p],cwd=ROOT)
  after[where][p]=hashlib.sha256(data).hexdigest()
 check(before[where]==after[where] and len(after[where])==26,'26 protected SHA256 unchanged '+where)
(OUT/'story-protected-after.json').write_text(json.dumps(after,indent=2)+'\n')
(OUT/'story-contracts.json').write_text(json.dumps(dict(base=BASE,shapes=shape_result,staticPassed=len(checks),staticFailed=0,checks=checks,scope='Recording doubles and source contracts; no Unity rendering/PhysX'),indent=2)+'\n')
print(str(len(checks))+' static passed / 0 failed')
