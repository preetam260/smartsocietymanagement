import {
  Component,
  inject,
  signal,
  computed,
  OnInit,
  PLATFORM_ID
} from '@angular/core';

import { isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin, catchError, of } from 'rxjs';

import {
  ChartData,
  ChartOptions
} from 'chart.js';

import { BaseChartDirective } from 'ng2-charts';

import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../users/user.service';
import { BillService } from '../bills/bill.service';
import { ComplaintService } from '../complaints/complaint.service';
import { FacilityService } from '../facilities/facility.service';
import { AnnouncementService } from '../announcements/announcement.service';
import { VisitorService } from '../visitors/visitor.service';
import { ApartmentService } from '../apartments/apartment.service';

import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

interface DashboardStats {
  totalUsers?: number;
  openComplaints?: number;
  inProgressComplaints?: number;
  activeFacilities?: number;
  pendingVisitors?: number;
  totalAnnouncements?: number;

  myApartments?: number;
  myUnpaidBills?: number;
  myOpenComplaints?: number;
  myAnnouncements?: number;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    StatCardComponent,
    LoadingSpinnerComponent,
    BaseChartDirective
  ],
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
  private platformId = inject(PLATFORM_ID);
  isBrowser = isPlatformBrowser(this.platformId);

  loading = signal(true);
  stats = signal<DashboardStats>({});

  userName = computed(() => this.auth.currentUser()?.name ?? 'User');

  role = computed(() => this.auth.effectiveRole());

  doughnutData = signal<ChartData<'doughnut'>>({labels: [],datasets: []});
  
  barData = signal<ChartData<'bar'>>({labels: [],datasets: []});

  doughnutOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    maintainAspectRatio: false,

    cutout: '72%',

    plugins: {
      legend: {
        position: 'bottom',

        labels: {
          usePointStyle: true,
          pointStyle: 'circle',
          padding: 18
        }
      }
    }
  };

  barOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,

    plugins: {
      legend: {
        display: false
      }
    },

    scales: {
      x: {
        grid: {
          display: false
        }
      },

      y: {
        beginAtZero: true,

        ticks: {
          precision: 0
        }
      }
    }
  };

  ngOnInit(): void {
    if (this.isBrowser) {
      this.loadDashboard();
    } else {
      this.loading.set(false);
    }
  }

  private loadDashboard(): void {
    this.loading.set(true);

    switch (this.role()) {
      case 'Admin':
        this.loadAdminDashboard();
        break;

      case 'Owner':
        this.loadOwnerDashboard();
        break;

      case 'Resident':
        this.loadResidentDashboard();
        break;

      case 'SecurityStaff':
        this.loadSecurityDashboard();
        break;

      default:
        this.loading.set(false);
    }
  }


  private loadAdminDashboard(): void {
    forkJoin({
      users: this.userService.getAllPaged({pageNumber: 1,pageSize: 1})
        .pipe(catchError(() =>
            of({
              totalCount: 0,
              items: []
            })
          )
        ),

      complaints: this.complaintService
        .getAll().pipe(
          catchError(() => of([]))),

      facilities: this.facilityService
        .getActive().pipe(
          catchError(() => of([]))),

      visitors: this.visitorService
        .getByStatus('Pending').pipe(
          catchError(() => of([]))),

      announcements: this.announcementService
        .getAllPaged({pageNumber: 1,pageSize: 1})
        .pipe(catchError(() =>
            of({
              totalCount: 0,
              items: []
            })
          )
        )
    }).subscribe({
      next: ({users,complaints,facilities,visitors,announcements
      }) => {
        const complaintList =
          complaints as any[];

        const openComplaints =
          complaintList.filter(
            complaint =>
              complaint.status === 'Open'
          ).length;

        const inProgressComplaints =
          complaintList.filter(
            complaint =>
              complaint.status === 'InProgress'
          ).length;

        const resolvedComplaints =
          complaintList.filter(
            complaint =>
              complaint.status === 'Resolved' ||
              complaint.status === 'Closed'
          ).length;

        const totalUsers =
          (users as any).totalCount ?? 0;

        const activeFacilities =
          facilities.length;

        const pendingVisitors =
          visitors.length;

        const totalAnnouncements =
          (announcements as any).totalCount ?? 0;

        this.stats.set({totalUsers,openComplaints,inProgressComplaints,activeFacilities,pendingVisitors,totalAnnouncements
        });

        this.doughnutData.set({
          labels: ['Open','In Progress','Resolved'],

          datasets: [
            {
              data: [openComplaints,inProgressComplaints,resolvedComplaints],
              backgroundColor: ['#dc2626','#d97706','#0f766e'],
              borderWidth: 0,
              hoverOffset: 5
            }
          ]
        });

        this.barData.set({
          labels: ['Users','Facilities','Visitors','Announcements'],
          datasets: [
            {
              label: 'Total',
              data: [totalUsers,activeFacilities,pendingVisitors,totalAnnouncements],
              backgroundColor: ['#0f766e','#0891b2','#d97706','#2563eb'],
              borderRadius: 6,
              maxBarThickness: 48
            }
          ]
        });

        this.loading.set(false);
      },

      error: () => {
        this.loading.set(false);
      }
    });
  }

  private loadOwnerDashboard(): void {
    forkJoin({
      bills: this.billService
        .getMyBills().pipe(
          catchError(() => of([]))
        ),

      complaints: this.complaintService
        .getMyComplaints().pipe(
          catchError(() => of([]))
        ),

      apartments: this.apartmentService
        .getMyApartments().pipe(
          catchError(() => of([]))
        ),

      announcements: this.announcementService
        .getMine().pipe(
          catchError(() => of([]))
        )
    }).subscribe({
      next: ({bills,complaints,apartments,announcements
      }) => {
        const billList = bills as any[];
        const complaintList = complaints as any[];
        const unpaidBills = billList.filter(bill =>
              bill.status === 'Unpaid').length;
        const overdueBills = billList.filter(bill => bill.status === 'Overdue').length;
        const paidBills = billList.filter(
            bill => bill.status === 'Paid').length;

        const openComplaints =
          complaintList.filter(
            complaint =>
              complaint.status === 'Open'
          ).length;

        const inProgressComplaints =
          complaintList.filter(
            complaint =>
              complaint.status === 'InProgress'
          ).length;

        this.stats.set({
          myUnpaidBills: unpaidBills + overdueBills,

          myOpenComplaints: openComplaints +inProgressComplaints,

          myApartments:apartments.length,

          myAnnouncements:announcements.length
        });

        this.doughnutData.set({
          labels: ['Paid', 'Unpaid', 'Overdue'],

          datasets: [
            {
              data: [paidBills,unpaidBills,overdueBills],

              backgroundColor: ['#0f766e',
                '#d97706',
                '#dc2626'
              ],

              borderWidth: 0,
              hoverOffset: 5
            }
          ]
        });

        this.barData.set({
          labels: [
            'Apartments',
            'Complaints',
            'Announcements'
          ],

          datasets: [
            {
              label: 'Total',

              data: [
                apartments.length,
                openComplaints +
                  inProgressComplaints,
                announcements.length
              ],

              backgroundColor: [
                '#0f766e',
                '#d97706',
                '#2563eb'
              ],

              borderRadius: 6,
              maxBarThickness: 48
            }
          ]
        });

        this.loading.set(false);
      },

      error: () => {
        this.loading.set(false);
      }
    });
  }

  // =========================
  // RESIDENT
  // =========================

  private loadResidentDashboard(): void {
    forkJoin({
      bills: this.billService
        .getMyBills()
        .pipe(
          catchError(() => of([]))
        ),

      complaints: this.complaintService
        .getMyComplaints()
        .pipe(
          catchError(() => of([]))
        ),

      announcements: this.announcementService
        .getMine()
        .pipe(
          catchError(() => of([]))
        )
    }).subscribe({
      next: ({
        bills,
        complaints,
        announcements
      }) => {
        const billList = bills as any[];
        const complaintList = complaints as any[];

        const paidBills =
          billList.filter(
            bill =>
              bill.status === 'Paid'
          ).length;

        const unpaidBills =
          billList.filter(
            bill =>
              bill.status === 'Unpaid'
          ).length;

        const overdueBills =
          billList.filter(
            bill =>
              bill.status === 'Overdue'
          ).length;

        const openComplaints =
          complaintList.filter(
            complaint =>
              complaint.status === 'Open'
          ).length;

        const inProgressComplaints =
          complaintList.filter(
            complaint =>
              complaint.status === 'InProgress'
          ).length;

        const resolvedComplaints =
          complaintList.filter(
            complaint =>
              complaint.status === 'Resolved' ||
              complaint.status === 'Closed'
          ).length;

        this.stats.set({
          myUnpaidBills:
            unpaidBills + overdueBills,

          myOpenComplaints:
            openComplaints +
            inProgressComplaints,

          myAnnouncements:
            announcements.length
        });

        // Bill status doughnut
        this.doughnutData.set({
          labels: [
            'Paid',
            'Unpaid',
            'Overdue'
          ],

          datasets: [
            {
              data: [
                paidBills,
                unpaidBills,
                overdueBills
              ],

              backgroundColor: [
                '#0f766e',
                '#d97706',
                '#dc2626'
              ],

              borderWidth: 0,
              hoverOffset: 5
            }
          ]
        });

        // Personal activity
        this.barData.set({
          labels: [
            'Open',
            'In Progress',
            'Resolved',
            'Announcements'
          ],

          datasets: [
            {
              label: 'Total',

              data: [
                openComplaints,
                inProgressComplaints,
                resolvedComplaints,
                announcements.length
              ],

              backgroundColor: [
                '#dc2626',
                '#d97706',
                '#0f766e',
                '#2563eb'
              ],

              borderRadius: 6,
              maxBarThickness: 48
            }
          ]
        });

        this.loading.set(false);
      },

      error: () => {
        this.loading.set(false);
      }
    });
  }

  // =========================
  // SECURITY
  // =========================

  private loadSecurityDashboard(): void {
    this.visitorService
      .getByStatus('Pending')
      .pipe(
        catchError(() => of([]))
      )
      .subscribe({
        next: visitors => {
          const pendingVisitors =
            visitors.length;

          this.stats.set({
            pendingVisitors
          });

          this.barData.set({
            labels: [
              'Pending Visitors'
            ],

            datasets: [
              {
                label: 'Visitors',

                data: [
                  pendingVisitors
                ],

                backgroundColor: [
                  '#d97706'
                ],

                borderRadius: 6,
                maxBarThickness: 70
              }
            ]
          });

          this.loading.set(false);
        },

        error: () => {
          this.loading.set(false);
        }
      });
  }
}