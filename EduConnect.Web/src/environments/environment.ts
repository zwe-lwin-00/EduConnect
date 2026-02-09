/**
 * Development config. apiUrl must match the API base URL (see API launchSettings / appsettings).
 * Replaced by environment.prod.ts for production builds.
 */
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5049/api',
  /** IANA timezone for date display (e.g. Asia/Yangon). */
  timeZone: 'Asia/Yangon',
  /** Offset for Angular date pipe (e.g. +0630). */
  timeZoneOffset: '+0630',
};
