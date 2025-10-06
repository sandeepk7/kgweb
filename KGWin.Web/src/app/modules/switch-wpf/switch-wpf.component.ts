import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SwitchWpfService } from './switch-wpf.service';

@Component({
  selector: 'app-switch-wpf',
  templateUrl: './switch-wpf.component.html',
  styleUrls: ['./switch-wpf.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class SwitchWpfComponent implements OnInit, OnDestroy {
  countdown = 10;
  isCountingDown = false;
  private countdownInterval: any;

  constructor(private switchWpfService: SwitchWpfService) {}

  ngOnInit(): void {
    // Component initialization
  }

  ngOnDestroy(): void {
    this.clearCountdown();
  }

  onSwitchWithDelay(): void {
    if (this.isCountingDown) {
      return; // Prevent multiple countdowns
    }

    this.isCountingDown = true;
    this.countdown = 10;

    this.countdownInterval = setInterval(() => {
      this.countdown--;
      
      if (this.countdown <= 0) {
        this.clearCountdown();
        this.switchToWpfMap();
      }
    }, 1000);
  }

  onSwitchInstantly(): void {
    this.clearCountdown();
    this.switchToWpfMap();
  }

  onCancelCountdown(): void {
    this.clearCountdown();
  }

  private clearCountdown(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
      this.countdownInterval = null;
    }
    this.isCountingDown = false;
    this.countdown = 10;
  }

  private switchToWpfMap(): void {
    this.switchWpfService.switchToWpfMap();
  }

  onCloseWpfPopups(): void {
    this.switchWpfService.closeAllWpfPopups();
  }

  onMinimizePopup(): void {
    this.switchWpfService.minimizeLastWpfPopup();
  }

  onMaximizePopup(): void {
    this.switchWpfService.maximizeLastWpfPopup();
  }

  onRestorePopup(): void {
    this.switchWpfService.restoreLastWpfPopup();
  }
}
