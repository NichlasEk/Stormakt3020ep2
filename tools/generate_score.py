#!/usr/bin/env python3
"""One local ACE-Step score, under the shared EutherLink GPU lease."""
import json,time,urllib.request,subprocess,hashlib,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];LINK='http://127.0.0.1:8765';ACE='http://127.0.0.1:8001'
def req(url,data=None,timeout=180):
 r=urllib.request.Request(url,data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(r,timeout=timeout) as f:
  b=f.read();return json.loads(b) if b else {}
prompt={'prompt':'Instrumental Nordic romantic orchestral action RPG soundtrack. A haunted Swedish naval dock in blue twilight. Somber noble melody played by solo cello and low French horns, muted violas and nyckelharpa-like bowed folk strings, delicate harp, distant timpani and restrained military snare pulse. D minor, 96 BPM, steady purposeful walking and combat rhythm, warm amber melancholy against cold deep sea mystery. Beautiful memorable four-note leitmotif, cinematic but intimate, Swedish national romantic landscape painting in sound, subtle baroque counterpoint. Start immediately with the theme, consistent volume, repeating phrases suitable for seamless looping. No singing, no speech, no pop, no EDM, no comedy, no trailer braams, no long silent opening or fadeout.','lyrics':'[Instrumental]','vocal_language':'unknown','audio_duration':80,'audio_format':'wav','task_type':'text2music','batch_size':1,'thinking':False,'use_format':False,'bpm':96,'key_scale':'D minor','time_signature':'4/4','inference_steps':8,'guidance_scale':7,'use_random_seed':False,'seed':30220603}
name='boss-score' if '--boss' in sys.argv else 'score'
if name=='boss-score':
 prompt.update({'prompt':'Instrumental dark Nordic baroque boss battle music for a Swedish naval action RPG. D minor at 120 BPM. Relentless bowed low strings, ostinato cello, martial snare, deep timpani, imposing pipe organ chords, short mournful French horn theme and answering brass. Heavy purposeful duel against a haunted iron dockmaster. Noble tragic and threatening, detailed acoustic orchestration, vivid dynamics with steady intensity. Immediate musical entry, continuous 8-bar phrases, no vocals, no lyrics, no silence, no fadeout, no trailer braams, no modern drums, no electronic dance music.','bpm':120,'audio_duration':64,'seed':30220604})
(ROOT/f'assets/source/{name}.request.json').write_text(json.dumps(prompt,indent=2))
job=req(LINK+'/v1/gpu/jobs',{'owner':'stormakt-episode2','owner_id':f'likvarv-{name}-v1','label':'Atlands arv local orchestral score','priority':60,'ttl_seconds':1800});jid=job['id']
try:
 while req(LINK+'/v1/gpu/jobs/'+jid)['status']!='running':time.sleep(3)
 status=req(LINK+'/v1/resources')
 if status['tts']['queued_or_running']:raise RuntimeError('TTS became active; do not unload')
 print(req(LINK+'/v1/resources/dots.tts/stop',{}),flush=True)
 print(req(LINK+'/v1/resources/voxcpm2/unload',{}),flush=True)
 print(req(ACE+'/v1/init',{'model':'acestep-v15-turbo','init_llm':False}),flush=True)
 result=req(ACE+'/release_task',prompt);print('ACCEPTED',result,flush=True)
 task=result['data']['task_id']
 start=time.time()
 while time.time()-start<900:
  status=req(ACE+'/query_result',{'task_id_list':json.dumps([task])});print(status,flush=True)
  item=status['data'][0]
  if item['status']==1:
   info=json.loads(item['result']);entry=info[0] if isinstance(info,list) else info
   audio=entry.get('file') or entry.get('audio_path') or entry.get('audio_url')
   if not audio:raise RuntimeError(entry)
   if audio.startswith('/v1/'):data=urllib.request.urlopen(ACE+audio,timeout=180).read()
   elif audio.startswith('http'):data=urllib.request.urlopen(audio,timeout=180).read()
   else:data=Path(audio).read_bytes()
   raw=ROOT/f'assets/source/{name}-master.wav';raw.write_bytes(data)
   (ROOT/f'assets/source/{name}.manifest.json').write_text(json.dumps({'task':task,'result':info,'sha256':hashlib.sha256(data).hexdigest()},indent=2))
   subprocess.run(['ffmpeg','-y','-v','error','-i',str(raw),'-af','loudnorm=I=-19:TP=-2:LRA=10','-ar','48000','-c:a','libvorbis','-q:a','6',str(ROOT/f'assets/audio/{name}.ogg')],check=True)
   break
  if item['status']==2:raise RuntimeError(item)
  time.sleep(4)
 else:raise TimeoutError(task)
finally:req(LINK+'/v1/gpu/jobs/'+jid+'/release',{'message':'Atland music generation finished'})
