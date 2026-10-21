"""Package the validated Windows folder and verify its complete CRC / key hashes."""
import argparse
import hashlib
import json
import re
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

parser = argparse.ArgumentParser()
parser.add_argument('--version', required=True)
args = parser.parse_args()
if not re.fullmatch(r'\d+\.\d+\.\d+', args.version):
    parser.error('Use a numeric x.y.z version')
root = Path(__file__).resolve().parent.parent
folder = root / 'Builds/Windows'
result = (root / 'Artifacts/build-result.txt').read_text(encoding='utf-8-sig')
if not result.startswith('Succeeded') or 'Errors: 0' not in result:
    raise RuntimeError('A successful editor build is required')
key_files = {'exeSHA256': 'BigBattle.exe',
             'runtimeAssemblySHA256': 'BigBattle_Data/Managed/Assembly-CSharp.dll',
             'timelineAssemblySHA256': 'BigBattle_Data/Managed/Unity.Timeline.dll'}
for name in key_files.values():
    if not (folder / name).is_file():
        raise FileNotFoundError(name)
output = root / f'Builds/BigBattle-v{args.version}-Windows.zip'
with ZipFile(output, 'w', ZIP_DEFLATED, compresslevel=6) as archive:
    for path in sorted(folder.rglob('*')):
        if path.is_file():
            archive.write(path, path.relative_to(folder).as_posix())
with ZipFile(output) as archive:
    bad = archive.testzip()
    if bad:
        raise RuntimeError('CRC failed: '+bad)
    count = len(archive.infolist())
    hashes = {}
    for key, name in key_files.items():
        hashes[key] = hashlib.sha256((folder / name).read_bytes()).hexdigest()
        if hashlib.sha256(archive.read(name)).hexdigest() != hashes[key]:
            raise RuntimeError('Packaged file differs: '+name)
record = {'version': args.version, 'target': 'Windows x64 / Tuanjie 1.9.3',
          'buildBytes': int(re.search(r'Bytes: (\d+)', result)[1]),
          'buildErrors': 0, 'buildWarnings': int(re.search(r'Warnings: (\d+)', result)[1]),
          'zipBytes': output.stat().st_size, 'zipEntries': count,
          'zipSHA256': hashlib.sha256(output.read_bytes()).hexdigest(),
          **hashes, 'zipCRC': 'PASS'}
tag = 'v'+args.version.replace('.', '')
(root / f'Docs/Validation/build-{tag}.json').write_text(json.dumps(record, indent=2)+'\n', encoding='utf8')
print(json.dumps(record, indent=2))
