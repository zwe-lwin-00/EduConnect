import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { BreadcrumbModule } from 'primeng/breadcrumb';
import { MenuItem } from 'primeng/api';

/**
 * Renders a PrimeNG breadcrumb from the current URL.
 * Pass basePath (e.g. 'admin'), homeLabel, homeRouterLink, and segmentLabels (path segment -> label).
 */
@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, RouterModule, BreadcrumbModule],
  template: `
    <p-breadcrumb
      [model]="items"
      [home]="home"
      styleClass="app-breadcrumb"
    />
  `,
  styles: [`
    :host ::ng-deep .app-breadcrumb {
      padding: 0.5rem 0;
      background: transparent;
      border: none;
    }
  `]
})
export class AppBreadcrumbComponent implements OnInit, OnDestroy {
  @Input() basePath = '';
  @Input() homeLabel = 'Dashboard';
  @Input() homeRouterLink = '/';
  @Input() segmentLabels: Record<string, string> = {};

  items: MenuItem[] = [];
  home: MenuItem = { icon: 'pi pi-home', routerLink: this.homeRouterLink };

  private sub?: Subscription;

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.home = { icon: 'pi pi-home', routerLink: this.homeRouterLink };
    this.buildFromUrl(this.router.url);
    this.sub = this.router.events.subscribe(() => this.buildFromUrl(this.router.url));
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private buildFromUrl(url: string): void {
    const segments = url.split('/').filter(Boolean);
    if (segments[0] !== this.basePath) {
      this.items = [];
      return;
    }
    const afterBase = segments.slice(1);
    this.items = [];
    for (let i = 0; i < afterBase.length; i++) {
      if (/^\d+$/.test(afterBase[i])) continue; // Skip numeric ids (e.g. /parent/student/123)
      const label = this.segmentLabels[afterBase[i]] ?? this.formatSegment(afterBase[i]);
      const nextIsNumeric = i + 1 < afterBase.length && /^\d+$/.test(afterBase[i + 1]);
      const isLast = i === afterBase.length - 1 || nextIsNumeric;
      const pathSoFar = '/' + [this.basePath, ...afterBase.slice(0, i + 1)].join('/');
      this.items.push({ label, ...(isLast ? {} : { routerLink: pathSoFar }) });
    }
  }

  private formatSegment(seg: string): string {
    return seg
      .split('-')
      .map(s => s.charAt(0).toUpperCase() + s.slice(1).toLowerCase())
      .join(' ');
  }
}
