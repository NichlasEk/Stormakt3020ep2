#!/usr/bin/env python3
"""Generate new synthetic Episode II cast and lines on local EutherLink."""
import json,base64,time,urllib.request,hashlib,subprocess,sys,re,wave
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
name_lines={
 'names-intro':('ebba','Kartan följer vår egen kust. Men namnen är andra. Karl, det finns skrift under kajens sigill. Frilägg den. Jag tar fram liggaren.'),
 'names-warning':('ebba','Rörelse vid porten. De kommer för stenarna. Lägg undan avtrycket och möt dem.'),
 'name-0':('ebba','Ingrid Jonsdotter. I liggaren står att färjan övergavs. Men stenen säger att hon väntade på den sista. Någon har tagit bort människorna ur berättelsen.'),
 'name-1':('ebba','Mats Eriksson. Dömd för stöld ur kronans magasin. Här står vilka han gav säden till. Det var en lång vinter, Karl.'),
 'name-2':('ebba','Siri Nilsdotter. Barnen kom tillbaka och högg hennes namn. I rapporten står det inga civila förluster. Det är en mycket prydlig rapport.'),
 'broadcast':('ebba','Jag sänder namnen på öppen frekvens. Nu finns de hos fler än oss. Hela hamnen hörde det, Karl. Ta dig tillbaka till båten.'),
 'cipher':('ebba','Avtrycken är säkrade. Jag skickar dem krypterat och håller båten redo med förband. Kom tillbaka, Karl. Vi behöver ett levande vittne också.'),
 'homebound':('ebba','Alla tre namnen är ombord. På bronskartan står Uppsala där våra sjökort visar inland. Vi följer spåret i gryningen.')}
roles['hedvig']={'seed':30220606,'instruction':'A mature Swedish female historian around fifty-five, thoughtful low alto, slightly grainy warm voice, precise calm Swedish pronunciation, contemplative pacing, quiet wonder and resolve, distinctly older than the naval commander, no theatrical acting.','text':'Jag heter Hedvig Rålamb. Mitt arbete är att läsa det som andra har slutat se. En sten kan bära ett namn i tusen år. Men någon måste stanna och lyssna till berättelsen.'}
hedvig_lines={
 'hedvig-karta':('hedvig','Hedvig Rålamb här. Kartans linjer följer gamla vadställen och gravhögar. Karl, vi behöver avtryck av inskrifterna.'),
 'hedvig-minne':('hedvig','Rudbecks äpplen är minne, tal och skrift. Stenarna bevarar gärningar som kronan har strukit. Ta med avtrycken.')}
roles['arvid']={'seed':30220908,'instruction':'An adult Swedish male captain about fifty-five, restrained weathered baritone, clear natural Swedish, exhausted dignity, quiet human warmth, serious and calm, no villain voice, no theatrical growl.','text':'Jag heter Arvid Silfvergren. Mina män har hållit vägen öppen genom vintern. Nu väntar vi på order om avlösning. Jag vill se dem återvända hem medan någon ännu minns deras namn.'}
roles['bailiff']={'seed':30220910,'instruction':'An adult Swedish male crown bailiff around sixty, dry resonant low baritone, precise bureaucratic Swedish diction, measured calm severity, tired and utterly convinced, no theatrical monster growl, no comedy.','text':'Arbetet fortsätter enligt kronans beslut. Varje namn skall föras in i liggaren. Ingen lämnar sin post innan räkningen är avslutad.'}
if '--salt-only' in sys.argv: lines=json.loads((ROOT/'assets/story/salt-radio.json').read_text())
elif '--west-only' in sys.argv: lines=json.loads((ROOT/'assets/story/west-radio.json').read_text())
if '--gamla-only' in sys.argv: lines=json.loads((ROOT/'assets/story/gamla-radio.json').read_text())
elif '--observatory-only' in sys.argv: lines=json.loads((ROOT/'assets/story/observatory-radio.json').read_text())
elif '--cabin-only' in sys.argv: lines=json.loads((ROOT/'assets/story/cabin-radio.json').read_text())
elif '--meridian-only' in sys.argv: lines=json.loads((ROOT/'assets/story/meridian-radio.json').read_text())
elif '--continuity-only' in sys.argv: lines=json.loads((ROOT/'assets/story/continuity-radio.json').read_text())
elif '--uppsala-only' in sys.argv: lines=json.loads((ROOT/'assets/story/uppsala-radio.json').read_text())
elif '--foundry-only' in sys.argv: lines=json.loads((ROOT/'assets/story/foundry-radio.json').read_text())
elif '--mine-only' in sys.argv: lines=json.loads((ROOT/'assets/story/mine-radio.json').read_text())
elif '--regiment-only' in sys.argv: lines=json.loads((ROOT/'assets/story/regiment-radio.json').read_text())
elif '--intro-only' in sys.argv: lines=json.loads((ROOT/'assets/story/intro-radio.json').read_text())
elif '--roots-only' in sys.argv: lines=json.loads((ROOT/'assets/story/rootway-radio.json').read_text())
elif '--archive-only' in sys.argv: lines=json.loads((ROOT/'assets/story/archive-radio.json').read_text())
elif '--rooms-only' in sys.argv: lines=json.loads((ROOT/'assets/story/rooms-radio.json').read_text())
elif '--journey-only' in sys.argv: lines=json.loads((ROOT/'assets/story/journey-radio.json').read_text())
elif '--hedvig-only' in sys.argv: lines=hedvig_lines
elif '--names-only' in sys.argv: lines=name_lines
else: lines.update(name_lines);lines.update(hedvig_lines)
roles['meridian']={'seed':30220917,'instruction':'A mature Swedish male astronomer, restrained clear baritone, weary educated authority, measured natural speech, humane and serious rather than theatrical, precise intelligible Swedish.','text':'Morgonen ligger kvar över gården. Jag har räknat alla slagen. Ingen annan har skrivit sitt namn under ordern. Låt mig läsa den en gång till.'}
roles['marta']={'seed':30220919,'instruction':'An adult Swedish woman around forty, observatory technician, clear natural Swedish pronunciation, grounded warm mezzo voice, tired but brave and precise, soft steady delivery, no theatrical acting.','text':'Jag heter Märta Vinge. Jag arbetar med instrumenten i observatoriet. Min syster väntar på mig. Vi har hållit lampan tänd varje natt och jag tänker inte lämna henne här.'}
roles['elin']={'seed':30220931,'instruction':'Adult Swedish woman age thirty, quiet clear natural Swedish voice, tired but resolute, warm medium register, no theatrical whisper.','text':'Jag heter Elin Vinge. Jag minns min syster och vägen hem. Jag tänker inte skriva under ett annat namn. Det här är mitt vittnesmål.'}
roles['officer']={'seed':30220932,'instruction':'Swedish male official age fifty five, stern dry baritone, measured formal authority, clear intelligible Swedish, no shouting or monstrous effects.','text':'Mönstringen pågår. Varje namn skall bekräftas innan porten öppnas. Stanna vid vågen och invänta er tur. Jag ansvarar för förrättningen.'}
roles['nils']={'seed':30220925,'instruction':'A Swedish adult male clerk around forty five, clear natural Swedish, quiet tense tenor baritone with a dry soft grain, humane and precise, tired but relieved to speak, not theatrical, not a villain.','text':'Jag heter Nils Berg. Jag skrev kvittenserna vid den inre porten. Min uppgift var att räkna de ankommande. Jag minns deras ansikten bättre än deras nummer.'}
roles['saltwarden']={'seed':30220940,'instruction':'Swedish adult male sluice keeper, deep calm weathered bass, clear intelligible natural Swedish, grave responsibility and controlled authority, no monstrous distortion, no comedy.','text':'Jag vakar över källan. Varje vittne står under mitt ansvar. Ingen lämnar platsen innan överföringen är avslutad.'}
for role,v in roles.items():
 if not any(r==role for r,_ in lines.values()):continue
 ref=render(role+'-reference',{'text':v['text'],'voice_instruction':v['instruction'],'language':'sv','model_backend':'voxcpm2','output_format':'wav','normalize':False,'seed':v['seed']})
 for name,(r,line) in lines.items():
  if r!=role:continue
  if (ROOT/'assets/audio'/f'voice-{name}.ogg').exists() and not ('--retake-regiment' in sys.argv and name in {'regiment-captain','regiment-freed','regiment-marshal'}):continue
  raw=render(name,{'text':line,'voice_instruction':v['instruction'],'language':'sv','model_backend':'voxcpm2' if r=='saltwarden' else 'dots.tts-mf','output_format':'wav','normalize':False,'seed':v['seed']+100,'reference_wav_base64':base64.b64encode(ref.read_bytes()).decode(),'prompt_text':v['text'],'dots_num_steps':8 if '--regiment-only' in sys.argv or '--rooms-only' in sys.argv or '--archive-only' in sys.argv or '--roots-only' in sys.argv or '--intro-only' in sys.argv else 4})
  if '--salt-only' in sys.argv or '--west-only' in sys.argv or '--gamla-only' in sys.argv or '--observatory-only' in sys.argv or '--cabin-only' in sys.argv or '--meridian-only' in sys.argv or '--continuity-only' in sys.argv or '--uppsala-only' in sys.argv or '--foundry-only' in sys.argv or '--mine-only' in sys.argv or ('--retake-regiment' in sys.argv and name in {'regiment-captain','regiment-freed','regiment-marshal'}):
   parts=[]
   for index,sentence in enumerate(re.split(r'(?<=[.!?])\s+',line)):
    part=render(name+'-sentence-'+str(index),{'text':sentence,'voice_instruction':v['instruction'],'language':'sv','model_backend':'voxcpm2' if r=='saltwarden' else 'dots.tts-mf','output_format':'wav','normalize':False,'seed':v['seed']+211+index,'reference_wav_base64':base64.b64encode(ref.read_bytes()).decode(),'prompt_text':v['text'],'dots_num_steps':16})
    parts.append(part)
   raw=SRC/(name+'-complete.wav')
   with wave.open(str(raw),'wb') as out:
    for part in parts:
     with wave.open(str(part),'rb') as source:
      if part==parts[0]:out.setparams(source.getparams())
      out.writeframes(source.readframes(source.getnframes()))
   (SRC/(name+'-complete.manifest.json')).write_text(json.dumps({'parts':[p.name for p in parts],'sha256':hashlib.sha256(raw.read_bytes()).hexdigest(),'backend':'voxcpm2' if r=='saltwarden' else 'dots.tts-mf','synthetic_reference':True},indent=2))
  subprocess.run(['ffmpeg','-y','-v','error','-i',str(raw),'-af',('highpass=f=60,loudnorm=I=-18:TP=-2:LRA=8' if '--cabin-only' in sys.argv else 'highpass=f=110,lowpass=f=7500,loudnorm=I=-18:TP=-2:LRA=8'),'-ar','48000','-ac','1','-c:a','libvorbis','-q:a','5',str(ROOT/'assets/audio'/f'voice-{name}.ogg')],check=True)
  print('saved',name,flush=True)
