import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ComplaintService } from '../complaint.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-complaint-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './complaint-form.component.html'
})
export class ComplaintFormComponent {
  private svc = inject(ComplaintService);
  private router = inject(Router);
  private toast = inject(ToastService);
  loading = signal(false);

  form = new FormGroup({
    apartmentId: new FormControl(this.getSavedApartmentId(), [Validators.required]),
    title: new FormControl('', [Validators.required]),
    description: new FormControl('', [Validators.required]),
  });

  onSubmit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    // Save apartmentId for future reuse
    localStorage.setItem('ss_apartmentId', this.form.value.apartmentId!);
    this.svc.create({
      apartmentId: this.form.value.apartmentId!,
      title: this.form.value.title!,
      description: this.form.value.description!,
    }).subscribe({
      next: () => { this.toast.success('Complaint submitted'); this.router.navigate(['/complaints']); },
      error: () => this.loading.set(false)
    });
  }

  private getSavedApartmentId(): string {
    return localStorage.getItem('ss_apartmentId') ?? '';
  }
}
