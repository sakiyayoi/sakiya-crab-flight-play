#!/usr/bin/env python3
"""Production review Run/RunOne + guarded CLI, with offline engine/game adapters."""
from pathlib import Path
import hashlib
import importlib.util
import json
import os
import shutil
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[3]
INPUT = Path(__file__).resolve().parent
OUT = Path(os.environ.get('KANI_BRAKEGROUND_EVIDENCE_DIR', INPUT))
OUT.mkdir(parents=True, exist_ok=True)
BASE = '8b3e4211c2299dafcd65e14418fc1a2439da02bd'
PROJECT = Path('games/flight/crab-flight')
HELPER = PROJECT / 'Assets/PortPrototype/Editor/PortNativeReview.cs'
RUNNER = Path('tools/object-redesign/run_native_review.py')
SDK = Path('/Applications/Unity/Hub/Editor/6000.3.23f1/Unity.app/Contents/Resources/Scripting')
RT = next((SDK / 'NetCoreRuntime/shared/Microsoft.NETCore.App').iterdir())
checks = []


def check(ok, label):
    assert ok, label
    checks.append(label)
    print('PASS', label, flush=True)


def run(cmd, log, **kwargs):
    r = subprocess.run(list(map(str, cmd)), capture_output=True, text=True, **kwargs)
    (OUT / log).write_text(r.stdout + r.stderr)
    if r.returncode:
        print(r.stdout, r.stderr)
        r.check_returncode()
    return r


def old(p):
    return subprocess.check_output(['git', 'show', BASE + ':' + str(p)], cwd=ROOT)


rules = PROJECT / 'Assets/ObjectRedesign/ObjectRules.cs'
expected = old(rules).decode().replace('Kind.Brake,19,20', 'Kind.Brake,21,2.5f').replace('Kind.Brake,64,20', 'Kind.Brake,58,3').replace('Kind.Brake,108,20', 'Kind.Brake,108,3')
expected = expected.replace('Kind.Death,22,26', 'Kind.Death,13,26').replace('Kind.Death,114,26', 'Kind.Death,95,3')
check((ROOT / rules).read_text() == expected, 'runtime Rules differ only at the three brake and two death centers')
check((ROOT / HELPER).read_bytes() == old(HELPER), 'existing native input/camera/capture/hit writer code unchanged')
changed = subprocess.check_output(['git', 'diff', BASE, '--name-only', '--', str(PROJECT / 'Assets')], cwd=ROOT, text=True).splitlines()
allowed={str(rules)}|{'games/flight/crab-flight/Assets/'+p for p in ['ObjectRedesign/ObjectVisual.cs','ObjectRedesign/ObjectPresentation.cs','ObjectRedesign/ObjectHitStopRules.cs','Scripts/CrabFlightGame.cs']}
allowed|={'games/flight/crab-flight/Assets/ObjectRedesign/'+name+suffix for name in ['ObjectCaptureTimeline','ObjectReadability','ObjectCaptureView'] for suffix in ['.cs','.cs.meta']}
check(set(changed)<=allowed, 'only requested presentation delta and historical Rules delta; other tracked game Assets unchanged')
driver = Path('/tmp/build-object.sh')
# The driver is local orchestration. Keep a committed patch for reproducing the
# fix elsewhere, without executing its build/deploy stages in this offline job.
before = (INPUT / 'before-build-object.sh.txt').read_text()
check('cp /tmp/PortNativeReviewOpening.cs' in before, 'original builder overwrites the archived helper')
check('WriteActionEvents' not in (INPUT / 'before-PortNativeReviewOpening.cs.txt').read_text(), 'injected legacy helper has no action writer')
if driver.exists():
    after = driver.read_text()
    check('cp /tmp/PortNativeReviewOpening.cs' not in after and 'run_native_review.py' in after, 'local builder uses guarded archived review runner')
    run(['zsh', '-n', driver], 'build-driver-syntax.log')
else:
    raise AssertionError('local build driver unavailable; apply the saved patch before checking this integration')

spec = importlib.util.spec_from_file_location('review_runner', ROOT / RUNNER)
runner = importlib.util.module_from_spec(spec)
spec.loader.exec_module(runner)
entry_results = {}
with tempfile.TemporaryDirectory(prefix='kani brakeground offline ') as temporary:
    tmp = Path(temporary)
    fixture = tmp / 'source copy'
    project = fixture / PROJECT
    (project / 'Assets/StreamingAssets').mkdir(parents=True)
    (project / 'Assets/StreamingAssets/feel.json').write_text('{}')
    (fixture / HELPER).parent.mkdir(parents=True)
    shutil.copy2(ROOT / HELPER, fixture / HELPER)
    (fixture / RUNNER).parent.mkdir(parents=True)
    shutil.copy2(ROOT / RUNNER, fixture / RUNNER)
    dll = tmp / 'entry.dll'
    run([SDK / 'NetCoreRuntime/dotnet', SDK / 'DotNetSdkRoslyn/csc.dll', '-nologo', '-nostdlib', '-target:exe', '-out:' + str(dll),
         *['-r:' + str(p) for p in RT.glob('*.dll')], ROOT / rules, ROOT / HELPER, INPUT / 'ReviewContracts.cs'], 'review-entry-compile.log')
    dll.with_suffix('.runtimeconfig.json').write_text(json.dumps({'runtimeOptions': {'tfm': 'net6.0', 'framework': {'name': 'Microsoft.NETCore.App', 'version': RT.name}}}))
    # This explicitly labelled stand-in invokes the real C# review entry with
    # adapters; it is not a Unity process or a native physics/rendering test.
    shim = tmp / 'offline-unity-adapter'
    shim.write_text('#!' + sys.executable + '\nimport os,sys,subprocess\na=sys.argv\nassert a[a.index("-executeMethod")+1]=="PortNativeReview.Run"\nos.chdir(a[a.index("-projectPath")+1])\nsys.exit(subprocess.call(' + repr([str(SDK / 'NetCoreRuntime/dotnet'), str(dll)]) + '))\n')
    shim.chmod(0o700)
    input_path = tmp / 'accepted input fixture.json'
    input_path.write_text('{"offlineFixture":true,"untouched":"input payload"}')
    for name, count in [('hits', 48), ('empty', 0)]:
        output = tmp / ('review output ' + name)
        env = dict(os.environ, PORT_REVIEW_INPUT=str(input_path), PORT_REVIEW_OUTPUT=str(output), KANI_REVIEW_FIXTURE_HITS=str(count))
        cmd = [sys.executable, fixture / RUNNER, '--unity', shim, '--log-file', tmp / (name + '.log')]
        result = run(cmd, 'review-entry-' + name + '.log', cwd=tmp, env=env)
        receipt = json.loads(result.stdout.splitlines()[-1])
        check(receipt['events'] == count and Path(receipt['path']) == (output / 'camon/action-events.jsonl').resolve(), name + ': guarded CLI and actual Run produce exact output destination/count')
        check(receipt['helperSha256'] == hashlib.sha256(old(HELPER)).hexdigest(), name + ': exact committed helper was executed')
        entry_results[name] = next(line for line in result.stdout.splitlines() if 'passed / 0 failed' in line)
        saved = OUT / ('offline-review-' + name)
        (saved / 'camon').mkdir(parents=True, exist_ok=True)
        for filename in ['action-events.jsonl', 'route.csv', 'break-events.jsonl', 'roles.json']:
            shutil.copy2(output / 'camon' / filename, saved / 'camon' / filename)
        shutil.copy2(output / 'receipts.json', saved / 'receipts.json')
        check(not any(saved.rglob('*.png')), 'adapter captures are never saved as screenshot evidence: ' + name)
        rerun = subprocess.run(list(map(str, cmd)), cwd=tmp, env=env, capture_output=True, text=True)
        check(rerun.returncode != 0 and 'fresh PORT_REVIEW_OUTPUT' in rerun.stderr, name + ': existing output cannot be overwritten')
    # Reproduce the actual failure as a stale injected helper. The guarded
    # runner must stop before invoking even the offline engine adapter.
    (fixture / HELPER).write_bytes((INPUT / 'before-PortNativeReviewOpening.cs.txt').read_bytes())
    stale_output = tmp / 'stale output'
    env['PORT_REVIEW_OUTPUT'] = str(stale_output)
    stale = subprocess.run(list(map(str, cmd)), cwd=tmp, env=env, capture_output=True, text=True)
    (OUT / 'stale-helper-rejection.log').write_text(stale.stdout + stale.stderr)
    check(stale.returncode != 0 and 'Stale PortNativeReview' in stale.stderr and not stale_output.exists(), 'stale helper is rejected before running the review')
    check(runner.verify_output(OUT / 'offline-review-hits')['events'] == 48, 'persisted review output reads back as 48 events')
    broken = tmp / 'broken'
    shutil.copytree(OUT / 'offline-review-hits', broken)
    event_file = broken / 'camon/action-events.jsonl'
    event_file.unlink()
    try:
        runner.verify_output(broken)
        raise AssertionError('missing output accepted')
    except FileNotFoundError:
        check(True, 'review success without action-events is rejected')
    good_event = json.loads((OUT / 'offline-review-hits/camon/action-events.jsonl').read_text().splitlines()[0])
    invalid = [dict(good_event, kind='unknown'), dict(good_event, time=-1), dict(good_event, time=float('nan')),
               dict(good_event, position={'x': 1, 'y': 2}), dict(good_event, time=True)]
    for i, event in enumerate(invalid):
        event_file.write_text(json.dumps(event) + '\n')
        try:
            runner.verify_output(broken)
            raise AssertionError('malformed event accepted')
        except ValueError:
            check(True, 'invalid kind/coordinate/time rejected case ' + str(i))
    event_file.write_text(json.dumps(dict(good_event, time=2)) + '\n' + json.dumps(dict(good_event, time=1)) + '\n')
    try:
        runner.verify_output(broken)
        raise AssertionError('out of order time accepted')
    except ValueError:
        check(True, 'out-of-order action times rejected')

(OUT / 'review-contracts.json').write_text(json.dumps({'base': BASE, 'staticPassed': len(checks), 'staticFailed': 0,
    'entryResults': entry_results, 'checks': checks, 'scope': 'Offline CLI and production Run/RunOne/writer with game/Unity/serialization adapters. No actual rendering, PhysX, Unity or native replay.'}, indent=2) + '\n')
print(len(checks), 'static passed / 0 failed')
