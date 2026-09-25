"""Consistencia da proposta documental. Nao implementa nem testa o motor SaaS."""
import csv
import json

def validate_contract(root, read_json, require, unique):
    prefix='docs/mestre/'
    c=read_json(root,prefix+'contratos/continuity.proposta.json')
    require(c['schema_version']==1 and c['contract_version']=='0.1-proposal', 'Continuity: versao invalida')
    require(c['reference_only'] is True and c['runtime_activation'] is False and c['grants_authority'] is False and c['runtime_status']=='NOT_IMPLEMENTED','Continuity: ativacao/alegacao indevida')
    mod=c['module']
    require(mod['id']=='continuity' and mod['kind']=='shared' and mod['optional'] is True and mod['phase']=='M5-CO' and mod['required_dependencies']==['core'] and mod['core_imports_continuity'] is False,'Continuity: fronteira Core/fase invalida')
    require(mod['first_consumer']=='monthly-close' and mod['first_consumer_gate']=='M4','Continuity: consumidor/gate invalido')
    require(mod['role']=='internal_cross_module_capability' and mod['standalone_product'] is False and mod['commercial_packaging']=='not_decided_no_separate_addon_assumed','Continuity: natureza interna divergente')
    reuse=c['reuse_policy']
    require(reuse['strategy']=='fragment_existing_then_adapt' and reuse['equivalent_search_required'] is True and reuse['new_code_requires_proven_gap'] is True and reuse['specialization_in_core'] is False and reuse['source_code_audited_here'] is False,'Continuity: politica de reuso invalida')
    scope=c['scope']
    require(scope['canonical_tenant']=='organization_id' and scope['envelope_alias']=='tenant_id' and scope['server_validated'] is True and scope['hierarchy_must_match'] is True and scope['scope_key_nullable'] is False and scope['scope_key_server_derived'] is True,'Continuity: escopo invalido')
    keys=c['keys']
    for key,expected in {
      'work_item_unique':['organization_id','source_module','scope_key','stable_key'],
      'inbox_unique':['organization_id','producer','event_id'],
      'action_unique':['organization_id','action_name','idempotency_key'],
      'action_binding':['actor_ref','target_id','expected_version','action_version','normalized_parameters_hash'],
      'notification_unique':['organization_id','recipient_ref','work_item_id','episode','transition','channel']}.items():
        require(keys[key]==expected,'Continuity: chave/escopo idempotente invalido')
    events=c['events']
    require(events['delivery']=='at_least_once' and events['order_by']=='source_sequence' and events['duplicate_payload_mismatch']=='reject_conflict' and events['late_event']=='ignore_or_reconcile_never_regress' and events['gap']=='reconcile_with_source' and events['replay_preserves_ids'] is True and events['scope_revalidated'] is True,'Continuity: ordem/replay invalido')
    require(events['transaction']==['inbox','work_item','history','outbox_if_effect'] and events['producer_commit_with_outbox'] is True,'Continuity: transacao documental invalida')
    require(events['schema_validation'] is True and events['unknown_fields']=='reject' and events['unsupported_version']=='quarantine','Continuity: schema permissivo')
    concurrency=c['concurrency']
    require(concurrency['write']=='expected_version' and concurrency['conflict']=='explicit' and concurrency['human_fields_merge']=='preserve' and concurrency['dependency_cycle_guard']=='transactional_scope_lock_or_serializable' and concurrency['cross_scope_dependencies'] is False and concurrency['cancelled_satisfies_dependency'] is False and concurrency['dependency_identity']==['work_item_id','episode'],'Continuity: concorrencia/dependencia invalida')
    h=c['human_authority']
    require(all(h[k] is True for k in ['critical_requester_must_differ_from_approver','source_owns_gate','decision_requires_source_receipt']) and all(h[k] is False for k in ['automated_approval','acknowledgement_is_approval','disabled_module_bypasses_gate']) and h['decision_bound_to']==['source_entity_revision','episode','policy_version'],'Continuity: autoridade humana invalida')
    q=c['queue']
    require(q['source_context_required'] is True and q['repeat_promotes_urgency'] is False and q['stale_projection_is_empty_success'] is False and q['deferred_reason_required'] is True and q['order']==['priority_desc','due_at_asc_nulls_last','created_at_asc','id_asc'],'Continuity: fila/ruido invalido')
    registry=c['action_registry']
    require(registry['controlled_by']=='trusted_server_release' and registry['arbitrary_execution'] is False and registry['client_selected_handler'] is False and registry['unknown_parameters']=='reject' and registry['authorize_before_enqueue_execute_and_receipt'] is True and registry['uncertain_result']=='reconcile_without_blind_retry','Continuity: execucao arbitraria/autoridade invalida')
    actions=c['action_definitions']; names=unique(actions,'name')
    require(names=={'monthly_close.open','monthly_close.request_review'},'Continuity: acoes MVP divergentes')
    require(set(registry['handlers'])=={'monthly-close.open.v1','monthly-close.request-review.v1'},'Continuity: handler nao permitido')
    for a in actions:
        require(a['handler_ref'] in registry['handlers'] and a['owner']=='monthly-close' and a['required_permission'] in {'monthly-close.read','monthly-close.review.request'} and a['effect'] in {'read','request_verification'} and a['human_approval'] is False,'Continuity: handler/acao nao permitido')
        schema=a['parameter_schema']
        require(schema['type']=='object' and schema['additionalProperties'] is False and set(schema['required'])==set(schema['properties']) and set(schema['properties'])<={'close_id','expected_version'},'Continuity: parametros amplos')
        for s in schema['properties'].values():
            require((s['type']=='string' and 0<s.get('maxLength',0)<=128) or (s['type']=='integer' and s.get('minimum')==1),'Continuity: parametro sem limite')
    require(c['lifecycle']['no_data_drop'] is True and c['lifecycle']['source_native_flow_retains_gate'] is True,'Continuity: suspensao destrutiva')
    limits=c['limits']
    require(limits['fixture_only_not_production_policy'] is True and all(type(limits[k]) is int and limits[k]>0 for k in ['max_event_bytes','max_batch_items','max_nesting_depth','max_comment_characters']),'Continuity: limites ausentes')
    e=c['examples']
    envelope={'event_id','event_type','schema_version','producer','tenant_id','workspace_id','unit_id','source_entity_id','source_entity_revision','source_sequence','occurred_at','correlation_id','causation_id','payload'}
    for name in ['monthly_event','device_event']:
        event=e[name]
        require(set(event)==envelope and event['schema_version']==1 and type(event['source_sequence']) is int and event['source_sequence']>0 and event['tenant_id']=='synthetic-org-a','Continuity: envelope incompleto')
    require(e['device_stable_key']=='device:'+e['device_event']['source_entity_id']+':connectivity','Continuity: chave de dispositivo divergente')
    items=e['work_items']; unique(items); byid={i['id']:i for i in items}; itemkeys=set()
    for i in items:
        key=tuple(i[k] for k in keys['work_item_unique'])
        require(all(key) and key not in itemkeys and i['status'] in c['work_item_statuses'] and type(i['episode']) is int and i['episode']>0 and type(i['version']) is int and i['version']>0,'Continuity: item duplicado/invalido')
        itemkeys.add(key)
    graph={i:[] for i in byid}
    for d in e['dependencies']:
        require(d['item_id'] in byid and d['prerequisite_id'] in byid,'Continuity: dependencia ausente')
        a,b=byid[d['item_id']],byid[d['prerequisite_id']]
        require(a['organization_id']==b['organization_id']==d['organization_id'] and a['scope_key']==b['scope_key']==d['scope_key'] and a['episode']==d['item_episode'] and b['episode']==d['prerequisite_episode'],'Continuity: dependencia entre escopos/episodios')
        graph[a['id']].append(b['id'])
    done,active=set(),set()
    def visit(i):
        require(i not in active,'Continuity: ciclo no exemplo')
        if i in done:return
        active.add(i)
        for dep in graph[i]:visit(dep)
        active.remove(i);done.add(i)
    for i in graph:visit(i)
    compositions=read_json(root,prefix+'contratos/composicoes.proposta.json')['compositions']
    bycomp={x['id']:set(x['modules']) for x in compositions}
    require('continuity' not in bycomp['neutral'] and 'continuity' not in bycomp['admin-first-cycle'] and bycomp['admin-continuity-reference']==bycomp['admin-first-cycle']|{'continuity'},'Continuity: composicao prematura')
    modules=read_json(root,prefix+'contratos/catalogo_modulos.proposta.json')['modules']
    declared=next((m for m in modules if m['id']=='continuity'),None)
    require(declared is not None and declared['kind']=='shared' and declared['required_dependencies']==['core'] and declared['phase']=='M5-CO' and set(declared['optional_integrations'])=={'monthly-close','agenda','devices','displays'},'Continuity: catalogo divergente')
    # Preserve CSV as a usable mirror, including JSON-valued cells and nulls.
    for name,collection in [('requisitos-mestre','requirements'),('migracao','items')]:
        source=read_json(root,prefix+'matrizes/'+name+'.json')[collection]
        with (root/(prefix+'matrizes/'+name+'.csv')).open(encoding='utf-8-sig',newline='') as f: rows=list(csv.DictReader(f))
        require(len(rows)==len(source),'Continuity: CSV incompleto')
        for csvrow,obj in zip(rows,source):
            expected={k:json.dumps(v,ensure_ascii=False) if isinstance(v,(dict,list)) else ('' if v is None else str(v)) for k,v in obj.items()}
            require(csvrow==expected,'Continuity: CSV/JSON divergente')
    heritage=read_json(root,prefix+'matrizes/reaproveitamento-nexus-cc.json')
    require(heritage['schema_version']==1 and heritage['reference_only'] is True and heritage['runtime_activation'] is False and heritage['repository_code_audited_here'] is False,'Continuity: patrimonio promove implementacao')
    assets=heritage['assets']; aids=unique(assets)
    require(aids=={f'NC-{i:02}' for i in range(1,29)},'Continuity: patrimonio incompleto')
    retired={f'NC-{i:02}' for i in range(23,29)}
    for a in assets:
        require(all(a[k] for k in ['idea','destination','reason','phase','migration_candidates']) and a['implementation_status']=='NOT_RUN' and a['destination_document']=='docs/mestre/16_CONTINUIDADE_OPERACIONAL.md','Continuity: patrimonio sem destino/motivo')
        require(set(a['migration_candidates'])<={f'ABS-{i:03}' for i in range(41,49)},'Continuity: patrimonio sem candidato valido')
        require((a['decision']=='aposentar_no_destino')==(a['id'] in retired),'Continuity: aposentadoria divergente')
        require(a['decision'] in {'preservar','generalizar','preservar_principio','preservar_futuro','aposentar_no_destino'},'Continuity: decisao desconhecida')
    uses=heritage['use_cases']; unique(uses,'key')
    require(unique(uses)=={f'UC-NC-{i:02}' for i in range(1,19)} and len(uses)==18,'Continuity: usos preservados incompletos')
    covered=set()
    for u in uses:
        require(all(u[k] for k in ['scenario','owner','phase','preservation_rule','assets']) and set(u['assets'])<=aids and u['core_business_rule'] is False and u['status']=='PRESERVED_NOT_IMPLEMENTED','Continuity: uso sem fronteira/regra')
        covered.update(u['assets'])
    require(aids-retired-{'NC-20'}<=covered,'Continuity: principio sem aplicacao')
    require(set(heritage['future_envelope_uses'])=={'offline','app móvel','filas de eventos','integrações','importação segura','migração','edge devices','recuperação','APIs'},'Continuity: envelope futuro reduzido')
    require(set(heritage['health_labels'])=={'saudável','desatualizado','indisponível','não configurado','aguardando sincronização','degradado','requer verificação humana'},'Continuity: saude operacional reduzida')
    require(set(heritage['future_actions'])=={'device.run_diagnostic','display.republish','schedule.open','notice.request_approval','inspection.start','report.generate','incident.acknowledge'} and set(heritage['future_events'])=={'DeviceOfflineDetected','ScheduleReadyForApproval','ShiftClosing'},'Continuity: exemplos futuros perdidos')
    return {'contract':'PASS','scope':'document_only_no_runtime_simulation','heritage_decisions':len(assets),'preserved_use_cases':len(uses)}
