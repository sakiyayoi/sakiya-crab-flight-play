#!/usr/bin/env python3
"""Real deterministic policy execution plus source/data boundary contracts. No Unity."""
from pathlib import Path
import hashlib,json,os,re,subprocess,sys
ROOT=Path(__file__).resolve().parents[2];INPUT=ROOT/'docs/object-redesign';OUT=Path(os.environ.get('KANI_OBJECT_EVIDENCE_DIR',INPUT));OUT.mkdir(parents=True,exist_ok=True)
sys.path.insert(0,str(INPUT/'blastfix'))
from boundaries import restore_city2
ASSETS=ROOT/'games/flight/crab-flight/Assets'
SDK=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
RUNTIME=next((SDK/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
PRODUCTS=Path('/tmp/kani-object-offline');PRODUCTS.mkdir(exist_ok=True)
command=[str(SDK/'NetCoreRuntime/dotnet'),str(SDK/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib','-target:exe','-out:'+str(PRODUCTS/'rules.dll')]
command+=['-r:'+str(p) for p in RUNTIME.glob('*.dll')]+[str(ASSETS/'ObjectRedesign/ObjectRules.cs'),str(INPUT/'RulesContracts.cs')]
run=subprocess.run(command,capture_output=True,text=True);(OUT/'rules-compile.log').write_text(run.stdout+run.stderr);run.check_returncode()
(PRODUCTS/'rules.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':RUNTIME.name}}}))
run=subprocess.run([str(SDK/'NetCoreRuntime/dotnet'),str(PRODUCTS/'rules.dll')],capture_output=True,text=True)
(OUT/'rules-contracts.log').write_text(run.stdout+run.stderr);print(run.stdout);run.check_returncode()
checks=[]
def check(value,name):
 if not value:raise AssertionError(name)
 checks.append(name);print('PASS',name)
def read(p):return (ASSETS/p).read_text()
def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT)
BASE='68c007f59ad1cfaa76dd39a193783a3b216b15da'
# All actual authored placement data is byte-identical, not re-created for tests.
paths=git('ls-tree','-r','--name-only',BASE).decode().splitlines()
placement=[p for p in paths if p.endswith('.json') and any(x in p for x in ['OpeningGrammar/Resources','NeonDensity/Resources','CityDensity','PortPrototype/Resources'])]
for p in placement:
 data=(ROOT/p).read_bytes() if (ROOT/p).is_file() else git('show','HEAD:'+p)
 check(data==git('show',BASE+':'+p),'legacy placement bytes '+p)
for p,h in json.loads((INPUT/'protected-before.json').read_text()).items():
 check(hashlib.sha256((ROOT.parent/'source'/p).read_bytes()).hexdigest()==h,'protected main '+p)
 check(git('rev-parse',BASE+':'+p)==git('rev-parse','HEAD:'+p),'protected branch '+p)
for p in ['Scripts/FloorContactStepR10.cs','Scripts/OverdrivePhysics.cs','Scripts/FlightPhysics.cs','Scripts/City2Data.cs','Scripts/City2Contacts.cs','OpeningGrammar/OpeningLayout.cs']:
 data=(ASSETS/p).read_bytes()
 if p=='Scripts/City2Contacts.cs':data=restore_city2(data.decode()).encode()
 check(data==git('show',BASE+':games/flight/crab-flight/Assets/'+p),'unchanged base system (exact blastfix delta excluded) '+p)
rules=read('ObjectRedesign/ObjectRules.cs');runtime=read('ObjectRedesign/ObjectRuntime.cs');rigid=read('Scripts/RigidCrabFlight.cs');panel=read('Scripts/RestaurantPhysics.cs');game=read('Scripts/CrabFlightGame.cs')
check(not re.search(r'Body\.(?:position|rotation|linearVelocity|angularVelocity)\s*=',runtime),'no trajectory or velocity writes in action layer')
check('Random.' not in rules+runtime,'no randomized effect magnitudes')
check('Body.AddForce(' in runtime and 'ForceMode.Impulse' in runtime and 'ForceMode.Acceleration' in runtime and 'Body.AddTorque(' in runtime,'physical force APIs')
check(runtime.index('Kind.Death){')<runtime.index('switch(a.data.kind)'), 'death checked before all other actions')
check(rigid.index('if(BeforeObjectStep())return;')<rigid.index('PrepareCityDebris();SimulateFloorContact();'),'death preflight precedes physics')
check('SimulateFloorContact();if(DetectObjects(false))return;StepDebrisCalmR4();Time+=StepSeconds;' in rigid,'post contact death before debris and break resolution; legacy order preserved')
check('!simulation.ObjectRun.Dead&&(FeelOnR27?' in game,'result cannot be delayed by a previous hit stop')
check('if(!UseObjectRedesign)return false;' in runtime and 'ObjectRules.KeepLegacyPanel(' in runtime and 'p.objectSuppressed=!active' in runtime,'legacy bypass and scoped active placement')
check('a.consumed=true;a.collider.enabled=false;a.visual.SetActive(false);' in runtime,'one-shot physical and visible consumption')
check('ResetObjects();' in panel and 'world.ConfigureObjects(' in game and 'ObjectRun.Reset()' in runtime,'retry resets objects and actions')
check('ObjectRun.Break(Time)' in panel and 'maxChain=world' in game,'existing destruction aggregate owns maximum chain in both modes')
check('ObjectRules.SpinMultiplier' in panel and 'ObjectGroundBounce();' in rigid,'spin amplifies crush and bounce in actual paths')
check('objectBreakPending.Contains(p)' in panel and 'ObjectRules.BandRetention' in runtime,'bands avoid rigid solver/legacy boost recovery and lose fixed small momentum')
check('Body.SweepTestAll(' in runtime and 'ComputePenetration(' in runtime and 'UnityEngine.Physics.SyncTransforms();' in runtime,'moving real collider overlap plus swept translation')
check('public string objectMode="legacy"' in read('Scripts/FeelConfig.cs') and json.loads((ASSETS/'StreamingAssets/feel.json').read_text())['objectMode']=='legacy','serialized and JSON default legacy')
web=ROOT/'games/flight/web-template';js=(web/'object-controls.js').read_text()
check(all(x in js for x in ["send('Jet')","'DiveDown':'DiveUp'","'j','arrowup'","'k','arrowdown'","pointercancel","visibilitychange"]),'all touch/keyboard/cancel paths wired')
compiled=json.loads((OUT/'compile.json').read_text())
check(all(x['errors']==0 and x['exitCode']==0 for x in compiled['results']),'Editor/WebGL assemblies error zero')
for p,h in compiled['sources'].items():
 check(hashlib.sha256((ROOT/'games/flight/crab-flight'/p).read_bytes()).hexdigest()==h,'compiled same bytes '+p)
(OUT/'contracts.json').write_text(json.dumps({'base':BASE,'rules':run.stdout.strip().splitlines()[-1],'staticPassed':len(checks),'staticFailed':0,'checks':checks,'scope':'Offline contracts, not Unity/PhysX play verification'},indent=2)+'\n')
print(f'{len(checks)} static passed / 0 failed')
