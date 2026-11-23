// Offline evidence checks only: these do not render Unity or replay a throw.
const {test, expect} = require('playwright/test');
const fs = require('node:fs'), path = require('node:path'), cp = require('node:child_process'), crypto = require('node:crypto');
const root = path.resolve(__dirname, '../..');
const project = 'games/flight/crab-flight/';
const base = '5be0ca45214150e58981d0e947ec56c6a7708011';
const sha = value => crypto.createHash('sha256').update(value).digest('hex');
const original = file => cp.execFileSync('git', ['show', base + ':' + file], {cwd: root});

test('waterfix: current runtime sources match the successful managed compile', () => {
  const receipt = JSON.parse(fs.readFileSync(path.join(root, 'docs/port-prototype/waterfix/compile.json')));
  expect(receipt.exitCode).toBe(0); expect(receipt.errors).toBe(0);
  expect(Object.keys(receipt.sourceHashes).length).toBeGreaterThan(100);
  for (const [file, hash] of Object.entries(receipt.sourceHashes))
    expect(sha(fs.readFileSync(path.join(root, project, file))), file).toBe(hash);
});

test('waterfix: simulation, camera, roles, placement and static inlet inputs remain byte exact', () => {
  const allowed = new Set(['Assets/PortPrototype/PortHeroVisual.cs', 'Assets/NeonProto/NeonCrabVisual.cs', 'Assets/Scripts/CrabFlightGame.cs']);
  const files = cp.execFileSync('git', ['ls-tree', '-r', '--name-only', base, project + 'Assets'], {cwd: root}).toString().trim().split('\n');
  // All tracked source, shaders and authored JSON, not just a selected physics file.
  for (const file of files.filter(f => /\.(cs|shader|json)$/.test(f) && !allowed.has(f.slice(project.length)))) {
    // Stream large authored JSON through Git rather than buffering it in Node.
    const currentHash = cp.execFileSync('git', ['hash-object', file], {cwd: root}).toString().trim();
    const baseHash = cp.execFileSync('git', ['rev-parse', base + ':' + file], {cwd: root}).toString().trim();
    expect(currentHash, file).toBe(baseHash);
  }
  const game = project + 'Assets/Scripts/CrabFlightGame.cs';
  const current = fs.readFileSync(path.join(root, game), 'utf8');
  // The only controller difference is a render-visibility call after its existing late stamp.
  expect(current.replace('neonCrab?.SyncTankVisibility(phase);', '')).toBe(original(game).toString());
  const input = 'tools/blender/accepted-input.json';
  expect(sha(fs.readFileSync(path.join(root, input)))).toBe(sha(original(input)));
});
