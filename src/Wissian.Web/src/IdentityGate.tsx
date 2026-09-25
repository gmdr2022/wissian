import { useCallback, useEffect, useRef, useState, type FormEvent, type ReactNode } from 'react'
import { AccessFrame } from './AccessFrame'
import { AccessHelp } from './AccountAccess'
import { ApiError, command, getSession, messageFor, type OrganizationChoice, type Selection, type Session } from './api'
import { navigate } from './navigation'

type State = { phase: 'checking' | 'signed-out' | 'mfa' | 'unavailable' }
  | { phase: 'ready'; session: Session; choosing: boolean }
export interface ActiveIdentity {
  session: Session & { context: NonNullable<Session['context']> }
  chooseOrganization: () => void
  refresh: () => void
  signOut: () => void
  pending: boolean
  message: string | null
}

export function IdentityGate({ children, initialMessage }: { children: (identity: ActiveIdentity) => ReactNode; initialMessage?: string }) {
  const [state, setState] = useState<State>({ phase: 'checking' })
  const [pending, setPending] = useState(false)
  const [message, setMessage] = useState<string | null>(initialMessage ?? null)
  const [help, setHelp] = useState(false)
  const busy = useRef(false), request = useRef<AbortController | null>(null)
  const selection = useRef<Selection | null>(null)
  const channel = useRef<BroadcastChannel | null>(null)
  const phase = useRef(state.phase); phase.current = state.phase
  const focusAfterCommand = useRef<HTMLInputElement | null>(null)
  useEffect(() => {
    if (!pending && focusAfterCommand.current) {
      if (focusAfterCommand.current.isConnected) focusAfterCommand.current.focus()
      focusAfterCommand.current = null
    }
  }, [pending])

  const refresh = useCallback(async () => {
    request.current?.abort()
    const controller = new AbortController(); request.current = controller
    setState({ phase: 'checking' })
    try {
      const session = await getSession(controller.signal)
      if (controller.signal.aborted) return
      selection.current = null
      setState(session ? { phase: 'ready', session, choosing: session.context === null } : { phase: 'signed-out' })
    } catch {
      if (!controller.signal.aborted) {
        setState({ phase: 'unavailable' })
        setMessage('Não foi possível verificar seu acesso. O conteúdo foi fechado até a conexão ser restabelecida.')
      }
    }
  }, [])

  useEffect(() => { void refresh(); return () => request.current?.abort() }, [refresh])
  useEffect(() => {
    const sessionChanged = () => {
      if (!busy.current && (phase.current === 'ready' || phase.current === 'unavailable')) void refresh()
    }
    const visible = () => { if (document.visibilityState === 'visible') sessionChanged() }
    const receive = (event: MessageEvent<unknown>) => { if (event.data === 'changed' && !busy.current) void refresh() }
    const bus = new BroadcastChannel('wissian.session'); channel.current = bus
    bus.addEventListener('message', receive)
    window.addEventListener('focus', sessionChanged)
    document.addEventListener('visibilitychange', visible)
    return () => {
      bus.close(); channel.current = null
      window.removeEventListener('focus', sessionChanged)
      document.removeEventListener('visibilitychange', visible)
    }
  }, [refresh])
  const expiresAt = state.phase === 'ready' ? state.session.expiresAtUtc : null
  useEffect(() => {
    if (!expiresAt) return
    const timer = window.setTimeout(() => {
      request.current?.abort(); selection.current = null
      setState({ phase: 'signed-out' }); setMessage('Sua sessão expirou. Entre novamente para continuar.')
    }, Math.max(0, Date.parse(expiresAt) - Date.now()))
    return () => window.clearTimeout(timer)
  }, [expiresAt])

  async function authenticate(event: FormEvent<HTMLFormElement>, mfa: boolean) {
    event.preventDefault()
    if (busy.current) return
    busy.current = true; setPending(true); setMessage(null)
    const form = event.currentTarget, data = new FormData(form)
    try {
      const response = await command(`/api/identity/${mfa ? 'mfa' : 'login'}`, mfa
        ? { code: String(data.get('code') ?? '').trim() }
        : { identifier: String(data.get('identifier') ?? '').trim(), password: String(data.get('password') ?? '') })
      form.reset()
      if (response.status === 202) setState({ phase: 'mfa' })
      else { channel.current?.postMessage('changed'); await refresh() }
    } catch (error) {
      const secret = form.elements.namedItem(mfa ? 'code' : 'password')
      if (secret instanceof HTMLInputElement) {
        secret.value = ''
        focusAfterCommand.current = secret
      }
      if (error instanceof ApiError && error.code === 'already_authenticated') {
        await refresh(); setMessage('Há uma sessão aberta neste navegador. Confira a conta e a organização.')
      } else setMessage(messageFor(error, mfa ? 'mfa' : 'login'))
    } finally { busy.current = false; setPending(false) }
  }
  async function cancelMfa() {
    if (busy.current) return
    busy.current = true; setPending(true)
    try { await command('/api/identity/cancel-mfa'); setState({ phase: 'signed-out' }); setMessage(null) }
    catch (error) { setMessage(messageFor(error, 'mfa')) }
    finally { busy.current = false; setPending(false) }
  }
  async function choose(choice: OrganizationChoice, session: Session) {
    if (busy.current) return
    busy.current = true; setPending(true); setMessage(null)
    // Preserve the command id after a lost response. A refresh or definitive conflict retires it.
    if (selection.current?.organizationId !== choice.organizationId || selection.current.membershipId !== choice.membershipId
      || selection.current.expectedSessionVersion !== session.version) selection.current = {
      organizationId: choice.organizationId, membershipId: choice.membershipId,
      expectedSessionVersion: session.version, commandId: crypto.randomUUID(),
    }
    try {
      await command('/api/identity/context', selection.current)
      channel.current?.postMessage('changed'); await refresh()
    } catch (error) {
      setMessage(messageFor(error, 'context'))
      if (error instanceof ApiError && error.status !== null && error.status < 500) await refresh()
    } finally { busy.current = false; setPending(false) }
  }
  async function signOut() {
    if (busy.current) return
    busy.current = true; setPending(true); setMessage(null)
    request.current?.abort(); selection.current = null; setState({ phase: 'checking' })
    try {
      await command('/api/identity/logout')
      channel.current?.postMessage('changed'); navigate('/'); setState({ phase: 'signed-out' })
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) { navigate('/'); setState({ phase: 'signed-out' }) }
      else { setState({ phase: 'unavailable' }); setMessage(messageFor(error, 'logout')) }
    } finally { busy.current = false; setPending(false) }
  }

  if (help) return <AccessHelp onBack={() => { setHelp(false); void refresh() }} />
  if (state.phase === 'checking') return <AccessFrame title="Verificando seu acesso"><p role="status">Aguarde um instante…</p></AccessFrame>
  if (state.phase === 'unavailable') return <AccessFrame title="Vamos restabelecer seu acesso" message={message}>
    <div className="actions"><button disabled={pending} onClick={() => void refresh()}>Verificar meu acesso</button>
      <button className="quiet" disabled={pending} onClick={() => void signOut()}>Tentar sair</button></div>
  </AccessFrame>
  if (state.phase === 'signed-out' || state.phase === 'mfa') {
    const mfa = state.phase === 'mfa'
    return <AccessFrame title={mfa ? 'Confirme que é você' : 'Entre no Wissian'} message={message} tone={message === initialMessage ? 'success' : 'error'}
      description={mfa ? 'Digite o código atual do seu aplicativo autenticador.' : 'Acesse sua conta para escolher onde vai trabalhar.'}>
      <form key={state.phase} aria-describedby={message ? 'access-message' : undefined} onSubmit={event => void authenticate(event, mfa)}>
        <fieldset disabled={pending}>
          {mfa ? <><label htmlFor="code">Código do autenticador</label>
            <input id="code" name="code" inputMode="numeric" autoComplete="one-time-code" maxLength={16} required /></>
            : <><label htmlFor="identifier">Usuário ou e-mail</label>
              <input id="identifier" name="identifier" autoComplete="username" maxLength={256} required autoCapitalize="none" spellCheck={false} />
              <label htmlFor="password">Senha</label>
              <input id="password" name="password" type="password" autoComplete="current-password" maxLength={1024} required /></>}
          <button type="submit">{pending ? 'Verificando…' : mfa ? 'Confirmar código' : 'Entrar'}</button>
        </fieldset>
      </form>
      <button className="quiet" disabled={pending} onClick={() => mfa ? void cancelMfa() : setHelp(true)}>
        {mfa ? 'Voltar para entrar' : 'Preciso de ajuda para entrar'}</button>
    </AccessFrame>
  }
  if (state.phase !== 'ready') return null
  if (state.choosing || !state.session.context) return <AccessFrame title="Onde você vai trabalhar?" message={message}
    description={`Olá, ${state.session.displayName}. Escolha uma organização para continuar.`}>
    {state.session.organizations.length === 0 ? <div className="empty-state" role="status">
      <h2>Nenhuma organização disponível</h2><p>Solicite ao responsável pelo acesso que revise seu vínculo.</p>
    </div> : <ul className="organization-list">{state.session.organizations.map(choice => <li key={choice.membershipId}>
      <button className="organization-choice" disabled={pending} onClick={() => void choose(choice, state.session)}>
        <span className="organization-symbol" aria-hidden="true">{choice.displayName.slice(0, 1)}</span>
        <strong>{choice.displayName}</strong><span aria-hidden="true">→</span>
      </button></li>)}</ul>}
    {pending && <p role="status">Confirmando sua organização…</p>}
    <div className="actions"><button className="quiet" disabled={pending} onClick={() => void refresh()}>Atualizar acesso</button>
      <button className="quiet" disabled={pending} onClick={() => void signOut()}>Sair</button></div>
  </AccessFrame>
  return children({
    session: state.session as ActiveIdentity['session'], pending, message,
    chooseOrganization: () => { navigate('/'); setMessage(null); setState({ ...state, choosing: true }) },
    refresh: () => void refresh(), signOut: () => void signOut(),
  })
}
