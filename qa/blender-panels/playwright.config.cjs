const path=require('node:path');
module.exports={testDir:__dirname,testMatch:'panels.spec.cjs',workers:1,timeout:180000,
outputDir:path.resolve(__dirname,'../../.git/blender-panels-qa/results'),
reporter:[['list'],['json',{outputFile:path.resolve(__dirname,'../../../evidence/blender-panels/playwright.json')}]]};
