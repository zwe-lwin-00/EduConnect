import { environment } from '../../../environments/environment';

/**
 * Central app config. All runtime config comes from environment (set per build/deploy).
 * Add more keys here as needed; keep environment.ts / environment.prod.ts as the source of URLs and flags.
 */
export const appConfig = {
  production: environment.production,
  apiUrl: environment.apiUrl,
  appName: 'EduConnect',
  /** Timezone for date pipe (e.g. +0630). From environment.timeZoneOffset. */
  timezone: (environment as { timeZoneOffset?: string }).timeZoneOffset ?? '+0630',
  /** IANA timezone for toLocaleDateString etc. From environment.timeZone. */
  timeZone: (environment as { timeZone?: string }).timeZone ?? 'Asia/Yangon',
} as const;
