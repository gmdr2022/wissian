import assert from 'node:assert/strict'
import { randomUUID } from 'node:crypto'
import { join } from 'node:path'
import { expect } from '@playwright/test'

export function registerCounterpartyTests({ test, fixture, login, choose, control }) {
  const base = fixture.productionUrl
  const path = `/api/organizations/${fixture.primary.organizationId}/counterparties`
  async function start(page) {
    await login(page, fixture.primary, base); await choose(page)
    await page.getByRole('navigation').getByRole('link', { name: 'Clientes e fornecedores' }).click()
    await expect(page.getByRole('heading', { name: 'Clientes e fornecedores' })).toBeVisible()
  }
  async function add(page, name) {
    await page.getByRole('button', { name: 'Novo cadastro', exact: true }).click()
    await expect(page.getByLabel('Nome', { exact: true })).toBeFocused()
    await page.getByLabel('Nome', { exact: true }).fill(name)
    await page.getByLabel('Referência interna (opcional)').fill('DEMO-001')
    await page.getByRole('button', { name: 'Salvar cadastro' }).click()
    await expect(page.getByRole('status').filter({ hasText: 'Cadastro salvo.' })).toBeVisible()
  }
  async function search(page, name, status = 'active') {
    await page.getByLabel('Buscar nome ou referência').fill(name)
    await page.getByLabel('Situação', { exact: true }).selectOption(status)
    await page.getByRole('button', { name: 'Buscar', exact: true }).click()
  }
  async function post(page, url, body) {
    const token = (await (await page.request.get(`${base}/api/identity/csrf`)).json()).token
    return page.request.post(`${base}${url}`, { headers: { 'X-CSRF-TOKEN': token }, data: body })
  }
  test('M2-UI-01 create, locate and reopen keep filters and data after reload', async page => {
    await start(page); await add(page, 'Cliente Aurora Sintético')
    await page.getByRole('button', { name: 'Voltar à lista' }).click()
    await search(page, 'Aurora')
    await page.getByRole('button', { name: 'Abrir Cliente Aurora Sintético' }).click()
    await expect(page.getByLabel('Nome', { exact: true })).toHaveValue('Cliente Aurora Sintético')
    await page.getByLabel('Relação', { exact: true }).selectOption('both')
    await page.getByRole('button', { name: 'Salvar cadastro' }).click()
    await expect(page.getByRole('status').filter({ hasText: 'Cadastro salvo.' })).toBeVisible()
    await page.getByRole('button', { name: 'Voltar à lista' }).click()
    await expect(page.getByLabel('Buscar nome ou referência')).toHaveValue('Aurora')
    await page.reload()
    await page.getByRole('button', { name: 'Abrir Cliente Aurora Sintético' }).click()
    await expect(page.getByLabel('Relação', { exact: true })).toHaveValue('both')
    await page.screenshot({ path: join(fixture.screenshots, 'counterparties-desktop.png'), fullPage: true })
  })
  test('M2-UI-02 lost response reuses identical command and creates once', async (page, context) => {
    await start(page)
    const requests = []; let lost = false
    await context.route(`${base}${path}`, async route => {
      if (route.request().method() !== 'POST') return route.continue()
      requests.push(route.request().postDataJSON())
      if (!lost) { lost = true; await route.fetch(); return route.abort('failed') }
      return route.continue()
    })
    await page.getByRole('button', { name: 'Novo cadastro', exact: true }).click()
    await page.getByLabel('Nome', { exact: true }).fill('Pedido Repetido Sintético')
    await page.getByRole('button', { name: 'Salvar cadastro' }).click()
    await expect(page.getByRole('alert')).toContainText('Não foi possível confirmar')
    await expect(page.getByLabel('Nome', { exact: true })).toBeDisabled()
    await page.getByRole('button', { name: 'Repetir confirmação' }).click()
    await expect(page.getByRole('status').filter({ hasText: 'Cadastro salvo.' })).toBeVisible()
    assert.equal(requests.length, 2); assert.deepEqual(requests[0], requests[1])
    const results = await (await page.request.get(`${base}${path}?q=Pedido%20Repetido`)).json()
    assert.equal(results.total, 1)
  })
  test('M2-UI-03 conflict preserves draft until explicit reload', async page => {
    await start(page); await add(page, 'Conflito Sintético')
    const results = await (await page.request.get(`${base}${path}?q=Conflito`)).json(), item = results.items[0]
    const session = await (await page.request.get(`${base}/api/identity/session`)).json()
    const saved = await post(page, `${path}/${item.id}`, { commandId: randomUUID(), expectedSessionVersion: session.version,
      expectedVersion: item.version, action: 'edit', value: { name: 'Alteração externa sintética', kind: item.kind, relation: item.relation, reference: item.reference } })
    assert.equal(saved.status(), 200)
    await page.getByLabel('Nome', { exact: true }).fill('Meu rascunho sintético')
    await page.getByRole('button', { name: 'Salvar cadastro' }).click()
    await expect(page.getByRole('alert')).toContainText('Suas alterações estão preservadas')
    await expect(page.getByLabel('Nome', { exact: true })).toHaveValue('Meu rascunho sintético')
    page.once('dialog', dialog => dialog.accept())
    await page.getByRole('button', { name: 'Recarregar cadastro' }).click()
    await expect(page.getByLabel('Nome', { exact: true })).toHaveValue('Alteração externa sintética')
  })
  test('M2-UI-04 archive preserves detail and restore returns to active search', async page => {
    await start(page); await add(page, 'Arquivo Sintético')
    page.once('dialog', dialog => dialog.accept())
    await page.getByRole('button', { name: 'Arquivar cadastro', exact: true }).click()
    await expect(page.getByRole('heading', { name: 'Cadastro arquivado' })).toBeVisible()
    await expect(page.getByLabel('Nome', { exact: true })).toHaveValue('Arquivo Sintético')
    await page.getByRole('button', { name: 'Voltar à lista' }).click()
    await search(page, 'Arquivo', 'archived')
    await page.getByRole('button', { name: 'Abrir Arquivo Sintético' }).click()
    page.once('dialog', dialog => dialog.accept())
    await page.getByRole('button', { name: 'Restaurar cadastro', exact: true }).click()
    await expect(page.getByRole('status').filter({ hasText: 'Cadastro restaurado.' })).toBeVisible()
    await page.getByRole('button', { name: 'Voltar à lista' }).click(); await search(page, 'Arquivo')
    await expect(page.getByRole('button', { name: 'Abrir Arquivo Sintético' })).toBeVisible()
  })
  test('M2-UI-05 revoked write clears edit and leaves read-only menu', async page => {
    await start(page); await page.getByRole('button', { name: 'Novo cadastro', exact: true }).click()
    await page.getByLabel('Nome', { exact: true }).fill('Não deve gravar')
    await control('disable-write')
    try {
      await page.getByRole('button', { name: 'Salvar cadastro' }).click()
      await expect(page.getByRole('heading', { name: 'Clientes e fornecedores' })).toBeVisible()
      await expect(page.getByRole('button', { name: 'Novo cadastro', exact: true })).toHaveCount(0)
      const list = await (await page.request.get(`${base}${path}?q=N%C3%A3o%20deve%20gravar`)).json(); assert.equal(list.total, 0)
    } finally { await control('enable-write') }
  })
  test('M2-UI-06 disabled module hides menu and direct URL is denied', async page => {
    await start(page); await control('disable-module')
    try {
      await page.reload()
      await expect(page.getByRole('heading', { name: 'Esta área não está disponível' })).toBeVisible()
      await expect(page.getByRole('navigation').getByRole('link', { name: 'Clientes e fornecedores' })).toHaveCount(0)
      assert.equal((await page.request.get(`${base}${path}`)).status(), 404)
    } finally { await control('enable-module') }
  })
  test('M2-UI-07 two organizations keep different records on narrow screens', async page => {
    await start(page)
    await page.getByRole('button', { name: /Trocar organização/ }).click(); await choose(page, 'Empresa Exemplo')
    await page.getByRole('navigation').getByRole('link', { name: 'Clientes e fornecedores' }).click()
    await expect(page.getByRole('status').filter({ hasText: '0 cadastros encontrados' })).toBeVisible()
    await add(page, 'Fornecedor Empresa Exemplo')
    await page.setViewportSize({ width: 390, height: 844 })
    await page.screenshot({ path: join(fixture.screenshots, 'counterparties-mobile.png'), fullPage: true })
    for (const width of [390, 320]) {
      await page.setViewportSize({ width, height: 844 })
      assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth))
    }
    await page.getByRole('button', { name: /Trocar organização/ }).click(); await choose(page)
    await page.getByRole('button', { name: 'Abrir menu' }).click()
    await page.getByRole('navigation').getByRole('link', { name: 'Clientes e fornecedores' }).click()
    await search(page, 'Fornecedor Empresa Exemplo')
    await expect(page.getByRole('status').filter({ hasText: '0 cadastros encontrados' })).toBeVisible()
  })
  test('M2-UI-08 unsaved edit can cancel internal navigation', async page => {
    await start(page); await page.getByRole('button', { name: 'Novo cadastro', exact: true }).click()
    await page.getByLabel('Nome', { exact: true }).fill('Rascunho preservado')
    page.once('dialog', dialog => dialog.dismiss())
    await page.getByRole('navigation').getByRole('link', { name: 'Início', exact: true }).click()
    await expect(page.getByLabel('Nome', { exact: true })).toHaveValue('Rascunho preservado')
    page.once('dialog', dialog => dialog.accept())
    await page.getByRole('navigation').getByRole('link', { name: 'Início', exact: true }).click()
    await expect(page.getByRole('heading', { name: 'Seu espaço de trabalho' })).toBeVisible()
  })
}
