const previous=require('../fisherman/playwright.config.cjs');
module.exports={...previous,
 testMatch:[...previous.testMatch,'docs/object-redesign/readability/readability.spec.cjs'],
 grep:new RegExp(previous.grep.source+'|readability'),
 outputDir:'/tmp/kani-readability-playwright',
 reporter:[['list'],['json',{outputFile:__dirname+'/playwright.json'}]]};
