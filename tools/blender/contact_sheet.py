from PIL import Image,ImageDraw,ImageFont
from pathlib import Path
import json
root=Path(__file__).resolve().parents[3];p=root/'evidence/blender-pilot';fs=list(json.loads((Path(__file__).parent/'families.json').read_text())['families'])
sheet=Image.new('RGB',(1600,6*365),(13,19,35));draw=ImageDraw.Draw(sheet);font=ImageFont.truetype('/System/Library/Fonts/Menlo.ttc',15)
for i,f in enumerate(fs):
 im=Image.open(p/(f+'.png')).convert('RGBA');im.thumbnail((360,320));x=(i%4)*400;y=(i//4)*365;sheet.paste(im,(x+(400-im.width)//2,y),im);draw.text((x+12,y+325),f.removeprefix('mall_r5_'),font=font,fill=(250,230,181))
sheet.save(p/'contact-sheet.png');print(sheet.size)
