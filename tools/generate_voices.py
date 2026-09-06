#!/usr/bin/env python3
"""Generate new synthetic Episode II cast and lines on local EutherLink."""
import json,base64,time,urllib.request,hashlib,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];SRC=ROOT/'assets/source/voices';SRC.mkdir(parents=True,exist_ok=True)
URL='http://127.0.0.1:8765'
def req(path,data=None):
 r=urllib.request.Request(URL+path,data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(r,timeout=90) as f:return json.load(f)
def render(name,payload):
 dest=SRC/(name+'.wav')
 if dest.exists():return dest
 safe={k:v for k,v in payload.items() if not k.endswith('base64')}
 (SRC/(name+'.request.json')).write_text(json.dumps(safe,ensure_ascii=False,indent=2))
 job=req('/v1/tts/jobs',payload);print(name,job['id'],flush=True)
 start=time.time()
 while time.time()-start<600:
  state=req(job['status_url'])
  if state['status']=='done':
   dest.write_bytes(urllib.request.urlopen(URL+state['audio_url'],timeout=90).read())
   (SRC/(name+'.manifest.json')).write_text(json.dumps({'job':job['id'],'sha256':hashlib.sha256(dest.read_bytes()).hexdigest(),'backend':payload['model_backend'],'synthetic_reference':True},indent=2))
   return dest
  if state['status'] in ['failed','cancelled']:raise RuntimeError(state)
  time.sleep(2)
 raise TimeoutError(job['id'])
roles={
 'ebba':{'seed':30220601,'instruction':'An adult Swedish female naval commander, low warm confident voice, crisp Swedish pronunciation, restrained urgency, intimate and natural, dry intelligence.','text':'Karl, håll dig nära kajen. Flottan väntar ute i dimman. Jag vill ha alla tillbaka ombord innan gryningen. Vi har förlorat nog med folk för kungars gamla drömmar.'},
 'collector':{'seed':30220602,'instruction':'A deep Swedish male dockmaster, weathered bass baritone, solemn bureaucratic authority, slow threatening calm, clearly intelligible Swedish.','text':'Varvets liggare ligger öppen. Här lämnar inget skepp sin plats innan skulden är betald. Jag har väntat längre än era kungar har haft namn.'}}
lines={
 'arrival':('ebba','Karl. Två sigill håller kajen stängd. Bryt dem. Och håll ett öga på skytten vid porten.'),
 'cannon':('ebba','Batteriet svarar. Håll undan från nedslaget.'),
 'collector':('collector','Fyra män. Två brutna sigill. Er skuld växer, Karl. Jag tar betalningen personligen.'),
 'rage':('collector','Även havet står i skuld till kronan.'),
 'fallen':('ebba','Vänta. Det ligger något vid porten. En karta, gjuten i brons. Den hör inte hemma här.'),
 'atland':('ebba','Atland. Det är vad Rudbeck kallade det. Karl, ta kartan ombord. Vi måste tala ostört.')}
for role,v in roles.items():
 ref=render(role+'-reference',{'text':v['text'],'voice_instruction':v['instruction'],'language':'sv','model_backend':'voxcpm2','output_format':'wav','normalize':False,'seed':v['seed']})
 for name,(r,line) in lines.items():
  if r!=role:continue
  raw=render(name,{'text':line,'voice_instruction':v['instruction'],'language':'sv','model_backend':'dots.tts-mf','output_format':'wav','normalize':False,'seed':v['seed']+100,'reference_wav_base64':base64.b64encode(ref.read_bytes()).decode(),'prompt_text':v['text'],'dots_num_steps':4})
  subprocess.run(['ffmpeg','-y','-v','error','-i',str(raw),'-af','highpass=f=110,lowpass=f=7500,loudnorm=I=-18:TP=-2:LRA=8','-ar','48000','-ac','1','-c:a','libvorbis','-q:a','5',str(ROOT/'assets/audio'/f'voice-{name}.ogg')],check=True)
  print('saved',name,flush=True)
