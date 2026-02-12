import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ToolbarModule } from 'primeng/toolbar';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';
import { AdminService } from '../../../../core/services/admin.service';
import { ClassPriceDto, UpsertClassPriceRequest, ClassType } from '../../../../core/models/admin.model';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-admin-settings-class-prices',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ToolbarModule,
    ButtonModule,
    DialogModule,
    CardModule,
    DropdownModule,
    InputNumberModule
  ],
  templateUrl: './admin-settings-class-prices.component.html',
  styleUrl: './admin-settings.component.css'
})
export class AdminSettingsClassPricesComponent implements OnInit {
  classPrices: ClassPriceDto[] = [];
  loadingClassPrices = false;
  showClassPriceDialog = false;
  classPriceId: number | null = null;
  classPriceGradeLevel: number | null = null;
  classPriceClassType: number | null = null;
  classPricePerMonth: number | null = null;
  savingClassPrice = false;
  readonly gradeOptions = [
    { label: 'P1', value: 1 },
    { label: 'P2', value: 2 },
    { label: 'P3', value: 3 },
    { label: 'P4', value: 4 }
  ];
  readonly classTypeOptions = [
    { label: 'One-to-one', value: ClassType.OneToOne },
    { label: 'Group', value: ClassType.Group }
  ];

  constructor(
    private adminService: AdminService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadClassPrices();
  }

  loadClassPrices(): void {
    this.loadingClassPrices = true;
    this.adminService.getClassPrices().subscribe({
      next: (data) => {
        this.classPrices = data;
        this.loadingClassPrices = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Failed to load class prices' });
        this.loadingClassPrices = false;
      }
    });
  }

  openAddClassPrice(): void {
    this.classPriceId = null;
    this.classPriceGradeLevel = null;
    this.classPriceClassType = null;
    this.classPricePerMonth = null;
    this.showClassPriceDialog = true;
  }

  openEditClassPrice(cp: ClassPriceDto): void {
    this.classPriceId = cp.id;
    this.classPriceGradeLevel = cp.gradeLevel;
    this.classPriceClassType = cp.classType;
    this.classPricePerMonth = cp.pricePerMonth;
    this.showClassPriceDialog = true;
  }

  saveClassPrice(): void {
    if (this.classPriceGradeLevel == null || this.classPriceClassType == null) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Select grade and class type.' });
      return;
    }
    const price = this.classPricePerMonth ?? 0;
    if (price < 0) {
      this.messageService.add({ severity: 'warn', summary: 'Validation', detail: 'Price cannot be negative.' });
      return;
    }
    this.savingClassPrice = true;
    const request: UpsertClassPriceRequest = {
      gradeLevel: this.classPriceGradeLevel,
      classType: this.classPriceClassType,
      pricePerMonth: price
    };
    this.adminService.upsertClassPrice(request).subscribe({
      next: () => {
        this.savingClassPrice = false;
        this.showClassPriceDialog = false;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Class price saved.' });
        this.loadClassPrices();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Save failed' });
        this.savingClassPrice = false;
      }
    });
  }

  deleteClassPrice(cp: ClassPriceDto): void {
    this.confirmationService.confirm({
      message: `Remove price for ${cp.gradeLevelName} – ${cp.classTypeName}?`,
      header: 'Delete class price',
      icon: 'pi pi-trash',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.adminService.deleteClassPrice(cp.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Class price removed.' });
            this.loadClassPrices();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.error || 'Delete failed' })
        });
      }
    });
  }
}
