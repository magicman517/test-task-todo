import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export type Todo = {
  id: string;
  title: string;
  description: string | null;
  isCompleted: boolean;
  createdAt: string;
  updatedAt: string | null;
  categoryId: string | null;
  categoryName: string | null;
};

export type PaginatedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type GetTodosParams = {
  search?: string;
  categoryId?: string;
  isCompleted?: boolean;
  page?: number;
  pageSize?: number;
};

export type CreateTodoRequest = {
  title: string;
  description?: string | null;
  categoryId?: string | null;
};

export type UpdateTodoRequest = {
  title: string;
  description?: string | null;
  isCompleted: boolean;
  categoryId?: string | null;
};

@Injectable({ providedIn: 'root' })
export class TodosService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/todo';

  public getAll(params: GetTodosParams = {}): Observable<PaginatedResult<Todo>> {
    let httpParams = new HttpParams();

    if (params.search?.trim()) {
      httpParams = httpParams.set('search', params.search.trim());
    }
    if (params.categoryId) {
      httpParams = httpParams.set('categoryId', params.categoryId);
    }
    if (params.isCompleted !== undefined) {
      httpParams = httpParams.set('isCompleted', String(params.isCompleted));
    }
    if (params.page !== undefined) {
      httpParams = httpParams.set('page', String(params.page));
    }
    if (params.pageSize !== undefined) {
      httpParams = httpParams.set('pageSize', String(params.pageSize));
    }

    return this.http.get<PaginatedResult<Todo>>(this.apiUrl, { params: httpParams });
  }

  public getById(id: string): Observable<Todo> {
    return this.http.get<Todo>(`${this.apiUrl}/${id}`);
  }

  public create(request: CreateTodoRequest): Observable<Todo> {
    return this.http.post<Todo>(this.apiUrl, request);
  }

  public update(id: string, request: UpdateTodoRequest): Observable<Todo> {
    return this.http.put<Todo>(`${this.apiUrl}/${id}`, request);
  }

  public delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  public seed(count: number = 10): Observable<Todo[]> {
    return this.http.post<Todo[]>(`${this.apiUrl}/seed?count=${count}`, {});
  }
}
