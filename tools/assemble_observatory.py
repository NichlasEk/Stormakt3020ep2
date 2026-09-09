"""Prepare the local LTX dome shot for native Theora playback with restrained ambient audio."""
from pathlib import Path
import json,subprocess
ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/'assets/source/cinematics'
duration=193/24
cmd=['ffmpeg','-y','-v','error','-i',str(SRC/'observatory-dome-ltx-v1.mp4'),
     '-vf',f'fade=t=in:d=0.5,fade=t=out:st={duration-.6}:d=0.6',
     '-af',f'volume=0.3,afade=t=in:d=0.4,afade=t=out:st={duration-1}:d=1',
     '-c:v','libtheora','-q:v','8','-c:a','libvorbis','-q:a','5','-ar','48000',
     str(ROOT/'assets/video/observatory-dome-v1.ogv')]
subprocess.run(cmd,check=True)
(SRC/'observatory-dome-assembly-v1.json').write_text(json.dumps({'command':cmd,'duration_seconds':duration,'audio':'LTX generated ambience at 0.3 gain; no recorded dialogue'},indent=2)+'\n')
