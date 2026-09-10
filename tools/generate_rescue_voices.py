"""Produce the rescue cast locally with consistent synthetic VoxCPM2 references."""
import base64,json,hashlib,time,urllib.request,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];SRC=ROOT/'assets/source/voices';BASE='http://127.0.0.1:8765'
def req(path,data=None):
 with urllib.request.urlopen(urllib.request.Request(BASE+path,data=json.dumps(data).encode() if data is not None else None,headers={'Content-Type':'application/json'}),timeout=120) as f:return json.load(f)
def render(name,payload):
 dest=SRC/(name+'.wav')
 if dest.exists():return dest
 (SRC/(name+'.request.json')).write_text(json.dumps({k:v for k,v in payload.items() if not k.endswith('base64')},ensure_ascii=False,indent=2))
 job=req('/v1/tts/jobs',payload);print(name,job['id'],flush=True)
 for _ in range(240):
  state=req(job['status_url'])
  if state['status']=='done':dest.write_bytes(urllib.request.urlopen(BASE+state['audio_url'],timeout=120).read());break
  if state['status'] in ['failed','cancelled']:raise RuntimeError(state)
  time.sleep(2)
 else:raise TimeoutError(name)
 (SRC/(name+'.manifest.json')).write_text(json.dumps({'job':job['id'],'backend':'voxcpm2','sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'synthetic_reference':True},indent=2))
 return dest
roles={'karl':('Adult Swedish male commander age thirty-five, warm low baritone, natural clear Swedish, calm and direct with restrained vulnerability, dignified but human, no caricature or impersonation.','Jag heter Karl. Jag följer vägen ned mot porten. Ebba, håll radion öppen. När vi kommer tillbaka vill jag tala med dig, utan någon som lyssnar.'),'censor':('Adult Swedish male royal official around sixty, precise resonant middle-low voice, stern controlled authority, clear natural Swedish, no monstrous effects and no comedy.','Jag förvaltar kansliets sigill. Ingen får återkalla en bekräftad order utan min underskrift. Lägg ned vapnet och invänta kontrollen.')}
for n,(instruction,text) in roles.items():render(n+'-reference',{'text':text,'voice_instruction':instruction,'model_backend':'voxcpm2','language':'sv','output_format':'wav','normalize':False,'seed':30221100+list(roles).index(n)})
for i,(name,(role,text)) in enumerate(json.loads((ROOT/'assets/story/rescue-radio.json').read_text()).items()):
 out=ROOT/'assets/audio'/('voice-'+name+'.ogg')
 if out.exists():continue
 reference=json.loads((SRC/(role+'-reference.request.json')).read_text())
 payload={'text':text,'reference_wav_base64':base64.b64encode((SRC/(role+'-reference.wav')).read_bytes()).decode(),'prompt_text':reference['text'],'voice_instruction':reference['voice_instruction'],'model_backend':'voxcpm2','language':'sv','output_format':'wav','normalize':False,'seed':30221130+i}
 raw=render(name,payload)
 subprocess.run(['ffmpeg','-y','-v','error','-i',str(raw),'-af','highpass=f=90,lowpass=f=7800,loudnorm=I=-18:TP=-2:LRA=8','-ar','48000','-ac','1','-c:a','libvorbis','-q:a','5',str(out)],check=True)
 print('saved',name,flush=True)
