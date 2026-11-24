// Offline contracts: deliberately no browser/player fixture or fake gameplay.
const {test,expect}=require('playwright/test');
const {spawnSync}=require('node:child_process');
const path=require('node:path');
const root=path.resolve(__dirname,'../..');
for(const [name,file] of [
 ['opening geometry, negative controls and protected source','tools/grammar-opening/test_layout.py'],
 ['landmark projection at four viewport sizes','tools/grammar-opening/visibility.py'],
 ['existing placement-fix regression (9 tests)','tools/placement-fix/test_placements.py'],
 ['actual C# layout and existing port contracts','tools/grammar-opening/contracts.py']]){
 test(name,async({},testInfo)=>{
  const r=spawnSync('python3',[file],{cwd:root,encoding:'utf8',timeout:110000,maxBuffer:8*1024*1024});
  await testInfo.attach('contract-output',{body:r.stdout+r.stderr,contentType:'text/plain'});
  expect(r.error).toBeUndefined();expect(r.status,r.stdout+r.stderr).toBe(0);
 });
}
