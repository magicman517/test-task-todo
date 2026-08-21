import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { HlmSidebarImports } from '@spartan-ng/helm/sidebar';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideHome, lucideListTodo } from '@ng-icons/lucide';
import { CategorySidebarComponent } from './category-sidebar.component';
import { UserSidebarMenuComponent } from './user-sidebar-menu.component';

@Component({
  selector: 'app-home-sidebar',
  standalone: true,
  imports: [
    CategorySidebarComponent,
    HlmSidebarImports,
    NgIcon,
    RouterLink,
    RouterLinkActive,
    UserSidebarMenuComponent,
  ],
  providers: [provideIcons({ lucideHome, lucideListTodo })],
  templateUrl: './home-sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeSidebarComponent {
  public readonly selectedCategoryId = input<string | null>(null);
  public readonly categorySelected = output<string | null>();

  protected clearCategoryFilter(): void {
    this.categorySelected.emit(null);
  }
}
