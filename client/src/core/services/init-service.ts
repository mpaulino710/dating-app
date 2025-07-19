import { inject, Injectable } from '@angular/core';
import { AccountService } from './account-service';
import { of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InitService {
  private accountService = inject(AccountService)

  init() {
    const userString = localStorage.getItem('user');
    if (userString) {
      this.accountService.currentUser.set(JSON.parse(userString));
    }

    return of(null); // Return an observable to ensure the service is compatible with Angular's DI system
  }
}
