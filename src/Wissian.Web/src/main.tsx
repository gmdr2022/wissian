import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from './App'
import { takeAccountAction } from './api'
import './styles.css'

const action = takeAccountAction()
createRoot(document.getElementById('root')!).render(<StrictMode><App initialAccount={action} /></StrictMode>)
