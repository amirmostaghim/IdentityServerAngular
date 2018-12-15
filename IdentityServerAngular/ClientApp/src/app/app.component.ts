import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'AngularIDP';
  isValid = false;

  onAddSubmit(t: boolean) {
    this.isValid = t;
  }

}
