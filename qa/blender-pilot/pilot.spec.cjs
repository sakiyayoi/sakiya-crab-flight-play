const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const source=path.resolve(__dirname,'../..'),root=path.resolve(source,'..'),ev=path.join(root,'evidence/blender-pilot');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8'));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const candidate=path.join(ev,process.env.BLENDER_PILOT_CANDIDATE||'native-final');
test('native fixed-input replay reaches result and retry with unchanged route and destruction',()=>{
 const a=read(path.join(ev,'native-baseline/receipt.json')),b=read(path.join(candidate,'receipt.json'));
 for(const r of[a,b]){expect(r.pass).toBe(true);expect(r.state.distance).toBeGreaterThanOrEqual(2500);expect(r.breaks).toBeGreaterThanOrEqual(96);expect(r.state.physicsFault).toBe('');expect(r.retryReady).toBe(true);expect(r.seed).toBe(42043);expect(r.inputSha256).toBe(sha(path.join(root,'accepted-input.json')));}
 expect(b.state.distance).toBe(a.state.distance);expect(b.breaks).toBe(a.breaks);expect(b.ticks).toBe(a.ticks);
 expect(fs.readFileSync(path.join(candidate,'route.csv'),'utf8')).toBe(fs.readFileSync(path.join(ev,'native-baseline/route.csv'),'utf8'));
 expect(fs.readFileSync(path.join(candidate,'break-events.jsonl'),'utf8')).toBe(fs.readFileSync(path.join(ev,'native-baseline/break-events.jsonl'),'utf8'));
});
test('74 mall placements only, all 22 families, all collider/mass/threshold signatures unchanged',()=>{
 const a=read(path.join(ev,'native-baseline/targets.json')).targets,b=read(path.join(candidate,'targets.json')).targets;
 expect(b.length).toBe(a.length);const old=new Map(a.map(x=>[x.id,x]));const changed=[];
 for(const r of b){const q=old.get(r.id);expect(r.physics,r.id).toBe(q.physics);expect(r.asset).toBe(q.asset);expect(r.position).toEqual(q.position);expect(r.scale).toEqual(q.scale);
 if(r.position.x>=28&&r.position.x<=135){expect(r.blenderParts,r.id).toBe(1);expect(r.appearance).not.toBe(q.appearance);changed.push(r);}else{expect(r.blenderParts,r.id).toBe(0);expect(r.appearance,r.id).toBe(q.appearance);}}
 expect(changed.length).toBe(74);expect(new Set(changed.map(r=>r.asset)).size).toBe(22);
 const inventory=read(path.join(source,'tools/blender/families.json')).families;
 for(const[f,info]of Object.entries(inventory))expect(changed.filter(r=>r.asset===f).length,f).toBe(info.count);
});
test('FBX dimensions and bottom-centre origins match legacy metre dimensions',()=>{
 const inventory=read(path.join(source,'tools/blender/families.json')).families;
 const imported=read(path.join(candidate,'imports.json')).models;
 expect(imported.length).toBe(22);
 for(const m of imported){const f=inventory[m.family];for(const[axis,i]of[['x',0],['y',1],['z',2]]){expect(m.max[axis]-m.min[axis],m.family+axis).toBeCloseTo(f.max[i]-f.min[i],3);expect(m.min[axis],m.family+axis).toBeCloseTo(axis==='y'?0:-(f.max[i]-f.min[i])/2,3);}
 if(m.family==='vending_unit')expect(m.frontFeatureMeanZ).toBeLessThan(0);
 expect(m.parts).toBe(f.shards.length+1);expect(fs.statSync(path.join(source,'tools/blender',m.family+'.blend')).size).toBeGreaterThan(1000);expect(fs.statSync(path.join(source,'games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels',m.family+'.fbx')).size).toBeGreaterThan(1000);
 }
});
test('protected placements, physics, feel, UI and preexisting scene bytes retained',()=>{
 const before=read(path.join(ev,'protected-before.json'));const allowed=new Set(['games/flight/crab-flight/Assets/Scripts/City2Data.cs']);
 for(const[f,h]of Object.entries(before))if(!allowed.has(f))expect(sha(path.join(source,f)),f).toBe(h);
 const names=['aim.png','aim-portrait.png','flight-32.png','flight-57.png','flight-82.png','flight-105.png','flight-123.png','result.png','retry.png'];
 for(const n of names){const buf=fs.readFileSync(path.join(candidate,n));expect(buf.subarray(0,8).toString('hex')).toBe('89504e470d0a1a0a');expect(buf.length).toBeGreaterThan(10000);}
});
