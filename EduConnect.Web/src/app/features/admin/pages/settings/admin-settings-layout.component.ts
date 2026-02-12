import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-admin-settings-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin-settings-layout.component.html',
  styleUrl: './admin-settings-layout.component.css'
})
export class AdminSettingsLayoutComponent {}
