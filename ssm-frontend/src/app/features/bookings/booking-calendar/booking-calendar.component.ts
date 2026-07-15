import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Subscription, timer } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { FacilityResponse } from '../../facilities/facility.model';
import { FacilityService } from '../../facilities/facility.service';
import { BookingAvailabilitySlot, BookingCalendarResponse } from '../booking.model';
import { BookingService } from '../booking.service';

type CalendarView = 'day' | 'week' | 'month';
type DayPart = 'all' | 'morning' | 'afternoon' | 'evening';

interface CalendarSlot extends BookingAvailabilitySlot {
  start: Date;
  end: Date;
  key: string;
}

interface MonthDaySummary {
  minAvailable: number;
  peakReserved: number;
  fullSlots: number;
  level: 'Available' | 'FillingFast' | 'Full';
}

@Component({
  selector: 'app-booking-calendar',
  standalone: true,
  imports: [RouterLink, DatePipe, CurrencyPipe, LoadingSpinnerComponent],
  templateUrl: './booking-calendar.component.html',
  styleUrls: ['./booking-calendar.component.css']
})
export class BookingCalendarComponent implements OnInit, OnDestroy {
  readonly auth = inject(AuthService);
  private readonly facilityService = inject(FacilityService);
  private readonly bookingService = inject(BookingService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  facilities = signal<FacilityResponse[]>([]);
  facilityId = signal('');
  anchorDate = signal(this.startOfDay(new Date()));
  view = signal<CalendarView>('day');
  dayPart = signal<DayPart>('all');
  calendar = signal<BookingCalendarResponse | null>(null);
  loading = signal(true);
  booking = signal(false);
  loadError = signal('');
  seats = signal(1);
  selectedSlots = signal<CalendarSlot[]>([]);

  private refreshSubscription?: Subscription;

  canBook = computed(() => this.auth.effectiveRole() === 'Resident');
  isAdmin = computed(() => this.auth.role() === 'Admin');
  selectedFacility = computed(() =>
    this.facilities().find(f => f.id === this.facilityId()) ?? null
  );

  slots = computed<CalendarSlot[]>(() =>
    (this.calendar()?.slots ?? []).map(slot => {
      const start = new Date(slot.startTime);
      return {
        ...slot,
        start,
        end: new Date(slot.endTime),
        key: start.toISOString()
      };
    })
  );

  slotMap = computed(() => {
    const map = new Map<string, CalendarSlot>();
    for (const slot of this.slots()) map.set(this.localDateTimeKey(slot.start), slot);
    return map;
  });

  visibleSlotIndexes = computed(() => {
    const indexes = Array.from({ length: 48 }, (_, index) => index);
    switch (this.dayPart()) {
      case 'morning': return indexes.filter(i => i >= 12 && i < 24);
      case 'afternoon': return indexes.filter(i => i >= 24 && i < 36);
      case 'evening': return indexes.filter(i => i >= 36);
      default: return indexes;
    }
  });

  daySlots = computed(() => {
    const dateKey = this.localDateKey(this.anchorDate());
    const allowed = new Set(this.visibleSlotIndexes());
    return this.slots().filter(slot =>
      this.localDateKey(slot.start) === dateKey && allowed.has(this.halfHourIndex(slot.start))
    );
  });

  weekDays = computed(() => {
    const start = this.startOfWeek(this.anchorDate());
    return Array.from({ length: 7 }, (_, index) => this.addDays(start, index));
  });

  monthDays = computed(() => {
    const anchor = this.anchorDate();
    const first = new Date(anchor.getFullYear(), anchor.getMonth(), 1);
    const start = this.startOfWeek(first);
    return Array.from({ length: 42 }, (_, index) => this.addDays(start, index));
  });

  rangeLabel = computed(() => {
    const anchor = this.anchorDate();
    if (this.view() === 'day') {
      return new Intl.DateTimeFormat('en-IN', {
        weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
      }).format(anchor);
    }
    if (this.view() === 'week') {
      const days = this.weekDays();
      const format = new Intl.DateTimeFormat('en-IN', { day: 'numeric', month: 'short' });
      return `${format.format(days[0])} – ${format.format(days[6])}`;
    }
    return new Intl.DateTimeFormat('en-IN', { month: 'long', year: 'numeric' }).format(anchor);
  });

  selectedDuration = computed(() => this.selectedSlots().length * 0.5);
  estimatedCost = computed(() =>
    (this.selectedFacility()?.hourlyRate ?? 0) * this.selectedDuration() * this.seats()
  );

  ngOnInit() {
    this.facilityService.getActive().subscribe({
      next: facilities => {
        this.facilities.set(facilities);
        if (facilities.length) {
          this.facilityId.set(facilities[0].id);
          this.loadCalendar();
        } else {
          this.loading.set(false);
        }
      },
      error: () => this.loading.set(false)
    });

    this.refreshSubscription = timer(60_000, 60_000).subscribe(() => {
      if (this.facilityId()) this.loadCalendar(false);
    });
  }

  ngOnDestroy() {
    this.refreshSubscription?.unsubscribe();
  }

  onFacilityChange(event: Event) {
    this.facilityId.set((event.target as HTMLSelectElement).value);
    this.clearSelection();
    this.loadCalendar();
  }

  setView(view: CalendarView) {
    if (this.view() === view) return;
    this.view.set(view);
    this.clearSelection();
    this.loadCalendar();
  }

  setDayPart(part: DayPart) {
    this.dayPart.set(part);
    this.clearSelection();
  }

  move(direction: number) {
    const current = this.anchorDate();
    const next = new Date(current);
    if (this.view() === 'day') next.setDate(next.getDate() + direction);
    if (this.view() === 'week') next.setDate(next.getDate() + direction * 7);
    if (this.view() === 'month') next.setMonth(next.getMonth() + direction, 1);
    this.anchorDate.set(this.startOfDay(next));
    this.clearSelection();
    this.loadCalendar();
  }

  goToToday() {
    this.anchorDate.set(this.startOfDay(new Date()));
    this.clearSelection();
    this.loadCalendar();
  }

  openDay(date: Date) {
    this.anchorDate.set(this.startOfDay(date));
    this.view.set('day');
    this.clearSelection();
    this.loadCalendar();
  }

  loadCalendar(showLoading = true) {
    const facilityId = this.facilityId();
    if (!facilityId) return;

    const { from, to } = this.calendarRange();
    if (showLoading) this.loading.set(true);
    this.loadError.set('');

    this.bookingService.getCalendar(facilityId, from.toISOString(), to.toISOString()).subscribe({
      next: calendar => {
        this.calendar.set(calendar);
        this.loading.set(false);
        this.reconcileSelection();
      },
      error: () => {
        this.loading.set(false);
        this.loadError.set('Availability could not be loaded. Try refreshing the calendar.');
      }
    });
  }

  slotFor(day: Date, slotIndex: number) {
    const date = new Date(day);
    date.setHours(Math.floor(slotIndex / 2), slotIndex % 2 === 0 ? 0 : 30, 0, 0);
    return this.slotMap().get(this.localDateTimeKey(date));
  }

  timeLabel(slotIndex: number) {
    const date = new Date(2000, 0, 1, Math.floor(slotIndex / 2), slotIndex % 2 === 0 ? 0 : 30);
    return new Intl.DateTimeFormat('en-IN', { hour: 'numeric', minute: '2-digit' }).format(date);
  }

  monthSummary(day: Date): MonthDaySummary {
    const daySlots = this.slots().filter(slot => this.localDateKey(slot.start) === this.localDateKey(day));
    if (!daySlots.length) {
      return { minAvailable: 0, peakReserved: 0, fullSlots: 0, level: 'Available' };
    }

    const minAvailable = Math.min(...daySlots.map(slot => slot.availableSeats));
    const peakReserved = Math.max(...daySlots.map(slot => slot.reservedSeats));
    const fullSlots = daySlots.filter(slot => slot.availableSeats === 0).length;
    const level = minAvailable === 0 ? 'Full' : minAvailable <= 5 ? 'FillingFast' : 'Available';
    return { minAvailable, peakReserved, fullSlots, level };
  }

  toggleSlot(slot: CalendarSlot) {
    if (!this.isSlotSelectable(slot)) return;

    const selected = this.selectedSlots();
    if (selected.some(item => item.key === slot.key)) {
      this.clearSelection();
      return;
    }

    if (!selected.length) {
      this.selectedSlots.set([slot]);
      return;
    }

    const first = selected[0];
    const last = selected[selected.length - 1];
    if (slot.start.getTime() === last.end.getTime()) {
      this.selectedSlots.set([...selected, slot]);
    } else if (slot.end.getTime() === first.start.getTime()) {
      this.selectedSlots.set([slot, ...selected]);
    } else {
      this.selectedSlots.set([slot]);
    }
  }

  isSelected(slot: CalendarSlot) {
    return this.selectedSlots().some(item => item.key === slot.key);
  }

  isSlotSelectable(slot: CalendarSlot) {
    return this.canBook() &&
      slot.start.getTime() > Date.now() &&
      slot.availableSeats >= this.seats();
  }

  isPast(slot: CalendarSlot) {
    return slot.start.getTime() <= Date.now();
  }

  changeSeats(delta: number) {
    const max = Math.min(5, this.selectedFacility()?.capacity ?? 5);
    this.seats.set(Math.max(1, Math.min(max, this.seats() + delta)));
    this.removeUnavailableSelections();
  }

  onSeatsInput(event: Event) {
    const value = Number((event.target as HTMLInputElement).value);
    const max = Math.min(5, this.selectedFacility()?.capacity ?? 5);
    this.seats.set(Math.max(1, Math.min(max, Number.isFinite(value) ? value : 1)));
    this.removeUnavailableSelections();
  }

  bookSelectedSlots() {
    const selected = this.selectedSlots();
    const facility = this.selectedFacility();
    if (!selected.length || !facility || this.booking()) return;

    const start = selected[0].start;
    const end = selected[selected.length - 1].end;
    this.booking.set(true);

    this.bookingService.create({
      facilityId: facility.id,
      date: this.localDateKey(start),
      startTime: start.toISOString(),
      endTime: end.toISOString(),
      seatsBooked: this.seats()
    }).subscribe({
      next: () => {
        this.toast.success('Seats held for 10 minutes. Complete payment to confirm.');
        this.booking.set(false);
        this.clearSelection();
        this.router.navigate(['/bookings/my']);
      },
      error: () => {
        this.booking.set(false);
        this.loadCalendar(false);
      }
    });
  }

  clearSelection() {
    this.selectedSlots.set([]);
  }

  isToday(date: Date) {
    return this.localDateKey(date) === this.localDateKey(new Date());
  }

  isInAnchorMonth(date: Date) {
    const anchor = this.anchorDate();
    return date.getMonth() === anchor.getMonth() && date.getFullYear() === anchor.getFullYear();
  }

  availabilityClass(level: string) {
    return `availability-${level.toLowerCase()}`;
  }

  private removeUnavailableSelections() {
    if (this.selectedSlots().some(slot => slot.availableSeats < this.seats())) {
      this.clearSelection();
    }
  }

  private reconcileSelection() {
    const selectedKeys = new Set(this.selectedSlots().map(slot => slot.key));
    if (!selectedKeys.size) return;

    const refreshed = this.slots().filter(slot => selectedKeys.has(slot.key));
    if (refreshed.length !== selectedKeys.size ||
        refreshed.some(slot => slot.availableSeats < this.seats())) {
      this.clearSelection();
      return;
    }

    this.selectedSlots.set(refreshed);
  }

  private calendarRange() {
    const anchor = this.anchorDate();
    if (this.view() === 'day') {
      const from = this.startOfDay(anchor);
      return { from, to: this.addDays(from, 1) };
    }
    if (this.view() === 'week') {
      const from = this.startOfWeek(anchor);
      return { from, to: this.addDays(from, 7) };
    }

    const first = new Date(anchor.getFullYear(), anchor.getMonth(), 1);
    const from = this.startOfWeek(first);
    return { from, to: this.addDays(from, 42) };
  }

  private halfHourIndex(date: Date) {
    return date.getHours() * 2 + (date.getMinutes() >= 30 ? 1 : 0);
  }

  private startOfDay(date: Date) {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
  }

  private startOfWeek(date: Date) {
    const start = this.startOfDay(date);
    const mondayOffset = (start.getDay() + 6) % 7;
    start.setDate(start.getDate() - mondayOffset);
    return start;
  }

  private addDays(date: Date, days: number) {
    const result = new Date(date);
    result.setDate(result.getDate() + days);
    return result;
  }

  private localDateKey(date: Date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private localDateTimeKey(date: Date) {
    const hour = String(date.getHours()).padStart(2, '0');
    const minute = String(date.getMinutes()).padStart(2, '0');
    return `${this.localDateKey(date)}T${hour}:${minute}`;
  }
}
