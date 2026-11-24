const path=require('node:path');
const {execFileSync}=require('node:child_process');
const root=path.resolve(__dirname,'../..');
const output=execFileSync('git',['rev-parse','--git-path','juice-layer-playwright'],{cwd:root,encoding:'utf8'}).trim();
module.exports={testDir:__dirname,testMatch:'juice.spec.cjs',timeout:120000,workers:1,
 outputDir:path.resolve(root,output),use:{browserName:'chromium',headless:true,launchOptions:{executablePath:'/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'}},
 reporter:[['list'],['json',{outputFile:path.join(root,'docs/juice-layer/playwright.json')}]]};
