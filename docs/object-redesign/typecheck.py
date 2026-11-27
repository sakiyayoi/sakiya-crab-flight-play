#!/usr/bin/env python3
"""Offline Roslyn compile using actual 1.80.8 sources; does not start Unity."""
from pathlib import Path
import hashlib,json,re,subprocess,sys
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/object-redesign'
PKG=Path(sys.argv[1] if len(sys.argv)>1 else '/tmp/kani-effekseer-1808-reference')
assert subprocess.check_output(['git','rev-parse','HEAD'],cwd=PKG,text=True).strip()=='f02f739f5910a4a14aac12a9a4a57022930e163f'
assert json.loads((PKG/'Assets/Effekseer/package.json').read_text())['version']=='1.80.8'
project=ROOT/'games/flight/crab-flight'
sdk=Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
products=Path('/tmp/kani-object-offline');products.mkdir(exist_ok=True)
args=(ROOT/'docs/port-prototype/waterfix/Assembly-CSharp.rsp').read_text().splitlines()
args=[a for a in args if not a.startswith(('-out:','-refout:'))]
reference_project=ROOT.parent/'source/games/flight/crab-flight'
args=[a.replace(chr(34)+'Library/',chr(34)+str(reference_project/'Library')+'/') for a in args]
for folder in ['OpeningGrammar','JuiceLayer','ObjectRedesign']:
 for f in sorted((project/'Assets'/folder).glob('*.cs')):
  name=str(f.relative_to(project))
  if not any(name==a.strip('"') for a in args):args.append('"'+name+'"')
sourceargs=[a for a in args if a.strip('"').endswith('.cs')]
common=[a for a in args if a not in sourceargs]
common.append('-r:"'+str(sdk/'Managed/UnityEngine/UnityEngine.VRModule.dll')+'"')
package_sources=sorted((PKG/'Assets/Effekseer/Scripts').rglob('*.cs'))
commands=[];results=[]
for flavor in ['editor','webgl']:
 flags=common if flavor=='editor' else [a for a in common if not a.startswith('-define:UNITY_EDITOR')]+['-define:UNITY_WEBGL']
 for name,extra in [('Effekseer',[str(p) for p in package_sources]),('Assembly-CSharp',sourceargs+['-r:"'+str(products/('Effekseer-'+flavor+'.dll'))+'"'])]:
  rsp=products/(name+'-'+flavor+'.rsp')
  rsp.write_text('\n'.join(flags+['-unsafe','-out:"'+str(products/(name+'-'+flavor+'.dll'))+'"']+extra)+'\n')
  cmd=[str(sdk/'NetCoreRuntime/dotnet'),str(sdk/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp),'-nologo'];commands.append(cmd)
  r=subprocess.run(cmd,cwd=project,capture_output=True,text=True);log=r.stdout+r.stderr
  (OUT/(name+'-'+flavor+'.log')).write_text(log)
  result=dict(assembly=name,flavor=flavor,exitCode=r.returncode,errors=len(re.findall(r'error CS\d+',log)),warnings=len(re.findall(r'warning CS\d+',log)))
  results.append(result);print(result)
  if r.returncode:print(log);break
sources={a.strip('"'):hashlib.sha256((project/a.strip('"')).read_bytes()).hexdigest() for a in sourceargs}
packageHashes={str(p.relative_to(PKG)):hashlib.sha256(p.read_bytes()).hexdigest() for p in package_sources}
(OUT/'compile.json').write_text(json.dumps(dict(results=results,sourceCount=len(sources),sources=sources,packageSources=packageHashes,commands=commands,scope='Roslyn only. No Unity import, native plugin load, shader compile, IL2CPP, build or play. WebGL preprocessor typecheck retains installed Editor reference assemblies for upstream namespace imports.'),indent=2)+'\n')
raise SystemExit(0 if len(results)==4 and all(r['exitCode']==0 for r in results) else 1)
