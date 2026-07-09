import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { BillService } from '../bill.service';
import { ApartmentService } from '../../apartments/apartment.service';
import { ApartmentResponse } from '../../apartments/apartment.model';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-bill-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './bill-form.component.html'
})
export class BillFormComponent implements OnInit {
  private svc = inject(BillService);
  private aptService = inject(ApartmentService);
  private router = inject(Router);
  private toast = inject(ToastService);
  loading = signal(false);
  apartments = signal<ApartmentResponse[]>([]);

  form = new FormGroup({
    apartmentId: new FormControl('', [Validators.required]),
    period: new FormControl('', [Validators.required, Validators.pattern(/^\d{2}-\d{4}$/)]),
    baseAmount: new FormControl<number>(0, [Validators.required, Validators.min(1)]),
    dueDate: new FormControl('', [Validators.required]),
  });

  ngOnInit() {
    this.aptService.getAllForDropdown().subscribe(r => this.apartments.set(r.items));
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.svc.create({
      apartmentId: this.form.value.apartmentId!,
      period: this.form.value.period!,
      baseAmount: this.form.value.baseAmount!,
      dueDate: new Date(this.form.value.dueDate!).toISOString(),
    }).subscribe({
      next: () => { this.toast.success('Bill created'); this.router.navigate(['/bills']); },
      error: () => this.loading.set(false)
    });
  }
}
