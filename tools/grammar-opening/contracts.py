#!/usr/bin/env python3
"""Actual layout + existing port pure C# contracts under .NET, no Unity."""
import json
import subprocess
from pathlib import Path
import author as A

sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
runtime=next((sdk/'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
products=Path(subprocess.check_output(['git','rev-parse','--git-path','grammar-opening-contracts'],cwd=A.ROOT,text=True).strip())
if not products.is_absolute():products=A.ROOT/products
products.mkdir(parents=True,exist_ok=True)
out=A.ROOT/'docs/grammar-opening';out.mkdir(parents=True,exist_ok=True)
(products/'baseline.json').write_text(json.dumps(A.before()))
for name,sources,params in [
    ('layout',['games/flight/crab-flight/Assets/OpeningGrammar/OpeningLayout.cs','qa/grammar-opening/LayoutContract.cs'],[str(A.OUT),str(products/'baseline.json')]),
    ('port',['games/flight/crab-flight/Assets/PortPrototype/PortRules.cs','qa/port-prototype/RulesContract.cs'],[])]:
    args=['-nologo','-nostdlib','-target:exe','-out:'+str(products/(name+'.dll'))]+['-r:'+str(p) for p in runtime.glob('*.dll')]+[str(A.ROOT/p) for p in sources]
    subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),*args],check=True)
    (products/(name+'.runtimeconfig.json')).write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':runtime.name}}}))
    r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(products/(name+'.dll')),*params],capture_output=True,text=True)
    (out/(name+'-contracts.log')).write_text(r.stdout+r.stderr);print(r.stdout+r.stderr)
    r.check_returncode()
