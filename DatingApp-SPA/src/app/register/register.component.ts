import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // output will send variables back up to home, this sends a new event back up
  @Output() cancelRegister = new EventEmitter();
  // create an empty model thatll eventually hold params
  model: any = {};

  constructor(private authService: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
  }

  register() {
    this.authService.register(this.model).subscribe(() => {
      this.alertify.success('registration successful');
    }, error => {
      this.alertify.error(error);
    });
  }

  cancel() {
    // we use.emit because we are emitting something out of this register, we can send anything in the param
    // in this case we are using a boolean, but it could be an object
    this.cancelRegister.emit(false);
    console.log('cancelled');
  }

}
