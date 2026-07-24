import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    @if (auth.isAuthenticated()) {
      <nav class="navbar">
        <div class="container">
          <span class="brand">AssetFlow</span>
          <a routerLink="/work-orders" routerLinkActive="active">Work Orders</a>
          <a routerLink="/assets" routerLinkActive="active">Assets</a>
          <span class="spacer"></span>
          <span class="muted" style="color:#cbd5e1">{{ auth.user()?.fullName }} · {{ auth.user()?.role }}</span>
          <a href="#" (click)="logout($event)">Sign out</a>
        </div>
      </nav>
    }
    <main class="container">
      <router-outlet />
    </main>
  `
})
export class AppComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(event: Event): void {
    event.preventDefault();
    this.auth.logout();
    void this.router.navigate(['/login']);
  }
}
