import { Routes } from '@angular/router';
import { AboutComponent } from './pages/about/about.component';
import { BondValuationPageComponent } from './features/bond-valuation/pages/bond-valuation-page/bond-valuation-page.component';

export const routes: Routes = [
  { path: '', component: AboutComponent },
  { path: 'valuation', component: BondValuationPageComponent },
  { path: 'about', component: AboutComponent }
];
