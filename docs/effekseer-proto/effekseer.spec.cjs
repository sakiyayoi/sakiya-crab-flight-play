const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process');
const root=path.resolve(__dirname,'../..');
const run=p=>cp.execFileSync('python3',[p],{cwd:root,encoding:'utf8',timeout:120000});
test('real bridge and existing juice policies: cap, priority, dispatch, lifecycle',()=>{
 const out=run('docs/effekseer-proto/contracts.py');expect(out).toContain('41 passed / 0 failed');expect(out).toContain('33 passed / 0 failed');
});
test('same source, protected tree and complete imported sample paths',()=>{
 const receipt=JSON.parse(run('docs/effekseer-proto/verify.py'));expect(receipt.protectedFiles).toBe(26);expect(receipt.sampleCount).toBe(32);expect(receipt.directResourceCount).toBe(9);expect(receipt.noSampleAudio).toBe(true);
});
test('feel callbacks wire initialization, pause, settings and result cleanup',()=>{
 const s=fs.readFileSync(path.join(root,'games/flight/crab-flight/Assets/Scripts/FeelDirectorR27.cs'),'utf8');
 expect(s).toContain('ConfigureJuiceBurst();ResetJuice();Report();');
 expect(s).toContain('void BeginFeelFrameR27(){if(!FeelOnR27)return;SyncJuiceBurst();');
 expect(s).toContain('void FinishFeelFrameR27(float dt){if(!FeelOnR27)return;SyncJuiceBurst();');
 expect(s).toContain('}SyncJuiceBurst();Report();');
 const host=fs.readFileSync(path.join(root,'games/flight/web-template/feel.js'),'utf8');expect(host).toContain('LoadFeelR27');
});
