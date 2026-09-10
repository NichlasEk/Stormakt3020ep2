import runpy,json,re,base64,subprocess,shutil
from pathlib import Path
x=runpy.run_path(str(Path(__file__).with_name('generate_rescue_voices.py')));root=x['ROOT'];src=x['SRC'];render=x['render']
story=json.loads((root/'assets/story/rescue-radio.json').read_text())
for name in ['rescue-debrief','rescue-release','rescue-censor']:
 role,text=story[name];ref=json.loads((src/(role+'-reference.request.json')).read_text());parts=[]
 for i,line in enumerate(re.findall(r'[^.!?]+[.!?]?',text)):
  raw=render(name+'-part-'+str(i),dict(text=line.strip(),reference_wav_base64=base64.b64encode((src/(role+'-reference.wav')).read_bytes()).decode(),prompt_text=ref['text'],voice_instruction=ref['voice_instruction'],model_backend='voxcpm2',language='sv',output_format='wav',normalize=False,seed=30221300+i))
  parts.append(raw)
 concat=src/(name+'-concat.txt');concat.write_text(''.join("file '"+str(p)+"'\n" for p in parts));out=root/'assets/audio'/('voice-'+name+'.ogg')
 subprocess.run(['ffmpeg','-y','-v','error','-f','concat','-safe','0','-i',str(concat),'-af','highpass=f=90,lowpass=f=7800,loudnorm=I=-18:TP=-2:LRA=8','-ar','48000','-ac','1','-c:a','libvorbis','-q:a','5',str(out)],check=True)
 concat.unlink()
 (src/(name+'-delivery.json')).write_text(json.dumps(dict(parts=[p.name for p in parts],text=text),ensure_ascii=False,indent=2))
