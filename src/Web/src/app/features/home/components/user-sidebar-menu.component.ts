import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmAvatarImports } from '@spartan-ng/helm/avatar';
import { HlmDropdownMenuImports } from '@spartan-ng/helm/dropdown-menu';
import { HlmSidebarImports } from '@spartan-ng/helm/sidebar';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideChevronUp, lucideLogOut } from '@ng-icons/lucide';
import { getApiErrorMessage } from '../../../core/http/problem-details';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-user-sidebar-menu',
  standalone: true,
  imports: [HlmAvatarImports, HlmDropdownMenuImports, HlmSidebarImports, NgIcon],
  providers: [provideIcons({ lucideChevronUp, lucideLogOut })],
  templateUrl: './user-sidebar-menu.component.html',
  host: { class: 'contents' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserSidebarMenuComponent {
  private readonly authService = inject(AuthService);

  protected readonly currentUser = this.authService.currentUser;
  protected readonly isLoggingOut = signal(false);
  protected readonly userInitial = computed(
    () => this.currentUser()?.email.charAt(0).toUpperCase() ?? 'U',
  );

  protected async logout(): Promise<void> {
    this.isLoggingOut.set(true);

    try {
      await firstValueFrom(this.authService.logout());
    } catch (error) {
      toast.error(getApiErrorMessage(error));
      this.isLoggingOut.set(false);
    }
  }
}
