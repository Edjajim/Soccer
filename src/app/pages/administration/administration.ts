import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { forkJoin } from 'rxjs';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { League } from '../../models/league';
import { Team } from '../../models/team';
import { LeaguesService } from '../../services/leagues.service';
import { LeagueTeamService } from '../../services/league-team.service';
import { TeamsService } from '../../services/teams.service';

type AdministrationType = 'leagues' | 'teams';
type AdministrationItem = League | Team;

function dateOrderValidator(control: AbstractControl): { dateOrder: true } | null {
  const startDate = control.get('startDate')?.value as string;
  const endDate = control.get('endDate')?.value as string;

  if (!startDate || !endDate || startDate <= endDate) {
    return null;
  }

  return { dateOrder: true };
}

function minimumLettersValidator(
  minimum: number
): (control: AbstractControl) => { minimumLetters: true } | null {
  return (control) => {
    const letters = (control.value as string).match(/\p{L}/gu) ?? [];
    return letters.length >= minimum ? null : { minimumLetters: true };
  };
}

@Component({
  selector: 'app-administration',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './administration.html',
  styleUrl: './administration.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Administration {
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly leaguesService = inject(LeaguesService);
  private readonly leagueTeamService = inject(LeagueTeamService);
  private readonly teamsService = inject(TeamsService);

  protected readonly title = this.route.snapshot.data['title'] as string;
  protected readonly type = this.route.snapshot.data['type'] as AdministrationType;
  protected readonly items = signal<AdministrationItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal(false);
  protected readonly errorMessage = signal(
    'No se pudieron cargar los datos. Verifica que la API esté ejecutándose.'
  );
  protected readonly modalOpen = signal(false);
  protected readonly saving = signal(false);
  protected readonly formError = signal('');
  protected readonly actionError = signal('');
  protected readonly editingItem = signal<AdministrationItem | null>(null);
  protected readonly deleteModalOpen = signal(false);
  protected readonly deleteTarget = signal<AdministrationItem | null>(null);
  protected readonly relatedNames = signal<string[]>([]);
  protected readonly relatedLoading = signal(false);
  protected readonly relatedError = signal('');
  protected readonly leagueTeams = signal<Record<number, Team[]>>({});
  protected readonly leagueTeamsLoading = signal<Record<number, boolean>>({});
  protected readonly leagueTeamsError = signal<Record<number, string>>({});
  protected readonly registerModalOpen = signal(false);
  protected readonly registerLeague = signal<League | null>(null);
  protected readonly availableTeams = signal<Team[]>([]);
  protected readonly registerLoading = signal(false);
  protected readonly registerSaving = signal(false);
  protected readonly registerError = signal('');
  protected readonly registerForm = this.formBuilder.group({
    teamId: [0, [Validators.required, Validators.min(1)]]
  });
  protected readonly form = this.formBuilder.group({
    name: ['', [Validators.required, minimumLettersValidator(2)]],
    country: ['', [Validators.required, minimumLettersValidator(2)]],
    startDate: ['', Validators.required],
    endDate: ['', Validators.required],
    playersQuantity: [11, [Validators.required, Validators.min(11), Validators.max(22)]],
    enabled: [true]
  }, { validators: dateOrderValidator });

  constructor() {
    this.configureForm();
    this.loadData();
  }

  protected isLeague(item: AdministrationItem): item is League {
    return this.type === 'leagues';
  }

  protected openCreateModal(): void {
    this.editingItem.set(null);
    this.form.reset({
      name: '',
      country: '',
      startDate: this.getToday(),
      endDate: this.getToday(),
      playersQuantity: 11,
      enabled: true
    });
    this.formError.set('');
    this.modalOpen.set(true);
  }

  protected openEditModal(item: AdministrationItem): void {
    this.editingItem.set(item);
    this.form.patchValue({
      name: item.name,
      country: item.country,
      startDate: this.isLeague(item) ? item.startDate : this.getToday(),
      endDate: this.isLeague(item) ? item.endDate : this.getToday(),
      playersQuantity: this.isLeague(item) ? 11 : item.playersQuantity,
      enabled: item.enabled
    });
    this.formError.set('');
    this.modalOpen.set(true);
  }

  protected closeCreateModal(): void {
    if (!this.saving()) {
      this.modalOpen.set(false);
    }
  }

  protected openDeleteModal(item: AdministrationItem): void {
    this.deleteTarget.set(item);
    this.relatedNames.set([]);
    this.relatedError.set('');
    this.relatedLoading.set(true);
    this.deleteModalOpen.set(true);

    if (this.type === 'leagues') {
      this.leagueTeamService.getTeamsByLeague(item.id).subscribe({
        next: (relatedItems) => this.setRelatedNames(relatedItems.map((relatedItem) => relatedItem.name)),
        error: (error: unknown) => this.setRelatedError(error)
      });
      return;
    }

    this.leagueTeamService.getLeaguesByTeam(item.id).subscribe({
      next: (relatedItems) => this.setRelatedNames(relatedItems.map((relatedItem) => relatedItem.name)),
      error: (error: unknown) => this.setRelatedError(error)
    });
  }

  protected closeDeleteModal(): void {
    if (!this.saving()) {
      this.deleteModalOpen.set(false);
      this.deleteTarget.set(null);
    }
  }

  protected openRegisterModal(league: League): void {
      this.registerLeague.set(league);
      this.availableTeams.set([]);
      this.registerError.set('');
      this.registerForm.reset({ teamId: 0 });
      this.registerLoading.set(true);
      this.registerModalOpen.set(true);

      forkJoin({
        teams: this.teamsService.getTeams(),
        registered: this.leagueTeamService.getTeamsByLeague(league.id)
      }).subscribe({
        next: ({ teams, registered }) => {
          const registeredIds = new Set(registered.map((team) => team.id));
          this.availableTeams.set(teams.filter((team) => !registeredIds.has(team.id)));
          this.registerLoading.set(false);
        },
        error: (error: unknown) => {
          console.error('Error al cargar equipos disponibles:', error);
          this.registerError.set('No se pudieron cargar los equipos disponibles.');
          this.registerLoading.set(false);
        }
      });
  }

  protected closeRegisterModal(): void {
    if (!this.registerSaving()) {
      this.registerModalOpen.set(false);
      this.registerLeague.set(null);
    }
  }

  protected registerTeam(): void {
    if (this.registerForm.invalid || !this.registerLeague()) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.registerSaving.set(true);
    this.registerError.set('');

    this.leagueTeamService.registerTeam({
      leagueId: this.registerLeague()!.id,
      teamId: this.registerForm.getRawValue().teamId
    }).subscribe({
      next: () => {
        const league = this.registerLeague();
        if (league) {
          this.loadTeamsForLeague(league.id);
        }
        this.registerSaving.set(false);
        this.closeRegisterModal();
      },
      error: (error: unknown) => {
        console.error('Error al registrar equipo:', error);
        this.registerError.set(
          'No se pudo registrar el equipo. Puede que ya esté registrado en esta liga.'
        );
        this.registerSaving.set(false);
      }
    });
  }

  protected confirmDelete(): void {
    const item = this.deleteTarget();

    if (!item) {
      return;
    }

    this.actionError.set('');
    this.saving.set(true);

    const request = this.type === 'leagues'
      ? this.leaguesService.deleteLeague(item.id)
      : this.teamsService.deleteTeam(item.id);

    request.subscribe({
      next: () => {
        this.items.update((items) => items.filter((current) => current.id !== item.id));
        this.saving.set(false);
        this.closeDeleteModal();
      },
      error: (error: unknown) => {
        console.error(`Error al eliminar ${this.type}:`, error);
        this.actionError.set(
          `No se pudo eliminar la ${this.type === 'leagues' ? 'liga' : 'equipo'}. Inténtalo nuevamente.`
        );
        this.saving.set(false);
        this.closeDeleteModal();
      }
    });
  }

  protected createItem(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.formError.set('');

    if (this.editingItem()) {
      this.updateItem(this.editingItem()!.id);
    } else if (this.type === 'leagues') {
      this.createLeague();
    } else {
      this.createTeam();
    }
  }

  protected hasDateOrderError(): boolean {
    return this.form.hasError('dateOrder') &&
      (this.form.controls.startDate.touched || this.form.controls.endDate.touched);
  }

  private loadData(): void {
    if (this.type === 'leagues') {
      this.leaguesService.getLeagues().subscribe({
        next: (items) => {
          this.setItems(items);
        },
        error: (error: unknown) => {
          this.setError(error);
        }
      });
      return;
    }

    this.teamsService.getTeams().subscribe({
      next: (items) => {
        this.setItems(items);
      },
      error: (error: unknown) => {
        this.setError(error);
      }
    });
  }

  private setItems(items: AdministrationItem[]): void {
    this.items.set(items);
    this.loading.set(false);

    if (this.type === 'leagues') {
      this.leagueTeams.set({});
      this.leagueTeamsLoading.set({});
      this.leagueTeamsError.set({});
      for (const league of items) {
        this.loadTeamsForLeague(league.id);
      }
    }
  }

  protected teamsForLeague(leagueId: number): Team[] {
    return this.leagueTeams()[leagueId] ?? [];
  }

  protected teamsLoadingForLeague(leagueId: number): boolean {
    return this.leagueTeamsLoading()[leagueId] ?? false;
  }

  protected teamsErrorForLeague(leagueId: number): string {
    return this.leagueTeamsError()[leagueId] ?? '';
  }

  private setRelatedNames(names: string[]): void {
    this.relatedNames.set(names);
    this.relatedLoading.set(false);
  }

  private setRelatedError(error: unknown): void {
    console.error(`Error al cargar relaciones de ${this.type}:`, error);
    this.relatedError.set('No se pudieron cargar las relaciones de este registro.');
    this.relatedLoading.set(false);
  }

  private loadTeamsForLeague(leagueId: number): void {
    this.leagueTeamsLoading.update((states) => ({ ...states, [leagueId]: true }));
    this.leagueTeamsError.update((errors) => {
      const nextErrors = { ...errors };
      delete nextErrors[leagueId];
      return nextErrors;
    });

    this.leagueTeamService.getTeamsByLeague(leagueId).subscribe({
      next: (teams) => {
        this.leagueTeams.update((current) => ({ ...current, [leagueId]: teams }));
        this.leagueTeamsLoading.update((states) => ({ ...states, [leagueId]: false }));
      },
      error: (error: unknown) => {
        console.error(`Error al cargar equipos de la liga ${leagueId}:`, error);
        this.leagueTeamsError.update((errors) => ({
          ...errors,
          [leagueId]: 'No se pudieron cargar los equipos de esta liga.'
        }));
        this.leagueTeamsLoading.update((states) => ({ ...states, [leagueId]: false }));
      }
    });
  }

  private configureForm(): void {
    if (this.type === 'leagues') {
      this.form.controls.playersQuantity.clearValidators();
      this.form.controls.playersQuantity.updateValueAndValidity();
      return;
    }

    this.form.controls.startDate.clearValidators();
    this.form.controls.endDate.clearValidators();
    this.form.setValidators(null);
    this.form.controls.startDate.updateValueAndValidity();
    this.form.controls.endDate.updateValueAndValidity();
  }

  private getToday(): string {
    const today = new Date();
    const month = String(today.getMonth() + 1).padStart(2, '0');
    const day = String(today.getDate()).padStart(2, '0');
    return `${today.getFullYear()}-${month}-${day}`;
  }

  private createLeague(): void {
    const value = this.form.getRawValue();

    this.leaguesService.createLeague({
      name: value.name,
      country: value.country,
      startDate: value.startDate,
      endDate: value.endDate,
      enabled: value.enabled
    }).subscribe({
      next: (league) => this.addItem(league),
      error: (error: unknown) => this.setFormError(error)
    });
  }

  private createTeam(): void {
    const value = this.form.getRawValue();

    this.teamsService.createTeam({
      name: value.name,
      country: value.country,
      playersQuantity: value.playersQuantity,
      enabled: value.enabled
    }).subscribe({
      next: (team) => this.addItem(team),
      error: (error: unknown) => this.setFormError(error)
    });
  }

  private updateItem(id: number): void {
    const value = this.form.getRawValue();

    if (this.type === 'leagues') {
      this.leaguesService.updateLeague(id, {
        name: value.name,
        country: value.country,
        startDate: value.startDate,
        endDate: value.endDate,
        enabled: value.enabled
      }).subscribe({
        next: () => this.replaceItem(id, {
          id,
          name: value.name,
          country: value.country,
          startDate: value.startDate,
          endDate: value.endDate,
          enabled: value.enabled
        }),
        error: (error: unknown) => this.setFormError(error)
      });
      return;
    }

    this.teamsService.updateTeam(id, {
      name: value.name,
      country: value.country,
      playersQuantity: value.playersQuantity,
      enabled: value.enabled
    }).subscribe({
      next: () => this.replaceItem(id, {
        id,
        name: value.name,
        country: value.country,
        playersQuantity: value.playersQuantity,
        enabled: value.enabled
      }),
      error: (error: unknown) => this.setFormError(error)
    });
  }

  private addItem(item: AdministrationItem): void {
    this.items.update((items) => [...items, item]);
    this.saving.set(false);
    this.modalOpen.set(false);
  }

  private replaceItem(id: number, item: AdministrationItem): void {
    this.items.update((items) =>
      items.map((current) => current.id === id ? item : current)
    );
    this.saving.set(false);
    this.modalOpen.set(false);
    this.editingItem.set(null);
  }

  private setFormError(error: unknown): void {
    console.error(`Error al crear ${this.type}:`, error);
    this.formError.set('No se pudo crear el registro. Revisa los datos e inténtalo nuevamente.');
    this.saving.set(false);
  }

  private setError(error: unknown): void {
    console.error(`Error al cargar ${this.type}:`, error);

    if (this.isHttpError(error) && error.status === 0) {
      this.errorMessage.set(
        'La API respondió, pero el navegador bloqueó la petición. Reinicia la API con CORS habilitado.'
      );
    }

    this.error.set(true);
    this.loading.set(false);
  }

  private isHttpError(error: unknown): error is { status: number } {
    return (
      typeof error === 'object' &&
      error !== null &&
      'status' in error &&
      typeof error.status === 'number'
    );
  }
}
