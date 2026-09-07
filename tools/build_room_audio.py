#!/usr/bin/env python3
"""Original deterministic room Foley. No recordings or third-party samples."""
from pathlib import Path
import hashlib,json,math,random,struct,subprocess,wave
ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/'assets/source/room-audio';SRC.mkdir(parents=True,exist_ok=True)
RATE=48000
specs={'pump-pressure':1.8,'pump-drain':3.2,'stone-door':2.0,'oath-lock':.72,'oath-rush':.65,'oath-impact':1.5,'vault-ambience':24.0}
manifest=[]
for name,duration in specs.items():
    rng=random.Random(302207+sum(map(ord,name)));data=bytearray();low=0;slow=0
    for i in range(int(RATE*duration)):
        t=i/RATE;n=rng.uniform(-1,1);low=.96*low+.04*n;slow=.998*slow+.002*n
        edge=min(1,t*100)*min(1,(duration-t)*60)
        if name=='pump-pressure':
            v=(n-low)*.09*math.exp(-t*1.8)+low*.16*math.exp(-t*2.7)
        elif name=='pump-drain':
            env=math.sin(math.pi*t/duration)**2
            v=(low*.45+math.sin(math.tau*(110*t+7*math.sin(t*5)))*.04)*env
        elif name=='stone-door':
            env=math.sin(math.pi*t/duration)**2
            v=(low*.7+math.sin(math.tau*53*t)*.08)*env*(.7+.3*math.sin(t*31)**2)
        elif name=='oath-lock':
            v=sum(math.sin(math.tau*f*t)*math.exp(-t*d)*a for f,d,a in [(240,7,.09),(397,9,.055),(611,12,.025)])+low*.2*math.exp(-t*18)
        elif name=='oath-rush':
            v=(n-low)*.045*math.sin(math.pi*t/duration)**2+low*.22*math.sin(math.pi*t/duration)
        elif name=='oath-impact':
            v=low*.7*math.exp(-t*9)+sum(math.sin(math.tau*f*t)*math.exp(-t*d)*a for f,d,a in [(83,9,.18),(187,5,.10),(319,4,.07),(523,7,.045)])
        else:
            # Periodic sub-bass air and quiet pipe resonance; no sea/birds or speech.
            v=slow*.25+low*.025+math.sin(math.tau*48*t)*.008*(.7+.3*math.sin(math.tau*t/24))
            v+=math.sin(math.tau*73*t)*.003*(.6+.4*math.sin(math.tau*t/12))
            edge=1
        data.extend(struct.pack('<h',int(max(-.9,min(.9,v*edge))*32767)))
    source=SRC/(name+'.wav')
    with wave.open(str(source),'wb') as f:
        f.setnchannels(1);f.setsampwidth(2);f.setframerate(RATE);f.writeframes(data)
    dest=ROOT/'assets/audio'/(name+'.ogg')
    args=['ffmpeg','-y','-v','error','-i',str(source)]
    if name=='vault-ambience':
        # Blend the final second with the first, then loop from body to blend.
        args+=['-filter_complex','[0:a]asplit=3[a][b][c];[a]atrim=1:23,asetpts=PTS-STARTPTS[body];[b]atrim=23:24,asetpts=PTS-STARTPTS[tail];[c]atrim=0:1,asetpts=PTS-STARTPTS[head];[tail][head]acrossfade=d=1[join];[body][join]concat=n=2:v=0:a=1,volume=12dB[out]','-map','[out]']
    args+=['-c:a','libvorbis','-q:a','5',str(dest)]
    subprocess.run(args,check=True)
    manifest.append({'asset':str(dest.relative_to(ROOT)),'source':str(source.relative_to(ROOT)),'duration':duration-1 if name=='vault-ambience' else duration,'seed':302207+sum(map(ord,name)),'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'method':'original procedural PCM; room Foley and looped air','loop_gain_db':12 if name=='vault-ambience' else 0})
(SRC/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print('Built',len(manifest),'original room sounds')
