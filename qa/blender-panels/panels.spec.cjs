const {test,expect}=require('playwright/test');
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),cp=require('node:child_process');
const source=path.resolve(__dirname,'../..'),root=path.resolve(source,'..'),ev=path.join(root,'evidence/blender-panels');
const read=p=>JSON.parse(fs.readFileSync(p,'utf8')),sha=p=>crypto.createHash('sha256').update(fs.readFileSync(p)).digest('hex');
const candidate=path.join(ev,process.env.BLENDER_PANELS_CANDIDATE||'native-final');
const baseline=path.join(ev,'native-baseline');
const nativeAvailable=fs.existsSync(path.join(candidate,'receipt.json'))&&fs.existsSync(path.join(baseline,'receipt.json'));
const nativeSkip=()=>test.skip(process.env.BLENDER_PANELS_NATIVE_BLOCKED==='license'&&!nativeAvailable,'BLOCKED: Unity Editor license unavailable; Native registry/replay/import/screenshots have not run.');const doc=()=>read(path.join(source,'tools/blender/panels-families.json'));
const rows=d=>read(path.join(d,'panels.json')).targets;
const panel=r=>['Restaurant','Core','Overdrive'].includes(r.system);
test('rollout -> panels: same fixed input, distance, every route byte and break event, result and retry',()=>{
 nativeSkip();
 const prev=read(path.join(root,'evidence/blender-rollout/native-final02/receipt.json'));
 const a=read(path.join(baseline,'receipt.json')),b=read(path.join(candidate,'receipt.json'));
 for(const r of[a,b]){expect(r.pass).toBe(true);expect(r.error).toBe('');expect(r.state.physicsFault).toBe('');expect(r.state.phase).toBe('result');expect(r.retryReady).toBe(true);expect(r.seed).toBe(42043);expect(r.inputSha256).toBe(sha(path.join(root,'accepted-input.json')));expect(r.state.distance).toBe(prev.state.distance);expect(r.breaks).toBe(prev.breaks);expect(r.ticks).toBe(prev.ticks);}
 for(const file of ['route.csv','break-events.jsonl']){
  expect(fs.readFileSync(path.join(candidate,file))).toEqual(fs.readFileSync(path.join(baseline,file)));
  expect(fs.readFileSync(path.join(candidate,file))).toEqual(fs.readFileSync(path.join(root,'evidence/blender-rollout/native-final02',file)));
 }
});
test('all registered objects and City targets preserve physical signatures, positions and scales',()=>{
 nativeSkip();
 const a=rows(baseline),b=rows(candidate);expect(b.length).toBe(a.length);expect(new Set(b.map(x=>x.id)).size).toBe(b.length);
 const old=new Map(a.map(x=>[x.id,x]));for(const r of b){const q=old.get(r.id);expect(q,r.id).toBeDefined();for(const k of ['type','system','physics','position','size','scale'])expect(r[k],r.id+k).toEqual(q[k]);}
 const aa=read(path.join(baseline,'targets.json')).targets,bb=read(path.join(candidate,'targets.json')).targets;expect(bb).toEqual(aa);
 const previous=read(path.join(root,'evidence/blender-rollout/native-final02/targets.json')).targets;expect(bb).toEqual(previous);
});
test('independent full registry census: no non-Blender destructible system, explicit people/bird exclusions',()=>{
 nativeSkip();
 const all=rows(candidate),before=rows(baseline),catalog=read(path.join(source,'games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels/panels-catalog.json'));
 const selected=all.filter(panel);expect(selected.length).toBe(before.filter(panel).length);expect(selected.length).toBe(40);expect(catalog.entries.map(r=>r.id).sort()).toEqual(selected.map(r=>r.id).sort());
 const systems={};for(const r of all){systems[r.system]=(systems[r.system]||0)+1;if(r.system==='Life'){expect(['person','bird']).toContain(r.type);continue;}expect(r.blenderRenderers,r.id).toBeGreaterThan(0);expect(r.legacyRenderers,r.id).toBe(0);}
 expect(systems).toEqual({City:1407,Core:4,Life:34,Restaurant:20,Overdrive:16});
 expect(all.filter(r=>r.system==='Life'&&r.type==='person').length).toBe(22);expect(all.filter(r=>r.system==='Life'&&r.type==='bird').length).toBe(12);
 for(const r of selected){const c=catalog.entries.find(c=>c.id===r.id);expect(r.blenderRenderers,r.id).toBe(1);expect(r.renderers[0]).toContain(c.model);expect(r.renderers[0]).toContain('Sakiya/BlenderPilotFlat');}
 const again=read(path.join(candidate,'panels-retry.json')).targets;expect(again).toEqual(all);
});
test('panel FBX meshes use metres, bottom centre, vertex colours, no collider/body and actual blend sources',()=>{
 nativeSkip();
 const definitions=doc().models,models=read(path.join(candidate,'panel-imports.json')).models;expect(models.length).toBe(Object.keys(definitions).length);
 for(const m of models){const d=definitions[m.family];expect(d).toBeDefined();expect(m.colliders).toBe(0);expect(m.bodies).toBe(0);expect(m.colours).toBe(m.vertices);
  for(const [axis,i] of [['x',0],['y',1],['z',2]]){expect(m.min[axis],m.family+axis).toBeCloseTo(d.min[i],3);expect(m.max[axis],m.family+axis).toBeCloseTo(d.max[i],3);}
  for(const file of [path.join(source,'tools/blender',m.family+'.blend'),path.join(source,'games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels',m.family+'.fbx')])expect(fs.statSync(file).size).toBeGreaterThan(1000);
 }
});
test('real replay panel debris also uses Blender; legacy body sizes and mass/damping stay identical',()=>{
 nativeSkip();
 const a=read(path.join(baseline,'fragments.json')).fragments,b=read(path.join(candidate,'fragments.json')).fragments;expect(a.length).toBeGreaterThan(0);expect(b.length).toBe(a.length);
 const normal=s=>s.replace(/"instanceID":-?\d+/g,'"instanceID":0');
 const panelIds=new Set(read(path.join(source,'games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels/panels-catalog.json')).entries.map(e=>e.id));
 const isPanelFragment=name=>{const m=/ for (.+)$/.exec(name);return m&&panelIds.has(m[1]);};
 for(let i=0;i<a.length;i++){expect(b[i].name).toBe(a[i].name);expect(normal(b[i].physics)).toBe(normal(a[i].physics));if(isPanelFragment(b[i].name)){expect(b[i].blenderRenderers,b[i].name).toBe(1);expect(b[i].legacyRenderers,b[i].name).toBe(0);}}
});
test('only two display hooks change the existing runtime; physical/UI/placement/scene files protected',()=>{
 const file='games/flight/crab-flight/Assets/Scripts/RestaurantPhysics.cs',base=read(path.join(ev,'start.json')).baseCommit;
 const old=cp.execFileSync('git',['show',base+':'+file],{cwd:source,maxBuffer:8*1024*1024}).toString();
 const current=fs.readFileSync(path.join(source,file),'utf8').replace('BlenderPanelOverride.Apply(panel.visual,id);','').replace('    BlenderPanelOverride.ApplyFragment(obj,p.id,p.type);\n','');expect(current).toBe(old);
 for(const[f,h]of Object.entries(read(path.join(ev,'protected-before.json')))){if(f===file)continue;expect(sha(path.join(source,f)),f).toBe(h);}
 const changed=cp.execFileSync('git',['diff',base,'--name-only','--','games/flight/crab-flight/Assets'],{cwd:source}).toString().trim().split('\n').filter(Boolean);
 expect(changed.filter(f=>!f.includes('/BlenderModels/')&&f!==file&&f!=='games/flight/crab-flight/Assets/FlightField.unity')).toEqual([]);
});
test('Native target-sector evidence includes both directions and real result/retry',()=>{
 nativeSkip();
 const png=(name,w,h)=>{const b=fs.readFileSync(path.join(candidate,name));expect(b.subarray(0,8).toString('hex')).toBe('89504e470d0a1a0a');expect(b.length).toBeGreaterThan(10000);expect(b.readUInt32BE(16)).toBe(w);expect(b.readUInt32BE(20)).toBe(h);};
 for(const side of ['positive','negative'])for(const x of [7,17,25,112,205])png('panels-'+side+'-'+x+'.png',1280,800);
 for(const name of ['aim','result','retry'])png(name+'.png',1280,800);
 png('aim-portrait.png',390,844);
});

test('source enumeration covers 40 IDs and 19 authored variants; 25 blend files reopened with exact metre bounds',()=>{
 const d=doc(),catalog=read(path.join(source,'games/flight/crab-flight/Assets/BlenderModels/Resources/BlenderModels/panels-catalog.json'));
 expect(d.status).toBe('SOURCE_ENUMERATION_NATIVE_PENDING');expect(d.total).toBe(40);expect(d.counts).toEqual({Restaurant:20,Overdrive:16,Core:4});
 expect(new Set(d.targets.map(t=>t.id)).size).toBe(d.total);expect(catalog.entries.map(e=>e.id)).toEqual(d.targets.map(t=>t.id));
 expect(Object.values(d.models).filter(m=>m.ids.length).length).toBe(19);
 const modelRows=read(path.join(ev,'blender-readback.json')).models;expect(modelRows.length).toBe(Object.keys(d.models).length);
 for(const m of modelRows){expect(m.boundsError).toBeLessThan(.00001);expect(m.metres&&m.bottomCentre&&m.vertexColours&&m.blendReopened).toBe(true);expect(fs.statSync(path.join(source,'tools/blender',m.family+'.blend')).size).toBeGreaterThan(1000);}
 for(const[name,h]of Object.entries(d.sourceHashes))expect(sha(path.join(source,'games/flight/crab-flight/Assets/Scripts',name))).toBe(h);
});
