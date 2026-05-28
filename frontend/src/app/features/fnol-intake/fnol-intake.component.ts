import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormArray, Validators, AbstractControl } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import { ClaimsService } from '../../core/services/claims.service';
import { ReferenceService } from '../../core/services/reference.service';
import { CauseOfLossCode, Policy } from '../../core/models/claim.models';

@Component({
  selector: 'app-fnol-intake',
  standalone: false,
  templateUrl: './fnol-intake.component.html',
  styleUrls: ['./fnol-intake.component.scss']
})
export class FnolIntakeComponent implements OnInit {
  private fb = inject(FormBuilder);
  loading = false;
  submitting = false;
  causeCodes: CauseOfLossCode[] = [];
  policyResults: Policy[] = [];
  selectedPolicy: Policy | null = null;
  today = new Date();

  // Step 1 - Policy & Loss
  step1 = this.fb.group({
    policySearch: [''],
    policyId: [null as string | null],
    unknownPolicy: [false],
    lossDate: [null as Date | null, Validators.required],
    lossDescription: ['', [Validators.required, Validators.minLength(20)]],
    lossLocation: [''],
    causeOfLossCode: ['', Validators.required],
    estimatedLossAmount: [null as number | null],
    policeReportNumber: ['']
  });

  // Step 2 - Parties & Risk Objects
  step2 = this.fb.group({
    parties: this.fb.array([]),
    riskObjects: this.fb.array([])
  });

  // Step 3 - Reserve & Review
  step3 = this.fb.group({
    addReserve: [false],
    component: ['Indemnity'],
    amount: [null as number | null],
    changeReason: ['']
  });

  constructor(
    private claimsService: ClaimsService,
    private referenceService: ReferenceService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this.referenceService.getCauseOfLossCodes().subscribe(c => this.causeCodes = c);

    // Policy typeahead
    this.step1.get('policySearch')!.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(q => q && q.length >= 2 ? this.referenceService.searchPolicies(q) : of([]))
    ).subscribe(policies => this.policyResults = policies);

    // Add default party (claimant)
    this.addParty();
    // Add default risk object
    this.addRiskObject();
  }

  // Parties
  get parties() { return this.step2.get('parties') as FormArray; }

  addParty() {
    this.parties.push(this.fb.group({
      partyRole: ['Claimant', Validators.required],
      partyType: ['Person', Validators.required],
      firstName: [''],
      lastName: [''],
      companyName: [''],
      email: [''],
      phone: ['']
    }));
  }

  removeParty(i: number) { this.parties.removeAt(i); }

  // Risk Objects
  get riskObjects() { return this.step2.get('riskObjects') as FormArray; }

  addRiskObject() {
    this.riskObjects.push(this.fb.group({
      assetType: ['Vehicle', Validators.required],
      assetDescription: ['', Validators.required],
      damageDescription: [''],
      isPrimary: [this.riskObjects.length === 0],
      assetReference: ['']
    }));
  }

  removeRiskObject(i: number) { this.riskObjects.removeAt(i); }

  // Policy selection
  selectPolicy(policy: Policy) {
    this.selectedPolicy = policy;
    this.step1.patchValue({ policyId: policy.id, policySearch: policy.policyNumber });
    this.policyResults = [];
  }

  clearPolicy() {
    this.selectedPolicy = null;
    this.step1.patchValue({ policyId: null, policySearch: '' });
  }

  isPolicyInForce(): boolean | null {
    if (!this.selectedPolicy || !this.step1.value.lossDate) return null;
    const ld = new Date(this.step1.value.lossDate);
    const eff = new Date(this.selectedPolicy.effectiveDate);
    const exp = new Date(this.selectedPolicy.expirationDate);
    return ld >= eff && ld <= exp;
  }

  // Reserve authority
  reserveAuthority(): string {
    const amt = this.step3.value.amount ?? 0;
    if (amt <= 10000) return '✓ Auto-approved (≤ $10,000)';
    if (amt <= 100000) return '⚠ Supervisor approval required';
    return '⚠ Manager approval required';
  }

  reserveAuthorityClass(): string {
    const amt = this.step3.value.amount ?? 0;
    if (amt <= 10000) return 'auth-auto';
    if (amt <= 100000) return 'auth-supervisor';
    return 'auth-manager';
  }

  hasClaimant(): boolean {
    return this.parties.controls.some(c => c.value.partyRole === 'Claimant');
  }

  // Submission
  submit() {
    this.submitting = true;
    const s1 = this.step1.value;
    const s3 = this.step3.value;

    const payload: any = {
      policyId: s1.policyId || null,
      lossDate: s1.lossDate ? new Date(s1.lossDate).toISOString() : null,
      lossDescription: s1.lossDescription,
      lossLocation: s1.lossLocation || null,
      causeOfLossCode: s1.causeOfLossCode,
      estimatedLossAmount: s1.estimatedLossAmount || null,
      policeReportNumber: s1.policeReportNumber || null,
      parties: this.parties.value.map((p: any) => ({
        partyRole: p.partyRole, partyType: p.partyType,
        firstName: p.firstName || null, lastName: p.lastName || null,
        companyName: p.companyName || null, email: p.email || null, phone: p.phone || null
      })),
      riskObjects: this.riskObjects.value.map((r: any) => ({
        assetType: r.assetType, assetDescription: r.assetDescription,
        damageDescription: r.damageDescription || null, isPrimary: r.isPrimary,
        assetReference: r.assetReference || null
      })),
      initialReserve: (s3.addReserve && s3.amount)
        ? { component: s3.component, amount: s3.amount, changeReason: s3.changeReason || 'Initial reserve at FNOL' }
        : null
    };

    this.claimsService.createClaim(payload).subscribe({
      next: result => {
        this.submitting = false;
        this.snackBar.open(`✅ Claim ${result.claimNumber} created!`, 'View', { duration: 6000 })
          .onAction().subscribe(() => this.router.navigate(['/claims', result.claimId]));
        this.router.navigate(['/claims', result.claimId]);
      },
      error: () => { this.submitting = false; }
    });
  }

  partyRoles = ['Claimant', 'Insured', 'ThirdParty', 'Witness', 'Attorney'];
  partyTypes = ['Person', 'Company'];
  assetTypes = ['Vehicle', 'Property', 'Person', 'Equipment', 'Other'];
  reserveComponents = ['Indemnity', 'Expense', 'ALAE', 'SubrogationRecoverable'];
}
