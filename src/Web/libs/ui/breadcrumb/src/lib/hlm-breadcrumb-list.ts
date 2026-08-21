import { Directive } from '@angular/core';
import { classes } from '@spartan-ng/helm/utils';

@Directive({
  selector: '[hlmBreadcrumbList]',
  host: {
    'data-slot': 'breadcrumb-list',
  },
})
export class HlmBreadcrumbList {
  constructor() {
    classes(
      () =>
        'text-muted-foreground gap-1.5 text-xs/relaxed flex flex-wrap items-center wrap-break-word',
    );
  }
}
