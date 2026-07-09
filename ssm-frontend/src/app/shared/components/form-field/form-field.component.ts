import { Component, input } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './form-field.component.html'
})
export class FormFieldComponent {
  label = input.required<string>();
  control = input<FormControl | null>(null);
  errorMessages = input<Record<string, string>>({});
  fieldId = input<string>('');

  errorEntries() {
    return Object.entries(this.errorMessages());
  }
}
