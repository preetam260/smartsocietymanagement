import { Component, inject, signal, OnInit, computed } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { UserService } from '../user.service';
import { UserResponse } from '../user.model';
import { ToastService } from '../../../core/services/toast.service';
import { UserRole } from '../../../core/models/enums';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './user-form.component.html'
})
export class UserFormComponent implements OnInit {
  private userService = inject(UserService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);

  isEdit = signal(false);
  loading = signal(false);
  userId = signal('');
  existingUsers = signal<UserResponse[]>([]);
  emailInput = signal('');

  // Signal-based email duplicate hint — UX only, NOT authoritative (fix #6)
  emailHint = computed(() => {
    const email = this.emailInput().toLowerCase();
    if (!email) return false;
    return this.existingUsers().some(u => u.email.toLowerCase() === email);
  });

  form = new FormGroup({
    name: new FormControl('', [Validators.required]),
    email: new FormControl('', [Validators.required, Validators.email]),
    phoneNumber: new FormControl('', [Validators.required]),
    role: new FormControl<UserRole | ''>('', [Validators.required]),
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.userId.set(id);
      this.form.controls.email.disable();
      this.form.controls.role.disable();
      this.userService.getById(id).subscribe(user => {
        this.form.patchValue({ name: user.name, phoneNumber: user.phoneNumber });
      });
    } else {
      // Load existing users for email hint (current page only — UX helper)
      this.userService.getAllPaged({ pageNumber: 1, pageSize: 50 }).subscribe(result => {
        this.existingUsers.set(result.items);
      });
    }
  }

  onEmailInput(event: Event) {
    this.emailInput.set((event.target as HTMLInputElement).value);
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading.set(true);

    if (this.isEdit()) {
      this.userService.update(this.userId(), {
        name: this.form.value.name!,
        phoneNumber: this.form.value.phoneNumber!,
      }).subscribe({
        next: () => {
          this.toast.success('User updated successfully');
          this.router.navigate(['/users']);
        },
        error: () => this.loading.set(false)
      });
    } else {
      this.userService.create({
        name: this.form.value.name!,
        email: (this.form.value as any).email,
        phoneNumber: this.form.value.phoneNumber!,
        role: (this.form.value as any).role,
      }).subscribe({
        next: () => {
          this.toast.success('User created successfully');
          this.router.navigate(['/users']);
        },
        error: () => this.loading.set(false)
      });
    }
  }
}
