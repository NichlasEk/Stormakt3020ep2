"""Render the Ebba reunion shot through the verified local LTX graph and GPU broker."""
import json,time,urllib.request,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];SRC=ROOT/'assets/source/cinematics'
PROFILE=Path('/home/nichlas/ai/comfy-profiles/ltx-2')
def api(base,path,data=None):
    req=urllib.request.Request(base+path,data=json.dumps(data).encode() if data is not None else None,headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=90) as f:return json.load(f)
base='http://127.0.0.1:8198';broker='http://127.0.0.1:8765'
shots={'reunion':'An eight second quiet tender reunion between two adult Swedish officers in a candlelit stone room. The man and woman remain embracing after a gentle closed-mouth kiss. Very slight natural breathing, she rests against him, candle flames flicker. Locked camera. Preserve their exact faces, costumes, anatomy and hands. No extra hands, no new people, no camera rotation, no morphing. Dark historical oil-painted texture. Audio: quiet room tone and soft fabric movement, no dialogue, no music.'}
for index,(name,prompt) in enumerate(shots.items()):
    dest=SRC/f'rescue-{name}-ltx-v1.mp4'
    if dest.exists():print('Already rendered',name,flush=True);continue
    input_name=f'ep2-rescue-{name}-v1.png';shutil.copyfile(ROOT/'assets/art/rescue-reunion-v1.png',PROFILE/'input'/input_name)
    g=json.loads((SRC/'archive-gate-ltx-v1.json').read_text())
    g['5180']['inputs']['image']=input_name;g['5175']['inputs']['value']=prompt
    g['5189:5111']['inputs']['noise_seed']=30221180+index
    g['4958']['inputs']['filename_prefix']=f'video/Ep2_Rescue_{name}_v1'
    (SRC/f'rescue-{name}-workflow-v1.json').write_text(json.dumps(g,indent=2))
    lease=api(broker,'/v1/gpu/jobs',{'owner':'Stormakt3020ep2','owner_id':'rescue-'+name,'label':'Ebbas räddning '+name,'kind':'video','ttl_seconds':3600})
    try:
        for _ in range(120):
            if lease['status']=='running':break
            if lease['status'] not in ['queued','pending']:raise RuntimeError(lease)
            time.sleep(5);lease=api(broker,'/v1/gpu/jobs/'+lease['id'])
        else:raise TimeoutError('GPU queue did not grant a slot')
        job=api(base,'/prompt',{'prompt':g,'client_id':'stormakt-ep2-rescue-rest'})
        (SRC/f'rescue-{name}-job-v1.json').write_text(json.dumps(job,indent=2));print(name,job,flush=True)
        for i in range(360):
            h=api(base,'/history/'+job['prompt_id'])
            if h:
                (SRC/f'rescue-{name}-history-v1.json').write_text(json.dumps(h,indent=2))
                data=next(iter(h.values()))
                if data.get('status',{}).get('status_str')!='success':raise RuntimeError('LTX failed; see history')
                result=data['outputs']['4958']['images'][0]
                shutil.copyfile(PROFILE/'output'/result['subfolder']/result['filename'],dest)
                print('Saved',dest,flush=True);break
            if i%6==0:print(name,'rendering',i*10,'seconds',flush=True)
            time.sleep(10)
        else:raise TimeoutError('LTX render did not finish within one hour')
    finally:api(broker,'/v1/gpu/jobs/'+lease['id']+'/release',{})
