import { useEffect, useRef, useState, type ComponentType, type MouseEvent } from 'react'
import { AccountAccess } from './AccountAccess'
import { Appearance } from './Appearance'
import { IdentityGate, type ActiveIdentity } from './IdentityGate'
import { navigate, usePathname } from './navigation'
import type { AccountAction } from './api'

export interface WebModule {
  id: string
  title: string
  capability: string
  View: ComponentType<{ identity: ActiveIdentity }>
}

function Link({ path, children, onNavigate }: { path: string; children: string; onNavigate?: () => void }) {
  const current = usePathname()
  function click(event: MouseEvent<HTMLAnchorElement>) {
    if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
    event.preventDefault(); navigate(path); onNavigate?.()
  }
  return <a href={path} onClick={click} aria-current={current === path ? 'page' : undefined}>{children}</a>
}

function Workspace({ identity, modules }: { identity: ActiveIdentity; modules: readonly WebModule[] }) {
  const pathname = usePathname(), heading = useRef<HTMLHeadingElement>(null), main = useRef<HTMLElement>(null)
  const [navigationOpen, setNavigationOpen] = useState(false)
  const available = modules.filter(module => identity.session.context.capabilities.includes(module.capability))
  const selected = available.find(module => pathname === `/m/${module.id}`)
  useEffect(() => { (heading.current ?? main.current)?.focus() }, [pathname])
  return <div className="workspace">
    <a className="skip-link" href="#workspace-content">Ir para o conteúdo</a>
    <header className="workspace-header">
      <span className="wordmark">Wissian<span aria-hidden="true">.</span></span>
      <button className="navigation-toggle quiet" aria-expanded={navigationOpen} aria-controls="workspace-navigation"
        onClick={() => setNavigationOpen(!navigationOpen)}>{navigationOpen ? 'Fechar menu' : 'Abrir menu'}</button>
      <button className="context-switch quiet" disabled={identity.pending} onClick={identity.chooseOrganization}
        aria-label={`Trocar organização. Atual: ${identity.session.context.displayName}`}>
        <span className="field-help">Organização ativa</span><strong>{identity.session.context.displayName}</strong><span aria-hidden="true">⌄</span>
      </button>
      <span className="user-name">{identity.session.displayName}</span>
      <button className="quiet" disabled={identity.pending} onClick={identity.signOut}>Sair</button>
    </header>
    <div className="workspace-body">
      <aside id="workspace-navigation" className={`sidebar ${navigationOpen ? 'is-open' : ''}`}>
        <p className="eyebrow">Espaço de trabalho</p>
        <nav aria-label="Áreas de trabalho"><ul>
          <li><Link path="/" onNavigate={() => setNavigationOpen(false)}>Início</Link></li>
          {available.map(module => <li key={module.id}><Link path={`/m/${module.id}`}
            onNavigate={() => setNavigationOpen(false)}>{module.title}</Link></li>)}
        </ul></nav>
        <div className="sidebar-footer"><Appearance /><span>Wissian · Fultuma Tech</span></div>
      </aside>
      <main id="workspace-content" className="workspace-content" tabIndex={-1} ref={main}>
        {identity.message && <p className="notice" role="alert">{identity.message}</p>}
        {pathname === '/' ? <>
          <p className="eyebrow">{identity.session.context.displayName}</p>
          <h1 tabIndex={-1} ref={heading}>Seu espaço de trabalho</h1>
          <p className="description">Olá, {identity.session.displayName}. Você está na organização acima.</p>
          {available.length ? <section className="module-list" aria-labelledby="modules-title">
            <h2 id="modules-title">O que você precisa fazer?</h2>
            {available.map(module => <Link path={`/m/${module.id}`} key={module.id}>{module.title}</Link>)}
          </section> : <section className="empty-state" role="status">
            <span className="empty-symbol" aria-hidden="true">◇</span>
            <h2>Nenhuma atividade disponível para esta conta</h2>
            <p>Se você esperava encontrar um módulo, solicite a revisão do seu acesso ao responsável pela organização.</p>
            <button className="secondary" onClick={identity.refresh}>Atualizar acesso</button>
          </section>}
        </> : selected ? <selected.View identity={identity} /> : <section className="empty-state">
          <h1 tabIndex={-1} ref={heading}>Esta área não está disponível</h1>
          <p>Escolha uma das áreas liberadas para sua conta ou atualize seu acesso.</p>
          <div className="actions"><Link path="/">Voltar ao início</Link><button className="secondary" onClick={identity.refresh}>Atualizar acesso</button></div>
        </section>}
      </main>
    </div>
  </div>
}

// Only reviewed, compiled composition supplies views; server capabilities filter the menu,
// and every module request still requires server authorization.
export function App({ initialAccount, modules = [] }: { initialAccount: AccountAction | null; modules?: readonly WebModule[] }) {
  const [accountPage, setAccountPage] = useState(window.location.pathname === '/access')
  const [action, setAction] = useState(initialAccount)
  const [message, setMessage] = useState<string>()
  if (accountPage) return <AccountAccess action={action} onDone={text => {
    setAction(null); setAccountPage(false); setMessage(text); navigate('/')
  }} />
  return <IdentityGate initialMessage={message}>{identity => <Workspace
    key={`${identity.session.sessionId}:${identity.session.version}:${identity.session.context.organizationId}`}
    identity={identity} modules={modules} />}</IdentityGate>
}
