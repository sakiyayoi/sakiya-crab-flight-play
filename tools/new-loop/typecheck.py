#!/usr/bin/env python3
"""Managed runtime compile only, using the waterfix references; never starts Unity."""
from pathlib import Path
import subprocess,json,hashlib,re
ROOT=Path(__file__).resolve().parents[2]
project=ROOT/'games/flight/crab-flight'
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
out=ROOT/'docs/new-loop/fix';out.mkdir(parents=True,exist_ok=True); products=ROOT.parent/'evidence/new-loop/fix/managed';products.mkdir(parents=True,exist_ok=True)
args=(ROOT/'docs/port-prototype/waterfix/Assembly-CSharp.rsp').read_text().splitlines()
args=[a for a in args if not a.startswith(('-out:','-refout:'))]
for f in sorted((project/'Assets/PortPrototype').glob('*.cs')):
 name=str(f.relative_to(project))
 if not any(name==a.strip('"') for a in args):args.append('"'+name+'"')
args+=['-out:"'+str(products/'Assembly-CSharp.dll')+'"','-refout:"'+str(products/'Assembly-CSharp.ref.dll')+'"']
rsp=out/'Assembly-CSharp.rsp';rsp.write_text('\n'.join(args)+'\n')
sources={a.strip('"'):hashlib.sha256((project/a.strip('"')).read_bytes()).hexdigest() for a in args if a.strip('"').startswith('Assets/') and a.strip('"').endswith('.cs')}
r=subprocess.run([str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp),'-nologo'],cwd=project,capture_output=True,text=True)
log=r.stdout+r.stderr;(out/'typecheck.log').write_text(log)
assert all(hashlib.sha256((project/p).read_bytes()).hexdigest()==sha for p,sha in sources.items())
result={'exitCode':r.returncode,'errors':len(re.findall(r'error CS\d+',log)),'warnings':len(re.findall(r'warning CS\d+',log)),'sources':sources,'scope':'managed runtime compile only; no Unity Editor/import/PlayMode'}
(out/'compile.json').write_text(json.dumps(result,indent=2)+'\n');print({k:v for k,v in result.items() if k!='sources'});print(log[-6000:]);raise SystemExit(r.returncode)
