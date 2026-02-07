import { environment } from '../../../environments/environment';

/**
 * Central app config. All runtime config comes from environment (set per build/deploy).
 * Add more keys here as needed; keep environment.ts / environment.prod.ts as the source of URLs and flags.
 */
/** Myanmar timezone (UTC+6:30) for all date/time display and business "today". */
export const MYANMAR_TIMEZONE = '+0630';

export const appConfig = {
  production: environment.production,
  apiUrl: environment.apiUrl,
  appName: 'EduConnect',
  timezone: MYANMAR_TIMEZONE,
} as const;
