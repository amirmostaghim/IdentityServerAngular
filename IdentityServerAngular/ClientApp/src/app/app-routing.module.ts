import { AppComponent } from './app.component';
import { NgModule }             from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  { path: 'home/error', component: AppComponent },
  // { path: '**', redirectTo: ''} // Must be at the end of path

];

@NgModule({
  imports: [
    RouterModule.forRoot(routes)
],
  exports: [ RouterModule ]
})
export class AppRoutingModule {}
