#!/usr/bin/env python3
"""A painted character study, deliberately separate from production animations."""
import json,time,urllib.request,urllib.parse,hashlib
from generate_art import ROOT,COMFY,LINK,req
name='karl-oil-study-v3'
folder=ROOT/'assets/source/concepts';folder.mkdir(exist_ok=True)
g=json.loads((ROOT/'assets/source/likvarvet.json').read_text())
g['7']['inputs'].update({'width':1024,'height':1536})
g['4']['inputs']['text']='A full-length oil-painted portrait of one battle-worn adult Swedish Carolean officer, circa 1700, a physically believable lean tall man aged forty, with mature realistic human anatomy, eight-head-tall proportions, small anatomically sized head, long legs, narrow natural hands, tired angular face, pronounced cheekbones, short stubble, stern eyes. His entire body and both boots are visible. He stands in a three-quarter view, seen slightly from above, right hand holding a long battle-scarred steel officers saber pointing diagonally down. Dark battered black felt tricorne, dirty deep indigo blue knee-length wool uniform coat, very muted ochre cuffs, a few tarnished small brass buttons, worn leather crossbelt, black mud-caked tall boots, patched cloth and rain-soaked hem. The sword is actual human sword size, the coat has real folds and hanging weight. Deep raw umber and charcoal background with no environment, isolated full figure occupying almost the entire height. A serious seventeenth-century Dutch oil painting of a soldier, visible impasto brush strokes, thin glazes over coarse linen, muted earth colors, restrained warm side lighting, deep shadows, believable painted skin, tragic dignity and exhaustion. Paint the human being with the realism of an old master military portrait. No ornament beyond the uniform, no large shoulder pads, no stylized anatomy.'
g['6']['inputs']['text']='cute, cartoon, chibi, toy, doll, wooden puppet, miniature, figurine, plastic, low poly, 3d render, voxel, oversized head, short legs, large eyes, oversized hands, round face, anime, fantasy armor, ornate pauldrons, oversized weapon, gold armor, exaggerated proportions, disney, pixar, game icon, smile, polished, airbrush, cel shaded, clean vector illustration, text, frame, cropped feet, missing boots'
g['5']['inputs']['multiplier']=2.5
g['8']['inputs'].update({'seed':30220631,'steps':12,'cfg':2.0,'denoise':1.0})
g['10']['inputs']['filename_prefix']='AtlandsArv/'+name
(folder/(name+'.json')).write_text(json.dumps(g,indent=2))
lease=req(LINK+'/v1/gpu/jobs',{'owner':'stormakt-episode2','owner_id':name,'label':'Adult painted Karl character study','priority':60,'ttl_seconds':1800});jid=lease['id']
try:
 while req(LINK+'/v1/gpu/jobs/'+jid)['status']!='running':time.sleep(3)
 pid=req(COMFY+'/prompt',{'prompt':g,'client_id':'atlands-arv'})['prompt_id'];print(pid,flush=True)
 start=time.time()
 while time.time()-start<900:
  h=req(COMFY+'/history/'+pid).get(pid)
  if h:
   if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h)
   im=h['outputs']['10']['images'][0];data=urllib.request.urlopen(COMFY+'/view?'+urllib.parse.urlencode(im),timeout=90).read()
   out=folder/(name+'.png');out.write_bytes(data)
   (folder/(name+'.manifest.json')).write_text(json.dumps({'prompt_id':pid,'seed':30220631,'sha256':hashlib.sha256(data).hexdigest(),'status':'character art study, not yet a runtime sprite'},indent=2))
   print(out,flush=True);break
  time.sleep(3)
 else:raise TimeoutError(pid)
finally:
 try:req(COMFY+'/free',{'unload_models':True,'free_memory':True})
 finally:req(LINK+'/v1/gpu/jobs/'+jid+'/release',{'message':'Karl painted study finished'})
