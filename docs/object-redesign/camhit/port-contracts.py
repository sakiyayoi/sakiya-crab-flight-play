#!/usr/bin/env python3
"""Run the unchanged 30-case PortRules suite, keeping historical evidence intact."""
from pathlib import Path
import json,subprocess,os
ROOT=Path(__file__).resolve().parents[3];OUT=Path(os.environ.get('KANI_CAMHIT_EVIDENCE_DIR',Path(__file__).resolve().parent));OUT.mkdir(parents=True,exist_ok=True)
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
runtime=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
products=Path('/tmp/kani-camhit-offline');products.mkdir(exist_ok=True)
cmd=[str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),'-nologo','-nostdlib','-target:exe','-out:'+str(products/'port.dll')]+['-r:'+str(p) for p in runtime.glob('*.dll')]+[str(ROOT/'games/flight/crab-flight/Assets/PortPrototype/PortRules.cs'),str(ROOT/'qa/port-prototype/RulesContract.cs')]
r=subprocess.run(cmd,capture_output=True,text=True);(OUT/'port-compile.log').write_text(r.stdout+r.stderr);r.check_returncode()
(products/'port.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':runtime.name}}}))
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(products/'port.dll')],capture_output=True,text=True);(OUT/'port-contracts.log').write_text(r.stdout+r.stderr);print(r.stdout,r.stderr);r.check_returncode()
