#!/usr/bin/env python3
from pathlib import Path
import hashlib,json,subprocess
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).resolve().parent
before=json.loads((OUT/'protected-before.json').read_text());after={}
for where,base in [('main',ROOT.parent/'source'),('branch',ROOT)]:
 after[where]={}
 for p in before[where]:
  data=(base/p).read_bytes() if (base/p).is_file() else subprocess.check_output(['git','show','HEAD:'+p],cwd=ROOT)
  after[where][p]=hashlib.sha256(data).hexdigest()
 assert len(after[where])==26 and before[where]==after[where],where
(OUT/'protected-after.json').write_text(json.dumps(after,indent=2)+'\n')
compiled=json.loads((OUT/'compile.json').read_text())
assert len(compiled['results'])==4 and all(x['errors']==0 and x['exitCode']==0 for x in compiled['results'])
for p,h in compiled['sources'].items():assert hashlib.sha256((ROOT/'games/flight/crab-flight'/p).read_bytes()).hexdigest()==h,p
suite=json.loads((OUT/'playwright.json').read_text());stats=suite['stats']
assert stats['expected']==35 and stats['unexpected']==stats['skipped']==stats['flaky']==0,stats
paths=subprocess.check_output(['git','diff','--name-only','6f5bb2e','--','games','docs/object-redesign/blastfix/contracts.py'],cwd=ROOT,text=True).splitlines()
paths+=subprocess.check_output(['git','ls-files','--others','--exclude-standard','--','games/flight/crab-flight/Assets/ObjectRedesign'],cwd=ROOT,text=True).splitlines()
paths+=[str(p.relative_to(ROOT)) for p in OUT.iterdir() if p.suffix in ['.py','.cs','.cjs']]
manifest={p:hashlib.sha256((ROOT/p).read_bytes()).hexdigest() for p in sorted(set(paths))}
(OUT/'source-sha256.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('PASS protected main 26 / branch 26; compiled source hashes',len(compiled['sources']),'; Playwright',stats['expected'],'passed / 0 failed; manifest',len(manifest))
