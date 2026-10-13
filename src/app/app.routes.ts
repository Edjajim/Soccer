import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/home/home').then((module) => module.Home)
  },
  {
    path: 'leagues',
    loadComponent: () =>
      import('./pages/administration/administration').then(
        (module) => module.Administration
      ),
    data: {
      type: 'leagues',
      title: 'Ligas'
    }
  },
  {
    path: 'teams',
    loadComponent: () =>
      import('./pages/administration/administration').then(
        (module) => module.Administration
      ),
    data: {
      type: 'teams',
      title: 'Equipos'
    }
  }
];
