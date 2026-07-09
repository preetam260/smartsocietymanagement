import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ResidentService } from '../resident.service';
import { UserService } from '../../users/user.service';
import { ApartmentService } from '../../apartments/apartment.service';
import { UserResponse } from '../../users/user.model';
import { ApartmentResponse } from '../../apartments/apartment.model';
import { ToastService } from '../../../core/services/toast.service';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-resident-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './resident-form.component.html'
})
export class ResidentFormComponent implements OnInit {
  private svc = inject(ResidentService);
  private userService = inject(UserService);
  private aptService = inject(ApartmentService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  isEdit = signal(false);
  loading = signal(false);
  residentId = '';
  users = signal<UserResponse[]>([]);
  apartments = signal<ApartmentResponse[]>([]);

  form = new FormGroup({
    userId: new FormControl('', [Validators.required]),
    apartmentId: new FormControl('', [Validators.required]),
    moveInDate: new FormControl('', [Validators.required]),
    vehicleNumber: new FormControl(''),
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.residentId = id;
      this.form.controls.userId.disable();
      this.form.controls.apartmentId.disable();
      this.form.controls.moveInDate.disable();
      this.svc.getById(id).subscribe(r => this.form.patchValue({ vehicleNumber: r.vehicleNumber ?? '' }));
    } else {
      forkJoin([
        this.userService.getByRole('Resident'),
        this.userService.getByRole('Owner'),
        this.aptService.getAllForDropdown()
      ]).subscribe(([residents, owners, aptResult]) => {
        this.users.set([...residents, ...owners]);
        this.apartments.set(aptResult.items);
      });
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    if (this.isEdit()) {
      this.svc.update(this.residentId, { vehicleNumber: this.form.value.vehicleNumber || undefined }).subscribe({
        next: () => { this.toast.success('Resident updated'); this.router.navigate(['/residents']); },
        error: () => this.loading.set(false)
      });
    } else {
      this.svc.create({
        userId: this.form.value.userId!,
        apartmentId: this.form.value.apartmentId!,
        moveInDate: new Date(this.form.value.moveInDate!).toISOString(),
        vehicleNumber: this.form.value.vehicleNumber || undefined,
      }).subscribe({
        next: () => { this.toast.success('Resident added'); this.router.navigate(['/residents']); },
        error: () => this.loading.set(false)
      });
    }
  }
}
