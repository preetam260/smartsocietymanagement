import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../users/user.service';
import { BillService } from '../bills/bill.service';
import { ComplaintService } from '../complaints/complaint.service';
import { FacilityService } from '../facilities/facility.service';
import { AnnouncementService } from '../announcements/announcement.service';
import { VisitorService } from '../visitors/visitor.service';
import { ApartmentService } from '../apartments/apartment.service';
import { ResidentService } from '../residents/resident.service';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { forkJoin, catchError, of } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, StatCardComponent, LoadingSpinnerComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  auth = inject(AuthService);
  private userService = inject(UserService);
  private billService = inject(BillService);
  private complaintService = inject(ComplaintService);
  private facilityService = inject(FacilityService);
  private announcementService = inject(AnnouncementService);
  private visitorService = inject(VisitorService);
  private apartmentService = inject(ApartmentService);
  private residentService = inject(ResidentService);

  loading = signal(true);
  stats = signal<any>({});

  userName = computed(() => this.auth.currentUser()?.name ?? 'User');

  ngOnInit() {
    const role = this.auth.effectiveRole();
    switch (role) {
      case 'Admin':
        forkJoin({
          users: this.userService.getAllPaged({ pageNumber: 1, pageSize: 1 }).pipe(catchError(() => of({ totalCount: 0, items: [] }))),
          complaints: this.complaintService.getAll().pipe(catchError(() => of([]))),
          facilities: this.facilityService.getActive().pipe(catchError(() => of([]))),
          visitors: this.visitorService.getByStatus('Pending').pipe(catchError(() => of([]))),
          announcements: this.announcementService.getAllPaged({ pageNumber: 1, pageSize: 1 }).pipe(catchError(() => of({ totalCount: 0, items: [] }))),
        }).subscribe(({ users, complaints, facilities, visitors, announcements }) => {
          this.stats.set({
            totalUsers: (users as any).totalCount,
            openComplaints: (complaints as any[]).filter(c => c.status === 'Open').length,
            inProgressComplaints: (complaints as any[]).filter(c => c.status === 'InProgress').length,
            activeFacilities: facilities.length,
            pendingVisitors: visitors.length,
            totalAnnouncements: (announcements as any).totalCount,
          });
          this.loading.set(false);
        });
        break;

      case 'Owner':
        forkJoin({
          bills: this.billService.getMyBills().pipe(catchError(() => of([]))),
          complaints: this.complaintService.getMyComplaints().pipe(catchError(() => of([]))),
          apartments: this.apartmentService.getMyApartments().pipe(catchError(() => of([]))),
          announcements: this.announcementService.getMine().pipe(catchError(() => of([]))),
        }).subscribe(({ bills, complaints, apartments, announcements }) => {
          this.stats.set({
            myUnpaidBills: (bills as any[]).filter(b => b.status === 'Unpaid' || b.status === 'Overdue').length,
            myOpenComplaints: (complaints as any[]).filter(c => c.status === 'Open' || c.status === 'InProgress').length,
            myApartments: apartments.length,
            myAnnouncements: announcements.length,
          });
          this.loading.set(false);
        });
        break;

      case 'Resident':
        forkJoin({
          bills: this.billService.getMyBills().pipe(catchError(() => of([]))),
          complaints: this.complaintService.getMyComplaints().pipe(catchError(() => of([]))),
          announcements: this.announcementService.getMine().pipe(catchError(() => of([]))),
        }).subscribe(({ bills, complaints, announcements }) => {
          this.stats.set({
            myUnpaidBills: (bills as any[]).filter(b => b.status === 'Unpaid' || b.status === 'Overdue').length,
            myOpenComplaints: (complaints as any[]).filter(c => c.status === 'Open' || c.status === 'InProgress').length,
            myAnnouncements: announcements.length,
          });
          this.loading.set(false);
        });
        break;

      case 'SecurityStaff':
        this.visitorService.getByStatus('Pending').pipe(catchError(() => of([]))).subscribe(visitors => {
          this.stats.set({ pendingVisitors: visitors.length });
          this.loading.set(false);
        });
        break;

      default:
        this.loading.set(false);
    }
  }
}
