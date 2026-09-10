#!/usr/bin/env python3
"""Generate restartable local ACE-Step masters and normalized bar-overlap game loops."""
import json,time,urllib.request,subprocess,hashlib,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];SRC=ROOT/'assets/source/music';OUT=ROOT/'assets/audio/music'
SRC.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
LINK='http://127.0.0.1:8765';ACE='http://127.0.0.1:8001'
def req(url,data=None):
 r=urllib.request.Request(url,data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
 with urllib.request.urlopen(r,timeout=240) as f:return json.load(f)
def run(args):subprocess.run(args,check=True)
tracks=json.loads((ROOT/'assets/story/music.json').read_text());initialized=False
for track in tracks:
 name=track['id'];out=OUT/(name+'.ogg');master=SRC/(name+'.flac');manifest=SRC/(name+'.json')
 if len(sys.argv)>1 and name not in sys.argv[1:]:continue
 if out.exists() and manifest.exists():print('CACHED',name,flush=True);continue
 job=req(LINK+'/v1/gpu/jobs',{'owner':'stormakt-episode2','owner_id':'music-suite-'+name,'label':'Atland soundtrack: '+name,'priority':60,'ttl_seconds':1800});jid=job['id']
 try:
  while job['status']!='running':
   if job['status'] not in ('queued','pending'):raise RuntimeError(job)
   time.sleep(3);job=req(LINK+'/v1/gpu/jobs/'+jid)
  if not master.exists():
   if not initialized:
    if req(LINK+'/v1/resources')['tts']['queued_or_running']:raise RuntimeError('TTS active; leaving other work intact')
    req(LINK+'/v1/resources/dots.tts/stop',{});req(LINK+'/v1/resources/voxcpm2/unload',{})
    health=req(ACE+'/health')['data']
    if not health['models_initialized']:
     result=req(ACE+'/v1/init',{'model':'acestep-v15-turbo','init_llm':False})
     if not req(ACE+'/health')['data']['models_initialized']:raise RuntimeError(result)
    initialized=True
   payload={'prompt':track['prompt'],'lyrics':'[Instrumental]','vocal_language':'unknown','audio_duration':track['duration'],'audio_format':'wav','task_type':'text2music','batch_size':1,'thinking':False,'use_cot_caption':False,'use_cot_language':False,'use_cot_metas':False,'use_format':False,'bpm':track['bpm'],'key_scale':track['key'],'time_signature':'4/4','inference_steps':8,'guidance_scale':7,'use_random_seed':False,'seed':track['seed']}
   request=SRC/(name+'.request.json');request.write_text(json.dumps(payload,indent=2))
   taskfile=SRC/(name+'.task.json')
   if taskfile.exists():task=json.loads(taskfile.read_text())['task_id']
   else:
    result=req(ACE+'/release_task',payload);task=result['data']['task_id'];taskfile.write_text(json.dumps({'task_id':task},indent=2))
   print('GENERATING',name,task,flush=True);start=time.time()
   while time.time()-start<1200:
    result=req(ACE+'/query_result',{'task_id_list':json.dumps([task])});item=result['data'][0]
    if item['status']==1:
     info=json.loads(item['result']);entry=info[0] if isinstance(info,list) else info;path=entry.get('file') or entry.get('audio_path') or entry.get('audio_url')
     data=urllib.request.urlopen(ACE+path if path.startswith('/v1/') else path,timeout=180).read() if path.startswith(('/v1/','http')) else Path(path).read_bytes()
     raw=SRC/(name+'.download.wav');raw.write_bytes(data);run(['ffmpeg','-y','-v','error','-i',str(raw),'-c:a','flac',str(master)]);raw.unlink();(SRC/(name+'.result.json')).write_text(json.dumps(info,indent=2));break
    if item['status']==2:raise RuntimeError(item)
    time.sleep(3)
   else:raise TimeoutError(task)
  bar=240/track['bpm'];end=(track['bars']+1)*bar
  graph=f'[0:a]asplit=3[h][b][t];[h]atrim=0:{bar},asetpts=PTS-STARTPTS[head];[b]atrim={bar}:{end-bar},asetpts=PTS-STARTPTS[body];[t]atrim={end-bar}:{end},asetpts=PTS-STARTPTS[tail];[tail][head]acrossfade=d={bar}:c1=tri:c2=tri[join];[body][join]concat=n=2:v=0:a=1,loudnorm=I=-22:TP=-3:LRA=8[out]'
  run(['ffmpeg','-y','-v','error','-i',str(master),'-filter_complex',graph,'-map','[out]','-ar','48000','-c:a','libvorbis','-q:a','5',str(out)])
  analysis=subprocess.run(['ffmpeg','-hide_banner','-i',str(out),'-af','loudnorm=I=-22:TP=-3:LRA=8:print_format=json','-f','null','-'],capture_output=True,text=True,check=True).stderr
  levels=json.JSONDecoder().raw_decode(analysis[analysis.rfind('{'):])[0];manifest.write_text(json.dumps({'id':name,'master':str(master.relative_to(ROOT)),'file':str(out.relative_to(ROOT)),'duration':track['bars']*bar,'overlap_seconds':bar,'filter':graph,'levels':levels,'sha256':hashlib.sha256(out.read_bytes()).hexdigest()},indent=2))
  print('SAVED',name,levels['input_i'],levels['input_tp'],flush=True)
 finally:req(LINK+'/v1/gpu/jobs/'+jid+'/release',{'message':'music track done'})
