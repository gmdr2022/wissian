import { useEffect, useState, type FormEvent } from 'react'
import { AccessFrame } from './AccessFrame'
import { ApiError, apiFetch, command, getSession, messageFor, type AccountAction } from './api'

export function AccessHelp({ onBack }: { onBack: () => void }) {
  const [message, setMessage] = useState('Consultando as orientações de acesso…')
  useEffect(() => {
    const controller = new AbortController()
    apiFetch('/api/identity/access-help', { signal: controller.signal })
      .then(response => response.json())
      .then((data: { mode?: string; message?: string }) => {
        if (!controller.signal.aborted) setMessage(data.mode === 'assisted' && typeof data.message === 'string'
          ? data.message : 'Consulte o responsável pelo acesso à sua organização.')
      }).catch(() => {
        if (!controller.signal.aborted) setMessage('Não foi possível consultar o serviço. Entre em contato com o responsável pelo acesso à sua organização.')
      })
    return () => controller.abort()
  }, [])
  return <AccessFrame title="Precisa de ajuda para entrar?">
    <p role="status">{message}</p>
    <p className="description">Se perdeu o autenticador, informe isso ao responsável. Redefinir a senha não remove a verificação em duas etapas.</p>
    <button onClick={onBack}>Voltar para entrar</button>
  </AccessFrame>
}

export function AccountAccess({ action, onDone }: { action: AccountAction | null; onDone: (message?: string) => void }) {
  const [signedIn, setSignedIn] = useState<boolean | null>(null)
  const [pending, setPending] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  useEffect(() => {
    const controller = new AbortController()
    getSession(controller.signal).then(session => { if (!controller.signal.aborted) setSignedIn(Boolean(session)) })
      .catch(() => { if (!controller.signal.aborted) setMessage('Não foi possível verificar a conta atual. Reabra o link quando a conexão estiver disponível.') })
    return () => controller.abort()
  }, [])

  async function signOut() {
    if (pending) return
    setPending(true); setMessage(null)
    try { await command('/api/identity/logout'); setSignedIn(false) }
    catch (error) {
      if (error instanceof ApiError && error.status === 401) setSignedIn(false)
      else setMessage(messageFor(error, 'logout'))
    }
    finally { setPending(false) }
  }
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!action || pending) return
    const form = event.currentTarget, data = new FormData(form)
    const password = String(data.get('newPassword') ?? '')
    if (password !== data.get('confirmPassword')) {
      setMessage('As senhas não conferem. Digite a mesma senha nos dois campos.')
      const confirmation = form.elements.namedItem('confirmPassword')
      if (confirmation instanceof HTMLInputElement) confirmation.focus()
      return
    }
    setPending(true); setMessage(null)
    try {
      await command(`/api/identity/${action.purpose === 'activation' ? 'activate' : 'reset-password'}`,
        { userId: action.userId, token: action.token, newPassword: password })
      form.reset()
      onDone(action.purpose === 'activation' ? 'Acesso ativado. Entre com sua nova senha.' : 'Senha atualizada. Entre novamente; suas sessões anteriores foram encerradas.')
    } catch (error) { setMessage(messageFor(error, 'account')) }
    finally { setPending(false) }
  }
  if (!action) return <AccessFrame title="Abra um link de acesso válido" description="Este link está incompleto ou já foi fechado neste navegador.">
    <p>Reabra o link recebido ou solicite um novo ao responsável pelo seu acesso.</p>
    <button onClick={() => onDone()}>Voltar para entrar</button>
  </AccessFrame>
  return <AccessFrame title={action.purpose === 'activation' ? 'Crie sua senha de acesso' : 'Defina uma nova senha'}
    description="Use uma senha exclusiva para sua conta." message={message}>
    {signedIn === null ? <p role="status">Verificando a conta atual…</p> : signedIn ? <>
      <p>Existe uma conta aberta neste navegador. Saia dela para continuar com este link.</p>
      <button disabled={pending} onClick={() => void signOut()}>{pending ? 'Saindo…' : 'Sair da conta atual'}</button>
    </> : <form aria-describedby={message ? 'access-message' : undefined} onSubmit={event => void submit(event)}>
      <fieldset disabled={pending}>
        <label htmlFor="new-password">Nova senha</label>
        <input id="new-password" name="newPassword" type="password" autoComplete="new-password" required minLength={12} maxLength={1024} aria-describedby="password-requirements" />
        <p className="field-help" id="password-requirements">No mínimo 12 caracteres, incluindo maiúscula, minúscula, número e símbolo; use ao menos 6 caracteres diferentes.</p>
        <label htmlFor="confirm-password">Confirme a nova senha</label>
        <input id="confirm-password" name="confirmPassword" type="password" autoComplete="new-password" required minLength={12} maxLength={1024} />
        <button type="submit">{pending ? 'Atualizando acesso…' : 'Salvar nova senha'}</button>
      </fieldset>
    </form>}
    <button className="quiet" disabled={pending} onClick={() => onDone()}>Voltar para entrar</button>
  </AccessFrame>
}
