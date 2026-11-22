// No browser is launched here. New Unity runtime validation belongs to Chloe.
const { test, expect } = require('playwright/test');
const { execFileSync, spawnSync } = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(__dirname, '../..');
const base = 'd288db27c604ba716c2ef7af921ea0975e28efe1';

test('offline placement generation and 9 geometry regression tests', async ({}, testInfo) => {
  const result = spawnSync('python3', ['tools/placement-fix/test_placements.py'], { cwd: root, encoding: 'utf8', timeout: 55000 });
  await testInfo.attach('offline-tests', { body: result.stdout+result.stderr, contentType: 'text/plain' });
  expect(result.error).toBeUndefined();
  expect(result.status, result.stdout+result.stderr).toBe(0);
});

test('protected tracked files remain byte-identical to ui-simplify', async () => {
  const allowed = new Set([
    'tasks.md',
    'games/flight/crab-flight/Assets/NeonDensity/Resources/NeonDensity/Placements.json',
    'games/flight/crab-flight/Assets/NeonProto/NeonSectionArt.cs',
  ]);
  const files = execFileSync('git', ['ls-tree', '-r', '--name-only', base], { cwd: root, encoding: 'utf8' }).trim().split('\n');
  const changed = new Set(execFileSync('git', ['diff', '--name-only', base, '--'], { cwd: root, encoding: 'utf8' }).trim().split('\n'));
  let checked = 0;
  for (const file of files) {
    if (allowed.has(file)) continue;
    // Git's working-tree diff compares every baseline-tracked byte, including UI/feel,
    // physics, mall data, shared generation code, scenes and project settings.
    expect(changed.has(file), file).toBe(false);
    checked++;
  }
  expect(checked).toBeGreaterThan(600);
  const output = path.join(root, '.git/placement-fix-qa/protected-files.json');
  fs.writeFileSync(output, JSON.stringify({ base, checked, pass: true, scope: 'all baseline tracked files except 3 authorized modifications' }, null, 2));
});
