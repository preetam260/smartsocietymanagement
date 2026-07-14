import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AnnouncementService } from '../announcement.service';
import { AnnouncementResponse } from '../announcement.model';
import { AuthService } from '../../../core/services/auth.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-announcement-feed',
  standalone: true,
  imports: [RouterLink, LoadingSpinnerComponent, EmptyStateComponent, DatePipe],
  templateUrl: './announcement-feed.component.html'
})
export class AnnouncementFeedComponent implements OnInit {
  private svc = inject(AnnouncementService);
  auth = inject(AuthService);
  loading = signal(true);
  pinned = signal<AnnouncementResponse[]>([]);
  announcements = signal<AnnouncementResponse[]>([]);


  ngOnInit() {
    this.svc.getMine().subscribe(a => {
      this.pinned.set(a.filter(x => x.isPinned));
      this.announcements.set(a.filter(x => !x.isPinned));
      this.loading.set(false);
    });
  }
}
