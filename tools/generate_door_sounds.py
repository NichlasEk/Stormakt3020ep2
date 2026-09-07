"""Deterministic small Foley sketches for the physical oak-door trial."""
from pathlib import Path
import math,random,wave,struct,subprocess
root=Path(__file__).resolve().parents[1]
for name,duration in [('door-unlock',.35),('door-creak',.72),('door-hit',.35),('door-break',.85)]:
 rng=random.Random(name);samples=[];rate=24000
 for i in range(int(rate*duration)):
  t=i/rate;n=rng.uniform(-1,1)
  if name=='door-creak':v=(math.sin(t*1400+18*math.sin(t*17))+.25*n)*math.sin(math.pi*t/duration)*.10
  elif name=='door-unlock':v=(math.sin(t*4300)+n)*math.exp(-t*18)*.16
  elif name=='door-hit':v=(math.sin(t*590)+.6*n)*math.exp(-t*15)*.32
  else:v=(n*.65+math.sin(t*420)*.3)*math.exp(-t*5)*.35
  samples.append(struct.pack('<h',int(max(-1,min(1,v))*32767)))
 source=root/'assets/source'/f'{name}-v1.wav'
 with wave.open(str(source),'wb') as f:f.setparams((1,2,rate,0,'NONE','not compressed'));f.writeframes(b''.join(samples))
 subprocess.run(['ffmpeg','-y','-v','error','-i',str(source),'-c:a','libvorbis','-q:a','5',str(root/'assets/audio'/f'{name}.ogg')],check=True)
