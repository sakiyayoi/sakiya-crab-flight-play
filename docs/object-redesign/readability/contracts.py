#!/usr/bin/env python3
"""Production timeline/shape execution and unchanged simulation boundary."""
from pathlib import Path
import hashlib,json,os,re,subprocess
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).resolve().parent
A=ROOT/'games/flight/crab-flight/Assets';BASE='428892f8c99197742341650a678b2c8c494ab804'
SDK=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting');RT=next((SDK/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
TMP=Path('/tmp/kani-readability-offline');TMP.mkdir(exist_ok=True)
def run(cmd,log):
 r=subprocess.run(list(map(str,cmd)),cwd=ROOT,capture_output=True,text=True);(OUT/log).write_text(r.stdout+r.stderr)
 if r.returncode:print(r.stdout,r.stderr);r.check_returncode()
 return r.stdout
sources=[A/'ObjectRedesign/ObjectRules.cs',A/'ObjectRedesign/ObjectVisual.cs',A/'ObjectRedesign/ObjectCaptureTimeline.cs',OUT.parent/'story/ShapeContracts.cs',OUT/'ReadabilityContracts.cs']
run([SDK/'NetCoreRuntime/dotnet',SDK/'DotNetSdkRoslyn/csc.dll','-nologo','-nostdlib','-target:exe','-main:ReadabilityContracts','-out:'+str(TMP/'readability.dll')]+['-r:'+str(p) for p in RT.glob('*.dll')]+sources,'readability-compile.log')
(TMP/'readability.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':RT.name}}}))
result=run([SDK/'NetCoreRuntime/dotnet',TMP/'readability.dll'],'readability-unit-contracts.log');print(result)
checks=[]
def check(ok,label):
 assert ok,label
 checks.append(label);print('PASS',label)
def method(s,sig):
 start=s.index(sig);end=s.index('{',start)+1;depth=1
 while depth:depth+=(s[end]=='{')-(s[end]=='}');end+=1
 return s[start:end]
def old(p):return subprocess.check_output(['git','show',BASE+':'+p],cwd=ROOT)
allowed={'ObjectRedesign/'+p+'.cs' for p in ['ObjectPresentation','ObjectVisual','ObjectHitStopRules']}|{'Scripts/CrabFlightGame.cs'}
unchanged={}
for p in subprocess.check_output(['git','ls-tree','-r','--name-only',BASE,'games/flight/crab-flight/Assets'],cwd=ROOT,text=True).splitlines():
 f=ROOT/p;rel=p.split('/Assets/',1)[1]
 if rel in allowed or not f.is_file() or f.suffix not in ['.cs','.json','.shader','.mat']:continue
 data=f.read_bytes();assert data==old(p),p;unchanged[p]=hashlib.sha256(data).hexdigest()
check(len(unchanged)>120,'all existing physical, placement, effect, collider and material sources byte identical')
p='games/flight/crab-flight/Assets/Scripts/CrabFlightGame.cs';s=(ROOT/p).read_text()
for sig in [' public void Tick(float dt)',' public FlightState State()']:
 check(method(s,sig)==method(old(p).decode(),sig),'unchanged '+sig)
from boundaries import before_readability
for rel in ['ObjectRedesign/ObjectPresentation.cs','ObjectRedesign/ObjectHitStopRules.cs','Scripts/CrabFlightGame.cs']:
 check(before_readability(rel,(A/rel).read_text()).encode()==old('games/flight/crab-flight/Assets/'+rel),'exact requested presentation delta '+rel)
view=(A/'ObjectRedesign/ObjectCaptureView.cs').read_text();reader=(A/'ObjectRedesign/ObjectReadability.cs').read_text();presentation=(A/'ObjectRedesign/ObjectPresentation.cs').read_text()
code=re.sub(r'//[^\n]*','',view+reader)
check(not any(x in code for x in ['AddForce','Body.','AddComponent<Collider>','AddComponent<Rigidbody>','Physics.','Time.timeScale']),'capture copies only render components, no physics APIs')
check('o.AddComponent<MeshFilter>()' in view and 'o.AddComponent<MeshRenderer>()' in view and 'source.transform.position' in view,'render copies read source transforms and share meshes')
check('source.enabled=false' in view and 'r.enabled=true' in view and 'void OnDestroy(){Clear();}' in view,'renderer visibility and owned objects restored on cleanup')
check('Mathf.Clamp01(t/.2f)' in view and 'Vector3.Lerp(start,target,pull)' in view,'net casts before the render copy is pulled toward fisher')
check('if(hit.kind==ObjectRules.Kind.Death){BeginObjectCapture(hit.position);if(ReducedMotion||feelSettingsR27.reducedMotion)continue;}' in presentation and presentation.index('BeginObjectCapture(hit.position)')<presentation.index('if(!FeelEffectsR27||paused)continue'),'death subtitle clock starts even with effects off')
check('if(ObjectDeathPresentationHeld)return;' in method(s,' void UpdateCamera(') and 'objectCapture.Active' in presentation,'camera and browser result both wait for entire capture sequence')
check('LateObjectReadability();' in method(s,' void LateUpdate()') and 'AddObjectReadabilityReport(report);' in presentation,'real late frame and Report connect timeline and badges')
check('world.ReadObjectBadges(cam)' in reader and '!a.consumed' in reader and 'camera.WorldToViewportPoint' in reader,'badges follow actual unconsumed projected centers')
(OUT/'readability-contracts.json').write_text(json.dumps(dict(base=BASE,unitResult=result.strip().splitlines()[-1],staticPassed=len(checks),staticFailed=0,checks=checks,unchanged=unchanged,scope='Actual timeline and shape methods with recording doubles; source invariants. No Unity/GPU/PhysX execution.'),indent=2)+'\n')
print(len(checks),'static passed / 0 failed')
