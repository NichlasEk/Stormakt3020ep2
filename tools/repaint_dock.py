#!/usr/bin/env python3
"""Local img2img art-direction revision, preserving the playable quay layout."""
import json,time,urllib.request,urllib.parse,hashlib,shutil
from pathlib import Path
from generate_art import req,COMFY,LINK,ROOT
name='likvarvet-oil-v2'
source=ROOT/'assets/art/likvarvet.png'
target=Path('/home/nichlas/ai/comfy-profiles/krea-2-turbo/input/atland-quay-layout.png')
shutil.copy2(source,target)
graph=json.loads((ROOT/'assets/source/likvarvet.json').read_text())
graph['11']={'class_type':'LoadImage','inputs':{'image':target.name}}
graph['7']={'class_type':'VAEEncode','inputs':{'pixels':['11',0],'vae':['3',0]}}
graph['4']['inputs']['text']='A dark seventeenth-century Nordic oil painting on coarse linen canvas, a weatherbeaten Swedish military harbour in 1690, seen obliquely from above. Preserve the exact arrangement of the diagonal stone quay, moored wooden warship on the left, two round bronze pavement seals, and the iron sea gate at upper right. Deep tenebrist chiaroscuro. Everything is old, heavy, battered and damp. Angular broken grey granite cobbles with wet charcoal grout, black rotting timber, algae and soot, corroded dull brass, frayed rope. The stone quay has broad open traversable ground. A gaunt eroded lion carved on the ship, a massive decaying gate. Sparse amber tallow lanterns illuminate small patches of the quay, elsewhere muted moonlight and deep brown-black shadows. Palette: raw umber, burnt sienna, dirty grey, dark indigo, muted ochre. Thick irregular visible oil brushstrokes, scumbled shadows, dry brush edges, worn varnish, fine canvas grain, historic Dutch Golden Age maritime painting, solemn Nordic national romantic atmosphere, bleak grandeur and melancholy, museum quality painting. Hard angular architecture and natural irregular surfaces. No people, no writing.'
graph['6']['inputs']['text']='cute, cartoon, chibi, toy, rounded, plastic, glossy, shiny gold, clean, smooth airbrush, smooth gradient, mobile game, cheerful, vivid colors, saturated cyan, turquoise glow, fantasy illustration, vector art, 3D render, polished marble, bloom, text, watermark, people'
graph['5']['inputs']['multiplier']=2.5
graph['8']['inputs'].update({'seed':30220621,'denoise':.78,'steps':12,'cfg':2.0})
graph['10']['inputs']['filename_prefix']='AtlandsArv/'+name
(ROOT/f'assets/source/{name}.json').write_text(json.dumps(graph,indent=2))
lease=req(LINK+'/v1/gpu/jobs',{'owner':'stormakt-episode2','owner_id':name,'label':'Worn Nordic oil painting revision','priority':60,'ttl_seconds':1800});jid=lease['id']
try:
 while req(LINK+'/v1/gpu/jobs/'+jid)['status']!='running':time.sleep(3)
 job=req(COMFY+'/prompt',{'prompt':graph,'client_id':'atlands-arv'});pid=job['prompt_id'];print(pid,flush=True)
 start=time.time()
 while time.time()-start<900:
  history=req(COMFY+'/history/'+pid).get(pid)
  if history:
   if history.get('status',{}).get('status_str')=='error':raise RuntimeError(history)
   im=history['outputs']['10']['images'][0]
   data=urllib.request.urlopen(COMFY+'/view?'+urllib.parse.urlencode(im),timeout=90).read()
   out=ROOT/f'assets/art/{name}.png';out.write_bytes(data)
   (ROOT/f'assets/source/{name}.manifest.json').write_text(json.dumps({'prompt_id':pid,'seed':30220621,'reference_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'sha256':hashlib.sha256(data).hexdigest(),'revision':'User requested dark worn oil painting, less cute and rounded.'},indent=2))
   print(out,flush=True);break
  time.sleep(3)
 else:raise TimeoutError(pid)
finally:
 try:req(COMFY+'/free',{'unload_models':True,'free_memory':True})
 finally:req(LINK+'/v1/gpu/jobs/'+jid+'/release',{'message':'Oil painting revision finished'})
