"""Testes negativos do contrato documental, em copias temporarias. Nao testa SaaS."""
from pathlib import Path
import json
import os
import shutil
import tempfile
import unittest
from unittest.mock import patch
import validar_pacote as v
import atualizar_manifesto as maintenance

SOURCE = Path(__file__).resolve().parents[3]

class ValidationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix='wissian-doc-test-')
        self.root = Path(self.temp.name)/'package'
        # Na instalacao, nao copiar o backup/Git/ZIP para cada fixture.
        self.root.mkdir()
        for rel in v.inventory(SOURCE, workspace=True):
            out = self.root/rel; out.parent.mkdir(parents=True,exist_ok=True)
            shutil.copy2(SOURCE/rel, out)

    def tearDown(self):
        self.temp.cleanup()

    def refresh(self):
        p=self.root/v.MANIFEST
        obj=json.loads(p.read_text('utf-8'))
        obj['files']=[x for k,x in sorted(v.inventory(self.root).items()) if k!=v.MANIFEST]
        p.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

    def change_json(self,rel,change):
        p=self.root/rel;obj=json.loads(p.read_text('utf-8'));change(obj)
        p.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

    def rejected(self,pattern):
        with self.assertRaisesRegex(v.InvalidPackage,pattern):v.validate(self.root)

    def test_01_valid(self):
        self.assertEqual(v.validate(self.root)['result'],'PASS')

    def test_53_workspace_master_rule_is_governed(self):
        rule = 'WISSIAN_CODEX_REGRA_MESTRA.md'
        self.assertIn(rule, v.inventory(self.root, workspace=True))
        (self.root/rule).write_text('regra adulterada', encoding='utf-8')
        with self.assertRaisesRegex(v.InvalidPackage, 'Hash/tamanho'):
            v.validate(self.root, workspace=True)

    def test_02_tamper(self):
        (self.root/'README.md').write_text('alterado',encoding='utf-8')
        self.rejected('Hash/tamanho')

    def test_03_missing_source(self):
        (self.root/'docs/fontes/reestruturacao-v1.1/Prompt.txt').unlink()
        self.rejected('Conjunto diverge')

    def test_04_extra(self):
        (self.root/'unexpected.txt').write_text('x')
        self.rejected('extras=')

    def test_05_traversal(self):
        self.change_json(v.MANIFEST,lambda o:o['files'][0].update(path='../outside.txt'))
        self.rejected('Caminho')

    def test_06_windows_ads(self):
        self.change_json(v.MANIFEST,lambda o:o['files'][0].update(path='README.md:stream'))
        self.rejected('Caminho')

    def test_07_case_collision(self):
        self.change_json(v.MANIFEST,lambda o:o['files'].append({**o['files'][0],'path':o['files'][0]['path'].upper()}))
        self.rejected('Colisao')

    def test_08_source_rehashed_not_provenance(self):
        (self.root/'docs/fontes/reestruturacao-v1.1/Prompt.txt').write_text('alterado',encoding='utf-8')
        self.refresh();self.rejected('Fonte original alterada')

    def test_09_broken_link(self):
        with (self.root/'README.md').open('a',encoding='utf-8') as f:f.write('\n[Ausente](missing.md)\n')
        self.refresh();self.rejected('Link local ausente')

    def test_10_false_application_pass(self):
        self.change_json('docs/mestre/matrizes/testes-planejados.json',lambda o:o['cases'][0].update(status='PASS'))
        self.refresh();self.rejected('Alegacao indevida')

    def test_11_unknown_requirement(self):
        self.change_json('docs/mestre/matrizes/testes-planejados.json',lambda o:o['cases'][0]['requirements'].append('INVENTED'))
        self.refresh();self.rejected('requisito valido')

    def test_12_lost_origin(self):
        self.change_json('docs/mestre/matrizes/cobertura-origem.json',lambda o:o['legacy_requirements'].pop())
        self.refresh();self.rejected('Requisito herdado perdido')

    def test_13_core_dependency(self):
        self.change_json('docs/mestre/contratos/catalogo_modulos.proposta.json',lambda o:o['modules'][0]['required_dependencies'].append('socioeducativo'))
        self.refresh();self.rejected('Core depende')

    def test_14_cycle(self):
        self.change_json('docs/mestre/contratos/catalogo_modulos.proposta.json',lambda o:o['modules'][1]['required_dependencies'].append('monthly-close'))
        self.refresh();self.rejected('Ciclo de modulos')

    def test_15_activation(self):
        self.change_json('docs/mestre/contratos/composicoes.proposta.json',lambda o:o.update(runtime_activation=True))
        self.refresh();self.rejected('nao pode autorizar')

    def test_16_installed_mode_scoped(self):
        p=self.root/'backups/private.txt';p.parent.mkdir();p.write_text('fixture')
        self.assertEqual(v.validate(self.root,installed=True)['result'],'PASS')
        self.rejected('extras=')
        (self.root/'unexpected.txt').write_text('x')
        with self.assertRaises(v.InvalidPackage):v.validate(self.root,installed=True)

    def test_17_symlink(self):
        target=Path(self.temp.name)/'outside.txt';target.write_text('fixture')
        link=self.root/'external-link'
        try:link.symlink_to(target)
        except OSError as exc:
            if getattr(exc,'winerror',None)==1314 or getattr(exc,'errno',None) in (1,13):
                self.skipTest('Host nao permite criar fixture symlink; sem elevacao')
            raise
        self.rejected('Link/reparse')

    def test_18_duplicate_json(self):
        p=self.root/v.MANIFEST
        p.write_text('{"schema_version":1,"schema_version":2}',encoding='utf-8')
        self.rejected('Chave JSON repetida')

    def test_19_changed_WB_extraction(self):
        self.change_json('docs/mestre/matrizes/cobertura-origem.json',lambda o:o['base_operation'][0]['cells'].append('inventado'))
        self.refresh();self.rejected('Extracao WB/NF divergente')

    def test_20_missing_composition_dependency(self):
        self.change_json('docs/mestre/contratos/composicoes.proposta.json',lambda o:o['compositions'][1]['modules'].remove('documents'))
        self.refresh();self.rejected('Composicao sem fechamento')

    def change_continuity(self, change, pattern):
        self.change_json('docs/mestre/contratos/continuity.proposta.json',change)
        self.refresh();self.rejected(pattern)

    def test_21_continuity_key_without_tenant(self):
        self.change_continuity(lambda o:o['keys']['work_item_unique'].remove('organization_id'),'chave/escopo')

    def test_22_automatic_human_approval(self):
        self.change_continuity(lambda o:o['human_authority'].update(automated_approval=True),'autoridade humana')

    def test_23_arbitrary_handler(self):
        self.change_continuity(lambda o:o['action_definitions'][0].update(handler_ref='powershell.exe'),'handler/acao')

    def test_24_lost_continuity_requirement(self):
        self.change_json('docs/mestre/matrizes/requisitos-mestre.json',lambda o:o['requirements'].pop())
        self.refresh();self.rejected('Matriz mestre incompleta')

    def test_25_broken_csv_mirror(self):
        p=self.root/'docs/mestre/matrizes/requisitos-mestre.csv'
        with p.open('a',encoding='utf-8') as f:f.write('unexpected,row\n')
        self.refresh();self.rejected('CSV incompleto')

    def test_26_dependency_cycle_example(self):
        def mutate(o):
            d=dict(o['examples']['dependencies'][0]);d['item_id'],d['prerequisite_id']=d['prerequisite_id'],d['item_id']
            o['examples']['dependencies'].append(d)
        self.change_continuity(mutate,'ciclo no exemplo')

    def test_27_cross_tenant_dependency(self):
        self.change_continuity(lambda o:o['examples']['work_items'][1].update(organization_id='synthetic-org-b'),'dependencia entre escopos')

    def test_28_continuity_in_first_cycle(self):
        self.change_json('docs/mestre/contratos/composicoes.proposta.json',lambda o:o['compositions'][1]['modules'].append('continuity'))
        self.refresh();self.rejected('composicao prematura')

    def test_29_unscoped_receipt_replay(self):
        self.change_continuity(lambda o:o['action_registry'].update(authorize_before_enqueue_execute_and_receipt=False),'execucao arbitraria/autoridade')

    def test_30_unbounded_parameters(self):
        self.change_continuity(lambda o:o['action_definitions'][0]['parameter_schema'].update(additionalProperties=True),'parametros amplos')

    def test_31_disabled_module_bypasses_approval(self):
        self.change_continuity(lambda o:o['human_authority'].update(disabled_module_bypasses_gate=True),'autoridade humana')

    def test_32_timestamp_ordering(self):
        self.change_continuity(lambda o:o['events'].update(order_by='occurred_at'),'ordem/replay')

    def test_33_greenfield_rewrite(self):
        self.change_continuity(lambda o:o['reuse_policy'].update(equivalent_search_required=False),'politica de reuso')

    def test_34_standalone_product_assumed(self):
        self.change_continuity(lambda o:o['module'].update(standalone_product=True),'natureza interna')

    def make_distribution_copy(self):
        entries=list(v.inventory(self.root))
        copy=self.root/v.DISTRIBUTION_COPY
        for rel in entries:
            p=copy/rel;p.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(self.root/rel,p)
        return copy

    def test_35_installed_identical_distribution(self):
        self.make_distribution_copy()
        self.assertTrue(v.validate(self.root,installed=True)['distribution_copy_validated'])
        self.rejected('Entrada excluida|extras=')

    def test_36_installed_copy_tamper(self):
        copy=self.make_distribution_copy()
        (copy/'README.md').write_text('changed',encoding='utf-8')
        with self.assertRaisesRegex(v.InvalidPackage,'Hash/tamanho'):v.validate(self.root,installed=True)

    def test_37_installed_copy_rehashed_but_different(self):
        copy=self.make_distribution_copy()
        with (copy/'README.md').open('a',encoding='utf-8') as f:f.write('\nDivergence\n')
        manifest=copy/v.MANIFEST;obj=json.loads(manifest.read_text('utf-8'))
        obj['files']=[x for k,x in sorted(v.inventory(copy).items()) if k!=v.MANIFEST]
        manifest.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        with self.assertRaisesRegex(v.InvalidPackage,'Copia de distribuicao diverge'):v.validate(self.root,installed=True)

    def test_38_installed_copy_extra_file(self):
        copy=self.make_distribution_copy();(copy/'unexpected.txt').write_text('x')
        with self.assertRaisesRegex(v.InvalidPackage,'extras='):v.validate(self.root,installed=True)

    def test_39_future_use_lost(self):
        self.change_json('docs/mestre/matrizes/reaproveitamento-nexus-cc.json',lambda o:o['future_envelope_uses'].remove('app móvel'))
        self.refresh();self.rejected('envelope futuro reduzido')

    def test_40_retired_shell_reintroduced(self):
        self.change_json('docs/mestre/matrizes/reaproveitamento-nexus-cc.json',lambda o:o['assets'][23].update(decision='preservar'))
        self.refresh();self.rejected('aposentadoria divergente')

    def test_41_specialization_in_core(self):
        self.change_json('docs/mestre/matrizes/reaproveitamento-nexus-cc.json',lambda o:o['use_cases'][1].update(core_business_rule=True))
        self.refresh();self.rejected('uso sem fronteira')

    def test_42_workspace_excludes_product_and_dependencies(self):
        for name in ['src/app.py','node_modules/fixture/file.bin','.env','package.json']:
            p=self.root/name;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(b'fixture only')
        self.assertEqual(v.validate(self.root,workspace=True)['result'],'PASS')
        self.rejected('extras=')

    def test_43_workspace_rejects_unregistered_document(self):
        (self.root/'docs/unregistered.md').write_text('fixture',encoding='utf-8')
        with self.assertRaisesRegex(v.InvalidPackage,'extras='):v.validate(self.root,workspace=True)

    def test_44_workspace_rejects_tampered_contract(self):
        (self.root/'docs/mestre/17_BASE_ADMINISTRATIVA_E_TRANSICAO.md').write_text('changed',encoding='utf-8')
        with self.assertRaisesRegex(v.InvalidPackage,'Hash/tamanho'):v.validate(self.root,workspace=True)

    def test_45_workspace_manifest_cannot_capture_product(self):
        p=self.root/'src/app.py';p.parent.mkdir();p.write_text('fixture')
        self.refresh()
        with self.assertRaisesRegex(v.InvalidPackage,'fora do escopo'):v.validate(self.root,workspace=True)

    def test_46_updater_preview_and_apply(self):
        before=(self.root/v.MANIFEST).read_bytes()
        with (self.root/'README.md').open('a',encoding='utf-8') as f:f.write('\nClarificacao de fixture.\n')
        result=maintenance.refresh_manifest(self.root)
        self.assertEqual(result['changed'],['README.md'])
        self.assertEqual((self.root/v.MANIFEST).read_bytes(),before)
        self.assertEqual(maintenance.refresh_manifest(self.root,apply=True)['result'],'APPLIED')
        self.assertEqual(v.validate(self.root,workspace=True)['result'],'PASS')

    def test_47_updater_rejects_source_tamper_without_write(self):
        before=(self.root/v.MANIFEST).read_bytes()
        (self.root/'docs/fontes/reestruturacao-v1.1/Prompt.txt').write_text('changed',encoding='utf-8')
        with self.assertRaisesRegex(v.InvalidPackage,'Fonte original alterada'):
            maintenance.refresh_manifest(self.root,apply=True)
        self.assertEqual((self.root/v.MANIFEST).read_bytes(),before)

    def test_48_updater_requires_explicit_add_remove(self):
        p=self.root/'docs/additional.md';p.write_text('fixture',encoding='utf-8')
        with self.assertRaisesRegex(v.InvalidPackage,'--add/--remove'):
            maintenance.refresh_manifest(self.root,apply=True)
        maintenance.refresh_manifest(self.root,apply=True,additions=['docs/additional.md'])
        p.unlink()
        with self.assertRaisesRegex(v.InvalidPackage,'--add/--remove'):
            maintenance.refresh_manifest(self.root,apply=True)
        maintenance.refresh_manifest(self.root,apply=True,removals=['docs/additional.md'])
        self.assertEqual(v.validate(self.root,workspace=True)['result'],'PASS')

    def test_49_updater_rejects_broken_link_without_write(self):
        before=(self.root/v.MANIFEST).read_bytes()
        with (self.root/'README.md').open('a',encoding='utf-8') as f:f.write('\n[Missing](absent.md)\n')
        with self.assertRaisesRegex(v.InvalidPackage,'Link local ausente'):
            maintenance.refresh_manifest(self.root,apply=True)
        self.assertEqual((self.root/v.MANIFEST).read_bytes(),before)

    def test_50_updater_cannot_remove_entry_rules(self):
        (self.root/'AGENTS.md').unlink()
        with self.assertRaisesRegex(v.InvalidPackage,'obrigatoria ausente'):
            maintenance.refresh_manifest(self.root,apply=True,removals=['AGENTS.md'])

    def test_51_updater_concurrent_document_edit_preserves_manifest(self):
        before=(self.root/v.MANIFEST).read_bytes()
        validate=v.validate
        def edit_after_validation(*args,**kwargs):
            result=validate(*args,**kwargs)
            with (self.root/'README.md').open('a',encoding='utf-8') as f:f.write('\nConcurrent fixture edit.\n')
            return result
        with patch.object(maintenance.v,'validate',side_effect=edit_after_validation):
            with self.assertRaisesRegex(v.InvalidPackage,'concorrente'):
                maintenance.refresh_manifest(self.root,apply=True)
        self.assertEqual((self.root/v.MANIFEST).read_bytes(),before)

    def test_52_updater_publication_failure_is_recoverable(self):
        before=(self.root/v.MANIFEST).read_bytes()
        with (self.root/'README.md').open('a',encoding='utf-8') as f:f.write('\nIntentional fixture edit.\n')
        with patch.object(maintenance.os,'replace',side_effect=OSError('controlled publication failure')):
            with self.assertRaisesRegex(OSError,'controlled publication failure'):
                maintenance.refresh_manifest(self.root,apply=True)
        self.assertEqual((self.root/v.MANIFEST).read_bytes(),before)
        self.assertFalse(list((self.root/v.MANIFEST).parent.glob('*.pending')))

if __name__=='__main__':
    unittest.main(verbosity=2)
