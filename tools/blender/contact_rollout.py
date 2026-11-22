"""Assemble actual Blender renders by effective Native sector families."""
from pathlib import Path
import json, math
from PIL import Image, ImageDraw, ImageFont
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[2]
EV=ROOT/'evidence/blender-rollout'
data=json.loads((HERE/'rollout-families.json').read_text())
font=ImageFont.truetype('/System/Library/Fonts/Menlo.ttc',14)
large=ImageFont.truetype('/System/Library/Fonts/Menlo.ttc',25)
def make_sheet(name,families,counts=None):
    cols=min(4,len(families));sheet=Image.new('RGB',(cols*400,math.ceil(len(families)/cols)*365+65),(13,19,35));draw=ImageDraw.Draw(sheet)
    draw.text((18,15),name+' / Blender models',font=large,fill=(250,230,181))
    for i,f in enumerate(families):
        path=(ROOT/'evidence/blender-pilot' if data['families'][f]['pilotModel'] else EV/'models')/(f+'.png')
        im=Image.open(path).convert('RGBA');im.thumbnail((372,305))
        x=i%cols*400;y=i//cols*365+65
        sheet.paste(im,(x+(400-im.width)//2,y),im)
        label=f.removeprefix('mall_r5_')
        if len(label)>40:label=label.replace('_','_\n',1)
        draw.text((x+12,y+308),label,font=font,fill=(250,230,181))
        if counts:draw.text((x+12,y+332),'positive '+str(counts[0].get(f,0))+' / negative '+str(counts[1].get(f,0)),font=font,fill=(128,196,174))
    out=EV/(name+'-contact-sheet.png');sheet.save(out);print(out.name,sheet.size)
make_sheet('new-families',[f for f,d in data['families'].items()if not d['pilotModel']])
for sector in dict.fromkeys(g['sector']for g in data['groups']):
    groups=[g for g in data['groups']if g['sector']==sector]
    families=sorted({f for g in groups for f in g['families']})
    if families:make_sheet(sector,families,[g['families']for g in groups])
