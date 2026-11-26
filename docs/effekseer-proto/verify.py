#!/usr/bin/env python3
"""Byte preservation, source identity and importer-resource closure (no engine)."""
from pathlib import Path
import hashlib,json,struct,subprocess
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'docs/effekseer-proto'
BASE='e5aa6d1d6e240d7ad5c53ecfc0d2eeefbf3ee6a9'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
asset=ROOT/'games/flight/crab-flight/Assets/Resources/Effekseer/ToonHit'
source=Path('/tmp/efk-sample')
manifest={str(p.relative_to(source)):sha(p) for p in sorted(source.rglob('*')) if p.is_file()}
assert len(manifest)==32
for p,h in manifest.items():assert sha(asset/p)==h,p
# Same EFKE / BIN_ and version-conditioned path decoding as 1.80.8 source.
b=(asset/'ToonHit.efkefc').read_bytes();assert b[:4]==b'EFKE';pos=8;data=None
while pos<len(b):
 tag=b[pos:pos+4];n=struct.unpack_from('<i',b,pos+4)[0];assert 0<=n<=len(b)-pos-8
 if tag==b'BIN_':data=b[pos+8:pos+8+n]
 pos+=8+n
assert data is not None and data[:4]==b'SKFE';pos=8
version=struct.unpack_from('<i',data,4)[0]
paths={}
for name,minimum in [('color',0),('normal',9),('distortion',9),('sound',1),('model',6),('material',15),('curve',1607)]:
 paths[name]=[]
 if version<minimum:continue
 n=struct.unpack_from('<i',data,pos)[0];pos+=4
 for i in range(n):
  size=struct.unpack_from('<i',data,pos)[0];pos+=4
  paths[name].append(data[pos:pos+(size-1)*2].decode('utf-16le'));pos+=size*2
for group in paths.values():
 for p in group:assert p in manifest and (asset/p).is_file(),p
assert not paths['sound'],'Visual-only sample must not introduce audio'
(OUT/'sample-manifest.json').write_text(json.dumps(dict(source='/tmp/efk-sample (user supplied official 02_Tktk03 sample)',files=manifest,binaryVersion=version,resourcePaths=paths),indent=2)+'\n')
protected=json.loads((OUT/'protected-before.json').read_text())
for p,h in protected.items():assert sha(ROOT.parent/'source'/p)==h,p
# Only these tracked files are permitted to differ from the requested base.
allowed={'games/flight/crab-flight/Assets/JuiceLayer/JuicePresentation.cs','games/flight/crab-flight/Assets/Scripts/FeelDirectorR27.cs','games/flight/crab-flight/Assets/Scripts/FeelConfig.cs','tasks.md'}
changed=subprocess.check_output(['git','diff',BASE,'--name-only','--diff-filter=MDR'],cwd=ROOT,text=True).splitlines()
assert set(changed)<=allowed,changed
compiled=json.loads((OUT/'compile.json').read_text())
for p,h in compiled['sources'].items():assert sha(ROOT/'games/flight/crab-flight'/p)==h,p
assert len(compiled['results'])==4 and all(r['errors']==0 and r['exitCode']==0 for r in compiled['results'])
result=dict(base=BASE,protectedFiles=len(protected),protectedUnchanged=True,trackedChanges=changed,sampleCount=len(manifest),directResourceCount=sum(map(len,paths.values())),sampleSourceExact=True,noSampleAudio=True,compiledSourceCount=len(compiled['sources']),compiledSourceHashesMatch=True,unity='NOT_RUN: explicit user instruction',scope='offline source/resource validation only')
(OUT/'verification.json').write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result,ensure_ascii=False))
