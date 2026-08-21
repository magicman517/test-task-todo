import {
  email,
  form,
  FormField,
  FormRoot,
  maxLength,
  minLength,
  required,
} from '@angular/forms/signals';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toast } from '@spartan-ng/brain/sonner';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmInputGroupImports } from '@spartan-ng/helm/input-group';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideEye, lucideEyeOff } from '@ng-icons/lucide';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { getApiErrorMessage } from '../../../core/http/problem-details';

@Component({
  selector: 'app-register',
  standalone: true,
  templateUrl: './register.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [provideIcons({ lucideEye, lucideEyeOff })],
  imports: [
    HlmButtonImports,
    HlmCardImports,
    RouterLink,
    HlmFieldImports,
    HlmInputImports,
    HlmInputGroupImports,
    FormRoot,
    FormField,
    NgIcon,
  ],
})
export class RegisterComponent {
  private readonly _authService = inject(AuthService);

  protected readonly _model = signal({
    email: '',
    password: '',
  });

  protected readonly _isPasswordVisible = signal(false);
  public _togglePasswordVisibility = () => {
    this._isPasswordVisible.update((visible) => !visible);
  };

  public readonly form = form(
    this._model,
    (schemaPath) => {
      required(schemaPath.email, { message: 'Email is required' });
      email(schemaPath.email, { message: 'Email is not valid' });

      required(schemaPath.password, { message: 'Password is required' });
      minLength(schemaPath.password, 6, { message: 'Password must be at least 6 characters' });
      maxLength(schemaPath.password, 64, { message: 'Password must be at most 64 characters' });
    },
    {
      submission: {
        action: async () => {
          const model = this._model();
          try {
            await firstValueFrom(this._authService.register(model));
          } catch (error) {
            toast.error(getApiErrorMessage(error));
          }
        },
      },
    },
  );

  public reset() {
    this.form().reset({
      email: '',
      password: '',
    });
  }
}
