const {test,expect}=require('playwright/test');
const cp=require('node:child_process'),path=require('node:path');
test('camera and presentation hold execute real methods without changing fixed-step code',()=>{
 const out=cp.execFileSync('python3',['docs/object-redesign/camhit/contracts.py'],{cwd:path.resolve(__dirname,'../../..'),encoding:'utf8',timeout:160000});
 expect(out).toContain('154 passed / 0 failed');expect(out).toContain('10 static passed / 0 failed');
});
