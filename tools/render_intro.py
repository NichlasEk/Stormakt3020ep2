"""Render three intro shots through the verified local LTX graph and GPU broker."""
import json,time,urllib.request,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];SRC=ROOT/'assets/source/cinematics'
PROFILE=Path('/home/nichlas/ai/comfy-profiles/ltx-2')
def api(base,path,data=None):
    req=urllib.request.Request(base+path,data=json.dumps(data).encode() if data is not None else None,headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(req,timeout=90) as f:return json.load(f)
base='http://127.0.0.1:8198';broker='http://127.0.0.1:8765'
shots={
'ship':'The ship moves slowly forward through the water. Small waves slide along its solid oak hull. A little fog drifts across the distant rocks and the rigging sways very slightly. Camera tracks gently beside the bow, no orbit. Preserve the old oil painting brush texture, stable ship geometry, sombre lighting and exact composition. No new objects, no characters, no cuts, no transformation. Audio: low water against wood, faint rigging creaks and wind, no speech, no music.',
'chart':'A very slow camera push toward the ancient bronze chart. Cold light from the window moves subtly across the tarnished metal as the ship rocks almost imperceptibly. Everything on the table stays firmly in place. Preserve the engraved coastal contours, sun disc, rigid metal geometry and old oil painting brush texture exactly. No glowing lines, no new marks, no hands, no people, no text, no cuts. Audio: quiet wooden hull creaks, faint water outside the porthole, no speech, no music.',
'coast':'Camera advances very slowly from the water toward the small stone harbor entrance. Gentle waves lap at the lowest steps. Mist thins gradually to reveal the roots gripping the ruined wall. Keep the architecture, trees and roots motionless and rigid. Preserve the old oil painting brush texture and muted grey dawn palette. No buildings appear, no new objects, no people, no magical light, no cuts. Audio: low wash of water against stone and faint coastal wind, no speech, no music.'}
for index,(name,prompt) in enumerate(shots.items()):
    dest=SRC/f'intro-{name}-ltx-v1.mp4'
    if dest.exists():print('Already rendered',name,flush=True);continue
    input_name=f'ep2-intro-{name}-v1.png';shutil.copyfile(SRC/f'intro-{name}-keyframe-v1.png',PROFILE/'input'/input_name)
    g=json.loads((SRC/'archive-gate-ltx-v1.json').read_text())
    g['5180']['inputs']['image']=input_name;g['5175']['inputs']['value']=prompt
    g['5189:5111']['inputs']['noise_seed']=30201010+index
    g['4958']['inputs']['filename_prefix']=f'video/Ep2_Intro_{name}_v1'
    (SRC/f'intro-{name}-workflow-v1.json').write_text(json.dumps(g,indent=2))
    lease=api(broker,'/v1/gpu/jobs',{'owner':'Stormakt3020ep2','owner_id':'intro-'+name,'label':'Introfilm '+name,'kind':'video','ttl_seconds':3600})
    try:
        for _ in range(120):
            if lease['status']=='running':break
            if lease['status'] not in ['queued','pending']:raise RuntimeError(lease)
            time.sleep(5);lease=api(broker,'/v1/gpu/jobs/'+lease['id'])
        else:raise TimeoutError('GPU queue did not grant a slot')
        job=api(base,'/prompt',{'prompt':g,'client_id':'stormakt-ep2-intro'})
        (SRC/f'intro-{name}-job-v1.json').write_text(json.dumps(job,indent=2));print(name,job,flush=True)
        for i in range(360):
            h=api(base,'/history/'+job['prompt_id'])
            if h:
                (SRC/f'intro-{name}-history-v1.json').write_text(json.dumps(h,indent=2))
                data=next(iter(h.values()))
                if data.get('status',{}).get('status_str')!='success':raise RuntimeError('LTX failed; see history')
                result=data['outputs']['4958']['images'][0]
                shutil.copyfile(PROFILE/'output'/result['subfolder']/result['filename'],dest)
                print('Saved',dest,flush=True);break
            if i%6==0:print(name,'rendering',i*10,'seconds',flush=True)
            time.sleep(10)
        else:raise TimeoutError('LTX render did not finish within one hour')
    finally:api(broker,'/v1/gpu/jobs/'+lease['id']+'/release',{})
