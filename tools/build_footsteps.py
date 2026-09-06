#!/usr/bin/env python3
from pathlib import Path
import math,random,wave,struct,subprocess
root=Path(__file__).resolve().parents[1];rate=48000;rng=random.Random(302003);low=0;samples=[]
for i in range(int(rate*.24)):
 t=i/rate;noise=rng.uniform(-1,1);low=.8*low+.2*noise
 v=(low*.21+math.sin(math.tau*83*t)*.07)*math.exp(-t*30)*min(1,t*650)
 v+=(noise-low)*.025*math.exp(-((t-.055)/.035)**2)
 samples.append(struct.pack('<h',int(v*32767)))
source=root/'assets/source/footstep.wav'
with wave.open(str(source),'wb') as out:
 out.setnchannels(1);out.setsampwidth(2);out.setframerate(rate);out.writeframes(b''.join(samples))
subprocess.run(['ffmpeg','-y','-v','error','-i',str(source),'-c:a','libvorbis','-q:a','5',str(root/'assets/audio/footstep.ogg')],check=True)
