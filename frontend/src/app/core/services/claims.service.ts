import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ClaimListItem, ClaimDetail, PagedResult, CreateClaimResult,
  AuditLogEntry, ClaimDocument, ReserveComponent
} from '../models/claim.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ClaimsService {
  private readonly base = `${environment.apiUrl}/api/claims`;

  constructor(private http: HttpClient) {}

  listClaims(filters: {
    status?: string; dateFrom?: string; dateTo?: string;
    assignedHandlerId?: string; causeOfLossCode?: string;
    policyId?: string; search?: string; page?: number; pageSize?: number;
  }): Observable<PagedResult<ClaimListItem>> {
    let params = new HttpParams();
    Object.entries(filters).forEach(([k, v]) => { if (v != null && v !== '') params = params.set(k, v); });
    return this.http.get<PagedResult<ClaimListItem>>(this.base, { params });
  }

  getClaim(id: string): Observable<ClaimDetail> {
    return this.http.get<ClaimDetail>(`${this.base}/${id}`);
  }

  createClaim(payload: unknown): Observable<CreateClaimResult> {
    return this.http.post<CreateClaimResult>(this.base, payload);
  }

  transitionStatus(id: string, targetStatus: string, reason?: string): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}/status`, { targetStatus, reason });
  }

  getAuditLog(id: string, page = 1, pageSize = 50): Observable<PagedResult<AuditLogEntry>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<AuditLogEntry>>(`${this.base}/${id}/audit`, { params });
  }

  addParty(claimId: string, party: unknown): Observable<{ partyId: string }> {
    return this.http.post<{ partyId: string }>(`${this.base}/${claimId}/parties`, party);
  }

  removeParty(claimId: string, partyId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${claimId}/parties/${partyId}`);
  }

  getDocuments(claimId: string): Observable<ClaimDocument[]> {
    return this.http.get<ClaimDocument[]>(`${this.base}/${claimId}/documents`);
  }

  uploadDocument(claimId: string, file: File, documentType: string, notes?: string): Observable<{ documentId: string }> {
    const fd = new FormData();
    fd.append('file', file);
    fd.append('documentType', documentType);
    if (notes) fd.append('notes', notes);
    return this.http.post<{ documentId: string }>(`${this.base}/${claimId}/documents`, fd);
  }

  getReserves(claimId: string): Observable<ReserveComponent[]> {
    return this.http.get<ReserveComponent[]>(`${this.base}/${claimId}/reserves`);
  }

  createReserve(claimId: string, reserve: unknown): Observable<unknown> {
    return this.http.post(`${this.base}/${claimId}/reserves`, reserve);
  }

  approveReserve(claimId: string, historyId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${claimId}/reserves/${historyId}/approve`, {});
  }

  rejectReserve(claimId: string, historyId: string, rejectionReason: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${claimId}/reserves/${historyId}/reject`, { rejectionReason });
  }

  retractReserve(claimId: string, historyId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${claimId}/reserves/${historyId}/retract`, {});
  }
}
