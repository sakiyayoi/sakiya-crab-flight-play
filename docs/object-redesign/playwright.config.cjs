module.exports={testDir:__dirname,testMatch:'object.spec.cjs',timeout:120000,workers:1,
 outputDir:'/tmp/kani-object-offline/playwright',
 use:{hasTouch:true,browserName:'chromium',headless:true,launchOptions:{executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'}},
 reporter:[['list'],['json',{outputFile:__dirname+'/playwright.json'}]]};
