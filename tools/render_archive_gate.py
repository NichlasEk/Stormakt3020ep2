"""Local LTX-2 image-to-video pass. Saves exact workflow and job results for reproduction."""
import json,time,urllib.request
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/'assets/source/cinematics'
def api(base,path,data=None):
    req=urllib.request.Request(base+path,data=json.dumps(data).encode() if data is not None else None,headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=60) as f:return json.load(f)
base='http://127.0.0.1:8198';broker='http://127.0.0.1:8765'
p=Path('/home/nichlas/ai/comfy-profiles/ltx-2/user/default/workflows/Stormakt_Intro_Smoke_API_working.json')
g=json.loads(p.read_text())['output']
for n in g.values():
    if 'ckpt_name' in n['inputs']:n['inputs']['ckpt_name']='LTX-2/ltx-2-19b-distilled-fp8.safetensors'
g['5180']['inputs']['image']='ep2-archive-gate-v1.png'
g['5175']['inputs']['value']='The chain slowly pulls taut with a heavy metallic creak. The massive stone slab slides to the LEFT into the wall, widening the existing crack on its right. Dust falls from the lintel. Cold daylight enters and reveals colossal ash roots outside the doorway. The camera gently advances toward the widening opening, then holds. Preserve the weathered oil-painted texture and rigid stone architecture. Restrained solemn motion, no morphing, no new objects, no people, no text, no cuts. Audio: quiet chain tension, low dry grinding stone, falling grit, faint wind beyond. No speech, no music.'
g['5185']['inputs'].update(width=768,height=512)
g['5186']['inputs']['value']=193
g['5189:5075']['inputs']['scale_by']=1.0
g['5189:5244']['inputs']['longer_edge']=768
g['5189:5254']['inputs']['strength']=0.85
g['5189:5111']['inputs']['noise_seed']=30200901
g['5189:5245']['inputs']['latents']=['5189:5114',0]
g['5189:5103']['inputs']['samples']=['5189:5114',1]
g['4958']['inputs'].update(filename_prefix='video/Ep2_Archive_Gate_v1',format='mp4',codec='h264')
# Keep only the first distilled pass, with no temporal/spatial upscaler or unused loaders.
needed=set()
def visit(key):
    if key in needed:return
    needed.add(key)
    for value in g[key]['inputs'].values():
        if isinstance(value,list) and len(value)==2 and isinstance(value[0],str) and value[0] in g:visit(value[0])
visit('4958');g={key:value for key,value in g.items() if key in needed}
(SRC/'archive-gate-ltx-v1.json').write_text(json.dumps(g,indent=2))
lease=api(broker,'/v1/gpu/jobs',{'owner':'Stormakt3020ep2','owner_id':'archive-gate-v1','label':'Arkivets port filmprov','kind':'video','ttl_seconds':3600})
(ROOT/'artifacts/cinematic-lease.json').write_text(json.dumps(lease))
try:
    if lease['status']!='running':raise RuntimeError('GPU lease not granted: '+str(lease))
    result=api(base,'/prompt',{'prompt':g,'client_id':'stormakt-ep2-cinematic'})
    (SRC/'archive-gate-job-v1.json').write_text(json.dumps(result,indent=2));print(result,flush=True)
    if 'prompt_id' not in result:raise RuntimeError(result)
    for i in range(360):
        h=api(base,'/history/'+result['prompt_id'])
        if h:
            (SRC/'archive-gate-history-v1.json').write_text(json.dumps(h,indent=2));print(json.dumps(h)[:4000],flush=True)
            if next(iter(h.values())).get('status',{}).get('status_str')!='success':raise RuntimeError('Video generation failed; see saved history')
            break
        if i%6==0:print('Rendering',i*10,'seconds',flush=True)
        time.sleep(10)
    else:raise TimeoutError('LTX render exceeded one hour')
finally:
    api(broker,'/v1/gpu/jobs/'+lease['id']+'/release',{})
