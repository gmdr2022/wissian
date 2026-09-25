export interface OrganizationChoice {
  organizationId: string
  membershipId: string
  displayName: string
}
export interface ActiveContext extends OrganizationChoice { capabilities: string[] }
export interface Session {
  sessionId: string
  version: number
  expiresAtUtc: string
  displayName: string
  context: ActiveContext | null
  organizations: OrganizationChoice[]
}
export interface Selection {
  organizationId: string
  membershipId: string
  expectedSessionVersion: number
  commandId: string
}
export interface AccountAction {
  purpose: 'activation' | 'recovery'
  userId: string
  token: string
}

export class ApiError extends Error {
  constructor(readonly status: number | null, readonly code = 'unavailable') {
    super('Não foi possível concluir a solicitação.')
    this.name = 'ApiError'
  }
}

export async function apiFetch(path: string, init: RequestInit = {}, accepted: number[] = []) {
  if (!path.startsWith('/api/') || path.includes('\\')) throw new Error('Caminho de API inválido.')
  let response: Response
  try {
    response = await fetch(path, {
      ...init, credentials: 'same-origin', cache: 'no-store', redirect: 'error',
      signal: init.signal ? AbortSignal.any([init.signal, AbortSignal.timeout(20_000)]) : AbortSignal.timeout(20_000),
      headers: { Accept: 'application/json', ...init.headers },
    })
  } catch (error) {
    if (init.signal?.aborted) throw error
    throw new ApiError(null)
  }
  if (!response.ok && !accepted.includes(response.status)) {
    let code = 'request_rejected'
    try {
      const body: unknown = await response.json()
      if (typeof body === 'object' && body !== null && 'code' in body && typeof body.code === 'string') code = body.code
    } catch { /* A non-JSON failure keeps the HTTP status; never render its body. */ }
    throw new ApiError(response.status, code)
  }
  return response
}

async function json<T>(response: Response): Promise<T> {
  try { return await response.json() as T } catch { throw new ApiError(response.status, 'invalid_response') }
}

export async function getSession(signal?: AbortSignal): Promise<Session | null> {
  const response = await apiFetch('/api/identity/session', { signal }, [401])
  if (response.status === 401) return null
  const session = await json<Session>(response)
  if (!session.sessionId || !Number.isSafeInteger(session.version) || session.version < 1
    || !Array.isArray(session.organizations) || !Number.isFinite(Date.parse(session.expiresAtUtc)))
    throw new ApiError(200, 'invalid_response')
  return session
}

export async function command(path: string, body?: unknown) {
  // A new token is obtained after every identity/context transition. No credential cache.
  const csrf = await json<{ token: string }>(await apiFetch('/api/identity/csrf'))
  if (!csrf.token) throw new ApiError(200, 'invalid_response')
  return apiFetch(path, {
    method: 'POST', headers: { 'Content-Type': 'application/json', 'X-CSRF-TOKEN': csrf.token },
    body: body === undefined ? undefined : JSON.stringify(body),
  })
}

export function takeAccountAction(): AccountAction | null {
  if (window.location.pathname !== '/access') return null
  const params = new URLSearchParams(window.location.hash.slice(1))
  // Fragments are not sent in HTTP. Consume immediately; never retain in history/storage.
  window.history.replaceState(null, '', '/access')
  const purpose = params.get('purpose'), userId = params.get('userId'), token = params.get('token')
  return (purpose === 'activation' || purpose === 'recovery') && userId
    && /^[0-9a-f-]{36}$/i.test(userId) && token && token.length <= 4096
    ? { purpose, userId, token } : null
}

export function messageFor(error: unknown, operation: 'login' | 'mfa' | 'account' | 'context' | 'logout') {
  if (!(error instanceof ApiError) || error.status === null)
    return 'Não foi possível confirmar a resposta. Verifique sua conexão e tente novamente.'
  if (error.status === 429) return 'Muitas tentativas. Aguarde um minuto antes de tentar novamente.'
  if (error.code === 'csrf_invalid') return 'A sessão de acesso mudou. Tente enviar o formulário novamente.'
  if (error.code === 'password_not_accepted') return 'A senha não atende aos requisitos abaixo. Revise e tente novamente.'
  if (error.code === 'access_link_invalid') return 'Este link é inválido, expirou ou já foi utilizado. Solicite um novo acesso.'
  if (error.code === 'sign_out_required') return 'Saia da conta atual antes de utilizar este link.'
  if (operation === 'mfa' && error.status === 401) return 'Código inválido ou verificação expirada. Tente o código atual ou volte para entrar.'
  if (operation === 'login' && error.status === 401) return 'Não foi possível entrar com esses dados. Confira seu usuário e sua senha.'
  if (operation === 'context' && [401, 404, 409].includes(error.status))
    return 'Seu acesso ou a organização ativa mudou. Confirme novamente onde deseja trabalhar.'
  if (operation === 'logout') return 'Não foi possível confirmar a saída. Tente novamente antes de deixar este dispositivo.'
  if (operation === 'account') return 'Não foi possível atualizar o acesso. Confira o link ou solicite um novo ao responsável.'
  return 'O serviço está indisponível no momento. Tente novamente em instantes.'
}
