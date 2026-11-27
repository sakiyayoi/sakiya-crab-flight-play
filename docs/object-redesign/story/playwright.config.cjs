module.exports={testDir:__dirname,testMatch:'story.spec.cjs',timeout:60000,workers:1,
 outputDir:'/tmp/kani-story-offline/playwright',
 use:{hasTouch:true,browserName:'chromium',headless:true,launchOptions:{executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'}},
 reporter:[['list'],['json',{outputFile:__dirname+'/playwright.json'}]]};
