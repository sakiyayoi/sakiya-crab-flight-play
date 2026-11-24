const path = require('node:path');
const {execFileSync} = require('node:child_process');
const root=path.resolve(__dirname,'../..');
const output=execFileSync('git',['rev-parse','--git-path','grammar-opening-playwright'],{cwd:root,encoding:'utf8'}).trim();
module.exports={testDir:__dirname,testMatch:'opening.spec.cjs',timeout:120000,workers:1,
 outputDir:path.resolve(root,output),reporter:[['list'],['json',{outputFile:path.join(root,'docs/grammar-opening/playwright.json')}]]};
