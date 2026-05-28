import { Component, OnInit, ViewChild, inject, ChangeDetectorRef } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Router } from '@angular/router';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ClaimsService } from '../../core/services/claims.service';
import { ReferenceService } from '../../core/services/reference.service';
import { ClaimListItem, ClaimStatus, STATUS_COLORS, STATUS_BG, CauseOfLossCode } from '../../core/models/claim.models';

@Component({
  selector: 'app-claims-list',
  standalone: false,
  templateUrl: './claims-list.component.html',
  styleUrls: ['./claims-list.component.scss']
})
export class ClaimsListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  displayedColumns = ['claimNumber', 'policyNumber', 'clientName', 'lossDate', 'causeOfLossCode', 'status', 'totalReserves'];
  claims: ClaimListItem[] = [];
  totalCount = 0;
  pageSize = 20;
  pageIndex = 0;
  loading = true;

  causeCodes: CauseOfLossCode[] = [];
  statusOptions = ['Draft','Open','UnderInvestigation','PendingPayment','Closed','Reopened','Withdrawn'];

  filters = this.fb.group({
    search: [''],
    status: [''],
    causeOfLossCode: [''],
    dateFrom: [null as Date | null],
    dateTo: [null as Date | null]
  });

  STATUS_COLORS = STATUS_COLORS;
  STATUS_BG = STATUS_BG;

  constructor(
    private claimsService: ClaimsService,
    private referenceService: ReferenceService,
    private router: Router
  ) {}

  ngOnInit() {
    this.referenceService.getCauseOfLossCodes().subscribe(c => { this.causeCodes = c; this.cdr.markForCheck(); });
    // Defer initial load to avoid NG0100
    Promise.resolve().then(() => this.loadClaims());

    this.filters.valueChanges.pipe(debounceTime(400), distinctUntilChanged()).subscribe(() => {
      this.pageIndex = 0;
      this.loadClaims();
    });
  }

  loadClaims() {
    this.loading = true;
    const f = this.filters.value;
    this.claimsService.listClaims({
      search: f.search || undefined,
      status: f.status || undefined,
      causeOfLossCode: f.causeOfLossCode || undefined,
      dateFrom: f.dateFrom ? new Date(f.dateFrom).toISOString() : undefined,
      dateTo: f.dateTo ? new Date(f.dateTo).toISOString() : undefined,
      page: this.pageIndex + 1,
      pageSize: this.pageSize
    }).subscribe({
      next: res => {
        this.claims = res.items;
        this.totalCount = res.totalCount;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => { this.loading = false; this.cdr.markForCheck(); }
    });
  }

  onPage(e: PageEvent) {
    this.pageIndex = e.pageIndex;
    this.pageSize = e.pageSize;
    this.loadClaims();
  }

  openClaim(claim: ClaimListItem) { this.router.navigate(['/claims', claim.id]); }
  newClaim() { this.router.navigate(['/claims/new']); }
  clearFilters() { this.filters.reset(); }

  statusColor(status: ClaimStatus) { return STATUS_COLORS[status] ?? '#666'; }
  statusBg(status: ClaimStatus) { return STATUS_BG[status] ?? '#f5f5f5'; }

  formatCurrency(val: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 0 }).format(val);
  }
}
