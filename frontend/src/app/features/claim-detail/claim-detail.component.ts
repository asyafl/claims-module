import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ClaimsService } from '../../core/services/claims.service';
import { ReferenceService } from '../../core/services/reference.service';
import { AuthService } from '../../core/services/auth.service';
import {
  ClaimDetail, AuditLogEntry, ClaimDocument, ReserveComponent,
  ReserveTransaction, ClaimStatusInfo, STATUS_COLORS, STATUS_BG
} from '../../core/models/claim.models';

@Component({
  selector: 'app-claim-detail',
  standalone: false,
  templateUrl: './claim-detail.component.html',
  styleUrls: ['./claim-detail.component.scss']
})
export class ClaimDetailComponent implements OnInit {
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);
  claim?: ClaimDetail;
  auditLog: AuditLogEntry[] = [];
  documents: ClaimDocument[] = [];
  statuses: ClaimStatusInfo[] = [];
  loading = true;
  claimId = '';

  // Reserve form
  showReserveForm = false;
  reserveForm = this.fb.group({
    component: ['Indemnity', Validators.required],
    amount: [null as number | null, [Validators.required, Validators.min(0.01)]],
    changeReason: ['', Validators.required]
  });
  reserveSubmitting = false;

  // Status transition
  transitionReason = '';

  STATUS_COLORS = STATUS_COLORS;
  STATUS_BG = STATUS_BG;
  reserveComponents = ['Indemnity', 'Expense', 'ALAE', 'SubrogationRecoverable'];
  reserveColumns = ['component', 'currentAmount', 'pendingAmount', 'actions'];
  historyColumns = ['createdAt', 'transactionType', 'amount', 'approvalStatus', 'postingStatus', 'submittedBy', 'actions'];
  auditColumns = ['createdAt', 'eventType', 'description'];
  docColumns = ['documentName', 'documentType', 'uploadedAt', 'fileSize', 'actions'];

  constructor(
    private route: ActivatedRoute,
    private claimsService: ClaimsService,
    private referenceService: ReferenceService,
    public authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.claimId = this.route.snapshot.paramMap.get('id')!;
    this.referenceService.getClaimStatuses().subscribe(s => { this.statuses = s; this.cdr.markForCheck(); });
    Promise.resolve().then(() => this.loadClaim());
  }

  loadClaim() {
    this.loading = true;
    this.claimsService.getClaim(this.claimId).subscribe({
      next: c => { this.claim = c; this.loading = false; this.cdr.markForCheck(); },
      error: () => { this.loading = false; this.cdr.markForCheck(); }
    });
    this.claimsService.getAuditLog(this.claimId).subscribe(r => { this.auditLog = r.items; this.cdr.markForCheck(); });
    this.claimsService.getDocuments(this.claimId).subscribe(d => { this.documents = d; this.cdr.markForCheck(); });
  }

  validNextStatuses(): string[] {
    if (!this.claim) return [];
    return this.statuses.find(s => s.status === this.claim!.status)?.validNextStatuses ?? [];
  }

  transitionTo(status: string) {
    const reason = status === 'Reopened' ? prompt('Enter reopen reason:') : undefined;
    if (status === 'Reopened' && !reason) return;
    this.claimsService.transitionStatus(this.claimId, status, reason ?? undefined).subscribe({
      next: () => {
        this.snackBar.open(`Status changed to ${status}`, '', { duration: 3000 });
        this.loadClaim();
      }
    });
  }

  // Reserves
  reserveAuthority(): string {
    const amt = this.reserveForm.value.amount ?? 0;
    if (amt <= 10000) return '✓ Auto-approved';
    if (amt <= 100000) return '⚠ Requires Supervisor';
    return '⚠ Requires Manager';
  }

  submitReserve() {
    if (this.reserveForm.invalid) return;
    this.reserveSubmitting = true;
    const v = this.reserveForm.value;
    this.claimsService.createReserve(this.claimId, {
      component: v.component, amount: v.amount, changeReason: v.changeReason
    }).subscribe({
      next: () => {
        this.reserveSubmitting = false;
        this.showReserveForm = false;
        this.reserveForm.reset({ component: 'Indemnity' });
        this.snackBar.open('Reserve created', '', { duration: 3000 });
        this.loadClaim();
      },
      error: () => { this.reserveSubmitting = false; }
    });
  }

  approveReserve(txn: ReserveTransaction) {
    this.claimsService.approveReserve(this.claimId, txn.id).subscribe({
      next: () => { this.snackBar.open('Reserve approved', '', { duration: 3000 }); this.loadClaim(); }
    });
  }

  rejectReserve(txn: ReserveTransaction) {
    const reason = prompt('Enter rejection reason:');
    if (!reason) return;
    this.claimsService.rejectReserve(this.claimId, txn.id, reason).subscribe({
      next: () => { this.snackBar.open('Reserve rejected', '', { duration: 3000 }); this.loadClaim(); }
    });
  }

  retractReserve(txn: ReserveTransaction) {
    this.claimsService.retractReserve(this.claimId, txn.id).subscribe({
      next: () => { this.snackBar.open('Reserve retracted', '', { duration: 3000 }); this.loadClaim(); }
    });
  }

  canApproveReserve(txn: ReserveTransaction): boolean {
    return txn.approvalStatus === 'PendingApproval' &&
      this.authService.hasRole('supervisor', 'manager') &&
      txn.submittedByUserId !== this.authService.user()?.userId;
  }

  canRetract(txn: ReserveTransaction): boolean {
    return txn.approvalStatus === 'PendingApproval' &&
      txn.submittedByUserId === this.authService.user()?.userId;
  }

  pendingAmount(comp: ReserveComponent): number {
    return comp.history
      .filter(h => h.approvalStatus === 'PendingApproval')
      .reduce((s, h) => s + h.amount, 0);
  }

  allHistory(): ReserveTransaction[] {
    return (this.claim?.reserveComponents ?? [])
      .flatMap(c => c.history)
      .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  }

  // Documents
  uploadDoc(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const type = prompt('Document type (e.g. PoliceReport, MedicalReport, Invoice, Other):') ?? 'Other';
    this.claimsService.uploadDocument(this.claimId, file, type).subscribe({
      next: () => { this.snackBar.open('Document uploaded', '', { duration: 3000 }); this.loadClaim(); }
    });
  }

  downloadDoc(doc: ClaimDocument) { window.open(doc.downloadUrl, '_blank'); }

  formatSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1048576) return `${(bytes/1024).toFixed(1)} KB`;
    return `${(bytes/1048576).toFixed(1)} MB`;
  }

  statusColor(s: string) { return (STATUS_COLORS as any)[s] ?? '#666'; }
  statusBg(s: string) { return (STATUS_BG as any)[s] ?? '#f5f5f5'; }
}
