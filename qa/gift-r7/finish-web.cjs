const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const source=path.resolve(process.argv[2]),out=path.resolve(process.argv[3]);const template=path.join(source,'games/flight/web-template');
const loaders=fs.readdirSync(path.join(out,'Build')).filter(n=>n.endsWith('.loader.js'));assert.equal(loaders.length,1);const built=loaders[0].slice(0,-10);
for(const item of fs.readdirSync(template,{withFileTypes:true}))if(item.isFile())fs.copyFileSync(path.join(template,item.name),path.join(out,item.name));
const host=path.join(out,'host.js');fs.writeFileSync(host,fs.readFileSync(host,'utf8').replaceAll('Build/kani-late-r9.','Build/'+built+'.'));
assert(fs.readFileSync(path.join(out,'index.html')).equals(fs.readFileSync(path.join(template,'index.html'))));
fs.writeFileSync(path.join(out,'PROTOTYPE-NOTE.txt'),'LateWorldR9 private comparison on a partial reconstructed r32-derived R2b baseline. Adopted3 mall areas and bold king crab retained. R9 renderer-only country terraces/launch works/orbital scenery; R7 gift-store shelf/product/material variants; R6 comic adult/bird contact and energy-bounded scatter retained. Shake and sound initially OFF. Frame drops/floor residual, M14/physical iPhone/listening and owner taste remain unresolved or unverified. Not original complete r49, not public or final adopted.\n');
console.log(JSON.stringify({version:'kani-late-r9',built,hostIndexExact:true,engineUnchanged:true}));
