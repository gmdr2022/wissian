import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from './App'
import { takeAccountAction } from './api'
import './styles.css'
import { counterpartiesModule } from './modules/Counterparties'

const action = takeAccountAction()
createRoot(document.getElementById('root')!).render(<StrictMode><App initialAccount={action} modules={[counterpartiesModule]} /></StrictMode>)
