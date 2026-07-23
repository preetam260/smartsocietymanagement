import { Injectable, inject, OnDestroy } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';


@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private auth = inject(AuthService);

  private readonly hubUrl = environment.apiUrl.replace('/api', '') + '/hubs/notifications';

  private connection: signalR.HubConnection | null = null;
  private started = false;

  startConnection(): void {
    if (this.started) return; 

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.auth.currentUser()?.token ?? ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.connection
      .start()
      .then(() => { this.started = true; })
      .catch(err => console.error('[SignalR] Connection failed:', err));
  }

  stopConnection(): void {
    this.connection?.stop();
    this.connection = null;
    this.started = false;
  }

  on<T>(eventName: string, callback: (data: T) => void): () => void {
    if (!this.connection) {
      console.warn(`[SignalR] on("${eventName}") called before startConnection(). Handler registered but connection not yet started.`);

      return () => {};
    }
    this.connection.on(eventName, callback);
  
    return () => this.connection?.off(eventName, callback);
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}
