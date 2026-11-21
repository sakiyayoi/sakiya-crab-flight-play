from pathlib import Path
import subprocess,os,json,datetime,sys,shutil,hashlib
R=Path(__file__).resolve().parents[1]; S=Path('/tmp/crab-r21-floor-first-github-20261028'); T=Path('/tmp/crab-r21-floor-first-build-20261028');mode=sys.argv[1];p=R/('evidence/unity-'+mode+'-receipt.json');assert not p.exists();now=lambda:datetime.datetime.now(datetime.timezone.utc).isoformat();s={'state':'PRELAUNCH','mode':mode,'atUTC':now(),'newUnityStarts':0,'publicChanges':0,'push':0,'fees':0,'newAuth':0}
def save():p.write_text(json.dumps(s,indent=2))
save();g=subprocess.run(['pgrep','-x','Unity'],capture_output=True,text=True);assert g.returncode in [0,1];s['pidsBefore']=[int(x) for x in g.stdout.split()];save()
if s['pidsBefore']:s.update(state='NOT_STARTED_OTHER_UNITY_ACTIVE');save();sys.exit(0)
lock=T/'crab-flight/Temp/UnityLockfile';assert not lock.exists(),str(lock)
for folder in ['Assets','Packages','ProjectSettings']:
 src=S/'games/flight/crab-flight'/folder;dst=T/'crab-flight'/folder
 shutil.copytree(src,dst,dirs_exist_ok=True)
shutil.copytree(S/'games/flight/web-template',T/'web-template',dirs_exist_ok=True)
identity={}
for f in sorted((S/'games/flight').rglob('*')):
 if f.is_file() and '.git' not in f.parts and 'SourceIdentityR21' not in f.name:identity[str(f.relative_to(S))]=hashlib.sha256(f.read_bytes()).hexdigest()
fingerprint=hashlib.sha256(json.dumps(identity,sort_keys=True).encode()).hexdigest();s['sourceFingerprint']=fingerprint;s['sourceHEAD']=subprocess.check_output(['git','-C',str(S),'rev-parse','HEAD'],text=True).strip();save()
(T/'crab-flight/Assets/Resources/SourceIdentityR21.txt').write_text(json.dumps({'commit':s['sourceHEAD'],'sha256':fingerprint,'scope':'source production+tests exact snapshot; identity excluded'},sort_keys=True))
(R/('evidence/source-'+mode+'.json')).write_text(json.dumps(identity,indent=2))
log=Path('/tmp')/('crab-r21-'+mode+'.log');assert not log.exists();args=['/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity','-batchmode','-forgetProjectPath','-projectPath',str(T/'crab-flight'),'-logFile',str(log)]
if mode.startswith('release'):args+=['-executeMethod','FloorBuildR10.R19VfxRelease','-quit']
else:args+=['-runTests','-testPlatform','PlayMode','-assemblyNames','CrabFlight.Measurement.PlayMode','-testResults',str(R/('evidence/'+mode+'-results.xml')),'-testFilter','FlightMeasurementPlayMode.'+('R21ObservedWeak70' if mode.startswith('residual') else 'R21ColliderFit' if mode.startswith('shape') else 'R21InspectAllSections' if mode.startswith('inspect') else 'R21Acceptance40' if mode.startswith('accept') else 'R21BaselineDiagnosis')]
env=dict(os.environ);env['CRAB_EVIDENCE_SCOPE']=mode;s.update(state='STARTING',newUnityStarts=1);save()
with (Path('/tmp')/('crab-r21-'+mode+'-launcher.log')).open('wb') as out:
 child=subprocess.Popen(args,stdout=out,stderr=subprocess.STDOUT,env=env);s.update(state='RUNNING',pid=child.pid);save();print(mode,'RUNNING',child.pid,flush=True)
 try:code=child.wait(timeout=1800)
 except subprocess.TimeoutExpired:s.update(state='UNKNOWN_CHILD_DO_NOT_RETRY');save();raise
s.update(state='TERMINAL',exitCode=code,atEndUTC=now());g=subprocess.run(['pgrep','-x','Unity'],capture_output=True,text=True);s['pidsAfter']=[int(x) for x in g.stdout.split()] if g.returncode in [0,1] else None;save();source=T/'evidence'/mode
if source.exists():shutil.copytree(source,R/('evidence/saved-'+mode))
print(json.dumps(s),flush=True)
