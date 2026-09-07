"""Optional local CPU Whisper transcription for pronunciation QA; never modifies assets."""
from faster_whisper import WhisperModel
from pathlib import Path
import json,sys
root=Path(__file__).resolve().parents[1]
model=WhisperModel('/home/nichlas/EutherVox/models/faster-whisper/models--Systran--faster-whisper-small/snapshots/536b0662742c02347bc0e980a01041f333bce120',device='cpu',compute_type='int8',cpu_threads=4,local_files_only=True)
roots='--roots' in sys.argv
archive='--archive' in sys.argv
rooms='--rooms' in sys.argv
journey='--journey' in sys.argv or rooms or archive or roots
journey_ids={'voice-'+key for key in json.loads((root/('assets/story/rootway-radio.json' if roots else 'assets/story/archive-radio.json' if archive else 'assets/story/rooms-radio.json' if rooms else 'assets/story/journey-radio.json')).read_text())}
report=root/('artifacts/rootway-voice-transcripts.json' if roots else 'artifacts/archive-voice-transcripts.json' if archive else 'artifacts/rooms-voice-transcripts.json' if rooms else 'artifacts/journey-voice-transcripts.json' if journey else 'artifacts/voice-transcripts.json')
result=json.loads(report.read_text()) if len(sys.argv)>1 and report.exists() else {}
for f in sorted((root/'assets/audio').glob('voice-*.ogg')):
    if journey and f.stem not in journey_ids:continue
    if not journey and len(sys.argv)>1 and not f.stem.startswith(sys.argv[1]):continue
    segments,info=model.transcribe(str(f),language='sv',beam_size=5)
    result[f.stem]=' '.join(s.text.strip() for s in segments)
    print(f.stem,result[f.stem],flush=True)
report.write_text(json.dumps(result,ensure_ascii=False,indent=2))
