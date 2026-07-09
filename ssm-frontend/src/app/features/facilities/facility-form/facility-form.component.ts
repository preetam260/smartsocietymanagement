import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { FacilityService } from '../facility.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-facility-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './facility-form.component.html'
})
export class FacilityFormComponent implements OnInit {
  private svc = inject(FacilityService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);
  isEdit = signal(false);
  loading = signal(false);
  facilityId = '';

  form = new FormGroup({
    name: new FormControl('', [Validators.required]),
    description: new FormControl('', [Validators.required]),
    hourlyRate: new FormControl<number>(0, [Validators.required, Validators.min(0)]),
    capacity: new FormControl<number>(1, [Validators.required, Validators.min(1)]),
    isActive: new FormControl<boolean>(true),
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.facilityId = id;
      this.svc.getById(id).subscribe(f => this.form.patchValue(f));
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    const dto = {
      name: this.form.value.name!,
      description: this.form.value.description!,
      hourlyRate: this.form.value.hourlyRate!,
      capacity: this.form.value.capacity!,
      isActive: this.form.value.isActive === true || this.form.value.isActive === 'true' as any,
    };
    const obs = this.isEdit() ? this.svc.update(this.facilityId, dto) : this.svc.create(dto);
    obs.subscribe({
      next: () => { this.toast.success(this.isEdit() ? 'Facility updated' : 'Facility created'); this.router.navigate(['/facilities']); },
      error: () => this.loading.set(false)
    });
  }
}
