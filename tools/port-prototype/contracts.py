#!/usr/bin/env python3
"""Run pure runtime formulas under .NET. No Unity DLLs, Editor or player involved."""
import json,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
runtime=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
out=ROOT/'docs/port-prototype/contract-tests';out.mkdir(exist_ok=True)
args=['-nologo','-nostdlib','-target:exe','-out:'+str(out/'rules.dll')]+['-r:'+str(p) for p in runtime.glob('*.dll')]+[str(ROOT/'games/flight/crab-flight/Assets/PortPrototype/PortRules.cs'),str(ROOT/'qa/port-prototype/RulesContract.cs')]
subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),*args],check=True)
(out/'rules.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':runtime.name}}},indent=2)+'\n')
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(out/'rules.dll')],capture_output=True,text=True)
(out/'result.log').write_text(r.stdout+r.stderr);print(r.stdout+r.stderr);raise SystemExit(r.returncode)
