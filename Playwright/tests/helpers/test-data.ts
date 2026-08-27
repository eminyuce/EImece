/**
 * Turkish test data generators — never use real personal or card data.
 * Identity number: 11111111111 passes the 11-digit format check used by checkout validation.
 */

/** Cheap product known to be in-stock on the demo dataset; env override for tunnel runs. */
export const DEFAULT_PRODUCT_URL =
  process.env.EIMECE_PRODUCT_URL ||
  '/p/mutfak/aquapure-cam-su-sisesi-750ml-106-2d0j7e0j4h1b';

/** Fallback product if the primary SKU is out of stock / 404 on a given seed. */
export const FALLBACK_PRODUCT_URLS = [
  '/p/mutfak/aquapure-cam-su-sisesi-750ml-106-2d0j7e0j4h1b',
  '/p/kosu--fitness/fitlife-yoga-mati-6mm-133-8c0j2d5i4h1b',
  '/p/oturma-grubu/nordline-mese-sehpa-90cm-130-4h4h2d5i4h1b',
];

export interface BuyerInfo {
  name: string;
  surname: string;
  email: string;
  gsmNumber: string;
  identityNumber: string;
  city: string; // display label preferred; helper also matches by value
  street: string;
  zipCode: string;
  country: string;
  description: string;
}

function randInt(min: number, max: number): number {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

export function uniqueEmail(prefix = 'e2e'): string {
  const stamp = Date.now().toString(36);
  const rnd = Math.random().toString(36).slice(2, 6);
  // When running against the live tunnel with a real mailbox, set EIMECE_TEST_EMAIL_PREFIX=outlook
  // and the helper yields eminyuce+e2e.*@outlook.com for manual order-mail inspection.
  if (prefix === 'outlook' || process.env.EIMECE_TEST_EMAIL_PREFIX === 'outlook') {
    return `eminyuce+e2e.${stamp}.${rnd}@outlook.com`;
  }
  return `${prefix}.${stamp}.${rnd}@eimece.test`;
}

/** Realistic Turkish guest checkout buyer — used for both membership and guest flows. */
export function makeBuyerInfo(overrides: Partial<BuyerInfo> = {}): BuyerInfo {
  const id = randInt(1000, 9999);
  return {
    name: 'E2E',
    surname: `Test ${id}`,
    email: uniqueEmail('e2e'),
    gsmNumber: `5${randInt(30, 59)}${randInt(1000000, 9999999)}`, // 10 digits starting with 5
    identityNumber: '11111111111',
    city: 'İstanbul',
    street: `Test Mah. E2E Sok. No:${randInt(1, 50)} D:${randInt(1, 10)}`,
    zipCode: '34000',
    country: 'Turkey',
    description: `E2E sipariş notu ${stamp()}`,
    ...overrides,
  };
}

function stamp(): string {
  return new Date().toISOString().slice(0, 19);
}

/** Password for newly registered membership users (meets default complexity). */
export const DEFAULT_TEST_PASSWORD = process.env.EIMECE_TEST_PASSWORD || 'Test123!';
