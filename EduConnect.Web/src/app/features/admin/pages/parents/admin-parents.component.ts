import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DxDataGridModule, DxButtonModule, DxPopupModule } from 'devextreme-angular';
import { AdminService } from '../../../../core/services/admin.service';
import { Parent, CreateParentRequest, PagedResult } from '../../../../core/models/admin.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-admin-parents',
  standalone: true,
  imports: [CommonModule, DxDataGridModule, DxButtonModule, DxPopupModule, ReactiveFormsModule],
  templateUrl: './admin-parents.component.html',
  styleUrl: './admin-parents.component.css'
})
export class AdminParentsComponent implements OnInit {
  parents: Parent[] = [];
  showCreatePopup = false;
  createForm: FormGroup;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
  ) {
    this.createForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phoneNumber: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadParents();
  }

  loadParents(): void {
    this.adminService.getParents().subscribe({
      next: (data) => {
        this.parents = Array.isArray(data) ? data : (data as PagedResult<Parent>).items;
      },
      error: (err) => console.error('Error loading parents:', err)
    });
  }

  openCreatePopup(): void {
    this.showCreatePopup = true;
    this.createForm.reset();
  }

  closeCreatePopup(): void {
    this.showCreatePopup = false;
    this.createForm.reset();
  }

  onSubmitCreate(): void {
    if (this.createForm.valid) {
      const request: CreateParentRequest = this.createForm.value;
      this.adminService.createParent(request).subscribe({
        next: () => {
          alert('Parent created successfully!');
          this.closeCreatePopup();
          this.loadParents();
        },
        error: (err) => alert('Error: ' + (err.error?.error || err.message))
      });
    }
  }
}
