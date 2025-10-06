import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommunicationService, CommunicationMessage } from './communication.service';

@Component({
  selector: 'app-communication',
  templateUrl: './communication.component.html',
  styleUrls: ['./communication.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule]
})
export class CommunicationComponent implements OnInit {
  messages: CommunicationMessage[] = [];
  newMessage: string = '';
  isConnected: boolean = false;

  constructor(private communicationService: CommunicationService) {}

  ngOnInit(): void {
    this.communicationService.messages$.subscribe(messages => {
      this.messages = messages;
    });

    this.communicationService.isConnected$.subscribe(isConnected => {
      this.isConnected = isConnected;
    });

    this.communicationService.addSystemMessage('KGWin.Web communication initialized. Waiting for WPF connection...');
  }

  sendMessage(): void {
    if (this.newMessage.trim()) {
      this.communicationService.sendMessage(this.newMessage);
      this.newMessage = '';
    }
  }

  addRandomAssets(): void {
    const jsonString = this.communicationService.addRandomAssets();
    this.newMessage = jsonString;
  }

  addRandomMarkers(): void {
    const jsonString = this.communicationService.addRandomMarkers();
    this.newMessage = jsonString;
  }

  clearCommunication(): void {
    this.communicationService.clearCommunication();
  }

  testConnection(): void {
    this.communicationService.testConnection();
  }

}
