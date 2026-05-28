import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: 'claims', pathMatch: 'full' },
  {
    path: 'login',
    loadChildren: () => import('./features/login/login.module').then(m => m.LoginModule)
  },
  {
    path: 'claims',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/claims-list/claims-list.module').then(m => m.ClaimsListModule)
  },
  {
    path: 'claims/new',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/fnol-intake/fnol-intake.module').then(m => m.FnolIntakeModule)
  },
  {
    path: 'claims/:id',
    canActivate: [AuthGuard],
    loadChildren: () => import('./features/claim-detail/claim-detail.module').then(m => m.ClaimDetailModule)
  },
  { path: '**', redirectTo: 'claims' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
