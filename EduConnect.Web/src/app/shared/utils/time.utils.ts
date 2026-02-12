/**
 * Format 24h time string (e.g. "09:00", "14:30") to 12h AM/PM (e.g. "9:00 AM", "2:30 PM").
 */
export function formatTime12h(value: string | null | undefined): string {
  if (value == null || typeof value !== 'string') return '';
  const trimmed = value.trim();
  if (!trimmed) return '';
  const [hStr, mStr] = trimmed.split(':');
  const h = parseInt(hStr ?? '0', 10);
  const m = parseInt(mStr ?? '0', 10);
  if (isNaN(h) || isNaN(m)) return trimmed;
  const period = h >= 12 ? 'PM' : 'AM';
  const h12 = h === 0 ? 12 : h > 12 ? h - 12 : h;
  const mm = String(m).padStart(2, '0');
  return `${h12}:${mm} ${period}`;
}
