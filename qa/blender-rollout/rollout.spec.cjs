const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),cp=require('node:child_process');
const source=path.resolve(__dirname,'../..'),root=path.resolve(source,'..'),ev=path.join(root,'evidence/blender-rollout');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8'));
const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const candidate=path.join(ev,process.env.BLENDER_ROLLOUT_CANDIDATE||'native-final02');
const inventory=()=>read(path.join(source,'tools/blender/rollout-families.json'));
test('same pilot replay: exact distance, all route bytes, break events, result and retry',()=>{
 const a=read(path.join(ev,'native-baseline/receipt.json')),b=read(path.join(candidate,'receipt.json'));
 for(const r of[a,b]){expect(r.pass).toBe(true);expect(r.state.physicsFault).toBe('');expect(r.state.phase).toBe('result');expect(r.retryReady).toBe(true);expect(r.seed).toBe(42043);expect(r.inputSha256).toBe(sha(path.join(root,'accepted-input.json')));expect(r.error).toBe('');}
 expect(b.state.distance).toBe(a.state.distance);expect(b.breaks).toBe(a.breaks);expect(b.ticks).toBe(a.ticks);
 expect(a.revision).toBe('blender-pilot-1');expect(b.revision).toBe('blender-rollout-1');
 for(const file of ['route.csv','break-events.jsonl'])expect(fs.readFileSync(path.join(candidate,file))).toEqual(fs.readFileSync(path.join(ev,'native-baseline',file)));
});
test('every actual registered City target retains physics and gains exactly one Blender intact renderer',()=>{
 const a=read(path.join(ev,'native-baseline/targets.json')).targets,b=read(path.join(candidate,'targets.json')).targets;
 const old=new Map(a.map(x=>[x.id,x]));expect(old.size).toBe(a.length);expect(b.length).toBe(a.length);expect(new Set(b.map(x=>x.id)).size).toBe(b.length);
 for(const r of b){const q=old.get(r.id);expect(q,r.id).toBeDefined();expect(r.physics,r.id).toBe(q.physics);expect(r.asset).toBe(q.asset);expect(r.position).toEqual(q.position);expect(r.scale).toEqual(q.scale);expect(r.blenderParts,r.id).toBe(1);if(q.blenderParts===1)expect(r.appearance,r.id).toBe(q.appearance);}
 // Legacy CityContactTargets also includes LifeContactsR6's 34 people/birds.
 // Compare that aggregate with the actual pilot receipt; use entity IDs for City coverage.
 const world=read(path.join(candidate,'world.json')),baseline=read(path.join(ev,'native-baseline/receipt.json'));
 expect(world.cityBreakTargets).toBe(baseline.state.cityTargets);expect(world.targets.length).toBe(b.length);expect(world.targets.every(t=>t.registered)).toBe(true);
 expect(b.length).toBe(inventory().total);expect(new Set(b.map(r=>r.asset)).size).toBe(inventory().familyCount);
});
test('all sectors and directions have zero unmodelled effective families, counted from current entities',()=>{
 const doc=inventory(),rows=read(path.join(candidate,'targets.json')).targets,seen=new Set();
 expect(doc.groups.length).toBe(18);expect(doc.sourceSha256).toBe(sha(path.join(candidate,'targets.json')));
 for(const group of doc.groups){const selected=rows.filter(r=>group.ids.includes(r.id));expect(selected.length).toBe(group.count);expect(new Set(selected.map(r=>r.asset)).size).toBe(group.familyCount);expect(group.unmodelledNow).toEqual([]);
  for(const r of selected){expect(seen.has(r.id)).toBe(false);seen.add(r.id);expect((r.position.x>=0)?1:-1).toBe(group.direction);expect(r.blenderParts).toBe(1);}
  for(const[f,count]of Object.entries(group.families))expect(selected.filter(r=>r.asset===f).length).toBe(count);
 }
 expect(seen.size).toBe(rows.length);
});
test('all family FBX metre dimensions, bottom centre origins, legacy shard IDs and blend sources',()=>{
 const families=inventory().families,models=read(path.join(candidate,'imports.json')).models;expect(models.length).toBe(Object.keys(families).length);
 for(const m of models){const f=families[m.family];for(const[axis,i]of[['x',0],['y',1],['z',2]]){expect(m.max[axis]-m.min[axis],m.family+axis).toBeCloseTo(f.max[i]-f.min[i],3);expect(m.min[axis],m.family+axis).toBeCloseTo(axis==='y'?0:-(f.max[i]-f.min[i])/2,3);}
  expect(m.parts).toBe(f.shards.length+1);if(m.family==='vending_unit')expect(m.frontFeatureMeanZ).toBeLessThan(0);
  for(const p of [path.join(source,'tools/blender',m.family+'.blend'),path.join(source,'games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels',m.family+'.fbx')])expect(fs.statSync(p).size).toBeGreaterThan(1000);
 }
});
test('protected placement, physics, feel, UI, scene and original pilot models remain byte-identical',()=>{
 for(const[f,h]of Object.entries(read(path.join(ev,'protected-before.json'))))expect(sha(path.join(source,f)),f).toBe(h);
 const base=read(path.join(ev,'start.json')).baseCommit;
 for(const[f,d]of Object.entries(inventory().families)){if(!d.pilotModel)continue;
  for(const p of ['tools/blender/'+f+'.blend','games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels/'+f+'.fbx'])expect(fs.readFileSync(path.join(source,p))).toEqual(cp.execFileSync('git',['show',base+':'+p],{cwd:source,maxBuffer:50*1024*1024}));
 }
});
test('native sector screenshots cover both directions; Blender sheets and real replay portraits exist',()=>{
 const sectors=[...new Set(inventory().groups.map(g=>g.sector))];const png=(file,w,h)=>{const b=fs.readFileSync(file);expect(b.subarray(0,8).toString('hex')).toBe('89504e470d0a1a0a');expect(b.length).toBeGreaterThan(10000);if(w){expect(b.readUInt32BE(16)).toBe(w);expect(b.readUInt32BE(20)).toBe(h);}};
 for(const s of sectors){png(path.join(ev,s+'-contact-sheet.png'));for(const direction of ['positive','negative'])png(path.join(candidate,'sector-'+s+'-'+direction+'.png'),1280,800);}
 const views=fs.readFileSync(path.join(candidate,'sector-views.jsonl'),'utf8').trim().split('\n').map(JSON.parse);expect(views.length).toBe(18);
 for(const g of inventory().groups){const v=views.find(v=>v.sector===g.sector&&v.direction===(g.direction>0?'positive':'negative'));expect(v).toBeDefined();if(g.count>0)expect(v.visible).toBeGreaterThan(0);}
 for(const mark of [105,240,790,1100,1370,2160,2800]){png(path.join(candidate,'flight-'+mark+'.png'),1280,800);png(path.join(candidate,'flight-'+mark+'-portrait.png'),390,844);}
});
