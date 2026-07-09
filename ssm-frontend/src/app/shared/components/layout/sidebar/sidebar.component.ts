import { Component, computed, inject, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { UserRole } from '../../../../core/models/enums';

interface NavLink {
  path: string;
  label: string;
  icon: string;
  roles: UserRole[] | 'all';
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent {
  open = input<boolean>(false);
  linkClicked = output<void>();

  private auth = inject(AuthService);

  private allLinks: NavLink[] = [
    { path: '/dashboard', label: 'Dashboard', icon: '📊', roles: 'all' },
    { path: '/users', label: 'Users', icon: '👥', roles: ['Admin'] },
    { path: '/residents', label: 'Residents', icon: '🏠', roles: ['Admin'] },
    { path: '/apartments', label: 'Apartments', icon: '🏢', roles: ['Admin', 'Owner'] },
    { path: '/complaints', label: 'Complaints', icon: '📝', roles: ['Admin', 'Resident', 'Owner'] },
    { path: '/bills', label: 'Bills', icon: '💰', roles: ['Admin', 'Resident', 'Owner'] },
    { path: '/facilities', label: 'Facilities', icon: '🏋️', roles: ['Admin', 'Resident', 'Owner'] },
    { path: '/bookings', label: 'Bookings', icon: '📅', roles: ['Admin', 'Resident', 'Owner'] },
    { path: '/visitors', label: 'Visitors', icon: '🚶', roles: ['Admin', 'Resident', 'Owner', 'SecurityStaff'] },
    { path: '/announcements', label: 'Announcements', icon: '📢', roles: 'all' },
    { path: '/notifications', label: 'Notifications', icon: '🔔', roles: 'all' },
  ];

  visibleLinks = computed(() => {
    const role = this.auth.role();
    if (!role) return [];
    return this.allLinks.filter(link => link.roles === 'all' || link.roles.includes(role))
    .map(link => {
      let path = link.path;
      if(link.path === '/bills' && (role === 'Resident' || role === 'Owner')) {
        path = '/bills/my';
      }
      else if(link.path === '/visitors' && (role === 'Resident' || role === 'Owner')) {
        path = '/visitors/my';
      }
      else if (link.path === '/apartments' && (role === 'Owner' || role == 'Resident')){
        path = '/apartments/my';
      }
      return { ...link, path }; 
    })
  });
}
