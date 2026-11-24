#!/usr/bin/env python3
"""Run actual policy and SpinCharge C# without Unity. Mathf shim is scalar only."""
from pathlib import Path
import subprocess,json
root=Path(__file__).resolve().parents[2];sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
runtime=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
out=root.parent/'evidence/new-loop/fix/contracts';out.mkdir(parents=True,exist_ok=True)
legacy=subprocess.check_output(['git','show','2d0d849:games/flight/crab-flight/Assets/Scripts/SpinCharge.cs'],cwd=root).decode().replace('SpinWorkInput','LegacySpinWorkInput').replace('SpinCharge','LegacySpinCharge');(out/'LegacySpinCharge.cs').write_text(legacy)
args=['-nologo','-nostdlib','-target:exe','-out:'+str(out/'rules.dll')]+['-r:'+str(p) for p in runtime.glob('*.dll')]+[str(root/p) for p in ['games/flight/crab-flight/Assets/PortPrototype/NewLoopRules.cs','games/flight/crab-flight/Assets/Scripts/SpinCharge.cs','qa/new-loop/MathfShim.cs','qa/new-loop/RulesContract.cs']]+[str(out/'LegacySpinCharge.cs')]
subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),*args],check=True)
(out/'rules.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':runtime.name}}}))
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(out/'rules.dll'),str(root/'games/flight/crab-flight/Assets/PortPrototype/Resources/PortPrototype/Layout.json')],capture_output=True,text=True)
(root/'docs/new-loop/fix/contracts.log').write_text(r.stdout+r.stderr);print(r.stdout+r.stderr);raise SystemExit(r.returncode)
