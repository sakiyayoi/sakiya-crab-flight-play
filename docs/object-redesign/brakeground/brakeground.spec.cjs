const {test,expect}=require('playwright/test');
const cp=require('node:child_process'),path=require('node:path');
test('brakeground review entry writes camon action events through the guarded runner',()=>{
 const out=cp.execFileSync('python3',['docs/object-redesign/brakeground/contracts.py'],{
  cwd:path.resolve(__dirname,'../../..'),encoding:'utf8',timeout:160000});
 expect(out).toContain('static passed / 0 failed');
});
