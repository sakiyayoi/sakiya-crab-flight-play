const path = require('node:path');
module.exports = {
  testDir: __dirname, testMatch: 'waterfix.spec.cjs', workers: 1,
  outputDir: path.resolve(__dirname, '../../docs/port-prototype/playwright-artifacts/waterfix'),
  reporter: [['list'], ['json', {outputFile: path.resolve(__dirname, '../../docs/port-prototype/waterfix/playwright.json')}]],
};
