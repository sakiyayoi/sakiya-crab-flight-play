#!/usr/bin/env python3
"""Run the installed managed C# compiler only. Does not launch Unity or import assets."""
from pathlib import Path
import subprocess,re,json,hashlib,argparse
parser=argparse.ArgumentParser();parser.add_argument('--editor',action='store_true');options=parser.parse_args()
stem='Assembly-CSharp-Editor' if options.editor else 'Assembly-CSharp'
prefix='editor-' if options.editor else ''
ROOT=Path(__file__).resolve().parents[2]
project=ROOT/'games/flight/crab-flight'
compiler=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
old=ROOT.parent/('evidence/blender-panels/managed-compile/'+stem+'.rsp')
out=ROOT/'docs/port-prototype/managed-compile';out.mkdir(exist_ok=True)
args=old.read_text().splitlines()
args=[x for x in args if not x.startswith(('-out:','-refout:'))]
if options.editor:args=[x.replace(str(old.parent/'Assembly-CSharp.ref.dll'),str(out/'Assembly-CSharp.ref.dll')) for x in args]
args+=['-out:"'+str(out/(stem+'.dll'))+'"','-refout:"'+str(out/(stem+'.ref.dll'))+'"']
for f in ([] if options.editor else sorted((project/'Assets/PortPrototype').rglob('*.cs'))):
 relative=str(f.relative_to(project))
 if not any(relative in line for line in args):args.append('"'+relative+'"')
rsp=out/(stem+'.rsp');rsp.write_text('\n'.join(args)+'\n')
sources={line.strip('"'):hashlib.sha256((project/line.strip('"')).read_bytes()).hexdigest() for line in args if line.strip('"').startswith('Assets/') and line.strip('"').endswith('.cs')}
r=subprocess.run([str(compiler/'NetCoreRuntime/dotnet'),str(compiler/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp),'-nologo'],cwd=project,capture_output=True,text=True)
assert all(hashlib.sha256((project/name).read_bytes()).hexdigest()==sha for name,sha in sources.items()),'Source changed during compile'
(out/(prefix+'sources.json')).write_text(json.dumps(sources,indent=2)+'\n')
log=r.stdout+r.stderr;(out/(prefix+'typecheck.log')).write_text(log)
result={'exitCode':r.returncode,'errors':len(re.findall(r'error CS\d+',log)),'warnings':len(re.findall(r'warning CS\d+',log)),'scope':'managed compile only; no Unity Editor, asset import, shader compiler or PlayMode'}
(out/(prefix+'result.json')).write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result));print(log[-4000:]);raise SystemExit(r.returncode)
