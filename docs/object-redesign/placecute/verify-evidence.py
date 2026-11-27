#!/usr/bin/env python3
from pathlib import Path
import hashlib,json,subprocess
ROOT=Path(__file__).resolve().parents[3];OUT=Path(__file__).resolve().parent
BASE='b8f8bb59d58589c06db9d46bd957e35aba4597a3'
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
initial=json.loads((OUT/'playwright-initial.json').read_text());recovery=json.loads((OUT/'playwright.json').read_text())
assert initial['stats']['expected']==35 and initial['stats']['unexpected']==1 and initial['stats']['skipped']==initial['stats']['flaky']==0
assert recovery['stats']['expected']==1 and recovery['stats']['unexpected']==recovery['stats']['skipped']==recovery['stats']['flaky']==0
checks={}
def walk(suites,values):
 for s in suites:
  for spec in s.get('specs',[]):
   for test in spec['tests']:values[spec['file']+':'+spec['title']]=test['status']
  walk(s.get('suites',[]),values)
walk(initial['suites'],checks);walk(recovery['suites'],checks)
assert len(checks)==36 and all(x=='expected' for x in checks.values()),checks
coverage=dict(uniqueTests=36,passed=36,failed=0,skipped=0,initial='playwright-initial.json: 35 passed / 1 file hydration timeout',recovery='playwright.json + playwright-recovery.log: only the failed test, 1 passed',checks=checks)
(OUT/'playwright-coverage.json').write_text(json.dumps(coverage,indent=2)+'\n')
paths=subprocess.check_output(['git','diff','--name-only',BASE,'--','games','docs/object-redesign'],cwd=ROOT,text=True).splitlines()
paths+=subprocess.check_output(['git','ls-files','--others','--exclude-standard','--','games/flight/crab-flight/Assets/PortPrototype/Editor','games/flight/crab-flight/Assets/PortPrototype/Editor.meta'],cwd=ROOT,text=True).splitlines()
paths+=[str(p.relative_to(ROOT)) for p in OUT.iterdir() if p.suffix in ['.py','.cs','.cjs']]
paths=[p for p in paths if p.endswith(('.cs','.py','.cjs','.meta'))]
manifest={p:hashlib.sha256((ROOT/p).read_bytes()).hexdigest() for p in sorted(set(paths))}
(OUT/'source-sha256.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('PASS protected main 26 / branch 26; compiled source hashes',len(compiled['sources']),'; Playwright coverage 36 passed / 0 remaining failed; manifest',len(manifest))
