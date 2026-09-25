#!/usr/bin/env python3
"""Valida somente esta entrega documental, sem rede nem alterações no SaaS."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path, PurePosixPath
import re
import sys
from urllib.parse import unquote, urlsplit

MANIFEST = "manifesto_pacote.json"
REQUIRED = (
    "00_COMECE_AQUI_CODEX.md", "01_LEIA_PRIMEIRO.md", "04_ARQUITETURA_ALVO.md",
    "12_ROADMAP_EXECUTAVEL.md", "13_TESTES_E_ACEITE.md", "99_VALIDACAO_DO_PACOTE.md",
    "matrizes/requisitos.json", "matrizes/casos_de_teste.json",
    "matrizes/inventario_inicial.json", "evidencias/repositorio_observado.json",
    "contratos/catalogo_modulos.exemplo.json", "contratos/fundacao_neutra.exemplo.json",
)


def safe_relative(value: object) -> bool:
    if not isinstance(value, str) or not value or "\\" in value or ":" in value:
        return False
    path = PurePosixPath(value)
    return not path.is_absolute() and ".." not in path.parts and str(path) == value


def validate(root: Path) -> dict:
    """Retorna diagnóstico estruturado; não altera nenhum arquivo."""
    root = root.resolve()
    errors: list[str] = []
    stats: dict[str, int] = {}
    if not root.is_dir():
        return {"ok": False, "errors": ["ROOT_MISSING"], "stats": {}}
    paths = sorted(root.rglob("*"))
    # Arquivos temporários de bytecode não fazem parte do material entregue.
    actual = {p.relative_to(root).as_posix(): p for p in paths
              if p.is_file() and "__pycache__" not in p.parts and p.suffix != ".pyc"}
    for p in paths:
        if p.is_symlink():
            errors.append(f"SYMLINK_FORBIDDEN: {p.relative_to(root).as_posix()}")
    # Não ler o destino de um link, mesmo quando o manifesto o menciona.
    if any(p.is_symlink() for p in paths):
        return {"ok": False, "errors": errors, "stats": {}}
    parsed: dict[str, object] = {}
    for name, path in actual.items():
        try:
            text = path.read_text(encoding="utf-8")
        except (OSError, UnicodeError) as exc:
            errors.append(f"TEXT_UNREADABLE: {name}: {type(exc).__name__}")
            continue
        if path.suffix == ".json":
            try:
                parsed[name] = json.loads(text)
            except json.JSONDecodeError:
                errors.append(f"JSON_INVALID: {name}")
        if path.suffix == ".md":
            text = re.sub(r"```.*?```", "", text, flags=re.S)
            for target in re.findall(r"\[[^\]\n]*\]\(([^)\n]+)\)", text):
                target = target.strip().strip("<>")
                parts = urlsplit(target)
                if parts.scheme or parts.netloc or not parts.path:
                    continue
                destination = (path.parent / unquote(parts.path)).resolve()
                try:
                    destination.relative_to(root)
                except ValueError:
                    errors.append(f"LINK_OUTSIDE_PACKAGE: {name}: {target}")
                    continue
                if not destination.exists():
                    errors.append(f"LINK_MISSING: {name}: {target}")
    for name in REQUIRED:
        if name not in actual:
            errors.append(f"REQUIRED_MISSING: {name}")
    manifest = parsed.get(MANIFEST)
    if not isinstance(manifest, dict) or not isinstance(manifest.get("files"), list):
        errors.append("MANIFEST_INVALID_OR_MISSING")
    else:
        listed: set[str] = set()
        for row in manifest["files"]:
            if not isinstance(row, dict) or not safe_relative(row.get("path")):
                errors.append("MANIFEST_PATH_UNSAFE")
                continue
            name = row["path"]
            if name in listed or name == MANIFEST:
                errors.append(f"MANIFEST_DUPLICATE_OR_SELF: {name}")
            listed.add(name)
            if name not in actual:
                errors.append(f"FILE_MISSING: {name}")
                continue
            data = actual[name].read_bytes()
            if row.get("bytes") != len(data):
                errors.append(f"SIZE_MISMATCH: {name}")
            if row.get("sha256") != hashlib.sha256(data).hexdigest():
                errors.append(f"HASH_MISMATCH: {name}")
        for name in sorted(set(actual) - listed - {MANIFEST}):
            errors.append(f"UNLISTED_FILE: {name}")
        stats["manifest_files"] = len(listed)
    def rows(name: str, key: str) -> list[dict]:
        obj = parsed.get(name)
        result = obj.get(key) if isinstance(obj, dict) else None
        if not isinstance(result, list) or not all(isinstance(x, dict) for x in result):
            errors.append(f"STRUCTURE_INVALID: {name}:{key}")
            return []
        return result
    reqs = rows("matrizes/requisitos.json", "requirements")
    tests = rows("matrizes/casos_de_teste.json", "cases")
    inventory = rows("matrizes/inventario_inicial.json", "items")
    evidence = rows("evidencias/repositorio_observado.json", "evidence")
    modules = rows("contratos/catalogo_modulos.exemplo.json", "modules")
    def unique_ids(items: list[dict], label: str) -> set[str]:
        ids: set[str] = set()
        for row in items:
            key = row.get("id")
            if not isinstance(key, str) or not key:
                errors.append(f"ID_INVALID: {label}")
            elif key in ids:
                errors.append(f"ID_DUPLICATE: {label}:{key}")
            else:
                ids.add(key)
        return ids
    req_ids = unique_ids(reqs, "requirements")
    unique_ids(tests, "tests")
    unique_ids(inventory, "inventory")
    ev_ids = unique_ids(evidence, "evidence")
    mod_ids = unique_ids(modules, "modules")
    covered: set[str] = set()
    for row in reqs:
        if row.get("status") != "NOT_RUN":
            errors.append(f"UNEXPECTED_EXECUTION_CLAIM: {row.get('id')}")
        if row.get("gate") not in {f"G{i}" for i in range(9)}:
            errors.append(f"GATE_INVALID: {row.get('id')}")
        if row.get("primary_phase") not in {f"R{i:02d}" for i in range(10)}:
            errors.append(f"PHASE_INVALID: {row.get('id')}")
    for row in tests:
        refs = row.get("requirements")
        if not isinstance(refs, list) or not refs or not all(isinstance(x, str) for x in refs):
            errors.append(f"TEST_REQUIREMENTS_INVALID: {row.get('id')}")
            continue
        for ref in refs:
            if ref not in req_ids:
                errors.append(f"UNKNOWN_REQUIREMENT: {row.get('id')}:{ref}")
            covered.add(ref)
        if (row.get("status") != "NOT_RUN" or
                any(row.get(k) is not None for k in ("actual_command", "exit_code", "evidence_path"))):
            errors.append(f"UNEXPECTED_EXECUTION_CLAIM: {row.get('id')}")
    for ref in sorted(req_ids - covered):
        errors.append(f"UNCOVERED_REQUIREMENT: {ref}")
    for row in inventory:
        ev = row.get("evidence_id")
        if ev is not None and ev not in ev_ids:
            errors.append(f"UNKNOWN_EVIDENCE: {row.get('id')}:{ev}")
    cat = parsed.get("contratos/catalogo_modulos.exemplo.json", {})
    if not isinstance(cat, dict) or cat.get("reference_only") is not True:
        errors.append("CATALOG_NOT_REFERENCE_ONLY")
    graph: dict[str, list[str]] = {}
    for row in modules:
        key, deps = row.get("id"), row.get("requires")
        if not isinstance(key, str):
            continue
        if (not isinstance(deps, list) or not all(isinstance(x, str) for x in deps)
                or len(deps) != len(set(deps))):
            errors.append(f"MODULE_DEPENDENCIES_INVALID: {key}")
            continue
        graph[key] = deps
        if any(d not in mod_ids for d in deps):
            errors.append(f"MODULE_DEPENDENCY_UNKNOWN: {key}")
    visited: set[str] = set()
    active: set[str] = set()
    def visit(key: str) -> None:
        if key in active:
            errors.append(f"MODULE_CYCLE: {key}")
            return
        if key in visited:
            return
        active.add(key)
        for dep in graph.get(key, []):
            visit(dep)
        active.remove(key)
        visited.add(key)
    for key in graph:
        visit(key)
    for name, obj in parsed.items():
        if not name.startswith("contratos/") or name.endswith("catalogo_modulos.exemplo.json"):
            continue
        if not isinstance(obj, dict):
            errors.append(f"PRODUCT_STRUCTURE_INVALID: {name}")
            continue
        if (obj.get("schema_version") != 1 or obj.get("reference_only") is not True or
                obj.get("real_data_authorized") is not False or obj.get("authorization_grants") != [] or
                obj.get("runtime_activation") != "not_configured_by_this_file"):
            errors.append(f"PRODUCT_SAFETY_INVALID: {name}")
        chosen = obj.get("module_ids")
        if not isinstance(chosen, list) or not all(isinstance(x, str) for x in chosen):
            errors.append(f"PRODUCT_MODULES_INVALID: {name}")
            continue
        selected = set(chosen)
        if len(chosen) != len(selected) or not selected <= mod_ids:
            errors.append(f"PRODUCT_MODULES_INVALID: {name}")
        for key in selected:
            if not set(graph.get(key, [])) <= selected:
                errors.append(f"PRODUCT_DEPENDENCY_MISSING: {name}:{key}")
        if name.endswith("fundacao_neutra.exemplo.json"):
            if any(row.get("id") in selected and row.get("layer") in {"vertical", "test_fixture"}
                   for row in modules):
                errors.append("FOUNDATION_HAS_DOMAIN")
    stats.update(files=len(actual), json_files=len(parsed), requirements=len(reqs),
                 test_cases=len(tests), inventory_items=len(inventory), evidence_items=len(evidence))
    return {"ok": not errors, "scope": "package_integrity_only_not_application_tests",
            "errors": errors, "stats": stats}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--json", action="store_true", dest="json_output")
    args = parser.parse_args()
    try:
        result = validate(args.root)
    except (OSError, ValueError, TypeError, RecursionError) as exc:
        result = {"ok": False, "errors": [f"VALIDATION_ERROR: {type(exc).__name__}: {exc}"], "stats": {}}
    if args.json_output:
        print(json.dumps(result, ensure_ascii=False, indent=2))
    else:
        print("PASS — integridade do pacote" if result["ok"] else "FAIL — integridade do pacote")
        for error in result["errors"]:
            print(error)
        print(json.dumps(result["stats"], ensure_ascii=False))
    return 0 if result["ok"] else 1


if __name__ == "__main__":
    sys.exit(main())
