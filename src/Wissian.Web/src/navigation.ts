import { useSyncExternalStore } from 'react'

function subscribe(listener: () => void) {
  window.addEventListener('popstate', listener)
  return () => window.removeEventListener('popstate', listener)
}
export function usePathname() { return useSyncExternalStore(subscribe, () => window.location.pathname, () => '/') }
export function navigate(path: string) {
  if (path !== '/' && path !== '/access' && !/^\/m\/[a-z0-9-]+$/.test(path)) throw new Error('Rota inválida.')
  window.history.pushState(null, '', path)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
