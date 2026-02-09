import { Pipe, PipeTransform } from '@angular/core';
import { DatePipe } from '@angular/common';
import { appConfig } from '../../core/constants/app-config';

/**
 * Formats a date using the app-configured timezone (environment.timeZoneOffset).
 * Use instead of hardcoding the timezone in date pipe.
 */
@Pipe({ name: 'displayDate', standalone: true })
export class DisplayDatePipe implements PipeTransform {
  private readonly datePipe = new DatePipe('en-US');

  transform(value: Date | string | number | null | undefined, format?: string): string | null {
    if (value == null) return null;
    return this.datePipe.transform(value, format ?? 'short', appConfig.timezone);
  }
}
