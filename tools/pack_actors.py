from pathlib import Path
from PIL import Image
root=Path(__file__).resolve().parents[1]
for kind in ['karl-saber','karl-hammer','guard','pikeman','gunner','collector']:
    atlas=Image.new('RGBA',(256*8,320*7))
    for pose in range(7):
        for direction in range(8):
            atlas.paste(Image.open(root/f'assets/source/actor-frames/{kind}-{pose}-{direction}.png'),(direction*256,pose*320))
    atlas.save(root/f'assets/art/{kind}.png')
