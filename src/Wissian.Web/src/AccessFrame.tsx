import { useEffect, useRef, type ReactNode } from 'react'
import { Appearance } from './Appearance'

export function AccessFrame({ title, description, message, children, tone = 'error' }: {
  title: string; description?: string; message?: string | null; children?: ReactNode; tone?: 'success' | 'error'
}) {
  const heading = useRef<HTMLHeadingElement>(null)
  useEffect(() => { heading.current?.focus() }, [title])
  return <div className="access-layout">
    <header className="access-header"><span className="wordmark">Wissian<span aria-hidden="true">.</span></span><Appearance /></header>
    <main className="access-main" id="main-content">
      <section className="access-card" aria-labelledby="access-title">
        <p className="eyebrow">Seu espaço de trabalho</p>
        <h1 id="access-title" tabIndex={-1} ref={heading}>{title}</h1>
        {description && <p className="description">{description}</p>}
        {message && <p className={`notice notice--${tone}`} id="access-message" role={tone === 'error' ? 'alert' : 'status'}>{message}</p>}
        {children}
      </section>
    </main>
    <footer className="access-footer">Wissian · Fultuma Tech</footer>
  </div>
}
