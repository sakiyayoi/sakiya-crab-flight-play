// ハーネスのテスト（harness/tests）用の設定。ゲートが KANI_BUILD_DIR（ZIP を解いた場所）と KANI_PORT を渡す。
// 手で動かすとき：KANI_BUILD_DIR=./build KANI_VERSION=$(cat VERSION) npx playwright test -c harness/playwright.harness.config.js
const path = require('path');
const { defineConfig } = require('@playwright/test');
const PORT = Number(process.env.KANI_PORT || 8010);
module.exports = defineConfig({
  testDir: path.join(__dirname, 'tests'),
  testMatch: /(feel|flow)\.spec\.js$/,
  workers: 1,
  retries: 0,
  timeout: 20 * 60 * 1000,
  outputDir: path.join(__dirname, 'out', 'test-results'),
  reporter: [['list'], ['json', { outputFile: path.join(__dirname, 'out', 'pw.json') }]],
  use: {
    baseURL: `http://127.0.0.1:${PORT}/`,
    // ヘッドレスで WebGL2 を出すため（元の harness-for-lily と同じ）。実機 GPU で見るときは KANI_HEADED=1
    headless: process.env.KANI_HEADED !== '1',
    launchOptions: { executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--ignore-gpu-blocklist'] },
  },
  webServer: {
    command: `python3 -m http.server ${PORT} --bind 127.0.0.1`,
    cwd: process.env.KANI_BUILD_DIR || path.join(__dirname, 'out', 'build'),
    url: `http://127.0.0.1:${PORT}/`,
    reuseExistingServer: false,
  },
});
