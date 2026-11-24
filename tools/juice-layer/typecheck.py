#!/usr/bin/env python3
"""Use installed Roslyn and reference assemblies; never start Unity."""
from pathlib import Path
import hashlib
import json
import re
import subprocess

ROOT=Path(__file__).resolve().parents[2]
project=ROOT/'games/flight/crab-flight'
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
out=ROOT/'docs/juice-layer';out.mkdir(parents=True,exist_ok=True)
products=Path(subprocess.check_output(['git','rev-parse','--git-path','juice-layer-managed'],cwd=ROOT,text=True).strip())
if not products.is_absolute():products=ROOT/products
products.mkdir(parents=True,exist_ok=True)
args=(ROOT/'docs/port-prototype/waterfix/Assembly-CSharp.rsp').read_text().splitlines()
args=[a for a in args if not a.startswith(('-out:','-refout:'))]
# Reuse the existing read-only reference cache in the protected original checkout.
reference_project=ROOT.parent/'source/games/flight/crab-flight'
args=[a.replace(chr(34)+'Library/',chr(34)+str(reference_project/'Library')+'/') for a in args]
for f in sorted((project/'Assets/OpeningGrammar').glob('*.cs'))+sorted((project/'Assets/JuiceLayer').glob('*.cs')):
    name=str(f.relative_to(project))
    if not any(name==a.strip('"') for a in args):args.append('"'+name+'"')
args+=['-out:"'+str(products/'Assembly-CSharp.dll')+'"','-refout:"'+str(products/'Assembly-CSharp.ref.dll')+'"']
rsp=products/'Assembly-CSharp.rsp';rsp.write_text('\n'.join(args)+'\n')
sources={a.strip('"'):hashlib.sha256((project/a.strip('"')).read_bytes()).hexdigest() for a in args if a.strip('"').startswith('Assets/') and a.strip('"').endswith('.cs')}
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp),'-nologo'],cwd=project,capture_output=True,text=True)
log=r.stdout+r.stderr;(out/'typecheck.log').write_text(log)
assert all(hashlib.sha256((project/p).read_bytes()).hexdigest()==sha for p,sha in sources.items())
result=dict(exitCode=r.returncode,errors=len(re.findall(r'error CS\d+',log)),warnings=len(re.findall(r'warning CS\d+',log)),sourceCount=len(sources),sources=sources,scope='managed runtime compile only; no Unity Editor/import/shader/PlayMode')
(out/'compile.json').write_text(json.dumps(result,indent=2)+'\n')
print({k:v for k,v in result.items() if k!='sources'});print(log[-6000:]);raise SystemExit(r.returncode)
