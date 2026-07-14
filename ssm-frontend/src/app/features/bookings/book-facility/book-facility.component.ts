import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { FacilityService } from '../../facilities/facility.service';
import { BookingService } from '../booking.service';
import { FacilityResponse } from '../../facilities/facility.model';
import { ToastService } from '../../../core/services/toast.service';
import { Router } from '@angular/router';
import { CurrencyPipe } from '@angular/common';

function timeRangeValidator(control: AbstractControl): ValidationErrors | null {
  const date = control.get('date')?.value;
  const start = control.get('startTime')?.value;
  const end = control.get('endTime')?.value;
  if (!date || !start || !end) return null;
  const startDt = new Date(`${date}T${start}:00`);
  const endDt = new Date(`${date}T${end}:00`);
  return endDt > startDt ? null : { timeRange: 'End time must be after start time.' };
}

@Component({
  selector: 'app-book-facility',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, CurrencyPipe],
  templateUrl: './book-facility.component.html'
})
export class BookFacilityComponent implements OnInit {
  private facilityService = inject(FacilityService);
  private bookingService = inject(BookingService);
  private router = inject(Router);
  private toast = inject(ToastService);
  facilities = signal<FacilityResponse[]>([]);
  loading = signal(false);

  form = new FormGroup({
    facilityId: new FormControl('', [Validators.required]),
    date: new FormControl('', [Validators.required]),
    startTime: new FormControl('', [Validators.required]),
    endTime: new FormControl('', [Validators.required]),
  }, { validators: timeRangeValidator });

  get timeRangeError() {
    return this.form.errors?.['timeRange'] &&
      this.form.get('startTime')?.touched &&
      this.form.get('endTime')?.touched;
  }

  ngOnInit() {
    this.facilityService.getActive().subscribe(f => this.facilities.set(f));
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading.set(true);

    const date = this.form.value.date!;
    const startTime = this.form.value.startTime!;
    const endTime = this.form.value.endTime!;

    const startISO = new Date(`${date}T${startTime}:00`).toISOString();
    const endISO = new Date(`${date}T${endTime}:00`).toISOString();

    this.bookingService.create({
      facilityId: this.form.value.facilityId!,
      date: date,          // ← send the date field so backend stores it correctly
      startTime: startISO,
      endTime: endISO,
    }).subscribe({
      next: () => { this.toast.success('Booking created!'); this.router.navigate(['/bookings']); },
      error: () => this.loading.set(false)
    });
  }
}
