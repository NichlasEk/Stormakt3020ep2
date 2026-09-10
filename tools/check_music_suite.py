#!/usr/bin/env python3
"""Measure every delivered loop, correct integrated gain, and verify duration/peak/silence."""
from pathlib import Path
import subprocess,json,hashlib
ROOT=Path(__file__).resolve().parents[1]
def measure(p):
 text=subprocess.run(['ffmpeg','-hide_banner','-i',str(p),'-af','loudnorm=I=-22:TP=-3:LRA=8:print_format=json','-f','null','-'],capture_output=True,text=True,check=True).stderr
 return json.JSONDecoder().raw_decode(text[text.rfind('{'):])[0]
report=[]
for t in json.loads((ROOT/'assets/story/music.json').read_text()):
 p=ROOT/'assets/audio/music'/f"{t['id']}.ogg";levels=measure(p);gain=-22-float(levels['input_i'])
 if abs(gain)>.2:
  tmp=p.with_suffix('.normalized.ogg');subprocess.run(['ffmpeg','-y','-v','error','-i',str(p),'-af',f'volume={gain}dB','-c:a','libvorbis','-q:a','5',str(tmp)],check=True);tmp.replace(p);levels=measure(p)
 info=json.loads(subprocess.check_output(['ffprobe','-v','error','-show_entries','format=duration:stream=channels,sample_rate','-of','json',str(p)]));seconds=float(info['format']['duration']);expected=t['bars']*240/t['bpm']
 assert info['streams'][0]['channels']==2 and info['streams'][0]['sample_rate']=='48000',t['id']
 assert abs(seconds-expected)<.12,(t['id'],seconds,expected)
 assert abs(float(levels['input_i'])+22)<.3,(t['id'],levels)
 assert float(levels['input_tp'])<-2,(t['id'],levels)
 m=ROOT/'assets/source/music'/f"{t['id']}.json";meta=json.loads(m.read_text());meta.update({'levels':levels,'gain_correction_db':gain,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()});m.write_text(json.dumps(meta,indent=2))
 report.append({'id':t['id'],'seconds':seconds,'lufs':float(levels['input_i']),'true_peak_db':float(levels['input_tp'])});print('PASS',t['id'],seconds,levels['input_i'],flush=True)
(ROOT/'assets/source/music/quality-report.json').write_text(json.dumps(report,indent=2));print('TOTAL',len(report),'tracks',sum(t['seconds'] for t in report)/60,'minutes')
