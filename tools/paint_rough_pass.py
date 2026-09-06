#!/usr/bin/env python3
"""Reference-preserving local Krea art studies. Does not edit the original game."""
import argparse, hashlib, json, shutil, time, urllib.parse, urllib.request
from pathlib import Path
from generate_art import ROOT, COMFY, LINK, req

ORIGINAL = Path('/home/nichlas/WaylandForge/assets/stormakt3020')
SPECS = {
    'quay': (
        ROOT/'assets/art/likvarvet-oil-v2.png', 'likvarvet-rough-v3', .48,
        'Repaint this exact harbour composition as gritty matte pre-rendered dark action RPG scenery, with the material realism and oppressive atmosphere of Diablo II and an austere seventeenth century Nordic military oil painting. Keep every quay edge, gate, ship, rope, lantern and pavement seal at exactly the same location, same camera and clear walkable floor. All surfaces are coarse, exhausted and unvarnished: splintered dry black timber, cracked granular granite, ash and ingrained charcoal dirt in stone joints, pitted rusty iron and oxidised dull bronze. Remove polished reflections from the paving, carved lion and hull. Cold murky grey Baltic water with barely visible diffuse ripples. Cloudy cold light with small dim amber lantern cores, no pools of glossy orange reflections. Dry scumbled thin oil paint and dense fine material detail, subdued soot grey, dirty umber and desaturated navy. Serious ruined military dock, sharp fractured shapes, convincing weight. Preserve scene readability and midtone separation. No people, no text, no canvas border.',
    ),
    'karl': (
        ORIGINAL/'dungeon-karl-combat-v1-source.png', 'karl-combat-rough-v4', .43,
        'Precisely preserve this four-column two-row eight-cell character combat sprite sheet, all eight poses, exact weapons, cell locations, scale, three-quarter overhead camera and body silhouettes. The SAME young adult Swedish Carolean in every cell: short brown hair, small brass forehead lamp, deep blue knee length wool coat, dark steel cuirass, muted ochre gloves and cuffs, tall worn boots. Keep his identity. Repaint clothing with battle wear, torn muddy hems, grime, sharp weighted fabric folds; paint realistic tired angular young adult face and natural human anatomy. Small pitted steel highlights, tarnished brass buttons, matte cloth and worn leather. Rich finely detailed gritty pre-rendered Diablo II dark action RPG sprite aesthetic mixed with sober historic oil-painted material texture. Strong readable silhouette, restrained colour, no simplification. Preserve exact flat solid bright magenta #ff00ff background between and around all figures, no floor or shadows. First row sword ready, sword swing, sword follow through, sword guard. Second row side ready, side sword strike, two handed mining hammer strike, stagger. Nothing added, no writing.',
    ),
    'guard': (
        ORIGINAL/'dungeon-danish-enemies-v1-source.png', 'guard-combat-rough-v4', .40,
        'Preserve this exact four-column two-row eight-cell combat sprite sheet. Same eight poses, positions, adult human anatomy, scale, weapons and three-quarter overhead camera. First row four sword soldier combat poses, second row four pikeman combat poses. Repaint as exhausted grim seventeenth century soldiers with dirty faded oxblood red wool coats, soot black dented helmets, coarse crossbelts, corroded buckles and mud caked boots. Stern gaunt faces beneath helmet shadows, lean human bodies, weighted angular folds and fine worn material detail. Rich finely detailed gritty pre-rendered Diablo II dark action RPG sprite aesthetic with sober historic oil-painted texture. Matte surfaces and restrained tiny steel highlights. Preserve the flat solid bright magenta #ff00ff backdrop everywhere around and between figures, no ground, no shadows. Clearly separate all eight figures, no labels or text.',
    ),
}

def main():
    parser=argparse.ArgumentParser();parser.add_argument('asset', choices=SPECS)
    args=parser.parse_args();source,name,denoise,prompt=SPECS[args.asset]
    folder=ROOT/'assets/source/rough-pass';folder.mkdir(exist_ok=True)
    target=Path('/home/nichlas/ai/comfy-profiles/krea-2-turbo/input')/(name+'-reference.png')
    shutil.copy2(source,target)
    g=json.loads((ROOT/'assets/source/likvarvet.json').read_text())
    g['11']={'class_type':'LoadImage','inputs':{'image':target.name}}
    g['7']={'class_type':'VAEEncode','inputs':{'pixels':['11',0],'vae':['3',0]}}
    g['4']['inputs']['text']=prompt
    g['6']['inputs']['text']='cute, chibi, toy, round plastic, glossy, polished, wax, varnish, wet shiny stones, bloom, smooth gradient, airbrush, miniature, wooden puppet, simplified geometry, low poly, oversized head, cartoon, bright saturated colours, text, labels, watermark, changed composition'
    g['5']['inputs']['multiplier']=2.5
    seed=30220640+list(SPECS).index(args.asset)
    g['8']['inputs'].update(seed=seed,denoise=denoise,steps=16,cfg=2.2)
    g['10']['inputs']['filename_prefix']='AtlandsArv/'+name
    (folder/(name+'.json')).write_text(json.dumps(g,indent=2))
    lease=req(LINK+'/v1/gpu/jobs',{'owner':'stormakt-episode2','owner_id':name,'label':'Rough matte reference art: '+args.asset,'priority':60,'ttl_seconds':1800});jid=lease['id']
    try:
        deadline=time.monotonic()+1500
        while req(LINK+'/v1/gpu/jobs/'+jid)['status']!='running':
            if time.monotonic()>deadline:raise TimeoutError('GPU lease '+jid)
            time.sleep(3)
        pid=req(COMFY+'/prompt',{'prompt':g,'client_id':'atlands-arv'})['prompt_id'];print(pid,flush=True)
        deadline=time.monotonic()+900
        while time.monotonic()<deadline:
            h=req(COMFY+'/history/'+pid).get(pid)
            if h:
                if h.get('status',{}).get('status_str')=='error':raise RuntimeError(h)
                im=h['outputs']['10']['images'][0]
                data=urllib.request.urlopen(COMFY+'/view?'+urllib.parse.urlencode(im),timeout=90).read()
                out=ROOT/'assets/art'/(name+'.png');out.write_bytes(data)
                (folder/(name+'.manifest.json')).write_text(json.dumps({'prompt_id':pid,'seed':seed,'reference':str(source),'reference_sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'sha256':hashlib.sha256(data).hexdigest(),'purpose':'Matte environment revision' if args.asset=='quay' else 'Reference-based sprite study; not a complete directional animation set'},indent=2))
                print(out,flush=True);break
            time.sleep(3)
        else:raise TimeoutError(pid)
    finally:
        try:req(COMFY+'/free',{'unload_models':True,'free_memory':True})
        finally:req(LINK+'/v1/gpu/jobs/'+jid+'/release',{'message':'Rough art pass finished'})

if __name__=='__main__':main()
