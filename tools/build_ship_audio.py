#!/usr/bin/env python3
"""Original subdued twin-engine start, deterministic PCM, no external samples."""
import math,random,struct,wave,subprocess,json,hashlib
from pathlib import Path
root=Path(__file__).resolve().parents[1];source=root/'assets/source/ship-audio';source.mkdir(parents=True,exist_ok=True)
rate=48000;duration=8;rng=random.Random(30220911);pcm=bytearray();low=0;phase=0
for i in range(rate*duration):
 t=i/rate;low=.98*low+.02*rng.uniform(-1,1);phase+=math.tau*(38+24*min(1,t/4))/rate
 env=min(1,t/1.5)*min(1,(duration-t)/2)
 v=env*(.028*math.sin(phase)+.016*math.sin(phase*1.012)+.01*math.sin(phase*2)+.05*low)
 pcm.extend(struct.pack('<h',int(v*32767)))
p=source/'ship-engine.wav'
with wave.open(str(p),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(pcm)
out=root/'assets/audio/ship-engine.ogg';subprocess.run(['ffmpeg','-y','-v','error','-i',str(p),'-c:a','libvorbis','-q:a','5',str(out)],check=True)
(source/'manifest.json').write_text(json.dumps({'method':'original procedural PCM','seed':30220911,'seconds':duration,'sha256':hashlib.sha256(out.read_bytes()).hexdigest()},indent=2)+'\n')
