import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VisitorService } from '../visitor.service';
import { VisitorEntryResponse } from '../visitor.model';
import { ToastService } from '../../../core/services/toast.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-visitor-checkin',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './visitor-checkin.component.html'
})
export class VisitorCheckinComponent {
  private svc = inject(VisitorService);
  private toast = inject(ToastService);
  selectedFile = signal<File | null>(null);
  loading = signal(false);
  result = signal<VisitorEntryResponse | null>(null);

  onFileSelect(event: Event) {
    const files = (event.target as HTMLInputElement).files;
    this.selectedFile.set(files?.[0] ?? null);
  }

  checkIn() {
    const file = this.selectedFile();
    if (!file) return;
    this.loading.set(true);
    this.svc.checkIn(file).subscribe({
      next: (entry) => { this.result.set(entry); this.loading.set(false); this.toast.success('Visitor checked in!'); },
      error: () => this.loading.set(false)
    });
  }
}
