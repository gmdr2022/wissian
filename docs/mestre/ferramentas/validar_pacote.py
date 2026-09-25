"""Validador documental read-only. Nao importa/executa codigo de produto ou das fontes."""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import stat
import sys
from urllib.parse import unquote, urlsplit
from validar_continuity import validate_contract

MANIFEST = 'docs/mestre/manifesto.json'
DISTRIBUTION_COPY = 'WISSIAN_PACOTE_MESTRE_v2.0.0_2026-09-24'
# This exact copy is validated separately and must match, never silently trusted.
IGNORED_INSTALLED = ('.git', 'backups', 'docs/mestre/entrega', DISTRIBUTION_COPY)
WORKSPACE_ROOT_FILES = frozenset({
    'README.md', 'AGENTS.md', '.editorconfig', '.gitattributes', '.gitignore',
    '.prettierignore', '.ignore', 'WISSIAN_CODEX_REGRA_MESTRA.md',
})

class InvalidPackage(Exception):
    pass

def require(condition, message):
    if not condition:
        raise InvalidPackage(message)

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def safe_path(name):
    require(isinstance(name, str) and name and '\\' not in name and ':' not in name,
            f'Caminho invalido: {name!r}')
    p = PurePosixPath(name)
    require(not p.is_absolute() and str(p) == name and all(x not in ('', '.', '..') for x in p.parts),
            f'Caminho nao relativo/canonico: {name!r}')
    for part in p.parts:
        require(not any(ord(c) < 32 or c in '<>"|?*' for c in part) and not part.endswith((' ', '.')),
                f'Nome inseguro no Windows: {name!r}')
        require(not re.fullmatch(r'(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])', part.split('.')[0], re.I),
                f'Nome reservado: {name!r}')
    return p

def reject_link(path):
    s = path.lstat()
    require(not stat.S_ISLNK(s.st_mode) and not (getattr(s, 'st_file_attributes', 0) & 0x400),
            f'Link/reparse point nao permitido: {path.name}')
    return s

def excluded(name, installed):
    return installed and any(name == x or name.startswith(x + '/') for x in IGNORED_INSTALLED)

def document_scope(name):
    return name in WORKSPACE_ROOT_FILES or name == 'docs' or name.startswith('docs/')

def inventory(root, installed=False, workspace=False):
    require(not (installed and workspace), 'Modos installed/workspace sao exclusivos')
    require(root.is_dir(), 'Raiz inexistente ou nao e diretorio')
    for parent in [root, *root.parents]:
        reject_link(parent)
    found, folded = {}, {}
    def walk(folder):
        for item in sorted(folder.iterdir()):
            rel = item.relative_to(root).as_posix()
            if workspace and not document_scope(rel):
                continue  # Produto/dependencias nao sao lidos pela validacao documental.
            if excluded(rel, installed):
                continue  # Nao ler Git, backup ou recibo no modo de instalacao.
            safe_path(rel)
            require(rel.casefold() not in folded, f'Colisao de nomes: {rel}')
            folded[rel.casefold()] = rel
            s = reject_link(item)
            if stat.S_ISDIR(s.st_mode):
                walk(item)
            else:
                require(stat.S_ISREG(s.st_mode), f'Arquivo especial: {rel}')
                found[rel] = {'path': rel, 'bytes': s.st_size, 'sha256': digest(item)}
    walk(root)
    return found

def no_duplicates(pairs):
    obj = {}
    for key, value in pairs:
        require(key not in obj, f'Chave JSON repetida: {key}')
        obj[key] = value
    return obj

def read_json(root, rel):
    safe_path(rel)
    return json.loads((root / rel).read_text('utf-8-sig'), object_pairs_hook=no_duplicates)

def unique(rows, field='id'):
    ids = [x[field] for x in rows]
    require(all(isinstance(x, str) and x for x in ids) and len(ids) == len(set(ids)),
            f'Identificadores ausentes/repetidos: {field}')
    return set(ids)

def check_links(root, files):
    total = 0
    # Fontes historicas mantem links originais e sao validadas pelo hash; nao reescreve-los.
    for rel in files:
        if not rel.endswith('.md') or rel.startswith('docs/fontes/'):
            continue
        text = (root / rel).read_text('utf-8-sig')
        text = re.sub(r'^```.*?^```\s*$', '', text, flags=re.M | re.S)
        for target in re.findall(r'\[[^\]\n]+\]\(([^)\n]+)\)', text):
            target = target.strip().strip('<>')
            parsed = urlsplit(target)
            if parsed.scheme or target.startswith('#'):
                continue
            require(not parsed.netloc, f'Link externo ambiguo: {rel}: {target}')
            link = unquote(parsed.path)
            require(link and not link.startswith(('/', '\\')) and '\\' not in link,
                    f'Link local invalido: {rel}: {target}')
            resolved = (root / rel).parent.joinpath(link).resolve()
            require(resolved.is_relative_to(root.resolve()), f'Link fora do pacote: {rel}: {target}')
            require(resolved.exists(), f'Link local ausente: {rel}: {target}')
            total += 1
    return total

def validate(root, installed=False, workspace=False, manifest_override=None):
    root = Path(root).absolute()
    files = inventory(root, installed, workspace)
    require(MANIFEST in files, 'Manifesto ausente')
    manifest = read_json(root, MANIFEST) if manifest_override is None else manifest_override
    require(manifest['schema_version'] == 1 and manifest['package_version'] == '2.1.0', 'Versao de manifesto invalida')
    require(manifest['scope'] == 'documentation_only', 'Escopo do manifesto invalido')
    records = manifest['files']
    unique(records, 'path')
    folded = set()
    for row in records:
        name = row['path']; safe_path(name)
        if workspace:
            require(document_scope(name), f'Entrada fora do escopo documental: {name}')
        require(name.casefold() not in folded, f'Colisao no manifesto: {name}')
        folded.add(name.casefold())
        require(name != MANIFEST and not excluded(name, True), f'Entrada excluida/autorreferente no manifesto: {name}')
        require(type(row['bytes']) is int and row['bytes'] >= 0 and re.fullmatch('[0-9a-f]{64}', row['sha256']),
                f'Metadados invalidos: {name}')
    expected = {x['path']: x for x in records}
    if workspace:
        require(WORKSPACE_ROOT_FILES <= set(expected), 'Entrada/configuracao documental obrigatoria ausente do manifesto')
    actual = {k: v for k, v in files.items() if k != MANIFEST}
    require(set(actual) == set(expected), f'Conjunto diverge: ausentes={sorted(set(expected)-set(actual))}; extras={sorted(set(actual)-set(expected))}')
    for name, row in expected.items():
        require(actual[name] == row, f'Hash/tamanho diverge: {name}')
    # Todos os JSON e textos da camada atual devem ser decodificaveis.
    for rel in files:
        if rel.endswith('.json'):
            read_json(root, rel)
        if rel.endswith(('.md', '.py', '.csv')) and not rel.startswith('docs/fontes/'):
            (root / rel).read_text('utf-8-sig')
    links = check_links(root, files)
    sources = read_json(root, 'docs/mestre/matrizes/fontes.json')
    unique(sources['sources'])
    require(len(sources['sources']) == 75, 'Quantidade de fontes originais diverge')
    require(sum(x['family'] == 'RAR' for x in sources['sources']) == 49, 'Cobertura RAR incompleta')
    require(sum(x['family'] == 'LOCAL' for x in sources['sources']) == 26, 'Cobertura local incompleta')
    src_paths = unique(sources['sources'], 'package_path')
    actual_source_paths = {x for x in files if x.startswith('docs/fontes/') and x != 'docs/fontes/README.md'}
    require(src_paths == actual_source_paths, 'Acervo diverge da matriz de fontes')
    for src in sources['sources']:
        require(files[src['package_path']]['sha256'] == src['sha256'] and files[src['package_path']]['bytes'] == src['bytes'],
                f'Fonte original alterada: {src["id"]}')
    for alias in sources['attachment_aliases'].values():
        src = next(x for x in sources['sources'] if x['family'] == 'LOCAL' and x['source_relative'] == alias['name'])
        require(alias['matches_local'] and alias['sha256'] == src['sha256'] and alias['bytes'] == src['bytes'], 'Alias de anexo divergente')
    oldbase = 'docs/fontes/reestruturacao-v1.1/matrizes/'
    oldreq = read_json(root, oldbase + 'requisitos.json')['requirements']
    oldtests = read_json(root, oldbase + 'casos_de_teste.json')['cases']
    oldinv = read_json(root, oldbase + 'inventario_inicial.json')['items']
    req = read_json(root, 'docs/mestre/matrizes/requisitos-mestre.json')['requirements']
    cases = read_json(root, 'docs/mestre/matrizes/testes-planejados.json')['cases']
    rids, tids = unique(req), unique(cases)
    require(rids == {f'WM-{i:02}' for i in range(1,41)} and
            tids == {f'TM-{i:02}' for i in range(1,21)} | {f'TC-{i:02}' for i in range(1,23)}, 'Matriz mestre incompleta')
    for r in req:
        require(r['status'] == 'NOT_RUN' and r['tests'] and set(r['tests']) <= tids, f'Requisito sem cobertura/estado incorreto: {r["id"]}')
        require(set(r['tests']) == {c['id'] for c in cases if r['id'] in c['requirements']}, f'Rastreio assimetrico: {r["id"]}')
    for c in cases:
        require(c['requirements'] and set(c['requirements']) <= rids, f'Teste sem requisito valido: {c["id"]}')
        require(c['status'] == 'NOT_RUN' and all(c[k] is None for k in ('actual_command', 'exit_code', 'evidence_path')),
                f'Alegacao indevida de teste executado: {c["id"]}')
        require(all(c[k] for k in ('fixture', 'action', 'expected', 'negative')), f'Roteiro incompleto: {c["id"]}')
    coverage = read_json(root, 'docs/mestre/matrizes/cobertura-origem.json')
    require(unique(coverage['legacy_requirements']) == unique(oldreq), 'Requisito herdado perdido')
    require(unique(coverage['legacy_tests']) == unique(oldtests), 'Teste herdado perdido')
    require(unique(coverage['base_operation']) == {f'WB-{i:02}' for i in range(1,17)} | {f'NF-{i:02}' for i in range(1,11)}, 'Cobertura WB/NF incompleta')
    for group in coverage.values():
        if isinstance(group, list):
            for row in group:
                require(row['status'] == 'NOT_RUN' and set(row['master_requirements']) <= rids, f'Crosswalk invalido: {row["id"]}')
    for row in coverage['base_operation']:
        source = root / row['source']
        cells = [x.strip() for x in source.read_text('utf-8-sig').splitlines()[row['source_line']-1].strip('|').split('|')]
        require(cells == row['cells'] and digest(source) == row['source_sha256'], f'Extracao WB/NF divergente: {row["id"]}')
    migration = read_json(root, 'docs/mestre/matrizes/migracao.json')['items']
    unique(migration)
    require({x['id'] for x in migration} == {f'ABS-{i:03}' for i in range(1,49)} and {x['source_id'] for x in migration if x['origin_kind']=='legacy_inventory_statement'} == unique(oldinv), 'Inventario de migracao incompleto')
    require(all(x['decision']=='PENDING_AUDIT' and x['runtime_status']=='NOT_RUN' for x in migration), 'Migracao indevidamente promovida')
    modsfile = read_json(root, 'docs/mestre/contratos/catalogo_modulos.proposta.json')
    compfile = read_json(root, 'docs/mestre/contratos/composicoes.proposta.json')
    eventfile = read_json(root, 'docs/mestre/contratos/evento_auditoria.exemplo.json')
    for obj in (modsfile, compfile, eventfile):
        require(obj['reference_only'] is True and obj['grants_authority'] is False and obj['runtime_activation'] is False,
                'Exemplo nao pode autorizar/ativar produto')
    mods = modsfile['modules']; mids = unique(mods); byid = {m['id']:m for m in mods}
    require('core' in mids and byid['core']['kind']=='core' and not byid['core']['required_dependencies'] and not byid['core']['optional_integrations'], 'Core depende de dominio')
    for m in mods:
        require(set(m['required_dependencies'] + m['optional_integrations']) <= mids, f'Dependencia desconhecida: {m["id"]}')
        require(m['runtime_status']=='NOT_IMPLEMENTED', f'Implementacao ficticia: {m["id"]}')
    visited, visiting = set(), set()
    def visit(mid):
        require(mid not in visiting, f'Ciclo de modulos: {mid}')
        if mid in visited: return
        visiting.add(mid)
        for dep in byid[mid]['required_dependencies']: visit(dep)
        visiting.remove(mid); visited.add(mid)
    for mid in mids: visit(mid)
    unique(compfile['compositions'])
    for c in compfile['compositions']:
        ids = set(c['modules'])
        require(ids <= mids and len(ids)==len(c['modules']) and 'core' in ids, f'Composicao invalida: {c["id"]}')
        require(all(set(byid[m]['required_dependencies']) <= ids for m in ids), f'Composicao sem fechamento: {c["id"]}')
        if c['id']=='neutral': require(ids=={'core'}, 'Composicao neutra contaminada')
    front = read_json(root, 'docs/mestre/matrizes/requisitos-frentes-origem.json')['rows']
    require(len(front)==114 and all(x['runtime_test_status_here']=='NOT_RUN' for x in front), 'Extracao de frentes divergente')
    owm = read_json(root, 'docs/mestre/matrizes/roadmap-owm-origem.json')['program']
    require(len(owm['packages'])==35, 'Roadmap OWM incompleto')
    legacy = read_json(root, 'docs/mestre/matrizes/fontes-legado.json')['sources']
    require(len(legacy)==55, 'Mapa do legado incompleto')
    continuity = validate_contract(root, read_json, require, unique)
    copy_validated = False
    distribution_copy = root / DISTRIBUTION_COPY
    if installed and (distribution_copy.exists() or distribution_copy.is_symlink()):
        reject_link(distribution_copy)
        validate(distribution_copy, installed=False)
        require(digest(distribution_copy / MANIFEST) == digest(root / MANIFEST), 'Copia de distribuicao diverge da instalacao')
        copy_validated = True
    return {'result':'PASS','scope':'documentation_only_not_application_validation','files':len(files),'bytes':sum(x['bytes'] for x in files.values()),'local_links_checked':links,'original_sources':75,'legacy_requirements':len(oldreq),'legacy_planned_tests':len(oldtests),'legacy_inventory':len(oldinv),'master_requirements':len(req),'master_planned_tests':len(cases),'migration_candidates':len(migration),'base_WB_NF':26,'front_requirements':len(front),'owm_packages':len(owm['packages']),'application_tests_run':False,'installed_mode':installed,'workspace_mode':workspace,'distribution_copy_validated':copy_validated,'continuity':continuity}

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--root', type=Path, default=Path(__file__).resolve().parents[3])
    modes = parser.add_mutually_exclusive_group()
    modes.add_argument('--installed', action='store_true', help='Compatibilidade com instalacao documental antiga')
    modes.add_argument('--workspace', action='store_true', help='Conferir documentos sem inventariar codigo/dependencias')
    parser.add_argument('--json', action='store_true')
    args = parser.parse_args()
    try:
        result = validate(args.root, args.installed, args.workspace)
    except (InvalidPackage, OSError, ValueError, KeyError, TypeError, StopIteration, IndexError) as exc:
        result = {'result':'FAIL','scope':'documentation_only','error':str(exc)}
    print(json.dumps(result,ensure_ascii=False,indent=2) if args.json else result)
    return 0 if result['result']=='PASS' else 1

if __name__ == '__main__':
    sys.exit(main())
