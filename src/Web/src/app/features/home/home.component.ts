import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { HlmSidebarImports } from '@spartan-ng/helm/sidebar';
import { HomeSidebarComponent } from './components/home-sidebar.component';
import { TasksComponent } from './components/tasks.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [HlmSidebarImports, HomeSidebarComponent, TasksComponent],
  templateUrl: './home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  protected readonly selectedCategoryId = signal<string | null>(null);
}
