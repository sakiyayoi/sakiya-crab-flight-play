#!/usr/bin/env python3
"""Compile exact production queue/consume/recycler/scatter methods with recording doubles."""
from pathlib import Path
import hashlib,json,subprocess
from boundaries import restore_city2,restore_runtime
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).resolve().parent
A=ROOT/'games/flight/crab-flight/Assets';BASE='75cb1a15ea43420ce23c8e2d7afb22a63506fa00'
def read(p):return (A/p).read_text()
def old(p):return subprocess.check_output(['git','show',BASE+':games/flight/crab-flight/Assets/'+p],cwd=ROOT,text=True)
def method(s,signature):
 start=s.index(signature);end=s.index('{',start);depth=1;end+=1
 while depth:
  depth+=(s[end]=='{')-(s[end]=='}');end+=1
 return s[start:end]
checks=[]
def check(ok,label):
 assert ok,label
 checks.append(label);print('PASS',label,flush=True)
city=read('Scripts/City2Contacts.cs');runtime=read('ObjectRedesign/ObjectRuntime.cs');restaurant=read('Scripts/RestaurantPhysics.cs');scatter=read('Scripts/ShardScatterR6.cs')
check(restore_city2(city)==old('Scripts/City2Contacts.cs'),'City2 changes are only batch exclusion and redesign-only caller; legacy null path retained')
check(restore_runtime(runtime)==old('ObjectRedesign/ObjectRuntime.cs'),'ObjectRuntime changes only queue guard/idempotency/source ownership')
check(restaurant==old('Scripts/RestaurantPhysics.cs'),'actual ApplyBreaks and legacy contact/counters byte unchanged')
check(scatter==old('Scripts/ShardScatterR6.cs'),'actual scatter byte unchanged; no exception masking/null-only workaround')
check('readonly Dictionary<BreakPanel,Vector3> pendingBreaks' in restaurant,'shared queue uses unique panel reference keys')
apply=method(restaurant,' void ApplyBreaks()')
check(apply.index('if(p.broken||')<apply.index('p.broken=true;')<apply.index('GlassBroken++')<apply.index('DestructionEvent++')<apply.index('SpawnCity2Shards('),'consumed check and flag precede all material/total/event/shard processing')
check('primaryPending.Contains(p))continue;' in city,'secondary contact enqueue rejects object-owned primary panels')
check('objectBreakPending.Clear();objectBlastPending.Clear();pendingBreaks.Clear();' in apply,'batch flush clears object and common queues')
check('ObjectRun.Reset();ObjectHits.Clear();objectBreakPending.Clear();objectBlastPending.Clear();' in runtime,'throw reset clears consumed queues')
extracted='using System;using System.Collections.Generic;using System.Linq;using UnityEngine;\npublic sealed partial class RigidCrabFlight {\n'+ '\n'.join([
 method(runtime,' void QueueObjectBreak('),method(restaurant,' void QueueBreak('),apply,method(city,' void RecycleCityFragment('),method(old('Scripts/City2Contacts.cs'),' void RecycleCityFragment(').replace('RecycleCityFragment()','RecycleCityFragmentBaseline()')])+ '\n}\n'
products=Path('/tmp/kani-blastfix-offline');products.mkdir(exist_ok=True);(products/'ProductionMethods.cs').write_text(extracted)
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting');rt=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
cmd=[str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib','-target:exe','-out:'+str(products/'blast.dll')]+['-r:'+str(p) for p in rt.glob('*.dll')]+[str(OUT/'BlastContracts.cs'),str(products/'ProductionMethods.cs'),str(A/'ObjectRedesign/ObjectRules.cs'),str(A/'Scripts/ShardScatterR6.cs')]
r=subprocess.run(cmd,capture_output=True,text=True);(OUT/'unit-compile.log').write_text(r.stdout+r.stderr)
if r.returncode:print(r.stdout,r.stderr);r.check_returncode()
(products/'blast.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':rt.name}}}))
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(products/'blast.dll')],capture_output=True,text=True);(OUT/'unit-contracts.log').write_text(r.stdout+r.stderr);print(r.stdout,r.stderr);r.check_returncode()
(OUT/'blast-contracts.json').write_text(json.dumps(dict(base=BASE,unitResult=r.stdout.strip().splitlines()[-1],staticPassed=len(checks),staticFailed=0,checks=checks,extractedSha256=hashlib.sha256(extracted.encode()).hexdigest(),sourceSha256={p:hashlib.sha256((A/p).read_bytes()).hexdigest() for p in ['ObjectRedesign/ObjectRuntime.cs','Scripts/City2Contacts.cs','Scripts/RestaurantPhysics.cs','Scripts/ShardScatterR6.cs','ObjectRedesign/ObjectRules.cs']},scope='Exact production method execution with lifecycle/Unity doubles; no native PhysX/replay/build'),indent=2)+'\n')
print(str(len(checks))+' static passed / 0 failed')
