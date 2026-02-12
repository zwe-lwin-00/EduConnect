import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { MessageModule } from 'primeng/message';
import { AuthService } from '../../../../core/services/auth.service';

function confirmPasswordValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const group = control.parent;
    if (!group) return null;
    const newPassword = group.get('newPassword')?.value;
    const confirm = control.value;
    return newPassword === confirm ? null : { confirmPassword: true };
  };
}

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, InputTextModule, ButtonModule, CardModule, MessageModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.css'
})
export class ChangePasswordComponent implements OnInit {
  form: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.form = this.fb.group({
      currentPassword: ['', [Validators.required, Validators.minLength(8)]],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmNewPassword: ['', [Validators.required, confirmPasswordValidator()]]
    });
  }

  ngOnInit(): void {
    this.form.get('newPassword')?.valueChanges.subscribe(() => {
      this.form.get('confirmNewPassword')?.updateValueAndValidity();
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.isLoading = true;
    this.errorMessage = '';
    const { currentPassword, newPassword } = this.form.value;

    this.authService.changePassword(currentPassword, newPassword).subscribe({
      next: () => {
        this.authService.setMustChangePasswordFalse();
        this.navigateByRole();
      },
      error: (err) => {
        this.errorMessage = err.error?.error || err.message || 'Failed to change password.';
        this.isLoading = false;
      }
    });
  }

  private navigateByRole(): void {
    const user = this.authService.currentUser();
    if (!user) {
      this.router.navigate(['/auth/login']);
      return;
    }
    const role = user.role;
    if (role === 'Admin' || role === 1) {
      this.router.navigate(['/admin']);
    } else if (role === 'Teacher' || role === 2) {
      this.router.navigate(['/teacher']);
    } else if (role === 'Parent' || role === 3) {
      this.router.navigate(['/parent']);
    } else {
      this.router.navigate(['/dashboard']);
    }
  }
}
