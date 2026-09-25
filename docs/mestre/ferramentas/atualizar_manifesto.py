"""Propoe ou grava o manifesto apos validar os contratos. Nao modifica fontes."""
from __future__ import annotations
import argparse
import json
import os
from pathlib import Path
import sys
import tempfile
import validar_pacote as v


def refresh_manifest(root, *, apply=False, additions=(), removals=()):
    root = Path(root).absolute()
    current = v.inventory(root, workspace=True)
    v.require(v.MANIFEST in current, 'Manifesto ausente')
    manifest_path = root / v.MANIFEST
    original_bytes = manifest_path.read_bytes()
    old = v.read_json(root, v.MANIFEST)
    v.unique(old['files'], 'path')
    expected = {row['path']: row for row in old['files']}
    for name in expected:
        v.safe_path(name)
        v.require(v.document_scope(name), f'Entrada fora do escopo documental: {name}')
    actual = {name: row for name, row in current.items() if name != v.MANIFEST}
    add, remove = set(additions), set(removals)
    for name in add | remove:
        v.safe_path(name)
        v.require(v.document_scope(name) and name != v.MANIFEST, f'Mudanca fora do escopo documental: {name}')
    v.require(add == set(actual) - set(expected) and remove == set(expected) - set(actual),
              'Mudanca de conjunto exige --add/--remove exatos: '
              f'add={sorted(set(actual)-set(expected))}; remove={sorted(set(expected)-set(actual))}')
    candidate = {**old, 'files': [actual[name] for name in sorted(actual)]}
    # Full validation precedes any write, including source hashes and semantic negatives.
    validation = v.validate(root, workspace=True, manifest_override=candidate)
    changed = sorted(name for name in set(actual) & set(expected) if actual[name] != expected[name])
    result = {'result': 'PREVIEW', 'changed': changed, 'added': sorted(add), 'removed': sorted(remove),
              'validation': validation['result'], 'application_tests_run': False}
    if not apply:
        return result
    # Detect edits between proposal and publication; never bless them silently.
    v.require(v.inventory(root, workspace=True) == current and manifest_path.read_bytes() == original_bytes,
              'Alteracao concorrente detectada; manifesto preservado')
    encoded = (json.dumps(candidate, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
    if encoded == original_bytes:
        return {**result, 'result': 'UNCHANGED'}
    pending = None
    try:
        with tempfile.NamedTemporaryFile(mode='wb', dir=manifest_path.parent,
                                         prefix='manifesto-', suffix='.pending', delete=False) as handle:
            pending = Path(handle.name)
            handle.write(encoded)
            handle.flush()
            os.fsync(handle.fileno())
        os.replace(pending, manifest_path)
    finally:
        if pending is not None:
            pending.unlink(missing_ok=True)
    return {**result, 'result': 'APPLIED'}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[3])
    parser.add_argument('--apply', action='store_true')
    parser.add_argument('--add', action='append', default=[])
    parser.add_argument('--remove', action='append', default=[])
    args = parser.parse_args()
    try:
        result = refresh_manifest(args.root, apply=args.apply, additions=args.add, removals=args.remove)
    except (v.InvalidPackage, OSError, ValueError, KeyError, TypeError, StopIteration, IndexError) as exc:
        print(json.dumps({'result': 'FAIL', 'error': str(exc)}, ensure_ascii=False, indent=2))
        return 1
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 0


if __name__ == '__main__':
    sys.exit(main())
