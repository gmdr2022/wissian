import assert from 'node:assert/strict'
import { createHmac } from 'node:crypto'
import { createInterface } from 'node:readline'
import { join } from 'node:path'
import { chromium, expect } from '@playwright/test'
import { registerCounterpartyTests } from './counterparties.mjs'

if (!process.env.WISSIAN_BROWSER_FIXTURE) throw new Error('Use Test-Postgres.ps1 -Suite Browser: laboratorio sintetico obrigatorio.')
const fixture = JSON.parse(process.env.WISSIAN_BROWSER_FIXTURE)
const origins = [fixture.baseUrl, fixture.productionUrl].map(value => {
  const url = new URL(value)
  assert.equal(url.protocol, 'https:'); assert.equal(url.hostname, '127.0.0.1'); assert.ok(url.port)
  return url.origin
})
fixture.baseUrl = origins[0]; fixture.productionUrl = origins[1]
const replies = createInterface({ input: process.stdin })
let reply
replies.on('line', line => { const resolve = reply; reply = undefined; resolve?.(JSON.parse(line)) })
function control(action) {
  assert.equal(reply, undefined)
  return new Promise(resolve => { reply = resolve; console.log(`WISSIAN_TEST_CONTROL ${action}`) })
}
function otp(key) {
  const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ234567'; let bits = ''
  for (const character of key.toUpperCase().replaceAll('=', '')) bits += alphabet.indexOf(character).toString(2).padStart(5, '0')
  const secret = Buffer.from(bits.match(/.{8}/g).map(byte => Number.parseInt(byte, 2)))
  const counter = Buffer.alloc(8); counter.writeBigUInt64BE(BigInt(Math.floor(Date.now() / 30_000)))
  const hash = createHmac('sha1', secret).update(counter).digest(), offset = hash.at(-1) & 15
  return ((hash.readUInt32BE(offset) & 0x7fffffff) % 1_000_000).toString().padStart(6, '0')
}
async function login(page, person = fixture.primary, origin = fixture.baseUrl) {
  await page.goto(origin)
  await expect(page.getByRole('heading', { name: 'Entre no Wissian' })).toBeVisible()
  await page.getByLabel('Usuário ou e-mail').fill(person.email)
  await page.getByLabel('Senha', { exact: true }).fill(person.password)
  await page.getByRole('button', { name: 'Entrar', exact: true }).click()
}
async function choose(page, name = 'Fultuma Exemplo') {
  await expect(page.getByRole('heading', { name: 'Onde você vai trabalhar?' })).toBeVisible()
  await page.getByRole('button', { name, exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Seu espaço de trabalho' })).toBeVisible()
}
async function post(context, path, body, csrf = true) {
  const token = csrf ? (await (await context.request.get(`${fixture.baseUrl}/api/identity/csrf`)).json()).token : null
  return context.request.post(`${fixture.baseUrl}${path}`, {
    headers: token ? { 'X-CSRF-TOKEN': token } : {}, data: body,
  })
}
async function probe(page) {
  await page.getByRole('navigation').getByRole('link', { name: 'Verificação sintética' }).click()
  await expect(page.getByTestId('probe-organization')).toBeVisible()
}

const tests = []
function test(name, run) { tests.push({ name, run }) }

test('M1-S4-01 production shell has no test module or speculative menu', async page => {
  const response = await page.goto(fixture.productionUrl)
  assert.equal(response.status(), 200)
  assert.match(response.headers()['content-security-policy'], /frame-ancestors 'none'/)
  assert.equal(response.headers()['referrer-policy'], 'no-referrer')
  assert.equal((await page.request.get(`${fixture.productionUrl}/api/not-a-route`)).status(), 404)
  assert.equal((await page.request.get(`${fixture.baseUrl}/private-fixture.json`)).status(), 404)
  await login(page, fixture.primary, fixture.productionUrl); await choose(page)
  await expect(page.getByRole('navigation').getByRole('link')).toHaveCount(1)
  await expect(page.getByText('Nenhuma atividade disponível para esta conta')).toBeVisible()
  await page.screenshot({ path: join(fixture.screenshots, 'workspace-desktop.png'), fullPage: true })
})
test('M1-S4-02 keyboard login, two contexts and confirmed logout', async page => {
  await page.goto(fixture.baseUrl)
  await expect(page.getByRole('heading', { name: 'Entre no Wissian' })).toBeFocused()
  await page.keyboard.press('Tab'); await expect(page.getByLabel('Usuário ou e-mail')).toBeFocused()
  await page.keyboard.type(fixture.primary.email); await page.keyboard.press('Tab')
  await expect(page.getByLabel('Senha', { exact: true })).toBeFocused()
  await page.keyboard.type(fixture.primary.password); await page.keyboard.press('Enter')
  await choose(page); await probe(page)
  await expect(page.getByTestId('probe-organization')).toHaveText(fixture.primary.organizationId)
  await page.getByRole('button', { name: /Trocar organização/ }).click()
  await expect(page.getByTestId('probe-organization')).toHaveCount(0)
  await choose(page, 'Empresa Exemplo'); await probe(page)
  await expect(page.getByTestId('probe-organization')).toHaveText(fixture.secondary.organizationId)
  await page.getByRole('button', { name: 'Sair', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Entre no Wissian' })).toBeVisible()
  assert.equal((await page.request.get(`${fixture.baseUrl}/api/identity/session`)).status(), 401)
  assert.equal(await page.evaluate(() => sessionStorage.length), 0)
})
test('M1-S4-03 no membership has an honest empty state', async page => {
  await login(page, fixture.noMembership)
  await expect(page.getByRole('heading', { name: 'Nenhuma organização disponível' })).toBeVisible()
  await expect(page.getByRole('button', { name: 'Sair', exact: true })).toBeEnabled()
})
test('M1-S4-04 invalid credentials retain identifier but clear password', async page => {
  await page.goto(fixture.baseUrl)
  await page.getByLabel('Usuário ou e-mail').fill('invalid@example.invalid')
  await page.getByLabel('Senha', { exact: true }).fill('Wrong!Synthetic123')
  await page.getByRole('button', { name: 'Entrar', exact: true }).click()
  await expect(page.getByRole('alert')).toContainText('Não foi possível entrar com esses dados')
  await expect(page.getByLabel('Usuário ou e-mail')).toHaveValue('invalid@example.invalid')
  await expect(page.getByLabel('Senha', { exact: true })).toHaveValue('')
  await expect(page.getByLabel('Senha', { exact: true })).toBeFocused()
  await page.evaluate(() => window.dispatchEvent(new Event('focus')))
  await expect(page.getByLabel('Usuário ou e-mail')).toHaveValue('invalid@example.invalid')
})
test('M1-S4-05 MFA survives focus and completes with an authenticator', async page => {
  await login(page, fixture.mfa)
  await expect(page.getByRole('heading', { name: 'Confirme que é você' })).toBeVisible()
  await page.evaluate(() => window.dispatchEvent(new Event('focus')))
  await expect(page.getByLabel('Código do autenticador')).toBeVisible()
  await page.getByLabel('Código do autenticador').fill(otp(fixture.mfaKey))
  await page.getByRole('button', { name: 'Confirmar código' }).click()
  await expect(page.getByRole('heading', { name: 'Onde você vai trabalhar?' })).toBeVisible()
})
test('M1-S4-06 cancel MFA invalidates challenge and requires CSRF', async (page, context) => {
  await login(page, fixture.mfa)
  await expect(page.getByLabel('Código do autenticador')).toBeVisible()
  assert.equal((await post(context, '/api/identity/cancel-mfa', undefined, false)).status(), 400)
  await page.getByRole('button', { name: 'Voltar para entrar' }).click()
  await expect(page.getByRole('heading', { name: 'Entre no Wissian' })).toBeVisible()
  assert.equal((await post(context, '/api/identity/mfa', { code: otp(fixture.mfaKey) })).status(), 401)
})
test('M1-S4-07 lost selection response retries same command without duplicate receipt', async page => {
  await login(page)
  const before = await control('receipt-count'); const commands = []
  await page.route('**/api/identity/context', async route => {
    commands.push(route.request().postDataJSON().commandId)
    const response = await route.fetch()
    if (commands.length === 1) await route.abort('failed')
    else await route.fulfill({ response })
  })
  await page.getByRole('button', { name: 'Fultuma Exemplo', exact: true }).click()
  await expect(page.getByRole('alert')).toContainText('Não foi possível confirmar a resposta')
  await choose(page)
  assert.equal(commands.length, 2); assert.equal(commands[0], commands[1])
  assert.equal(await control('receipt-count'), before + 1)
})
test('M1-S4-08 disabled module disappears; direct route and server request denied', async page => {
  await login(page); await choose(page); await probe(page)
  try {
    await control('disable-module')
    assert.equal((await page.request.get(`${fixture.baseUrl}/api/foundation-probe/${fixture.primary.organizationId}`)).status(), 404)
    await page.reload()
    await expect(page.getByRole('heading', { name: 'Esta área não está disponível' })).toBeVisible()
    await expect(page.getByRole('navigation').getByRole('link', { name: 'Verificação sintética' })).toHaveCount(0)
    await expect(page.getByTestId('probe-organization')).toHaveCount(0)
  } finally { await control('enable-module') }
})
test('M1-S4-09 revoked membership clears content on revalidation', async page => {
  await login(page); await choose(page); await probe(page)
  try {
    await control('revoke-membership')
    await page.evaluate(() => window.dispatchEvent(new Event('focus')))
    await expect(page.getByRole('heading', { name: 'Onde você vai trabalhar?' })).toBeVisible()
    await expect(page.getByRole('button', { name: 'Fultuma Exemplo', exact: true })).toHaveCount(0)
    await expect(page.getByTestId('probe-organization')).toHaveCount(0)
  } finally { await control('restore-membership') }
})
test('M1-S4-10 session revoked with open tab returns to login', async page => {
  await login(page); await choose(page); await probe(page)
  await control('revoke-sessions')
  await page.evaluate(() => window.dispatchEvent(new Event('focus')))
  await expect(page.getByRole('heading', { name: 'Entre no Wissian' })).toBeVisible()
  await expect(page.getByTestId('probe-organization')).toHaveCount(0)
})
test('M1-S4-11 context changes in a second tab revalidate the first', async (page, context) => {
  await login(page); await choose(page); await probe(page)
  const second = await context.newPage()
  await second.goto(fixture.baseUrl)
  await second.getByRole('button', { name: /Trocar organização/ }).click(); await choose(second, 'Empresa Exemplo')
  await expect(page.getByRole('button', { name: /Atual: Empresa Exemplo/ })).toBeVisible()
  await expect(page.getByTestId('probe-organization')).toHaveText(fixture.secondary.organizationId)
  await second.close()
})
test('M1-S4-12 unavailable service does not pretend successful logout', async page => {
  await page.route('**/api/identity/session', route => route.fulfill({ status: 503, body: '{}' }))
  await page.goto(fixture.baseUrl)
  await expect(page.getByRole('heading', { name: 'Vamos restabelecer seu acesso' })).toBeVisible()
  await page.unroute('**/api/identity/session')
  await page.getByRole('button', { name: 'Verificar meu acesso' }).click()
  await expect(page.getByRole('heading', { name: 'Entre no Wissian' })).toBeVisible()
})
test('M1-S4-13 recovery uses fragment, rejects weak password and consumes link once', async page => {
  const requests = []; page.on('request', request => requests.push(request.url()))
  await page.goto(fixture.baseUrl + fixture.recovery.relativeUrl)
  await expect(page.getByLabel('Nova senha', { exact: true })).toBeVisible()
  assert.equal(new URL(page.url()).hash, ''); assert.equal(new URL(page.url()).search, '')
  await page.getByLabel('Nova senha', { exact: true }).fill('aaaaaaaaaaaa')
  await page.getByLabel('Confirme a nova senha').fill('aaaaaaaaaaaa')
  await page.getByRole('button', { name: 'Salvar nova senha' }).click()
  await expect(page.getByRole('alert')).toContainText('A senha não atende')
  const password = 'New!SyntheticPassword973'
  await page.getByLabel('Nova senha', { exact: true }).fill(password)
  await page.getByLabel('Confirme a nova senha').fill(password)
  await page.getByRole('button', { name: 'Salvar nova senha' }).click()
  await expect(page.getByRole('heading', { name: 'Entre no Wissian' })).toBeVisible()
  await expect(page.getByRole('status')).toContainText('Senha atualizada')
  assert.ok(requests.every(url => !url.includes('token=') && !url.includes('userId=')))
  await login(page, { ...fixture.recovering, password })
  await expect(page.getByRole('heading', { name: 'Nenhuma organização disponível' })).toBeVisible()
  await page.getByRole('button', { name: 'Sair', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Entre no Wissian' })).toBeVisible()
  await page.goto(fixture.baseUrl + fixture.recovery.relativeUrl)
  await page.getByLabel('Nova senha', { exact: true }).fill(password)
  await page.getByLabel('Confirme a nova senha').fill(password)
  await page.getByRole('button', { name: 'Salvar nova senha' }).click()
  await expect(page.getByRole('alert')).toContainText('inválido, expirou ou já foi utilizado')
})
test('M1-S4-14 activation validates confirmation and grants no organization', async page => {
  await page.goto(fixture.baseUrl + fixture.activation.relativeUrl)
  await page.getByLabel('Nova senha', { exact: true }).fill('New!SyntheticPassword973')
  await page.getByLabel('Confirme a nova senha').fill('Different!Password973')
  await page.getByRole('button', { name: 'Salvar nova senha' }).click()
  await expect(page.getByRole('alert')).toContainText('As senhas não conferem')
  await expect(page.getByLabel('Confirme a nova senha')).toBeFocused()
  await page.getByLabel('Confirme a nova senha').fill('New!SyntheticPassword973')
  await page.getByRole('button', { name: 'Salvar nova senha' }).click()
  await expect(page.getByRole('status')).toContainText('Acesso ativado')
  await login(page, { ...fixture.activating, password: 'New!SyntheticPassword973' })
  await expect(page.getByRole('heading', { name: 'Nenhuma organização disponível' })).toBeVisible()
})
test('M1-S4-15 assisted help never claims to send an email', async page => {
  await page.goto(fixture.baseUrl)
  await page.getByRole('button', { name: 'Preciso de ajuda para entrar' }).click()
  await expect(page.getByRole('status')).toContainText('nenhum e-mail é enviado automaticamente')
  await page.getByRole('button', { name: 'Voltar para entrar' }).click()
  await expect(page.getByLabel('Usuário ou e-mail')).toBeVisible()
})
test('M1-S4-16 narrow layout, menu keyboard and appearance persist', async page => {
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto(fixture.baseUrl)
  await page.getByLabel('Aparência').selectOption('dark')
  await page.screenshot({ path: join(fixture.screenshots, 'login-mobile-dark.png'), fullPage: true })
  await login(page); await choose(page)
  await page.getByRole('button', { name: 'Abrir menu' }).focus(); await page.keyboard.press('Enter')
  await expect(page.getByRole('navigation')).toBeVisible()
  await page.getByLabel('Aparência').selectOption('light')
  await page.getByRole('navigation').getByRole('link', { name: 'Verificação sintética' }).click()
  await expect(page.getByRole('navigation')).toBeHidden()
  await expect(page.getByRole('main')).toBeFocused()
  await page.screenshot({ path: join(fixture.screenshots, 'workspace-mobile.png'), fullPage: true })
  await page.setViewportSize({ width: 320, height: 720 })
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth + 1))
  await page.reload()
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'light')
})

test('M1-S4-17 already authenticated response reconciles the real session', async (page, context) => {
  await page.goto(fixture.baseUrl)
  await expect(page.getByLabel('Usuário ou e-mail')).toBeVisible()
  assert.equal((await post(context, '/api/identity/login', { identifier: fixture.primary.email, password: fixture.primary.password })).status(), 200)
  await page.getByLabel('Usuário ou e-mail').fill(fixture.secondary.email)
  await page.getByLabel('Senha', { exact: true }).fill(fixture.secondary.password)
  await page.getByRole('button', { name: 'Entrar', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Onde você vai trabalhar?' })).toBeVisible()
  await expect(page.getByRole('alert')).toContainText('Há uma sessão aberta')
  await expect(page.getByRole('button', { name: 'Fultuma Exemplo', exact: true })).toBeVisible()
})

test('M1-S4-18 account action waits for logout and handles a revoked current session', async page => {
  await login(page); await choose(page)
  await page.goto(fixture.baseUrl + fixture.recovery.relativeUrl)
  await expect(page.getByRole('button', { name: 'Sair da conta atual' })).toBeVisible()
  await expect(page.getByLabel('Nova senha', { exact: true })).toHaveCount(0)
  await control('revoke-sessions')
  await page.getByRole('button', { name: 'Sair da conta atual' }).click()
  await expect(page.getByLabel('Nova senha', { exact: true })).toBeVisible()
})

if (fixture.administration) { tests.length = 0; registerCounterpartyTests({ test, fixture, login, choose, control }) }
const selectedTests = process.env.WISSIAN_BROWSER_CASE
  ? tests.filter(test => test.name.startsWith(process.env.WISSIAN_BROWSER_CASE)) : tests
assert.ok(selectedTests.length > 0, 'Caso de navegador desconhecido.')
const browser = await chromium.launch({ channel: 'msedge', headless: true })
let failures = 0
try {
  for (const { name, run } of selectedTests) {
    const context = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width: 1280, height: 900 } })
    await context.route('**/*', route => origins.includes(new URL(route.request().url()).origin) ? route.continue() : route.abort())
    const page = await context.newPage(), errors = []
    page.on('pageerror', error => errors.push(error.message))
    try { await run(page, context); assert.deepEqual(errors, []); console.log('PASS ' + name) }
    catch (error) {
      failures++
      let message = String(error.message)
      for (const person of [fixture.primary, fixture.secondary, fixture.mfa, fixture.recovering, fixture.activating, fixture.noMembership])
        message = message.replaceAll(person.password, '[synthetic-secret]')
      for (const action of [fixture.recovery, fixture.activation]) message = message.replaceAll(action.token, '[synthetic-token]')
      console.log(`FAIL ${name}: ${message}`)
      await page.screenshot({ path: join(fixture.screenshots, `failure-${name.slice(0, 8)}.png`), fullPage: true }).catch(() => {})
    } finally { await context.close() }
  }
} finally { await browser.close(); replies.close(); process.stdin.pause() }
console.log(`Browser: ${selectedTests.length - failures}/${selectedTests.length} PASS; ${failures} FAIL`)
process.exitCode = failures === 0 ? 0 : 1
