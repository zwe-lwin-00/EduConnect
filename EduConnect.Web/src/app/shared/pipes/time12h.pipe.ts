import { Pipe, PipeTransform } from '@angular/core';
import { formatTime12h } from '../utils/time.utils';

@Pipe({ name: 'time12h', standalone: true })
export class Time12hPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return formatTime12h(value);
  }
}
