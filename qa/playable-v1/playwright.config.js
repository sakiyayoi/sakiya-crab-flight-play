// カニとばし ZIP 受入テスト用 config。CRAB_BUILD_DIR = index.html のあるフォルダ（既定: ./build）
const { defineConfig } = require('@playwright/test');
const PORT = Number(process.env.CRAB_PORT || 8000);
module.exports = defineConfig({
  testDir: '.',
  testMatch: /crab-acceptance\.spec\.js/,
  workers: 1,
  timeout: 60 * 60 * 1000,
  reporter: [['list'], ['json', { outputFile: 'qa-out/results.json' }]],
  use: {
    baseURL: `http://127.0.0.1:${PORT}/`,
    // CI/ヘッドレスで WebGL2 を出すため（実機 GPU があるなら不要）
    launchOptions: { executablePath: process.env.CRAB_CHROME_EXECUTABLE || (process.platform==='darwin'?'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome':undefined), args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--ignore-gpu-blocklist', '--enable-precise-memory-info'] },
  },
  webServer: {
    command: `python3 -m http.server ${PORT} --bind 127.0.0.1`,
    cwd: process.env.CRAB_BUILD_DIR || './build',
    url: `http://127.0.0.1:${PORT}/`,
    reuseExistingServer: false,
  },
});
