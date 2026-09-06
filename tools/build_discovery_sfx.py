#!/usr/bin/env python3
"""Original quiet stone-brush, rubbing and journal Foley, deterministic PCM sources."""
from pathlib import Path
import math,random,wave,struct,subprocess

ROOT=Path(__file__).resolve().parents[1]
RATE=48000
for name,duration in [('scrape',.28),('inscription',1.1),('paper',.42)]:
    rng=random.Random(302206+sum(map(ord,name)));samples=[];low=0
    for i in range(int(RATE*duration)):
        t=i/RATE;noise=rng.uniform(-1,1);low=.92*low+.08*noise
        envelope=math.sin(math.pi*t/duration)**2
        if name=='scrape':value=(noise-low)*.09*envelope*(.55+.45*math.sin(t*71)**2)
        elif name=='paper':value=(noise-low)*.07*envelope*(.35+.65*math.sin(t*29)**2)
        else:
            value=(math.sin(math.tau*220*t)*.08+math.sin(math.tau*329.63*t)*.05)*math.exp(-4*t)
            value+=low*.18*math.exp(-9*t)
        value*=min(1,t*200)*min(1,(duration-t)*100)
        samples.append(struct.pack('<h',int(max(-.9,min(.9,value))*32767)))
    source=ROOT/f'assets/source/{name}.wav'
    with wave.open(str(source),'wb') as out:
        out.setnchannels(1);out.setsampwidth(2);out.setframerate(RATE);out.writeframes(b''.join(samples))
    subprocess.run(['ffmpeg','-y','-v','error','-i',str(source),'-c:a','libvorbis','-q:a','5',str(ROOT/f'assets/audio/{name}.ogg')],check=True)
