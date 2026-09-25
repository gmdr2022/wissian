import { useEffect, useState } from 'react'
import { createRoot } from 'react-dom/client'
import { App, type WebModule } from '../../src/App'
import { apiFetch, takeAccountAction } from '../../src/api'
import type { ActiveIdentity } from '../../src/IdentityGate'
import '../../src/styles.css'

function Probe({ identity }: { identity: ActiveIdentity }) {
  const [organization, setOrganization] = useState<string>()
  const [error, setError] = useState(false)
  useEffect(() => {
    const controller = new AbortController()
    apiFetch(`/api/foundation-probe/${identity.session.context.organizationId}`, { signal: controller.signal })
      .then(response => response.json()).then((data: { organizationId: string }) => {
        if (!controller.signal.aborted) setOrganization(data.organizationId)
      }).catch(() => { if (!controller.signal.aborted) setError(true) })
    return () => controller.abort()
  }, [identity.session.context.organizationId])
  return <><h1>Verificação sintética</h1>{error ? <p role="alert">Acesso negado. <button onClick={identity.refresh}>Atualizar acesso</button></p>
    : organization ? <p role="status" data-testid="probe-organization">{organization}</p> : <p role="status">Verificando organização…</p>}</>
}
const modules: WebModule[] = [{ id: 'foundation-probe', title: 'Verificação sintética', capability: 'foundation-probe.inspect', View: Probe }]
// Separate test build only. The production entry point has no reference to this module.
createRoot(document.getElementById('root')!).render(<App initialAccount={takeAccountAction()} modules={modules} />)
