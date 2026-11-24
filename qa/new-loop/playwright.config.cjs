const path=require('node:path');
module.exports={testDir:__dirname,testMatch:'new-loop.spec.cjs',workers:1,timeout:60000,
 outputDir:path.resolve(__dirname,'../../docs/new-loop/playwright-artifacts'),
 use:{headless:true,launchOptions:{executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'}},
 reporter:[['list'],['json',{outputFile:path.resolve(__dirname,'../../docs/new-loop/playwright.json')}]]};
