const {test,expect}=require('playwright/test');
const cp=require('node:child_process'),path=require('node:path');
test('placecute scaled faces and Native action writer execute with exact source boundaries',()=>{
 const out=cp.execFileSync('python3',['docs/object-redesign/placecute/contracts.py'],{cwd:path.resolve(__dirname,'../../..'),encoding:'utf8',timeout:160000});
 expect(out).toContain('66 passed / 0 failed');expect(out).toContain('51 passed / 0 failed');expect(out).toContain('static passed / 0 failed');
});
