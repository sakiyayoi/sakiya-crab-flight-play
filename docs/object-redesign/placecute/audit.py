"""Conservative full-motion AABB check against the owner's nominal 0..15m band.
Not a trajectory simulator. Crab-margin sensitivity is separate, never hidden.
"""
from pathlib import Path
import re,json,math,hashlib,os
ROOT=Path(__file__).resolve().parents[3];OUT=Path(os.environ.get("KANI_PLACECUTE_EVIDENCE_DIR",Path(__file__).resolve().parent))
p=ROOT/'games/flight/crab-flight/Assets/ObjectRedesign/ObjectRules.cs';s=p.read_text()
scale=float(re.search(r'ObjectScale=([\d.]+)f',s)[1]);half=1.15*scale
motion={m[0]:(m[1],float(m[2])) for m in re.findall(r'new Definition\(Kind\.(\w+),"[^"]+","[^"]+","([^"]+)",[^,]+,([\d.]+)f?,',s)}
rows=[]
for id,kind,x,y in re.findall(r'new Placement\("([^"]+)",Kind\.(\w+),([\d.]+)f?,([\d.]+)f?\)',s):
 x,y=float(x),float(y);m,a=motion[kind];r=half*math.sqrt(2) if m=='rotate' else half
 dx=a if m in ['slide','fast-slide'] else 0;dy=a if m not in ['slide','fast-slide','rotate'] else 0
 lo,hi=y-r-dy,y+r+dy;overlap=lo<=15 and hi>=0
 rows.append(dict(id=id,kind=kind,center=[x,y,0],x=[round(x-r-dx,4),round(x+r+dx,4)],y=[round(lo,4),round(hi,4)],z=[-half,half],nominalBandOverlap=overlap,plusCrabMarginOverlap=lo<=18.5 and hi>=-3.5,intent='positive action contact permitted' if kind in ['Boost','Bounce','Spin','Blast'] else 'ground-line brake; dive/ground contact intended' if kind=='Brake' else 'ground-line death hazard; dive/low flight contact intended' if id=='mall-death' else 'high-line death hazard',newUnintendedNonBrakeOverlap=kind=='Death' and overlap and (id,x,y)!=('mall-death',95,3)))
result=dict(method='Full motion AABB, both mirrored directions; rotating square uses sqrt(2) extent. Nominal absolute world-Y [0,15] is a design envelope from owner brief, not measured center/support bounds. Separate +3.5m legacy crab margin sensitivity. No guarantee of a miss after prior actions.',sourceSha256=hashlib.sha256(p.read_bytes()).hexdigest(),scale=scale,halfSize=half,rows=rows)
(OUT/'hit-map.json').write_text(json.dumps(result,indent=2)+'\n')
for r in rows:print(r['id'],r['center'],'Y',r['y'],'nominal',r['nominalBandOverlap'],'margin',r['plusCrabMarginOverlap'])
assert len(rows)==16 and not any(r['newUnintendedNonBrakeOverlap'] for r in rows)
