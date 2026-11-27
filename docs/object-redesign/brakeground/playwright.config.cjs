const previous=require('../placecute/playwright.config.cjs');
module.exports={...previous,
 testMatch:[...previous.testMatch,'docs/object-redesign/brakeground/brakeground.spec.cjs'],
 grep:new RegExp(previous.grep.source+'|brakeground'),
 outputDir:'/tmp/kani-brakeground-playwright',
 reporter:[['list'],['json',{outputFile:__dirname+'/playwright.json'}]]};
