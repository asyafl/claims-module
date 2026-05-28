import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { MOCK_USERS } from '../../core/models/auth.models';

@Component({
  selector: 'app-login',
  standalone: false,
  template: `
    <div class="login-wrapper">
      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>Claims Module</mat-card-title>
          <mat-card-subtitle>Sign in to continue</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <div class="quick-login">
            <p class="hint">Quick login (assessment):</p>
            <mat-form-field appearance="outline" style="width:100%">
              <mat-label>Select user</mat-label>
              <mat-select (selectionChange)="prefill($event.value)">
                <mat-option *ngFor="let u of mockUsers" [value]="u">{{ u.label }} ({{ u.role }})</mat-option>
              </mat-select>
            </mat-form-field>
          </div>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" style="width:100%">
              <mat-label>Email</mat-label>
              <input matInput formControlName="email" type="email" autocomplete="email">
            </mat-form-field>
            <mat-form-field appearance="outline" style="width:100%">
              <mat-label>Password</mat-label>
              <input matInput formControlName="password" type="password" autocomplete="current-password">
            </mat-form-field>
            <button mat-raised-button color="primary" type="submit"
              [disabled]="form.invalid || loading" style="width:100%">
              <mat-spinner *ngIf="loading" diameter="20" style="display:inline-block;margin-right:8px"></mat-spinner>
              Sign In
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .login-wrapper { display:flex; justify-content:center; align-items:center; min-height:100vh; background:#f5f7fa; }
    .login-card { width:400px; padding:24px; }
    .hint { font-size:12px; color:#666; margin:0 0 8px; }
    .quick-login { margin-bottom:16px; border-bottom:1px solid #eee; padding-bottom:16px; }
    mat-form-field { margin-bottom:12px; }
  `]
})
export class LoginComponent {
  form: any;
  loading = false;
  mockUsers = MOCK_USERS;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  prefill(user: typeof MOCK_USERS[0]) {
    this.form.patchValue({ email: user.email, password: user.password });
  }

  submit() {
    if (this.form.invalid) return;
    this.loading = true;
    const { email, password } = this.form.value;
    this.auth.login({ email: email!, password: password! }).subscribe({
      next: () => this.router.navigate(['/claims']),
      error: () => { this.loading = false; }
    });
  }
}
