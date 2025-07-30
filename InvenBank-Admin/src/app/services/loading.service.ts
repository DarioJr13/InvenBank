import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private activeRequests = 0;

  constructor() {}

  /**
   * Observable del estado de loading
   */
  get loading$(): Observable<boolean> {
    return this.loadingSubject.asObservable();
  }

  /**
   * Mostrar loading
   */
  show(): void {
    this.activeRequests++;
    this.loadingSubject.next(true);
  }

  /**
   * Ocultar loading
   */
  hide(): void {
    this.activeRequests--;
    if (this.activeRequests <= 0) {
      this.activeRequests = 0;
      this.loadingSubject.next(false);
    }
  }

  /**
   * Forzar ocultar loading
   */
  forceHide(): void {
    this.activeRequests = 0;
    this.loadingSubject.next(false);
  }
}
