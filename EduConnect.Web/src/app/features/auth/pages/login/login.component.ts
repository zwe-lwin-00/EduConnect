import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CardModule, InputTextModule, ButtonModule, MessageModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  private readonly redirectDelayMs = 1200;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private messageService: MessageService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]]
    });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.redirectByRole();
      return;
    }
    this.route.queryParams.subscribe(params => {
      if (params['unauthorized'] === '1') {
        this.errorMessage = 'You do not have access to that area. Please log in with an authorized account.';
      }
    });
  }

  private redirectByRole(): void {
    const user = this.authService.currentUser();
    if (!user) return;
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

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      this.authService.login(this.loginForm.value).subscribe({
        next: (response) => {
          if (response.user.mustChangePassword) {
            this.router.navigate(['/auth/change-password']);
          } else {
            const returnUrl = this.route.snapshot.queryParams['returnUrl'] as string | undefined;
            const role = response.user.role;
            const roleHome = role === 'Admin' || role === 1 ? '/admin' : role === 'Teacher' || role === 2 ? '/teacher' : role === 'Parent' || role === 3 ? '/parent' : '/dashboard';
            if (returnUrl && returnUrl.startsWith(roleHome) && returnUrl.length > roleHome.length) {
              this.messageService.add({
                severity: 'info',
                summary: 'Redirecting',
                detail: 'Taking you back to where you were…',
                life: this.redirectDelayMs
              });
              setTimeout(() => this.router.navigateByUrl(returnUrl), this.redirectDelayMs);
            } else {
              this.router.navigate([roleHome]);
            }
          }
        },
        error: (error) => {
          console.error('Login error:', error);
          console.error('Error details:', {
            status: error.status,
            statusText: error.statusText,
            error: error.error,
            message: error.message,
            url: error.url
          });
          
          // Extract error message from various possible formats
          let errorMsg = 'Login failed. Please try again.';
          
          if (error.error) {
            if (typeof error.error === 'string') {
              errorMsg = error.error;
            } else if (error.error.error) {
              errorMsg = error.error.error;
            } else if (error.error.message) {
              errorMsg = error.error.message;
            }
          } else if (error.message) {
            errorMsg = error.message;
          }
          
          // Add status code info for debugging
          if (error.status === 0) {
            errorMsg = 'Cannot connect to server. Please check if the API is running.';
          } else if (error.status === 401) {
            errorMsg = errorMsg || 'Invalid email or password.';
          } else if (error.status === 400) {
            errorMsg = errorMsg || 'Invalid request. Please check your credentials.';
          }
          
          this.errorMessage = errorMsg;
          this.isLoading = false;
        }
      });
    }
  }
}
