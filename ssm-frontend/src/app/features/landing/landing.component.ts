import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css']
})
export class LandingComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    if (this.auth.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
  }

  readonly roles = [
    {
      icon: '🛡️',
      title: 'Admin',
      description: 'Full control over users, apartments, bills, complaints, facilities, and announcements.',
      highlights: ['Manage all residents & owners', 'Generate & track bills', 'Handle complaints workflow']
    },
    {
      icon: '🏠',
      title: 'Resident',
      description: 'Your daily hub for bills, complaints, facility bookings, and visitor management.',
      highlights: ['Pay bills online', 'Register visitors with QR', 'Book facilities & track complaints']
    },
    {
      icon: '🏢',
      title: 'Owner',
      description: 'Oversee your apartments, track billing and manage bookings for all properties.',
      highlights: ['View all owned apartments', 'Track outstanding bills', 'Book society facilities']
    },
    {
      icon: '🔒',
      title: 'Security Staff',
      description: 'Gate management made easy — check in visitors with QR code scanning.',
      highlights: ['QR code check-in', 'View pending visitors', 'Real-time notifications']
    },
  ];

  readonly features = [
    { icon: '⚡', title: 'Real-time Notifications', desc: 'Instant updates via SignalR WebSocket push' },
    { icon: '🔐', title: 'Role-based Access', desc: 'Every user sees exactly what they need' },
    { icon: '📊', title: 'Smart Dashboards', desc: 'Personalised stats and quick actions per role' },
    { icon: '💳', title: 'Digital Billing', desc: 'Automated bills with penalty calculation' },
    { icon: '📅', title: 'Facility Booking', desc: 'Easy online booking with conflict detection' },
    { icon: '🚶', title: 'Visitor Management', desc: 'QR-based visitor check-in & checkout' },
  ];
}
