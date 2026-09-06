"""Optional local CPU Whisper transcription for pronunciation QA; never modifies assets."""
from faster_whisper import WhisperModel
from pathlib import Path
import json
root=Path(__file__).resolve().parents[1]
model=WhisperModel('/home/nichlas/EutherVox/models/faster-whisper/models--Systran--faster-whisper-small/snapshots/536b0662742c02347bc0e980a01041f333bce120',device='cpu',compute_type='int8',cpu_threads=4,local_files_only=True)
result={}
for f in sorted((root/'assets/audio').glob('voice-*.ogg')):
    segments,info=model.transcribe(str(f),language='sv',beam_size=5)
    result[f.stem]=' '.join(s.text.strip() for s in segments)
    print(f.stem,result[f.stem],flush=True)
(root/'artifacts/voice-transcripts.json').write_text(json.dumps(result,ensure_ascii=False,indent=2))
