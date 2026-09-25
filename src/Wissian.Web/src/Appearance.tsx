import { useEffect, useState } from 'react'

type Theme = 'system' | 'light' | 'dark'
const key = 'wissian.appearance.theme'
export function Appearance() {
  const [theme, setTheme] = useState<Theme>(() => {
    try { const value = localStorage.getItem(key); return value === 'light' || value === 'dark' ? value : 'system' }
    catch { return 'system' }
  })
  useEffect(() => {
    document.documentElement.dataset.theme = theme
    // A blocked preference store must not prevent access; the selected theme still applies.
    try { localStorage.setItem(key, theme) } catch { /* Session-only preference. */ }
  }, [theme])
  return <label className="appearance">Aparência
    <select aria-label="Aparência" value={theme} onChange={event => setTheme(event.target.value as Theme)}>
      <option value="system">Do dispositivo</option><option value="light">Clara</option><option value="dark">Escura</option>
    </select>
  </label>
}
