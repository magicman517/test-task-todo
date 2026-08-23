import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';
import { debounceTime, distinctUntilChanged, firstValueFrom, Subject } from 'rxjs';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCheckboxImports } from '@spartan-ng/helm/checkbox';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmDropdownMenuImports } from '@spartan-ng/helm/dropdown-menu';
import { HlmEmptyImports } from '@spartan-ng/helm/empty';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmNativeSelectImports } from '@spartan-ng/helm/native-select';
import { HlmPaginationImports } from '@spartan-ng/helm/pagination';
import { HlmSidebarImports } from '@spartan-ng/helm/sidebar';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideCheck,
  lucideEllipsis,
  lucideListTodo,
  lucidePencil,
  lucidePlus,
  lucideSearch,
  lucideSprout,
  lucideTrash2,
} from '@ng-icons/lucide';
import { getApiErrorMessage } from '../../../core/http/problem-details';
import { CategoriesService } from '../../../core/services/categories.service';
import { CategoryStateService } from '../../../core/services/category-state.service';
import { TodosService, type Todo } from '../../../core/services/todos.service';

type TaskFilter = 'all' | 'open' | 'completed';

@Component({
  selector: 'app-tasks',
  standalone: true,
  imports: [
    FormField,
    FormRoot,
    HlmButtonImports,
    HlmCheckboxImports,
    HlmDialogImports,
    HlmDropdownMenuImports,
    HlmEmptyImports,
    HlmInputImports,
    HlmNativeSelectImports,
    HlmPaginationImports,
    HlmSidebarImports,
    NgIcon,
  ],
  providers: [
    provideIcons({
      lucideCheck,
      lucideEllipsis,
      lucideListTodo,
      lucidePencil,
      lucidePlus,
      lucideSearch,
      lucideSprout,
      lucideTrash2,
    }),
  ],
  templateUrl: './tasks.component.html',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TasksComponent {
  private readonly todosService = inject(TodosService);
  private readonly categoriesService = inject(CategoriesService);
  private readonly categoryState = inject(CategoryStateService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchTerms = new Subject<string>();
  private readonly taskDialog = viewChild.required<HlmDialog>('taskDialog');
  private readonly deleteDialog = viewChild.required<HlmDialog>('deleteDialog');

  public readonly selectedCategoryId = input<string | null>(null);

  protected readonly tasks = signal<Todo[]>([]);
  protected readonly categories = this.categoryState.categories;
  protected readonly isLoading = signal(true);
  protected readonly search = signal('');
  protected readonly filter = signal<TaskFilter>('all');
  protected readonly page = signal(1);
  protected readonly pageSize = 10;
  protected readonly totalCount = signal(0);
  protected readonly totalPages = signal(0);
  protected readonly editingTask = signal<Todo | null>(null);
  protected readonly deletingTask = signal<Todo | null>(null);
  protected readonly isDeleting = signal(false);
  protected readonly isSeeding = signal(false);
  protected readonly taskModel = signal({ title: '', description: '', categoryId: '' });

  public readonly taskForm = form(
    this.taskModel,
    (schemaPath) => {
      required(schemaPath.title, { message: 'A task title is required.' });
      maxLength(schemaPath.title, 200, { message: 'A task title cannot exceed 200 characters.' });
      maxLength(schemaPath.description, 1000, {
        message: 'A description cannot exceed 1000 characters.',
      });
    },
    {
      submission: {
        action: async () => {
          const model = this.taskModel();
          const request = {
            title: model.title.trim(),
            description: model.description.trim() || null,
            categoryId: model.categoryId || null,
          };

          try {
            const editingTask = this.editingTask();
            if (editingTask) {
              const updated = await firstValueFrom(
                this.todosService.update(editingTask.id, {
                  ...request,
                  isCompleted: editingTask.isCompleted,
                }),
              );
              this.tasks.update((tasks) =>
                tasks.map((task) => (task.id === updated.id ? updated : task)),
              );
            } else {
              const created = await firstValueFrom(this.todosService.create(request));
              this.tasks.update((tasks) => [created, ...tasks]);
            }
            this.categoryState.refresh().subscribe({ error: () => undefined });
            this.taskDialog().close();
            this.loadTasks();
          } catch (error) {
            toast.error(getApiErrorMessage(error));
          }
        },
      },
    },
  );

  constructor() {
    this.searchTerms
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((search) => {
        this.search.set(search);
        this.page.set(1);
        this.loadTasks();
      });

    effect(() => {
      this.selectedCategoryId();
      untracked(() => {
        this.page.set(1);
        this.loadTasks();
      });
    });
  }

  protected setFilter(filter: TaskFilter): void {
    this.filter.set(filter);
    this.page.set(1);
    this.loadTasks();
  }

  protected changePage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) {
      return;
    }

    this.page.set(page);
    this.loadTasks();
  }

  protected pageNumbers(): number[] {
    const totalPages = this.totalPages();
    const currentPage = this.page();
    const start = Math.max(1, Math.min(currentPage - 2, totalPages - 4));
    const end = Math.min(totalPages, start + 4);

    return Array.from({ length: Math.max(0, end - start + 1) }, (_, index) => start + index);
  }

  protected searchTasks(event: Event): void {
    this.searchTerms.next((event.target as HTMLInputElement).value);
  }

  protected async seedDemoData(): Promise<void> {
    if (this.isSeeding()) return;

    this.isSeeding.set(true);
    try {
      await firstValueFrom(this.categoriesService.seed());
      await firstValueFrom(this.todosService.seed(10));
      this.categoryState.refresh().subscribe({ error: () => undefined });
      this.filter.set('all');
      this.page.set(1);
      this.loadTasks();
      toast.success('Demo data has been seeded');
    } catch (error) {
      toast.error(getApiErrorMessage(error));
    } finally {
      this.isSeeding.set(false);
    }
  }

  protected openCreateDialog(): void {
    this.editingTask.set(null);
    this.taskForm().reset({ title: '', description: '', categoryId: '' });
    this.taskDialog().open();
  }

  protected openEditDialog(task: Todo): void {
    this.editingTask.set(task);
    this.taskForm().reset({
      title: task.title,
      description: task.description ?? '',
      categoryId: task.categoryId ?? '',
    });
    this.taskDialog().open();
  }

  protected openDeleteDialog(task: Todo): void {
    this.deletingTask.set(task);
    this.deleteDialog().open();
  }

  protected async toggleTask(task: Todo, isCompleted: boolean): Promise<void> {
    try {
      const updated = await firstValueFrom(
        this.todosService.update(task.id, { ...task, isCompleted }),
      );
      this.categoryState.refresh().subscribe({ error: () => undefined });
      this.loadTasks();
    } catch (error) {
      toast.error(getApiErrorMessage(error));
    }
  }

  protected async deleteTask(): Promise<void> {
    const task = this.deletingTask();
    if (!task) return;

    this.isDeleting.set(true);
    try {
      await firstValueFrom(this.todosService.delete(task.id));
      this.categoryState.refresh().subscribe({ error: () => undefined });
      this.deleteDialog().close();
      if (this.tasks().length === 1 && this.page() > 1) {
        this.page.update((page) => page - 1);
      }
      this.loadTasks();
      this.deletingTask.set(null);
    } catch (error) {
      toast.error(getApiErrorMessage(error));
    } finally {
      this.isDeleting.set(false);
    }
  }

  protected categoryName(task: Todo): string | null {
    return (
      task.categoryName ??
      this.categories().find((category) => category.id === task.categoryId)?.name ??
      null
    );
  }

  private loadTasks(): void {
    this.isLoading.set(true);
    const isCompleted = this.filter() === 'all' ? undefined : this.filter() === 'completed';
    this.todosService
      .getAll({
        search: this.search(),
        categoryId: this.selectedCategoryId() ?? undefined,
        isCompleted,
        page: this.page(),
        pageSize: this.pageSize,
      })
      .subscribe({
        next: (result) => {
          this.tasks.set(result.items);
          this.page.set(result.page);
          this.totalCount.set(result.totalCount);
          this.totalPages.set(result.totalPages);
          this.isLoading.set(false);
        },
        error: (error) => {
          this.isLoading.set(false);
          toast.error(getApiErrorMessage(error));
        },
      });
  }
}
