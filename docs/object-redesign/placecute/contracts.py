#!/usr/bin/env python3
"""Real builder and diagnostic writer execution; source boundaries and offline typecheck."""
from pathlib import Path
import hashlib,json,os,re,subprocess
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).resolve().parent
A=ROOT/'games/flight/crab-flight/Assets';BASE='b8f8bb59d58589c06db9d46bd957e35aba4597a3'
SDK=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting');RT=next((SDK/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
TMP=Path('/tmp/kani-placecute-offline');TMP.mkdir(exist_ok=True)
def method(s,signature):
 start=s.index(signature);end=s.index('{',start)+1;depth=1
 while depth:depth+=(s[end]=='{')-(s[end]=='}');end+=1
 return s[start:end]
def run(cmd,log,env=None):
 r=subprocess.run(list(map(str,cmd)),cwd=ROOT,capture_output=True,text=True,env=env)
 (OUT/log).write_text(r.stdout+r.stderr)
 if r.returncode:print(r.stdout,r.stderr);r.check_returncode()
 return r.stdout
checks=[]
def check(ok,label):
 assert ok,label
 checks.append(label);print('PASS',label,flush=True)
def execute(name,main,sources,env):
 dll=TMP/(name+'.dll')
 cmd=[SDK/'NetCoreRuntime/dotnet',SDK/'DotNetSdkRoslyn/csc.dll','-nologo','-nostdlib','-target:exe','-out:'+str(dll),'-main:'+main]+['-r:'+str(p) for p in RT.glob('*.dll')]+sources
 run(cmd,name+'-compile.log')
 dll.with_suffix('.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':RT.name}}}))
 result=run([SDK/'NetCoreRuntime/dotnet',dll],name+'-contracts.log',dict(os.environ,**env));print(result.strip().splitlines()[-1]);return result.strip().splitlines()[-1]
faces=execute('faces','FaceContracts',[A/'ObjectRedesign/ObjectRules.cs',A/'ObjectRedesign/ObjectVisual.cs',OUT.parent/'story/ShapeContracts.cs',OUT/'FaceContracts.cs'],{'KANI_SHAPE_RECORD':str(OUT/'shape-record.json')})
native=(A/'PortPrototype/Editor/PortNativeReview.cs').read_text()
writer=method(native,'  public static void WriteActionEvents(')
dto='  [Serializable] public class ActionReceipt { public string kind; public Vector3 position; public float time; }'
(TMP/'NativeWriter.cs').write_text('using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;\npublic static class PortNativeReview {\n'+dto+'\n'+writer+'\n}\n')
logs=execute('action-log','LogContracts',[A/'ObjectRedesign/ObjectRules.cs',TMP/'NativeWriter.cs',OUT/'LogContracts.cs'],{'KANI_ACTION_TEST_DIR':str(TMP/'writer-fixture')})
# New entry was formerly injected by the existing native builder, not tracked in Git.
base=(OUT/'native-entry-baseline.cs.txt').read_text()
stripped=native.replace(dto+'\n','').replace('    WriteActionEvents(dir, sim.ObjectHits);\n','')
start=stripped.index('  // Read the authoritative');end=stripped.index('  public static void Capture(Camera',start)
stripped=stripped[:start]+stripped[end:]
check(stripped==base,'Native replay/input/camera/capture/retry unchanged except diagnostic DTO and writer')
runone=method(native,'  static void RunOne(')
check(runone.index('WriteActionEvents(dir, sim.ObjectHits);')<runone.index('game.Prepare("")'),'all consumed events saved before retry reset')
check('File.WriteAllLines(Path.Combine(dir, "action-events.jsonl")' in writer,'action events output under actual per-run directory')
check('sim.ObjectHits' in runone and '.events' not in writer,'authoritative ObjectHits not lossy UI event ring')
# Editor-only entry and its real importer typechecked against real Unity/Editor APIs.
source='games/flight/crab-flight/Assets/BlenderModels/Editor/BlenderPilotImport.cs'
(TMP/'BlenderPilotImport.cs').write_bytes(subprocess.check_output(['git','show',BASE+':'+source],cwd=ROOT))
args=(Path('/tmp/kani-object-offline/Assembly-CSharp-editor.rsp')).read_text().splitlines()
args=[a for a in args if not a.strip('"').endswith('.cs') and not a.startswith(('-out:','-refout:'))]
args+=['-r:/tmp/kani-object-offline/Assembly-CSharp-editor.dll','-out:"'+str(TMP/'NativeReview.dll')+'"','"'+str(A/'PortPrototype/Editor/PortNativeReview.cs')+'"','"'+str(TMP/'BlenderPilotImport.cs')+'"']
(TMP/'native.rsp').write_text('\n'.join(args)+'\n')
run([SDK/'NetCoreRuntime/dotnet',SDK/'DotNetSdkRoslyn/csc.dll','@'+str(TMP/'native.rsp'),'-nologo'],'native-typecheck.log')
check(True,'Native review entry and real importer Editor typecheck errors 0')
rules=(A/'ObjectRedesign/ObjectRules.cs').read_text()
oldrules=subprocess.check_output(['git','show',BASE+':games/flight/crab-flight/Assets/ObjectRedesign/ObjectRules.cs'],cwd=ROOT,text=True)
expected=oldrules.replace('Kind.Brake,18,12','Kind.Brake,19,20').replace('Kind.Brake,64,9','Kind.Brake,64,20').replace(' public const float HalfSize=1.15f, Emission=4, SpinSeconds=3, SpinMultiplier=2;',' public const float ObjectScale=1.6f;\n public const float HalfSize=1.15f*ObjectScale, Emission=4, SpinSeconds=3, SpinMultiplier=2;')
check(rules==expected,'only two specified coordinates and exact common multiplier changed in Rules')
for p in ['ObjectRedesign/ObjectRuntime.cs','ObjectRedesign/ObjectPresentation.cs','ObjectRedesign/ObjectCameraTargets.cs','ObjectRedesign/ObjectHitStopRules.cs','ObjectRedesign/ObjectHitStopFrame.cs','PortPrototype/PortCamera.cs','PortPrototype/PortRules.cs','Scripts/FlightCameraR19.cs','Scripts/CrabFlightGame.cs','Scripts/RigidCrabFlight.cs','Scripts/RestaurantPhysics.cs','Scripts/FloorContactStepR10.cs']:
 check((A/p).read_bytes()==subprocess.check_output(['git','show',BASE+':games/flight/crab-flight/Assets/'+p],cwd=ROOT),'unchanged runtime/camera/hitstop boundary '+p)
check('collider.size=Vector3.one*(ObjectRules.HalfSize*2)' in (A/'ObjectRedesign/ObjectRuntime.cs').read_text(),'real trigger size reads scaled half extent')
run(['python3',OUT/'audit.py'],'hit-map.log')
audit=json.loads((OUT/'hit-map.json').read_text())
check(len(audit['rows'])==16 and not any(r['newUnintendedNonBrakeOverlap'] for r in audit['rows']),'all 16 placements audited including rotation and motion extrema')
check([r['center'] for r in audit['rows'] if r['id'] in ['restaurant-brake','mall-brake-a']]==[[19,20,0],[64,20,0]],'both requested brake coordinates exact')
check(all(r['y'][0]>15 for r in audit['rows'] if r['kind']=='Death'),'death remains outside nominal standard band')
(OUT/'placecute-contracts.json').write_text(json.dumps(dict(base=BASE,faces=faces,actionLog=logs,staticPassed=len(checks),staticFailed=0,checks=checks,scope='Offline shape recording and diagnostic IO with serialization adapter. Native entry typechecked against Unity. No actual rendering, PhysX or Native replay.'),indent=2)+'\n')
print(len(checks),'static passed / 0 failed')
