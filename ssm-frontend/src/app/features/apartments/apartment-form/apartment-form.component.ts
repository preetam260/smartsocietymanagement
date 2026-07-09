import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { ApartmentService } from '../apartment.service';
import { UserService } from '../../users/user.service';
import { UserResponse } from '../../users/user.model';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-apartment-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './apartment-form.component.html'
})
export class ApartmentFormComponent implements OnInit {
  private svc = inject(ApartmentService);
  private userService = inject(UserService);
  private router = inject(Router);
  private toast = inject(ToastService);
  owners = signal<UserResponse[]>([]);
  loading = signal(false);
  form = new FormGroup({
    ownerId: new FormControl('', [Validators.required]),
    block: new FormControl('', [Validators.required]),
    floor: new FormControl(0, [Validators.required, Validators.min(0)]),
    number: new FormControl('', [Validators.required]),
  });

  ngOnInit() {
    this.userService.getByRole('Owner').subscribe(u => this.owners.set(u));
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    this.svc.create({
      ownerId: this.form.value.ownerId!,
      block: this.form.value.block!,
      floor: this.form.value.floor!,
      number: this.form.value.number!,
    }).subscribe({ next: () => { this.toast.success('Apartment created'); this.router.navigate(['/apartments']); }, error: () => this.loading.set(false) });
  }
}
