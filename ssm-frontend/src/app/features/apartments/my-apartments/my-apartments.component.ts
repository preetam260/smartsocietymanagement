import { Component, inject, signal, OnInit } from '@angular/core';
import { ApartmentService } from '../apartment.service';
import { ApartmentResponse } from '../apartment.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-my-apartments',
  standalone: true,
  imports: [LoadingSpinnerComponent, EmptyStateComponent],
  templateUrl: './my-apartments.component.html',
  styleUrls: ['./my-apartments.component.css']
})
export class MyApartmentsComponent implements OnInit {
  private svc = inject(ApartmentService);
  loading = signal(true);
  apartments = signal<ApartmentResponse[]>([]);
  ngOnInit() { 
    this.svc.getMyApartments().subscribe(a => { 
      this.apartments.set(a); this.loading.set(false); 
    }); 
  }
}
