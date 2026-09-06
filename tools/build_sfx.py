#!/usr/bin/env python3
"""Original deterministic multilayer audio design; PCM masters plus runtime Vorbis."""
import math,random,wave,struct,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];RATE=48000
def sound(name,duration,fn):
 rng=random.Random(3020206+sum(map(ord,name)));samples=[];low=0
 for i in range(int(RATE*duration)):
  t=i/RATE;noise=rng.uniform(-1,1);low=low*.965+noise*.035
  v=fn(t,noise,low,duration)*min(1,t*1000)*min(1,(duration-t)*100)
  samples.append(max(-.87,min(.87,v)))
 raw=ROOT/'assets/source'/f'{name}.wav'
 with wave.open(str(raw),'wb') as f:
  f.setnchannels(1);f.setsampwidth(2);f.setframerate(RATE);f.writeframes(b''.join(struct.pack('<h',int(v*32767)) for v in samples))
 subprocess.run(['ffmpeg','-y','-v','error','-i',str(raw),'-c:a','libvorbis','-q:a','5',str(ROOT/'assets/audio'/f'{name}.ogg')],check=True)
def tone(f,t):return math.sin(math.tau*f*t)
sound('swing',.30,lambda t,n,l,d:(n-l)*.22*math.sin(math.pi*t/d)**2+tone(240-200*t/d,t)*.05*math.sin(math.pi*t/d))
sound('hammer',.48,lambda t,n,l,d:(n*.10+tone(110-65*t/d,t)*.16)*math.sin(math.pi*t/d)**2)
sound('hit',.31,lambda t,n,l,d:(n*.32+tone(122,t)*.3+tone(773,t)*.08)*math.exp(-t*22))
sound('parry',.75,lambda t,n,l,d:(tone(1320,t)*.17+tone(2075,t)*.12+tone(3417,t)*.07+n*.12*math.exp(-t*60))*math.exp(-t*7))
sound('shot',.43,lambda t,n,l,d:(n*.4*math.exp(-t*30)+tone(73,t)*.29*math.exp(-t*13)))
sound('cannon',1.35,lambda t,n,l,d:((n*.5+tone(42,t)*.4)*math.exp(-t*5)+l*1.7*math.exp(-t*2)))
sound('seal',1.2,lambda t,n,l,d:(tone(440,t)*.12+tone(660,t)*.09+tone(882,t)*.08)*math.exp(-t*3)+n*.1*math.exp(-t*25))
sound('heal',1.1,lambda t,n,l,d:(tone(520+180*t,t)*.11+tone(780+270*t,t)*.08)*math.sin(math.pi*t/d)**2)
sound('dodge',.3,lambda t,n,l,d:(n*.14+l*.3)*math.sin(math.pi*t/d)**2)
sound('death',.8,lambda t,n,l,d:(tone(73-35*t,t)*.15+n*.13)*math.exp(-t*7))
# A looping harbour bed: filtered sea swell, wind and distant bronze bell.
sound('ambience',40,lambda t,n,l,d:l*(.42+.2*tone(.075,t))+.012*tone(47,t)+(.04*tone(196,t)+.025*tone(293.7,t))*math.exp(-(t%10)*1.3))
