import { Component, inject, signal, OnDestroy, AfterViewInit, ElementRef, ViewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { VisitorService } from '../visitor.service';
import { VisitorEntryResponse } from '../visitor.model';
import { ToastService } from '../../../core/services/toast.service';
import { Html5Qrcode } from 'html5-qrcode';

@Component({
  selector: 'app-visitor-checkin',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './visitor-checkin.component.html'
})
export class VisitorCheckinComponent implements AfterViewInit, OnDestroy {
  private svc = inject(VisitorService);
  private toast = inject(ToastService);

  mode = signal<'camera' | 'upload'>('camera');
  loading = signal(false);
  result = signal<VisitorEntryResponse | null>(null);
  selectedFile = signal<File | null>(null);
  cameraError = signal<string | null>(null);
  scanning = signal(false);

  private html5Qrcode: Html5Qrcode | null = null;
  private scannerInitialized = false;

  ngAfterViewInit() {
    // Small delay to let the DOM render the reader element
    setTimeout(() => this.startCamera(), 300);
  }

  ngOnDestroy() {
    this.stopCamera();
  }

  switchMode(newMode: 'camera' | 'upload') {
    if (this.mode() === newMode) return;

    if (newMode === 'upload') {
      this.stopCamera();
    }

    this.mode.set(newMode);
    this.result.set(null);
    this.cameraError.set(null);

    if (newMode === 'camera') {
      setTimeout(() => this.startCamera(), 300);
    }
  }

  private async startCamera() {
    const readerElement = document.getElementById('qr-reader');
    if (!readerElement) {
      this.cameraError.set('Scanner element not found. Please try refreshing.');
      return;
    }

    try {
      this.scanning.set(true);
      this.cameraError.set(null);
      this.html5Qrcode = new Html5Qrcode('qr-reader');

      await this.html5Qrcode.start(
        { facingMode: 'environment' },
        {
          fps: 10,
          qrbox: { width: 250, height: 250 },
          aspectRatio: 1.0,
        },
        (decodedText) => {
          this.onQrDecoded(decodedText);
        },
        () => {
          // Ignore scan failures (no QR found in frame)
        }
      );
      this.scannerInitialized = true;
    } catch (err: any) {
      this.scanning.set(false);
      const message = err?.message || err?.toString() || 'Unknown error';

      if (message.includes('NotAllowedError') || message.includes('Permission')) {
        this.cameraError.set('Camera permission denied. Please allow camera access and try again, or use the Upload mode.');
      } else if (message.includes('NotFoundError') || message.includes('no camera')) {
        this.cameraError.set('No camera found on this device. Use the Upload mode instead.');
      } else {
        this.cameraError.set(`Could not start camera: ${message}`);
      }
    }
  }

  private async stopCamera() {
    if (this.html5Qrcode && this.scannerInitialized) {
      try {
        await this.html5Qrcode.stop();
      } catch {
        // Ignore stop errors
      }
      this.scannerInitialized = false;
    }
    this.scanning.set(false);
  }

  private onQrDecoded(token: string) {
    // Prevent duplicate calls while processing
    if (this.loading()) return;

    this.stopCamera();
    this.loading.set(true);

    this.svc.checkInByToken(token).subscribe({
      next: (entry) => {
        this.result.set(entry);
        this.loading.set(false);
        this.toast.success('Visitor checked in!');
      },
      error: () => {
        this.loading.set(false);
        // Restart camera so they can try again
        setTimeout(() => this.startCamera(), 500);
      }
    });
  }

  // --- Upload fallback ---

  onFileSelect(event: Event) {
    const files = (event.target as HTMLInputElement).files;
    this.selectedFile.set(files?.[0] ?? null);
  }

  checkInFromFile() {
    const file = this.selectedFile();
    if (!file) return;
    this.loading.set(true);
    this.svc.checkIn(file).subscribe({
      next: (entry) => {
        this.result.set(entry);
        this.loading.set(false);
        this.toast.success('Visitor checked in!');
      },
      error: () => this.loading.set(false)
    });
  }

  scanAgain() {
    this.result.set(null);
    this.selectedFile.set(null);
    if (this.mode() === 'camera') {
      setTimeout(() => this.startCamera(), 300);
    }
  }
}
