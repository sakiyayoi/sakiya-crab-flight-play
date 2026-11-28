const previous=require('../brakeground/playwright.config.cjs');
module.exports={...previous,
 outputDir:'/tmp/kani-fisherman-playwright',
 reporter:[['list'],['json',{outputFile:__dirname+'/playwright.json'}]]};
