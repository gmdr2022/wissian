import { useSyncExternalStore } from 'react'

function subscribe(listener: () => void) {
  window.addEventListener('popstate', listener)
  return () => window.removeEventListener('popstate', listener)
}
export function usePathname() { return useSyncExternalStore(subscribe, () => window.location.pathname, () => '/') }
export function navigate(path: string) {
  if (path !== '/' && path !== '/access' && !/^\/m\/[a-z0-9-]+$/.test(path)) throw new Error('Rota inválida.')
  if (path !== window.location.pathname && !window.dispatchEvent(new Event('wissian:before-navigate', { cancelable: true }))) return
  window.history.pushState(null, '', path)
  window.dispatchEvent(new PopStateEvent('popstate'))
}
