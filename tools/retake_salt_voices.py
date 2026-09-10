"""Alternative local VoxCPM2 takes for lines flagged by the automatic speech review."""
import base64,json,hashlib,time,urllib.request,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];SRC=ROOT/'assets/source/voices';BASE='http://127.0.0.1:8765'
def req(path,data=None):
 with urllib.request.urlopen(urllib.request.Request(BASE+path,data=json.dumps(data).encode() if data is not None else None,headers={'Content-Type':'application/json'}),timeout=120) as f:return json.load(f)
lines=json.loads((ROOT/'assets/story/salt-radio.json').read_text())
for name in ['salt-proof','salt-order','salt-stairs','salt-release','salt-reunion-marta']:
 role,text=lines[name];dest=SRC/(name+'-retake-v1.wav')
 if not dest.exists():
  reference=json.loads((SRC/(role+'-reference.request.json')).read_text())
  payload={'text':text,'reference_wav_base64':base64.b64encode((SRC/(role+'-reference.wav')).read_bytes()).decode(),'prompt_text':reference['text'],'voice_instruction':reference['voice_instruction'],'model_backend':'voxcpm2','language':'sv','output_format':'wav','normalize':False,'seed':30221012}
  (SRC/(name+'-retake-v1.request.json')).write_text(json.dumps({k:v for k,v in payload.items() if not k.endswith('base64')},ensure_ascii=False,indent=2))
  job=req('/v1/tts/jobs',payload);print(name,job['id'],flush=True)
  for _ in range(180):
   state=req(job['status_url'])
   if state['status']=='done':dest.write_bytes(urllib.request.urlopen(BASE+state['audio_url'],timeout=120).read());break
   if state['status'] in ['failed','cancelled']:raise RuntimeError(state)
   time.sleep(2)
  else:raise TimeoutError(name)
  (SRC/(name+'-retake-v1.manifest.json')).write_text(json.dumps({'job':job['id'],'backend':'voxcpm2','sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'synthetic_reference':True},indent=2))
 subprocess.run(['ffmpeg','-y','-v','error','-i',str(dest),'-af','highpass=f=110,lowpass=f=7500,loudnorm=I=-18:TP=-2:LRA=8','-ar','48000','-ac','1','-c:a','libvorbis','-q:a','5',str(ROOT/'assets/audio'/('voice-'+name+'.ogg'))],check=True)
 print('saved',name,flush=True)
