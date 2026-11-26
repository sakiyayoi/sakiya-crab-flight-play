#!/usr/bin/env python3
from pathlib import Path
import json,subprocess
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'docs/effekseer-proto'
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
runtime=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
products=Path('/tmp/kani-effekseer-offline');products.mkdir(exist_ok=True)
base='games/flight/crab-flight/Assets/'
suites={
 'bridge':[base+'JuiceLayer/'+n+'.cs' for n in ['EffekseerBurstBridge','JuiceBurst','JuicePresentation','JuiceRules']]+[base+'Scripts/FeelConfig.cs','docs/effekseer-proto/BridgeContracts.cs'],
 'legacy-juice':[base+'JuiceLayer/JuiceRules.cs',base+'JuiceLayer/JuiceParticles.cs',base+'Scripts/FeelSafetyR27.cs','qa/juice-layer/RulesContract.cs']}
for name,sources in suites.items():
 args=['-nologo','-nostdlib','-target:exe','-out:'+str(products/(name+'.dll'))]+['-r:'+str(p) for p in runtime.glob('*.dll')]+[str(ROOT/p) for p in sources]
 r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),*args],capture_output=True,text=True)
 (OUT/(name+'-compile.log')).write_text(r.stdout+r.stderr);print(r.stdout+r.stderr);r.check_returncode()
 (products/(name+'.runtimeconfig.json')).write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':runtime.name}}}))
 r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(products/(name+'.dll'))],capture_output=True,text=True)
 (OUT/(name+'-contracts.log')).write_text(r.stdout+r.stderr);print(r.stdout+r.stderr);r.check_returncode()
