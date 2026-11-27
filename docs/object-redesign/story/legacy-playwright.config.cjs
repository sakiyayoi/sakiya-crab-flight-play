const path=require('node:path');
module.exports={testDir:path.resolve(__dirname,'../../..'),testMatch:['docs/object-redesign/object.spec.cjs','qa/juice-layer/juice.spec.cjs'],
 grep:/real rules|existing Effekseer|large thumb|jets by|dive pointer|legacy, charge|result shows|touchscreen|existing WAV|fallback receives|audio generation|HUD/,
 timeout:120000,workers:1,outputDir:'/tmp/kani-story-offline/legacy-playwright',
 use:{hasTouch:true,browserName:'chromium',headless:true,launchOptions:{executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'}},
 reporter:[['list'],['json',{outputFile:__dirname+'/legacy-playwright.json'}]]};
