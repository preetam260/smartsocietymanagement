import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { AnnouncementService } from '../announcement.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserRole } from '../../../core/models/enums';

@Component({
  selector: 'app-announcement-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './announcement-form.component.html'
})
export class AnnouncementFormComponent implements OnInit {
  private svc = inject(AnnouncementService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toast = inject(ToastService);
  isEdit = signal(false);
  loading = signal(false);
  announcementId = '';

  form = new FormGroup({
    title: new FormControl('', [Validators.required]),
    content: new FormControl('', [Validators.required]),
    audience: new FormControl<UserRole>('Resident', [Validators.required]),
    isPinned: new FormControl<boolean>(false),
    expiresAt: new FormControl('', [Validators.required]),
  });

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit.set(true);
      this.announcementId = id;
      this.svc.getById(id).subscribe(a => {
        this.form.patchValue({
          ...a,
          expiresAt: a.expiresAt.split('T')[0],
        });
      });
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading.set(true);
    const dto = {
      title: this.form.value.title!,
      content: this.form.value.content!,
      audience: this.form.value.audience!,
      isPinned: this.form.value.isPinned === true || this.form.value.isPinned === 'true' as any,
      expiresAt: new Date(this.form.value.expiresAt!).toISOString(),
    };
    const obs = this.isEdit() ? this.svc.update(this.announcementId, dto) : this.svc.create(dto);
    obs.subscribe({
      next: () => { this.toast.success(this.isEdit() ? 'Updated' : 'Created'); this.router.navigate(['/announcements/manage']); },
      error: () => this.loading.set(false)
    });
  }
}
