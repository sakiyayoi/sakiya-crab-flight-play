const path=require('node:path');
module.exports={testDir:__dirname,testMatch:'port.spec.cjs',workers:1,timeout:60000,
 outputDir:path.resolve(__dirname,'../../docs/port-prototype/playwright-artifacts'),
 use:{headless:true,launchOptions:{executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'}},
 reporter:[['list'],['json',{outputFile:path.resolve(__dirname,'../../docs/port-prototype/playwright.json')}]]};
