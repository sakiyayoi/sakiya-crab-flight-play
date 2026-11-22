const path = require('node:path');
module.exports = {
  testDir: __dirname,
  testMatch: 'placement.spec.cjs',
  timeout: 60000,
  workers: 1,
  outputDir: path.resolve(__dirname, '../../.git/placement-fix-qa/results'),
  reporter: [['list'], ['json', { outputFile: path.resolve(__dirname, '../../.git/placement-fix-qa/playwright.json') }]],
};
