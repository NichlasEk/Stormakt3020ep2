#!/usr/bin/env python3
"""Original periodic engine hum, quiet underneath direct cabin dialogue."""
import math,struct,wave,subprocess
from pathlib import Path
root=Path(__file__).resolve().parents[1];p=root/'assets/source/ship-audio/cabin-hum.wav'
p.parent.mkdir(parents=True,exist_ok=True)
rate=48000;seconds=8;pcm=bytearray()
for i in range(rate*seconds):
 t=i/rate
 v=(.031*math.sin(math.tau*48*t)+.016*math.sin(math.tau*48.125*t)+.008*math.sin(math.tau*96*t))*(.9+.1*math.cos(math.tau*t/8))
 pcm.extend(struct.pack('<h',int(v*32767)))
with wave.open(str(p),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(pcm)
subprocess.run(['ffmpeg','-y','-v','error','-i',str(p),'-c:a','libvorbis','-q:a','5',str(root/'assets/audio/cabin-hum.ogg')],check=True)
