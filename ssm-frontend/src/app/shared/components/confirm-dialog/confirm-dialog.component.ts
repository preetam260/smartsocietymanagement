import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  templateUrl: './confirm-dialog.component.html'
})
export class ConfirmDialogComponent {
  message = input.required<string>();
  open = input.required<boolean>();
  confirmed = output<void>();
  cancelled = output<void>();
}
