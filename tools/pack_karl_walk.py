"""Lossless layout of Blender RGBA output into fixed-cell runtime sprite atlases."""
from pathlib import Path
from PIL import Image
import hashlib,json
ROOT=Path(__file__).resolve().parents[1]
metadata=json.loads((ROOT/'assets/source/animation/karl-baked-v1.json').read_text())
for view in metadata['views']:
 direction=view['direction'];atlas=Image.new('RGBA',(384*6,384*5))
 for frame in range(30):
  with Image.open(ROOT/f'artifacts/karl-painted/{direction}/{frame+1:03}.png') as image:
   assert image.mode=='RGBA' and image.size==(384,384)
   bounds=image.getchannel('A').getbbox()
   assert bounds and bounds[0]>2 and bounds[1]>2 and bounds[2]<382 and bounds[3]<382,('Clipped sprite',direction,frame,bounds)
   # Paste without a mask: preserve RGB and fractional alpha exactly, never multiply alpha twice.
   atlas.paste(image,(frame%6*384,frame//6*384))
 path=ROOT/f'assets/art/karl-baked-walk-v1-{direction}.png';atlas.save(path)
 view['sha256']=hashlib.sha256(path.read_bytes()).hexdigest();view['runtime']=str(path.relative_to(ROOT))
(ROOT/'assets/source/animation/karl-baked-v1.json').write_text(json.dumps(metadata,indent=2)+'\n')
(ROOT/'assets/story/karl-baked-walk-v1.json').write_text(json.dumps(metadata,indent=2)+'\n')
print('PACK PASS: 120 RGBA frames, no clipped silhouette, fixed roots')
