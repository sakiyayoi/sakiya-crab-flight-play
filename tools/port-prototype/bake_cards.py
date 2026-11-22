"""Original harbor silhouette artwork, baked blur; no external illustrations or DoF."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFilter
import random
out=Path(__file__).resolve().parents[2]/'games/flight/crab-flight/Assets/PortPrototype/Resources/PortPrototype'
for layer in ['far','mid']:
 rng=random.Random(481 if layer=='far' else 149);im=Image.new('RGBA',(2048,512));d=ImageDraw.Draw(im)
 color=(35,47,82,255) if layer=='far' else (58,72,104,255)
 x=0
 while x<2048:
  width=rng.randint(90,220);height=rng.randint(45,105) if layer=='far' else rng.randint(70,130)
  d.polygon([(x,512),(x,512-height),(x+width*.45,492-height),(x+width,512-height),(x+width,512)],fill=color)
  if layer=='mid':
   for j in range(3):d.rectangle([x+18+j*28,530-height,x+27+j*28,539-height],fill=(165,131,86,255))
  x+=width+rng.randint(10,45)
 for x,h in [(230,240),(833,300),(1487,255),(1870,218)]:
  d.line([(x-23,500),(x,512-h),(x+38,500)],fill=color,width=8)
  d.line([(x-56,505-h),(x+133,505-h)],fill=color,width=10)
  d.line([(x,512-h),(x+110,505-h),(x+110,450-h//2)],fill=color,width=3)
 im=im.filter(ImageFilter.GaussianBlur(1.4 if layer=='far' else .65))
 im.save(out/f'harbor-{layer}.png')
 print(layer,im.size)
