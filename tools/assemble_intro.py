"""Assemble the three LTX shots with Ebba, a quiet existing score, and timed captions."""
from pathlib import Path
import subprocess,json
ROOT=Path(__file__).resolve().parents[1];SRC=ROOT/'assets/source/cinematics';AUDIO=ROOT/'assets/audio'
names=['ship','chart','coast'];duration=193/24;total=3*duration
cmd=['ffmpeg','-y','-v','error']
for name in names:cmd+=['-i',str(SRC/f'intro-{name}-ltx-v1.mp4')]
for name in names:cmd+=['-i',str(AUDIO/f'voice-intro-{name}.ogg')]
cmd+=['-stream_loop','-1','-i',str(AUDIO/'names-score.ogg')]
filters=[]
for i in range(3):
    filters += [f'[{i}:v]fps=24,trim=duration={duration},setpts=PTS-STARTPTS,setsar=1[v{i}]',f'[{i}:a]aresample=48000,apad,atrim=duration={duration},asetpts=PTS-STARTPTS[a{i}]']
filters += ['[v0][a0][v1][a1][v2][a2]concat=n=3:v=1:a=1[vcat][ambient]',f'[vcat]fade=t=in:d=0.5,fade=t=out:st={total-.65}:d=0.65[v]', '[ambient]volume=0.25[bed]']
for i,delay in enumerate([800,8900,17100]):filters += [f'[{i+3}:a]aresample=48000,atempo=0.9,adelay={delay}:all=1[voice{i}]']
filters += [f'[6:a]aresample=48000,atrim=duration={total},volume=0.18,afade=t=in:d=2,afade=t=out:st={total-2}:d=2[music]',f'[bed][voice0][voice1][voice2][music]amix=inputs=5:duration=longest:normalize=0,atrim=duration={total},loudnorm=I=-18:TP=-3:LRA=8,afade=t=in:d=0.4,afade=t=out:st={total-.6}:d=0.6[a]']
cmd+=['-filter_complex',';'.join(filters),'-map','[v]','-map','[a]','-c:v','libtheora','-q:v','8','-c:a','libvorbis','-q:a','5','-ar','48000',str(ROOT/'assets/video/atland-intro-v1.ogv')]
(SRC/'intro-assembly-v1.json').write_text(json.dumps({'command':cmd,'voice_speed':.9,'voice_offsets_seconds':[.8,8.9,17.1],'duration_seconds':total,'music':'assets/audio/names-score.ogg','captions':'assets/story/films.json'},indent=2))
subprocess.run(cmd,check=True)
