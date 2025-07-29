import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequestsCount = signal(0);

  busy() {
    this.busyRequestsCount.update(count => count + 1);
  }

  idle() {
    this.busyRequestsCount.update(count => Math.max(0, count - 1));
  } 
}
