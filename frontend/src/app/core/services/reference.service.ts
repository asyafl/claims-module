import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CauseOfLossCode, ClaimStatusInfo, Policy } from '../models/claim.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ReferenceService {
  private readonly api = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getCauseOfLossCodes(perilCategory?: string): Observable<CauseOfLossCode[]> {
    let params = new HttpParams();
    if (perilCategory) params = params.set('perilCategory', perilCategory);
    return this.http.get<CauseOfLossCode[]>(`${this.api}/api/reference/cause-of-loss-codes`, { params });
  }

  getClaimStatuses(): Observable<ClaimStatusInfo[]> {
    return this.http.get<ClaimStatusInfo[]>(`${this.api}/api/reference/claim-statuses`);
  }

  searchPolicies(q: string): Observable<Policy[]> {
    return this.http.get<Policy[]>(`${this.api}/api/policies/search`, { params: new HttpParams().set('q', q) });
  }
}
