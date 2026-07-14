import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { VisitorService } from '../visitor.service';
import { VisitorResponse } from '../visitor.model';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-register-visitor',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register-visitor.component.html',
  styleUrls: ['./register-visitor.component.css']
})
export class RegisterVisitorComponent implements OnInit {
  private svc = inject(VisitorService);
  private toast = inject(ToastService);
  private router = inject(Router);
  loading = signal(false);
  registeredVisitor = signal<VisitorResponse | null>(null);

  visitor = { name: '', email: '', purpose: '', eta: '' };

  ngOnInit() {
    this.svc.getMyVisitors().subscribe({
      error: () => {
        this.toast.error('You do not have an active residency to register visitors.');
        this.router.navigate(['/dashboard']);
      }
    });
  }

  onSubmit(valid: boolean | null | undefined) {
    if (!valid) return;
    this.loading.set(true);
    this.svc.register({
      name: this.visitor.name,
      email: this.visitor.email,
      purpose: this.visitor.purpose,
      eta: new Date(this.visitor.eta).toISOString(),
    }).subscribe({
      next: (v) => { this.registeredVisitor.set(v); this.loading.set(false); this.toast.success('Visitor registered!'); },
      error: () => this.loading.set(false)
    });
  }
}