export interface ClaimListItem {
  id: string;
  claimNumber: string;
  policyNumber: string | null;
  clientName: string | null;
  lossDate: string | null;
  causeOfLossCode: string | null;
  status: ClaimStatus;
  totalReserves: number;
  reportedDate: string;
  assignedHandlerId: string | null;
}

export interface ClaimDetail {
  id: string;
  claimNumber: string;
  policyId: string | null;
  policyNumber: string | null;
  clientName: string | null;
  status: ClaimStatus;
  severity: string | null;
  reportedDate: string;
  assignedHandlerId: string | null;
  closedAt: string | null;
  closureReason: string | null;
  notes: string | null;
  managerOverrideFlag: boolean;
  lossEvent: LossEvent | null;
  parties: ClaimParty[];
  riskObjects: ClaimRiskObject[];
  reserveComponents: ReserveComponent[];
  totalReserves: number;
}

export interface LossEvent {
  id: string;
  lossDate: string;
  lossDescription: string;
  lossLocation: string | null;
  causeOfLossCode: string;
  estimatedLossAmount: number | null;
  policeReportNumber: string | null;
}

export interface ClaimParty {
  id: string;
  partyRole: string;
  partyType: string;
  firstName: string | null;
  lastName: string | null;
  companyName: string | null;
  email: string | null;
  phone: string | null;
  notes: string | null;
  isActive: boolean;
}

export interface ClaimRiskObject {
  id: string;
  assetType: string;
  assetDescription: string;
  damageDescription: string | null;
  isPrimary: boolean;
  assetReference: string | null;
}

export interface ReserveComponent {
  id: string;
  component: string;
  currentAmount: number;
  status: string;
  notes: string | null;
  history: ReserveTransaction[];
}

export interface ReserveTransaction {
  id: string;
  transactionType: string;
  amount: number;
  previousBalance: number;
  newBalance: number;
  approvalStatus: ReserveApprovalStatus;
  postingStatus: string;
  changeReason: string;
  rejectionReason: string | null;
  submittedByUserId: string | null;
  approvedByUserId: string | null;
  approvedAt: string | null;
  rejectedByUserId: string | null;
  rejectedAt: string | null;
  idempotencyKey: string;
  changeSequence: number;
  createdAt: string;
}

export interface AuditLogEntry {
  id: string;
  eventType: string;
  description: string;
  oldValue: string | null;
  newValue: string | null;
  relatedEntityId: string | null;
  relatedEntityType: string | null;
  createdByUserId: string | null;
  createdAt: string;
}

export interface ClaimDocument {
  id: string;
  documentType: string;
  documentName: string;
  contentType: string;
  fileSizeBytes: number;
  uploadedAt: string;
  uploadedByUserId: string | null;
  notes: string | null;
  downloadUrl: string;
}

export interface Policy {
  id: string;
  policyNumber: string;
  clientName: string;
  effectiveDate: string;
  expirationDate: string;
  status: string;
  coverageTypes: string[];
}

export interface CauseOfLossCode {
  id: string;
  code: string;
  name: string;
  perilCategory: string;
  isActive: boolean;
  sortOrder: number;
}

export interface ClaimStatusInfo {
  status: string;
  validNextStatuses: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateClaimResult {
  claimId: string;
  claimNumber: string;
  warnings: string[];
}

export type ClaimStatus =
  | 'Draft' | 'Open' | 'UnderInvestigation'
  | 'PendingPayment' | 'Closed' | 'Reopened' | 'Withdrawn';

export type ReserveApprovalStatus =
  | 'AutoApproved' | 'PendingApproval' | 'Approved' | 'Rejected' | 'Cancelled';

export const STATUS_COLORS: Record<ClaimStatus, string> = {
  Draft: '#9e9e9e',
  Open: '#1976d2',
  UnderInvestigation: '#f57c00',
  PendingPayment: '#7b1fa2',
  Closed: '#388e3c',
  Reopened: '#f9a825',
  Withdrawn: '#616161'
};

export const STATUS_BG: Record<ClaimStatus, string> = {
  Draft: '#f5f5f5',
  Open: '#e3f2fd',
  UnderInvestigation: '#fff3e0',
  PendingPayment: '#f3e5f5',
  Closed: '#e8f5e9',
  Reopened: '#fffde7',
  Withdrawn: '#eeeeee'
};
