from pathlib import Path
import json
from PIL import Image,ImageDraw
p=Path(__file__).resolve().parents[3]/'evidence/blender-panels';manifest=json.loads((p/'models/manifest.json').read_text())
rows=[x for x in manifest['models']if not x['family'].startswith('panel_fragment_')]
rows.sort(key=lambda r:r['family']);im=Image.new('RGB',(1400,((len(rows)+4)//5)*310),(18,25,39));draw=ImageDraw.Draw(im)
for i,r in enumerate(rows):
 tile=Image.open(p/'models'/ (r['family']+'.png')).convert('RGBA');tile.thumbnail((270,270));x=(i%5)*280;y=(i//5)*310
 im.paste(tile,(x+(280-tile.width)//2,y),tile);draw.text((x+6,y+276),r['family'].replace('panel_',''),fill=(242,230,207));draw.text((x+6,y+291),' x '.join('%.2fm'%d for d in r['dimensions']),fill=(160,182,185))
im.save(p/'models-contact-sheet.png')
print(p/'models-contact-sheet.png')
