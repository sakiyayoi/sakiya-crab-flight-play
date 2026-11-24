#!/usr/bin/env python3
import json, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
runtime=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
products=Path(subprocess.check_output(['git','rev-parse','--git-path','juice-layer-contracts'],cwd=ROOT,text=True).strip())
if not products.is_absolute():products=ROOT/products
products.mkdir(parents=True,exist_ok=True)
sources=['games/flight/crab-flight/Assets/JuiceLayer/JuiceRules.cs','games/flight/crab-flight/Assets/JuiceLayer/JuiceParticles.cs','games/flight/crab-flight/Assets/Scripts/FeelSafetyR27.cs','qa/juice-layer/RulesContract.cs']
args=['-nologo','-nostdlib','-target:exe','-out:'+str(products/'juice.dll')]+['-r:'+str(p) for p in runtime.glob('*.dll')]+[str(ROOT/p) for p in sources]
subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),*args],check=True)
(products/'juice.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':runtime.name}}}))
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(products/'juice.dll')],capture_output=True,text=True)
(ROOT/'docs/juice-layer/rules-contracts.log').write_text(r.stdout+r.stderr);print(r.stdout+r.stderr);r.check_returncode()
