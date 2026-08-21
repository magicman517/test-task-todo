import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { CategoriesService, type Category } from './categories.service';

@Injectable({ providedIn: 'root' })
export class CategoryStateService {
  private readonly categoriesService = inject(CategoriesService);

  public readonly categories = signal<Category[]>([]);

  public refresh() {
    return this.categoriesService
      .getAll()
      .pipe(tap((categories) => this.categories.set(categories)));
  }

  public add(category: Category): void {
    this.categories.update((categories) =>
      [...categories, category].sort((a, b) => a.name.localeCompare(b.name)),
    );
  }

  public update(category: Category): void {
    this.categories.update((categories) =>
      categories
        .map((item) => (item.id === category.id ? category : item))
        .sort((a, b) => a.name.localeCompare(b.name)),
    );
  }

  public remove(categoryId: string): void {
    this.categories.update((categories) =>
      categories.filter((category) => category.id !== categoryId),
    );
  }
}
