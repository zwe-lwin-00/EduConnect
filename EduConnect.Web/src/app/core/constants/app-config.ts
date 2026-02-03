import { environment } from '../../../environments/environment';

/**
 * Central app config. All runtime config comes from environment (set per build/deploy).
 * Add more keys here as needed; keep environment.ts / environment.prod.ts as the source of URLs and flags.
 */
export const appConfig = {
  production: environment.production,
  apiUrl: environment.apiUrl,
  appName: 'EduConnect',
} as const;
