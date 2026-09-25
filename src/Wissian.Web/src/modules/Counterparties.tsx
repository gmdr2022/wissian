import { useEffect, useRef, useState, type FormEvent } from 'react'
import { ApiError, apiFetch, command } from '../api'
import type { ActiveIdentity } from '../IdentityGate'
import type { WebModule } from '../App'

type Input = { name: string; kind: string; relation: string; reference: string | null }
type Item = Input & { id: string; version: number; isArchived: boolean }
type Page = { items: Item[]; page: number; total: number }
type Request = { commandId: string; expectedSessionVersion: number; expectedVersion: number | null; action: string; value: Input | null }
const empty: Input = { name: '', kind: 'organization', relation: 'customer', reference: null }
const relations: Record<string, string> = { customer: 'Cliente', supplier: 'Fornecedor', both: 'Cliente e fornecedor', other: 'Outra relação' }

function Counterparties({ identity }: { identity: ActiveIdentity }) {
  const { context } = identity.session
  const base = `/api/organizations/${context.organizationId}/counterparties`
  const canWrite = context.capabilities.includes('counterparties.write')
  const [filters, setFilters] = useState({ q: '', status: 'active', page: 1 })
  const [query, setQuery] = useState(''), [status, setStatus] = useState('active'), [revision, setRevision] = useState(0)
  const [list, setList] = useState<Page | null>(null), [listError, setListError] = useState(false)
  const [editor, setEditor] = useState<Item | 'new' | null>(null), [draft, setDraft] = useState<Input>(empty)
  const [baseline, setBaseline] = useState<Input>(empty), [busy, setBusy] = useState(false)
  const [uncertain, setUncertain] = useState(false), [conflict, setConflict] = useState(false)
  const [notice, setNotice] = useState<{ text: string; error: boolean } | null>(null)
  const pending = useRef<{ url: string; request: Request } | null>(null), sending = useRef(false)
  const heading = useRef<HTMLHeadingElement>(null), editorHeading = useRef<HTMLHeadingElement>(null), name = useRef<HTMLInputElement>(null), feedback = useRef<HTMLParagraphElement>(null)
  const detailRequest = useRef<AbortController | null>(null)
  const dirty = editor !== null && JSON.stringify(draft) !== JSON.stringify(baseline)
  useEffect(() => {
    const controller = new AbortController(); setList(null); setListError(false)
    const params = new URLSearchParams({ q: filters.q, status: filters.status, page: String(filters.page) })
    void apiFetch(`${base}?${params}`, { signal: controller.signal }).then(r => r.json() as Promise<Page>)
      .then(value => { if (!controller.signal.aborted) setList(value) })
      .catch(error => {
        if (controller.signal.aborted) return
        setListError(true)
        if (error instanceof ApiError && [401, 404].includes(error.status ?? 0)) identity.refresh()
      })
    return () => controller.abort()
  }, [base, filters, revision, identity.refresh])
  useEffect(() => () => detailRequest.current?.abort(), [])
  useEffect(() => { if (notice?.error) feedback.current?.focus() }, [notice])
  useEffect(() => { if (!busy && editor === 'new') name.current?.focus() }, [editor, busy])
  const editorId = editor && editor !== 'new' ? editor.id : null
  useEffect(() => { if (editorId) editorHeading.current?.focus() }, [editorId])
  useEffect(() => {
    if (!dirty && !uncertain) return
    const unload = (event: BeforeUnloadEvent) => { event.preventDefault(); event.returnValue = '' }
    const leave = (event: Event) => { if (!window.confirm('Há alterações sem confirmação. Deseja sair desta tela?')) event.preventDefault() }
    window.addEventListener('beforeunload', unload); window.addEventListener('wissian:before-navigate', leave)
    return () => { window.removeEventListener('beforeunload', unload); window.removeEventListener('wissian:before-navigate', leave) }
  }, [dirty, uncertain])

  function allowDiscard() { return !dirty && !uncertain || window.confirm('Descartar as alterações desta tela?') }
  function close() {
    if (!allowDiscard()) return
    detailRequest.current?.abort(); setEditor(null); setNotice(null); pending.current = null; setUncertain(false); setConflict(false)
    heading.current?.focus()
  }
  function create() {
    if (!allowDiscard()) return
    setEditor('new'); setDraft(empty); setBaseline(empty); setNotice(null); pending.current = null; setUncertain(false); setConflict(false)
  }
  async function open(id: string) {
    if (!allowDiscard()) return
    detailRequest.current?.abort(); const controller = new AbortController(); detailRequest.current = controller
    setBusy(true); setNotice(null)
    try {
      const item: Item = await (await apiFetch(`${base}/${id}`, { signal: controller.signal })).json()
      if (controller.signal.aborted) return
      const value = { name: item.name, kind: item.kind, relation: item.relation, reference: item.reference }
      setEditor(item); setDraft(value); setBaseline(value); setUncertain(false); setConflict(false); pending.current = null
    } catch (error) {
      if (controller.signal.aborted) return
      setNotice({ text: 'Não foi possível abrir este cadastro. Atualize a lista e tente novamente.', error: true })
      if (error instanceof ApiError && error.status === 401) identity.refresh()
    } finally { if (!controller.signal.aborted) setBusy(false) }
  }
  async function save(action: string) {
    if (sending.current || editor === null) return
    if (!pending.current) pending.current = {
      url: editor === 'new' ? base : `${base}/${editor.id}`,
      request: { commandId: crypto.randomUUID(), expectedSessionVersion: identity.session.version,
        expectedVersion: editor === 'new' ? null : editor.version, action,
        value: action === 'create' || action === 'edit' ? { ...draft, name: draft.name.trim(), reference: draft.reference?.trim() || null } : null },
    }
    const operation = pending.current
    sending.current = true; setBusy(true); setNotice(null); setConflict(false)
    try {
      const result: { id: string; version: number } = await (await command(operation.url, operation.request)).json()
      if (!result.id || !Number.isSafeInteger(result.version) || result.version < 1) throw new ApiError(null)
      const value = operation.request.value ?? baseline
      const item: Item = { ...value, id: result.id, version: result.version, isArchived: operation.request.action === 'archive' }
      // A confirmed command is never retried just because refreshing the list fails afterward.
      pending.current = null; setUncertain(false); setEditor(item); setDraft(value); setBaseline(value)
      setRevision(v => v + 1)
      setNotice({ text: operation.request.action === 'archive' ? 'Cadastro arquivado. O histórico e os vínculos foram preservados.'
        : operation.request.action === 'restore' ? 'Cadastro restaurado.' : 'Cadastro salvo.', error: false })
    } catch (error) {
      if (error instanceof ApiError && error.status !== null && error.status < 500) {
        pending.current = null; setUncertain(false)
        if (error.status === 401 || error.status === 404 || error.code === 'session_changed') {
          identity.refresh(); setNotice({ text: 'Seu acesso ou contexto mudou. Atualize o acesso para continuar.', error: true })
        } else if (error.status === 409) {
          setConflict(true); setNotice({ text: 'O cadastro mudou desde a sua leitura. Suas alterações estão preservadas nesta tela. Recarregue antes de editar novamente.', error: true })
        } else setNotice({ text: 'Revise o nome, o tipo e os limites dos campos. O cadastro não foi salvo.', error: true })
      } else {
        setUncertain(true); setNotice({ text: 'Não foi possível confirmar a gravação. Repita a confirmação do mesmo pedido para evitar duplicidade.', error: true })
      }
    } finally { sending.current = false; setBusy(false) }
  }
  function submit(event: FormEvent) { event.preventDefault(); void save(editor === 'new' ? 'create' : 'edit') }
  const archived = editor !== null && editor !== 'new' && editor.isArchived
  return <section className="counterparties">
    <p className="eyebrow">{context.displayName}</p><h1 ref={heading} tabIndex={-1}>Clientes e fornecedores</h1>
    <p className="description">Pessoas e empresas relacionadas à sua organização.</p>
    <div className="counterparty-toolbar">
      <form onSubmit={e => { e.preventDefault(); setFilters({ q: query, status, page: 1 }) }} aria-label="Filtrar cadastros">
        <label htmlFor="counterparty-search">Buscar nome ou referência</label>
        <div className="filter-fields"><input id="counterparty-search" maxLength={100} value={query} onChange={e => setQuery(e.target.value)} />
          <div className="filter-select"><label htmlFor="counterparty-status">Situação</label><select id="counterparty-status" value={status} onChange={e => setStatus(e.target.value)}><option value="active">Ativos</option><option value="archived">Arquivados</option><option value="all">Todos</option></select></div>
          <button className="secondary" type="submit">Buscar</button></div>
      </form>
      {canWrite && <button onClick={create} disabled={busy || uncertain}>Novo cadastro</button>}
    </div>
    {notice && <p className={notice.error ? 'notice' : 'success-note'} ref={feedback} tabIndex={-1} role={notice.error ? 'alert' : 'status'}>{notice.text}</p>}
    {editor !== null && <section className="counterparty-editor" aria-labelledby="counterparty-editor-title">
      <div className="editor-title"><h2 id="counterparty-editor-title" ref={editorHeading} tabIndex={-1}>{editor === 'new' ? 'Novo cadastro' : archived ? 'Cadastro arquivado' : 'Detalhes do cadastro'}</h2>
        <button className="quiet" onClick={close} disabled={busy}>Voltar à lista</button></div>
      <form onSubmit={submit}>
        <fieldset disabled={busy || uncertain || !canWrite || archived || conflict}>
          <label htmlFor="counterparty-name">Nome</label><input id="counterparty-name" ref={name} required maxLength={160} value={draft.name} onChange={e => setDraft({ ...draft, name: e.target.value })} />
          <div className="field-pair"><div className="form-field"><label htmlFor="counterparty-kind">Tipo</label><select id="counterparty-kind" value={draft.kind} onChange={e => setDraft({ ...draft, kind: e.target.value })}><option value="organization">Empresa ou organização</option><option value="person">Pessoa</option></select></div>
            <div className="form-field"><label htmlFor="counterparty-relation">Relação</label><select id="counterparty-relation" value={draft.relation} onChange={e => setDraft({ ...draft, relation: e.target.value })}>{Object.entries(relations).map(([key, text]) => <option key={key} value={key}>{text}</option>)}</select></div></div>
          <label htmlFor="counterparty-reference">Referência interna (opcional)</label><input id="counterparty-reference" maxLength={80} value={draft.reference ?? ''} onChange={e => setDraft({ ...draft, reference: e.target.value || null })} />
          <p className="field-help">Uma identificação que sua organização já utiliza. Não verifica identidade ou situação fiscal.</p>
          {canWrite && !archived && <button type="submit">{busy ? 'Salvando…' : 'Salvar cadastro'}</button>}
        </fieldset>
      </form>
      <div className="actions">
        {uncertain && <button disabled={busy} onClick={() => void save(pending.current!.request.action)}>Repetir confirmação</button>}
        {conflict && editor !== 'new' && <button className="secondary" onClick={() => void open(editor.id)}>Recarregar cadastro</button>}
        {canWrite && editor !== 'new' && !uncertain && !conflict && <button className="secondary" disabled={busy || dirty}
          onClick={() => { if (window.confirm(archived ? 'Restaurar este cadastro?' : 'Arquivar este cadastro? Seus vínculos e histórico serão preservados.')) void save(archived ? 'restore' : 'archive') }}>
          {archived ? 'Restaurar cadastro' : 'Arquivar cadastro'}</button>}
      </div>
    </section>}
    <section aria-label="Resultados da busca" aria-busy={list === null && !listError}>
      {listError ? <div role="alert"><p>Não foi possível carregar os cadastros.</p><button onClick={() => setRevision(v => v + 1)}>Tentar novamente</button></div>
        : list === null ? <p role="status">Carregando cadastros…</p> : <>
          <p role="status">{list.total} {list.total === 1 ? 'cadastro encontrado' : 'cadastros encontrados'}</p>
          {list.items.length === 0 ? <p className="empty-state">Nenhum cadastro nesta busca. Ajuste os filtros{canWrite ? ' ou crie um cadastro' : ''}.</p>
            : <ul className="counterparty-results">{list.items.map(item => <li key={item.id}>
              <div><strong>{item.name}</strong><span>{relations[item.relation]}{item.reference ? ` · ${item.reference}` : ''}{item.isArchived ? ' · Arquivado' : ''}</span></div>
              <button className="secondary" disabled={busy || uncertain} onClick={() => void open(item.id)} aria-label={`Abrir ${item.name}`}>Abrir</button>
            </li>)}</ul>}
          {list.total > 25 && <nav aria-label="Páginas dos cadastros" className="actions">
            <button className="secondary" disabled={filters.page === 1} onClick={() => setFilters({ ...filters, page: filters.page - 1 })}>Anterior</button>
            <span>Página {filters.page} de {Math.ceil(list.total / 25)}</span>
            <button className="secondary" disabled={filters.page * 25 >= list.total} onClick={() => setFilters({ ...filters, page: filters.page + 1 })}>Próxima</button>
          </nav>}
        </>}
    </section>
  </section>
}

export const counterpartiesModule: WebModule = { id: 'counterparties', title: 'Clientes e fornecedores', capability: 'counterparties.read', View: Counterparties }
