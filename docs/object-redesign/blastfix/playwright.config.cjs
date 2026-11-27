const path=require('node:path');
module.exports={testDir:path.resolve(__dirname,'../../..'),
 testMatch:['docs/object-redesign/story/story.spec.cjs','docs/object-redesign/object.spec.cjs','qa/juice-layer/juice.spec.cjs'],
 grep:/exact caption|latest only|effects off|native flags|unknown kind|caption fits|death stays|real rules|existing Effekseer|large thumb|jets by|dive pointer|legacy, charge|result shows|touchscreen|existing WAV|fallback receives|audio generation|HUD/,
 timeout:120000,workers:1,outputDir:'/tmp/kani-blastfix-offline/playwright',
 use:{hasTouch:true,browserName:'chromium',headless:true,launchOptions:{executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'}},
 reporter:[['list'],['json',{outputFile:__dirname+'/playwright.json'}]]};
