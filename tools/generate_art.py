#!/usr/bin/env python3
"""Local Krea production; saves exact API graphs, source images and hashes."""
import json, time, urllib.request, hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
COMFY='http://192.168.32.88:8188'
LINK='http://127.0.0.1:8765'
def req(url,data=None):
    r=urllib.request.Request(url, data=None if data is None else json.dumps(data).encode(),headers={'Content-Type':'application/json'})
    with urllib.request.urlopen(r,timeout=90) as f:
        body=f.read()
        return json.loads(body) if body else {}
def generate(name,prompt,w,h,seed):
    graph={
      '1':{'class_type':'UNETLoader','inputs':{'unet_name':'Krea-2-Turbo/krea2_turbo_fp8_scaled.safetensors','weight_dtype':'default'}},
      '2':{'class_type':'CLIPLoader','inputs':{'clip_name':'Krea-2/qwen3vl_4b_fp8_scaled.safetensors','type':'krea2','device':'default'}},
      '3':{'class_type':'VAELoader','inputs':{'vae_name':'Krea-2/qwen_image_vae.safetensors'}},
      '4':{'class_type':'CLIPTextEncode','inputs':{'clip':['2',0],'text':prompt}},
      '5':{'class_type':'ConditioningKrea2Rebalance','inputs':{'conditioning':['4',0],'multiplier':4.0,'per_layer_weights':'1.0,1.0,1.0,1.0,1.0,1.0,1.0,2.5,5.0,1.1,4.0,1.0'}},
      '6':{'class_type':'CLIPTextEncode','inputs':{'clip':['2',0],'text':'text, watermark, interface, people, characters, blurry, modern buildings, photographic, perspective horizon'}},
      '7':{'class_type':'EmptyLatentImage','inputs':{'width':w,'height':h,'batch_size':1}},
      '8':{'class_type':'KSampler','inputs':{'model':['1',0],'positive':['5',0],'negative':['6',0],'latent_image':['7',0],'seed':seed,'steps':8,'cfg':3.5,'sampler_name':'euler','scheduler':'simple','denoise':1.0}},
      '9':{'class_type':'VAEDecode','inputs':{'samples':['8',0],'vae':['3',0]}},
      '10':{'class_type':'SaveImage','inputs':{'images':['9',0],'filename_prefix':'AtlandsArv/'+name}}}
    (ROOT/'assets/source'/f'{name}.json').write_text(json.dumps(graph,indent=2))
    job=req(COMFY+'/prompt',{'prompt':graph,'client_id':'atlands-arv'})
    pid=job['prompt_id'];print(name,pid,flush=True)
    start=time.time()
    while time.time()-start<900:
        result=req(COMFY+'/history/'+pid).get(pid)
        if result:
            if result.get('status',{}).get('status_str')=='error':raise RuntimeError(str(result))
            for out in result['outputs'].values():
                for im in out.get('images',[]):
                    from urllib.parse import urlencode
                    data=urllib.request.urlopen(COMFY+'/view?'+urlencode(im),timeout=90).read()
                    dest=ROOT/'assets/art'/f'{name}.png';dest.write_bytes(data)
                    (ROOT/'assets/source'/f'{name}.manifest.json').write_text(json.dumps({'prompt_id':pid,'seed':seed,'model':'Krea-2-Turbo','sha256':hashlib.sha256(data).hexdigest(),'source':im},indent=2))
                    print('saved',dest,flush=True);return
        time.sleep(3)
    raise TimeoutError(pid)
if __name__=='__main__':
    lease=req(LINK+'/v1/gpu/jobs',{'owner':'stormakt-episode2','owner_id':'likvarv-art-v1','label':'Atlands arv dock painting','priority':60,'ttl_seconds':1800})
    jid=lease['id']
    try:
        while req(LINK+'/v1/gpu/jobs/'+jid)['status']!='running':time.sleep(3)
        generate('likvarvet', 'A magnificent hand-painted isometric action RPG environment, full level background, oblique overhead orthographic camera looking down 55 degrees, NO horizon. Nordic romantic dark naval shipyard at blue hour, seventeenth century Swedish baroque science fiction. A very broad EMPTY rectangular pale wet stone quay occupies the middle seventy percent of the image, extends from bottom center towards upper center, generous completely clear arena floor for fighting. Ancient cobblestones, moss, subtle engraved brass circles in ground. Around the OUTER edges only: black teal Baltic seawater, decaying huge wooden warship hull on the left, massive carved lion prow, a copper crane upper right, coiled mooring ropes, stone sea wall and amber lanterns. Back edge a monumental closed sea gate of iron and tarnished gold with faint turquoise runes. Birch trees and weeds on stone at corners. The central playable courtyard is flat and unobstructed. Beautiful restrained luminous atmosphere, moonlight on water, gold light from lanterns, painterly historical illustration, detailed believable materials, old oil painting, high quality game art, no people, no text, no interface.',1536,1024,302020601)
    finally:
        try: req(COMFY+'/free',{'unload_models':True,'free_memory':True})
        finally: req(LINK+'/v1/gpu/jobs/'+jid+'/release',{'message':'Atland art pass finished'})
