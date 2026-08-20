import fs from 'node:fs';
import { chromium } from 'playwright';

const baseUrl = process.env.MR_SAASY_WEB_URL || 'http://127.0.0.1:5099';
const results = [];

function assert(condition, message) {
  if (!condition) throw new Error(message);
}

async function expectText(page, selector, text) {
  const locator = page.locator(selector);
  await locator.waitFor({ state: 'visible' });
  const actual = (await locator.innerText()).trim();
  assert(actual.includes(text), `${selector} expected to contain ${JSON.stringify(text)} but was ${JSON.stringify(actual)}`);
}

async function runScenario(browser, name, viewport, isMobile) {
  const context = await browser.newContext({ viewport, isMobile });
  const page = await context.newPage();
  const consoleErrors = [];
  const pageErrors = [];
  page.on('console', (message) => {
    if (message.type() === 'error') consoleErrors.push(message.text());
  });
  page.on('pageerror', (error) => pageErrors.push(String(error)));

  await page.goto(`${baseUrl}/calendar`, { waitUntil: 'networkidle' });
  await page.locator('#calendar-page').waitFor({ state: 'visible' });

  await expectText(page, '#calendar-page', 'Kalender');
  await expectText(page, '#event-customer', 'Kundebesøg');
  await expectText(page, '#event-sick', 'Sygdom');
  await expectText(page, '#event-coffee', 'Kaffe');
  await expectText(page, '#event-date', 'Date');
  await expectText(page, '#event-other', 'Andet');

  for (const prefix of ['customer', 'sick', 'coffee', 'date']) {
    const card = page.locator(`#event-${prefix}`);
    assert(await card.locator(`button[id="event-${prefix}-primary"]`).count() === 1,
      `${prefix} must expose exactly one primary action button`);
    assert(await card.locator(`button[id="event-${prefix}-more"]`).count() === 1,
      `${prefix} must expose one compact Mere toggle`);
    assert(await page.locator(`#event-${prefix}-secondary`).count() === 0,
      `${prefix} secondary actions must be progressively hidden before Mere`);
  }

  assert(await page.locator('#event-other button[id="event-other-primary"]').count() === 1,
    'other event must expose exactly one primary action');

  await page.locator('#event-customer-primary').click();
  await expectText(page, '#action-status', 'Åbn sag');

  await page.locator('#event-customer-more').click();
  await page.locator('#event-customer-secondary').waitFor({ state: 'visible' });
  await expectText(page, '#event-customer-secondary', 'Send bekræftelse');
  await expectText(page, '#event-customer-secondary', 'Ring kunde');
  const customerSecondaryButtons = await page.locator('#event-customer-secondary button').count();
  assert(customerSecondaryButtons > 0 && customerSecondaryButtons <= 3,
    `customer secondary action count must be 1-3, got ${customerSecondaryButtons}`);

  await page.locator('#event-sick-more').click();
  await expectText(page, '#event-sick-secondary', 'Informer team');
  await expectText(page, '#event-sick-secondary', 'Flyt møder');

  await page.locator('#event-coffee-more').click();
  await expectText(page, '#event-coffee-secondary', 'Flyt tid');
  await expectText(page, '#event-coffee-secondary', 'Invitér');

  await page.locator('#event-date-more').click();
  await expectText(page, '#event-date-secondary', 'Reminder');
  await expectText(page, '#event-date-secondary', 'Marker privat');

  const dayButton = page.locator('#calendar-view-day');
  const weekButton = page.locator('#calendar-view-week');
  const dayBox = await dayButton.boundingBox();
  const weekBox = await weekButton.boundingBox();
  assert(dayBox && dayBox.height >= 44, `day switch touch target must be >=44px, got ${dayBox?.height}`);
  assert(weekBox && weekBox.height >= 44, `week switch touch target must be >=44px, got ${weekBox?.height}`);

  await dayButton.click();
  assert((await dayButton.getAttribute('aria-pressed')) === 'true', 'day view must expose active aria-pressed state');
  await weekButton.click();
  assert((await weekButton.getAttribute('aria-pressed')) === 'true', 'week view must expose active aria-pressed state');

  const overflow = await page.evaluate(() => ({
    scrollWidth: document.documentElement.scrollWidth,
    innerWidth: window.innerWidth,
  }));
  assert(overflow.scrollWidth <= overflow.innerWidth + 1,
    `horizontal overflow detected: scrollWidth=${overflow.scrollWidth}, innerWidth=${overflow.innerWidth}`);

  if (isMobile) {
    const nav = page.locator('#mobile-bottom-nav');
    await nav.waitFor({ state: 'visible' });
    const navBox = await nav.boundingBox();
    assert(navBox, 'mobile bottom navigation must have a bounding box');

    const lastAction = page.locator('#event-other-primary');
    await lastAction.scrollIntoViewIfNeeded();
    const actionBox = await lastAction.boundingBox();
    assert(actionBox, 'last primary action must have a bounding box');
    assert(actionBox.height >= 44, `mobile primary action must be >=44px, got ${actionBox.height}`);
    assert(actionBox.y + actionBox.height <= navBox.y - 2,
      `last action is covered by bottom navigation: actionBottom=${actionBox.y + actionBox.height}, navTop=${navBox.y}`);

    const moreBox = await page.locator('#event-date-more').boundingBox();
    assert(moreBox && moreBox.height >= 44, `mobile Mere target must be >=44px, got ${moreBox?.height}`);
  }

  await page.screenshot({
    path: `/tmp/wor737-${name}.png`,
    fullPage: true,
  });

  await page.waitForTimeout(150);
  assert(pageErrors.length === 0, `${name} page errors: ${pageErrors.join(' | ')}`);
  assert(consoleErrors.length === 0, `${name} console errors: ${consoleErrors.join(' | ')}`);

  results.push({
    name,
    viewport,
    pageErrors,
    consoleErrors,
    horizontalOverflow: overflow.scrollWidth > overflow.innerWidth + 1,
    passed: true,
  });
  await context.close();
}

const browser = await chromium.launch({ headless: true });
try {
  await runScenario(browser, 'desktop', { width: 1440, height: 900 }, false);
  await runScenario(browser, 'mobile', { width: 390, height: 844 }, true);
  fs.writeFileSync('/tmp/wor737-browser-evidence.json', JSON.stringify(results, null, 2));
  console.log(JSON.stringify(results, null, 2));
} finally {
  await browser.close();
}
