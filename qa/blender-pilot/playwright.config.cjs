const path=require('node:path');
module.exports={testDir:__dirname,testMatch:'pilot.spec.cjs',workers:1,timeout:60000,outputDir:path.resolve(__dirname,'../../.git/blender-pilot-qa/results'),reporter:[['list'],['json',{outputFile:path.resolve(__dirname,'../../../evidence/blender-pilot/playwright.json')}]]};
