import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { AuthService } from '../../core/auth/auth.service';
import { BasketActions } from '../../features/basket/state/basket.actions';
import { selectBasketCount } from '../../features/basket/state/basket.selectors';

@Component({
  selector: 'app-shop-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shop-layout.component.html',
  styleUrl: './shop-layout.component.scss',
})
export class ShopLayoutComponent {
  protected readonly auth = inject(AuthService);
  protected readonly store = inject(Store);
  protected readonly basketCount = this.store.selectSignal(selectBasketCount);
  protected readonly menuOpen = signal(false);
  protected readonly currentYear = new Date().getFullYear();

  constructor() {
    this.store.dispatch(BasketActions.loadBasket());
  }

  protected toggleMenu(): void {
    this.menuOpen.update((v) => !v);
  }

  protected closeMenu(): void {
    this.menuOpen.set(false);
  }

  protected logout(): void {
    this.closeMenu();
    this.auth.logout();
  }
}
