#!/usr/bin/env python3
"""Execute production framing/hold/render/activation code with recording Unity doubles.
No rendered GPU/PhysX assertion. Keep all simulation and accepted art bytes fixed.
"""
from pathlib import Path
import hashlib,json,subprocess,re
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).resolve().parent
A=ROOT/'games/flight/crab-flight/Assets';BASE='6f5bb2e84e902b32f10a136095d9915368f92315'
def method(s,signature):
 start=s.index(signature);end=s.index('{',start)+1;depth=1
 while depth:
  depth+=(s[end]=='{')-(s[end]=='}');end+=1
 return s[start:end]
def old(p):return subprocess.check_output(['git','show',BASE+':games/flight/crab-flight/Assets/'+p],cwd=ROOT)
checks=[]
def check(ok,label):
 assert ok,label
 checks.append(label);print('PASS',label,flush=True)
presentation=(A/'ObjectRedesign/ObjectPresentation.cs').read_text()
fields=presentation[presentation.index(' int readObjectHits;'):presentation.index(' public void Jet(')]
extracted='using UnityEngine;\npublic sealed partial class CrabFlightGame {\n'+fields+'\n'+method(presentation,' void DrainObjectEvents()')+'\n'+method(presentation,' void ClearObjectBursts()')+'\n}\n'
products=Path('/tmp/kani-camhit-offline');products.mkdir(exist_ok=True);(products/'ProductionMethods.cs').write_text(extracted)
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting');rt=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
sources=[OUT/'Contracts.cs',products/'ProductionMethods.cs']+[A/p for p in ['PortPrototype/PortRules.cs','ObjectRedesign/ObjectRules.cs','ObjectRedesign/ObjectHitStopRules.cs','ObjectRedesign/ObjectHitStopFrame.cs']]
cmd=[str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib','-target:exe','-out:'+str(products/'camhit.dll')]+['-r:'+str(p) for p in rt.glob('*.dll')]+list(map(str,sources))
r=subprocess.run(cmd,capture_output=True,text=True);(OUT/'camhit-unit-compile.log').write_text(r.stdout+r.stderr)
if r.returncode:print(r.stdout,r.stderr);r.check_returncode()
(products/'camhit.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':rt.name}}}))
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(products/'camhit.dll')],capture_output=True,text=True);(OUT/'camhit-unit-contracts.log').write_text(r.stdout+r.stderr);print(r.stdout,r.stderr);r.check_returncode()
# Fixed-step loop, native State, force/placement/counter paths and all accepted art stay identical.
game=(A/'Scripts/CrabFlightGame.cs').read_text();base=old('Scripts/CrabFlightGame.cs').decode()
for signature in [' public void Tick(float dt)',' public FlightState State()']:
 check(method(game,signature)==method(base,signature),'byte-identical '+signature)
entries=subprocess.check_output(['git','ls-tree','-r',BASE,'games/flight/crab-flight/Assets'],cwd=ROOT,text=True).splitlines()
allowed=['ObjectRedesign/ObjectPresentation.cs','PortPrototype/PortCamera.cs','PortPrototype/PortRules.cs','Scripts/FlightCameraR19.cs','Scripts/CrabFlightGame.cs']
protected={}
for entry in entries:
 meta,full=entry.split('\t',1);blob=meta.split()[2]
 p=full.split('/Assets/',1)[1]
 if p in allowed or not (ROOT/full).is_file() or not full.endswith(('.cs','.json','.shader','.mat')):continue
 data=(ROOT/full).read_bytes()
 assert hashlib.sha1(b'blob '+str(len(data)).encode()+b'\0'+data).hexdigest()==blob,full
 protected[full]=hashlib.sha256(data).hexdigest()
check(len(protected)>120,'all existing non-camera/presentation code, configuration and art bytes identical')
check('Mathf.Min(desired,PortCameraSettings.Maximum)' in (A/'PortPrototype/PortCamera.cs').read_text(),'landmark bonus cannot exceed new hard cap')
check('size*=PortCameraSettings.BaseMultiplier' in (A/'Scripts/FlightCameraR19.cs').read_text(),'aim framing is wired to base multiplier')
check('PresentObjectHitStop(hit.kind);' in presentation,'all actual consumed kinds use one presentation entry')
check('ApplyObjectPresentationReport(report);' in game and game.index('ApplyObjectPresentationReport(report);')>game.index('void Report()'),'result hold touches browser Report only, never State/native replay')
check('if(ObjectDeathPresentationHeld)return;' in method(game,' void UpdateCamera('),'death retains impact camera before result framing')
new=''.join((A/p).read_text() for p in ['ObjectRedesign/ObjectHitStopFrame.cs','ObjectRedesign/ObjectHitStopRules.cs'])
new=re.sub(r'//[^\n]*|/\*.*?\*/','',new,flags=re.S)
check(not any(x in new for x in ['Time.timeScale=','feelClockR27','hitStop.Filter','accumulator','AddForce','Physics.Simulate']),'render hold cannot schedule or advance physics/legacy peak clock')
check(old('JuiceLayer/JuicePresentation.cs')==(A/'JuiceLayer/JuicePresentation.cs').read_bytes(),'existing hero/landmark/10-chain peaks byte identical, run concurrently')
(OUT/'camhit-contracts.json').write_text(json.dumps(dict(base=BASE,unitResult=r.stdout.strip().splitlines()[-1],staticPassed=len(checks),staticFailed=0,checks=checks,unchanged=protected,scope='Production methods executed with rendering doubles; sentinel report values are NOT a native replay. GPU rendering/PhysX NOT_RUN.'),indent=2)+'\n')
print(len(checks),'static passed / 0 failed')
