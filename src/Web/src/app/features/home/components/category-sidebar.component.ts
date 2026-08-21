import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';
import { firstValueFrom } from 'rxjs';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmDialog, HlmDialogImports } from '@spartan-ng/helm/dialog';
import { HlmDropdownMenuImports } from '@spartan-ng/helm/dropdown-menu';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmSidebarImports } from '@spartan-ng/helm/sidebar';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideEllipsis,
  lucideFolder,
  lucidePencil,
  lucidePlus,
  lucideTrash2,
} from '@ng-icons/lucide';
import { CategoryStateService } from '../../../core/services/category-state.service';
import { CategoriesService, type Category } from '../../../core/services/categories.service';
import { getApiErrorMessage } from '../../../core/http/problem-details';

@Component({
  selector: 'app-category-sidebar',
  standalone: true,
  imports: [
    HlmButtonImports,
    HlmDialogImports,
    HlmDropdownMenuImports,
    HlmInputImports,
    HlmSidebarImports,
    NgIcon,
    FormField,
    FormRoot,
  ],
  providers: [
    provideIcons({ lucideEllipsis, lucideFolder, lucidePencil, lucidePlus, lucideTrash2 }),
  ],
  templateUrl: './category-sidebar.component.html',
  host: { class: 'contents' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CategorySidebarComponent {
  private readonly categoriesService = inject(CategoriesService);
  private readonly categoryState = inject(CategoryStateService);
  private readonly createCategoryDialog = viewChild.required<HlmDialog>('createCategoryDialog');
  private readonly renameCategoryDialog = viewChild.required<HlmDialog>('renameCategoryDialog');
  private readonly deleteCategoryDialog = viewChild.required<HlmDialog>('deleteCategoryDialog');

  public readonly selectedCategoryId = input<string | null>(null);
  public readonly categorySelected = output<string>();

  protected readonly categories = this.categoryState.categories;
  protected readonly selectedCategory = signal<Category | null>(null);
  protected readonly isDeletingCategory = signal(false);
  protected readonly categoryModel = signal({ name: '' });
  public readonly form = form(
    this.categoryModel,
    (schemaPath) => {
      required(schemaPath.name, { message: 'A category name is required.' });
      maxLength(schemaPath.name, 50, { message: 'A category name cannot exceed 50 characters.' });
    },
    {
      submission: {
        action: async () => {
          try {
            const category = await firstValueFrom(
              this.categoriesService.create({ name: this.categoryModel().name.trim() }),
            );
            this.categoryState.add(category);
            this.form().reset({ name: '' });
            this.createCategoryDialog().close();
          } catch (error) {
            toast.error(getApiErrorMessage(error));
          }
        },
      },
    },
  );

  protected readonly renameCategoryModel = signal({ name: '' });
  public readonly renameForm = form(
    this.renameCategoryModel,
    (schemaPath) => {
      required(schemaPath.name, { message: 'A category name is required.' });
      maxLength(schemaPath.name, 50, { message: 'A category name cannot exceed 50 characters.' });
    },
    {
      submission: {
        action: async () => {
          const selectedCategory = this.selectedCategory();
          if (!selectedCategory) {
            return;
          }

          try {
            const category = await firstValueFrom(
              this.categoriesService.update(selectedCategory.id, {
                name: this.renameCategoryModel().name.trim(),
              }),
            );
            this.categoryState.update(category);
            this.renameCategoryDialog().close();
          } catch (error) {
            toast.error(getApiErrorMessage(error));
          }
        },
      },
    },
  );

  constructor() {
    this.categoryState.refresh().subscribe({
      next: () => undefined,
      error: (error) => toast.error(getApiErrorMessage(error)),
    });
  }

  protected selectCategory(categoryId: string): void {
    this.categorySelected.emit(categoryId);
  }

  protected openRenameDialog(category: Category): void {
    this.selectedCategory.set(category);
    this.renameForm().reset({ name: category.name });
    this.renameCategoryDialog().open();
  }

  protected openDeleteDialog(category: Category): void {
    this.selectedCategory.set(category);
    this.deleteCategoryDialog().open();
  }

  protected async deleteCategory(): Promise<void> {
    const selectedCategory = this.selectedCategory();
    if (!selectedCategory) {
      return;
    }

    this.isDeletingCategory.set(true);

    try {
      await firstValueFrom(this.categoriesService.delete(selectedCategory.id));
      this.categoryState.remove(selectedCategory.id);
      this.deleteCategoryDialog().close();
      this.selectedCategory.set(null);
    } catch (error) {
      toast.error(getApiErrorMessage(error));
    } finally {
      this.isDeletingCategory.set(false);
    }
  }
}
