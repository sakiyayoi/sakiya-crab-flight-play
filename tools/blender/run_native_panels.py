"""Resume after normal Unity licensing is restored. No activation/login/retry logic.
Creates a true rollout checkout for baseline; never simulates baseline by disabling the override.
"""
from pathlib import Path
import argparse,subprocess,os,json,datetime,shutil,tarfile,io,hashlib
HERE=Path(__file__).resolve().parent;SOURCE=HERE.parents[1];ROOT=SOURCE.parent;EV=ROOT/'evidence/blender-panels'
p=argparse.ArgumentParser();p.add_argument('stage',choices=['baseline','candidate']);p.add_argument('--name',required=True);args=p.parse_args()
assert args.name.replace('-','').replace('_','').isalnum()
assert shutil.disk_usage('/System/Volumes/Data').free>=5*1024**3,'Less than 5 GiB free'
run=EV/args.name;run.mkdir(exist_ok=False);output=run/'native';source=SOURCE
if args.stage=='baseline':
 base=json.loads((EV/'start.json').read_text())['baseCommit'];source=run/'source';source.mkdir()
 data=subprocess.check_output(['git','archive',base],cwd=SOURCE)
 with tarfile.open(fileobj=io.BytesIO(data))as archive:archive.extractall(source,filter='data')
 shutil.copy2(EV/'FlightField-at-start.unity',source/'games/flight/crab-flight/Assets/FlightField.unity')
 review=(SOURCE/'games/flight/crab-flight/Assets/BlenderModels/Editor/BlenderPanelsReview.cs').read_text().replace('revision="blender-panels-1"','revision="blender-rollout-baseline"')
 (source/'games/flight/crab-flight/Assets/BlenderModels/Editor/BlenderPanelsReview.cs').write_text(review)
 shutil.copy2(ROOT/'accepted-input.json',run/'accepted-input.json')
project=source/'games/flight/crab-flight'
cmd=['/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/MacOS/Unity','-batchmode','-quit','-forgetProjectPath','-projectPath',str(project),'-executeMethod','BlenderPanelsReview.Run','-logFile',str(run/'unity.log')]
env=dict(os.environ,BLENDER_PANELS_OUTPUT=str(output));record={'stage':args.stage,'command':cmd,'startedUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'state':'RUNNING','sourceCommit':subprocess.check_output(['git','rev-parse','HEAD'if args.stage=='candidate'else base],cwd=SOURCE,text=True).strip()}
# Do not start against a project already owned by another live Editor.
processes=subprocess.check_output(['ps','-axo','command'],text=True)
assert not any('Unity.app/Contents/MacOS/Unity' in line and str(project) in line for line in processes.splitlines()),'Editor already owns this project'
with (run/'launcher.log').open('w')as log:
 proc=subprocess.Popen(cmd,cwd=project,env=env,stdout=log,stderr=subprocess.STDOUT);record['pid']=proc.pid;(run/'execution.json').write_text(json.dumps(record,indent=2))
 try:code=proc.wait(timeout=2400)
 except subprocess.TimeoutExpired:proc.terminate();proc.wait(timeout=60);code=proc.returncode
record.update(state='TERMINAL',exitCode=code,endedUTC=datetime.datetime.now(datetime.timezone.utc).isoformat());(run/'execution.json').write_text(json.dumps(record,indent=2))
if code==0:
 destination=EV/('native-baseline'if args.stage=='baseline'else'native-final');assert not destination.exists(),destination
 shutil.copytree(output,destination)
 print('Native saved to',destination)
else:print('Unity did not pass. Evidence retained at',run)
raise SystemExit(code)
