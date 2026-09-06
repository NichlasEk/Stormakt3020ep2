#!/usr/bin/env python3
"""Preserve generated masters; overlap a whole bar at each runtime loop seam."""
from pathlib import Path
import subprocess,json,hashlib,sys
root=Path(__file__).resolve().parents[1]
loops=[('score',72.5,2.5),('boss-score',56,2),('names-score',43.333333,3.333333)]
for name,end,bar in loops:
    if '--names-only' in sys.argv and name!='names-score':continue
    raw=root/f'assets/source/{name}-master.wav';out=root/f'assets/audio/{name}.ogg'
    graph=f'[0:a]asplit=3[h][b][t];[h]atrim=0:{bar},asetpts=PTS-STARTPTS[head];[b]atrim={bar}:{end-bar},asetpts=PTS-STARTPTS[body];[t]atrim={end-bar}:{end},asetpts=PTS-STARTPTS[tail];[tail][head]acrossfade=d={bar}:c1=tri:c2=tri[join];[body][join]concat=n=2:v=0:a=1,loudnorm=I=-19:TP=-2:LRA=10[out]'
    subprocess.run(['ffmpeg','-y','-v','error','-i',str(raw),'-filter_complex',graph,'-map','[out]','-ar','48000','-c:a','libvorbis','-q:a','6',str(out)],check=True)
    (root/f'assets/source/{name}-loop.json').write_text(json.dumps({'master':str(raw.relative_to(root)),'end_seconds':end,'overlap_seconds':bar,'duration_seconds':end-bar,'filter':graph,'sha256':hashlib.sha256(out.read_bytes()).hexdigest()},indent=2))
