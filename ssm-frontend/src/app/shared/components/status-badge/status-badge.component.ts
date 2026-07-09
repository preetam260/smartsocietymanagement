import { Component, input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  templateUrl: './status-badge.component.html',
  styleUrl: './status-badge.component.css'
})
export class StatusBadgeComponent {
  status = input.required<string>();

  badgeClass() {
    const s = this.status();
    const greenStatuses = ['Paid', 'Confirmed', 'Resolved', 'Approved', 'CheckedOut', 'Completed', 'Active'];
    const amberStatuses = ['Pending', 'Processing', 'Held'];
    const redStatuses = ['Overdue', 'Cancelled', 'Denied', 'Expired', 'Disputed', 'Open', 'Inactive'];
    const greyStatuses = ['Closed', 'CheckedIn'];

    if (greenStatuses.includes(s)) return 'badge badge-green';
    if (amberStatuses.includes(s)) return 'badge badge-amber';
    if (redStatuses.includes(s)) return 'badge badge-red';
    if (greyStatuses.includes(s)) return 'badge badge-grey';
    return 'badge badge-blue';
  }
}
