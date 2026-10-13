import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { League } from '../models/league';

@Injectable({ providedIn: 'root' })
export class LeaguesService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5032/api/League';

  getLeagues(): Observable<League[]> {
    return this.http.get<League[]>(this.apiUrl);
  }

  createLeague(league: Omit<League, 'id'>): Observable<League> {
    return this.http.post<League>(this.apiUrl, league);
  }

  updateLeague(id: number, league: Omit<League, 'id'>): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, league);
  }

  deleteLeague(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
