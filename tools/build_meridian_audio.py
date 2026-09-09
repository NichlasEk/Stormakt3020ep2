#!/usr/bin/env python3
"""Original restrained clock bell and brass gearing. No external samples."""
import math,random,struct,wave,subprocess,json,hashlib
from pathlib import Path
root=Path(__file__).resolve().parents[1];src=root/'assets/source/meridian-audio';src.mkdir(parents=True,exist_ok=True)
rate=48000;manifest=[]
for name,duration in [('meridian-bell',3.5),('meridian-turn',.8)]:
 rng=random.Random(30220920);pcm=bytearray();low=0
 for i in range(int(rate*duration)):
  t=i/rate;low=.93*low+.07*rng.uniform(-1,1)
  if name.endswith('bell'):
   v=min(1,t/.008)*sum(a*math.sin(math.tau*f*t)*math.exp(-t*d) for f,a,d in [(174,.12,1.3),(351,.05,1.8),(469,.025,2.5),(731,.015,3.5)])
  else:
   env=math.sin(math.pi*t/duration)**2;v=env*(low*.13+.028*math.sin(math.tau*83*t))*(.5+.5*abs(math.sin(math.tau*13*t)))
  pcm.extend(struct.pack('<h',int(max(-.9,min(.9,v))*32767)))
 p=src/(name+'.wav')
 with wave.open(str(p),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(pcm)
 out=root/'assets/audio'/(name+'.ogg');subprocess.run(['ffmpeg','-y','-v','error','-i',str(p),'-c:a','libvorbis','-q:a','5',str(out)],check=True)
 manifest.append({'file':out.name,'seconds':duration,'sha256':hashlib.sha256(out.read_bytes()).hexdigest()})
(src/'manifest.json').write_text(json.dumps({'method':'original deterministic PCM','seed':30220920,'files':manifest},indent=2)+'\n')
