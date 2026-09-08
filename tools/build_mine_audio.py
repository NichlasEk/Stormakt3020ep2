#!/usr/bin/env python3
"""Deterministic original valve, pressure-warning and steam Foley. No samples."""
import math,random,struct,wave,subprocess,json,hashlib
from pathlib import Path
root=Path(__file__).resolve().parents[1];source=root/'assets/source/mine-audio';source.mkdir(parents=True,exist_ok=True)
report=[]
for name,length in [('mine-valve',1.25),('mine-warning',.7),('mine-steam',1.4)]:
 rng=random.Random(30220909);pcm=bytearray();low=0;rate=48000
 for i in range(int(rate*length)):
  t=i/rate;n=rng.uniform(-1,1);low=.97*low+.03*n;env=min(1,t*45)*min(1,(length-t)*10)
  if name=='mine-steam':v=(n-low)*.07*math.exp(-t*1.4)+low*.12*math.exp(-t)
  elif name=='mine-warning':v=(math.sin(math.tau*(360*t+160*t*t))*.025+(n-low)*.012)*(t/length)
  else:v=(low*.1+math.sin(math.tau*(178*t+3*math.sin(t*15)))*.026)*(.25+.75*math.sin(t*19)**8)
  pcm.extend(struct.pack('<h',int(v*env*32767)))
 path=source/(name+'.wav')
 with wave.open(str(path),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(pcm)
 dest=root/'assets/audio'/(name+'.ogg');subprocess.run(['ffmpeg','-y','-v','error','-i',str(path),'-c:a','libvorbis','-q:a','5',str(dest)],check=True)
 report.append({'asset':name,'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'method':'original procedural PCM','seed':30220909,'seconds':length})
(source/'manifest.json').write_text(json.dumps(report,indent=2)+'\n')
