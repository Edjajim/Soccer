import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { League } from '../models/league';
import { Team } from '../models/team';

export interface RegisterTeamRequest {
  leagueId: number;
  teamId: number;
}

@Injectable({ providedIn: 'root' })
export class LeagueTeamService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5032/api/LeagueTeam';

  getTeamsByLeague(leagueId: number): Observable<Team[]> {
    return this.http.get<Team[]>(`${this.apiUrl}/League/${leagueId}`);
  }

  getLeaguesByTeam(teamId: number): Observable<League[]> {
    return this.http.get<League[]>(`${this.apiUrl}/Team/${teamId}`);
  }

  registerTeam(request: RegisterTeamRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, request);
  }

  removeTeam(request: RegisterTeamRequest): Observable<void> {
    return this.http.delete<void>(this.apiUrl, { body: request });
  }
}
