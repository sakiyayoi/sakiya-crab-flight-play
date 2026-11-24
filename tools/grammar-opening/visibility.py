#!/usr/bin/env python3
"""Orthographic projection feasibility, NOT screenshots or animated camera proof."""
import json
import math
from pathlib import Path
import author as A

up_y=16/math.hypot(16,5.3);up_z=5.3/math.hypot(16,5.3)
data=A.build();cases=[]
for width,height in [(390,844),(320,568),(844,390),(1280,800)]:
    aspect=width/height
    for side in [1,-1]:
        for sector,anchor,alts in [('restaurant',0,[0]),('mall',20,[8,14]),('office',123,[19,24])]:
            sign=next(s for s in data['landmarks'] if s['nextSector']==sector and s['side']==side)
            face=[b for b in sign['boxes'] if b['key']=='lamp-face' or b['key'] in ['window','crown']]
            for alt in alts:
                for framing in (['ready'] if anchor==0 else ['ground-follow','high-follow']):
                    if framing=='ready':
                        size=3.7*max(1,.75/aspect)*(1.5 if aspect<.7 else 1.12);lead=0;cy=.7
                    else:
                        # Unchanged PortRules formula, representative speed 54m/s.
                        size=max(12 if aspect<1 else 10,min(30,9+54*.16+alt*.1))
                        lead=min(max(3,min(18,54*.22*(1.25 if aspect<1 else 1))),size*aspect*.45)
                        cy=size*.6/up_y if framing=='ground-follow' else alt-size*.1/up_y
                    cx=side*(anchor+lead)
                    points=[((b['p'][0]+dx*b['size'][0]/2-cx)/(size*aspect)*.5+.5,
                             ((b['p'][1]+dy*b['size'][1]/2-cy)*up_y+(b['p'][2]+dz*b['size'][2]/2)*up_z)/size*.5+.5)
                            for b in face for dx in [-1,1] for dy in [-1,1] for dz in [-1,1]]
                    bounds=[[min(p[k] for p in points),max(p[k] for p in points)] for k in [0,1]]
                    visible=[max(0,min(1,b)-max(0,a)) for a,b in bounds]
                    assert visible[0]*width>=12 and visible[1]*height>=24,(width,height,sector,alt,framing,bounds)
                    cases.append(dict(viewport=[width,height],direction=side,nextSector=sector,anchorX=side*anchor,
                        altitude=alt,framing=framing,orthographicSize=size,litFaceViewportBounds=bounds,
                        litFaceVisiblePixels=[visible[0]*width,visible[1]*height]))
result=dict(scope='steady framing projection of actual landmark box coordinates; no occlusion, HUD, smoothing, Native or perceptual approval',
            historicalAltitudeContext='unchanged prior route samples: restaurant 8.10–13.76m; mall 19.31–22.26m (not current-run measurements)',
            readyDefine='CRAB_KING_BOLD_R2, as in existing Assets/csc.rsp',cases=cases,
            passed=len(cases),minimumVisibleFaceWidthPixels=min(c['litFaceVisiblePixels'][0] for c in cases),
            minimumVisibleFaceHeightPixels=min(c['litFaceVisiblePixels'][1] for c in cases))
out=A.ROOT/'docs/grammar-opening/visibility.json';out.write_text(json.dumps(result,indent=2)+'\n')
print({k:v for k,v in result.items() if k!='cases'})
