import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // output will send variables back up to home, this sends a new event back up
  @Output() cancelRegister = new EventEmitter();
  // create an empty model thatll eventually hold params
  user: User;
  registerForm: FormGroup;
  // by making this Partial, all the config options are optional and not required
  bsConfig: Partial<BsDatepickerConfig>;

  constructor(private authService: AuthService, private alertify: AlertifyService, private fb: FormBuilder, private router: Router) { }

  ngOnInit() {
    // manually building formgroup
    // this.registerForm = new FormGroup({
    //   username: new FormControl('', Validators.required),
    //   password: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]),
    //   confirmPassword: new FormControl('', Validators.required)
    // }, this.passwordMatchValidator);

    this.bsConfig = {
      containerClass: 'theme-red'
    };
    // using formBuilder
    this.createRegisterFrom();
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value ? null : {mismatch: true};
  }

  // instead of building form groups manually in the ngOnInit, we can build the form group using angular boilerplate
  // FormBuilder inside a function, it tidies up the code and makes for easier readability, compare to commented out
  // formgroup call in ngoninit
  createRegisterFrom() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator});
  }

  register() {
    if (this.registerForm.valid) {
      // we clone the values from the registerForm object into the blank object
      // which gets saved into the user object
      this.user = Object.assign({}, this.registerForm.value);
      this.authService.register(this.user). subscribe(() => {
        this.alertify.success('Registration Successful');
      }, error => {
        this.alertify.error(error);
      }, () => {
        this.authService.login(this.user).subscribe(() => {
          this.router.navigate(['/members']);
        });
      });
    }
    // this.authService.register(this.model).subscribe(() => {
    //   this.alertify.success('registration successful');
    // }, error => {
    //   this.alertify.error(error);
    // });
  }

  cancel() {
    // we use.emit because we are emitting something out of this register, we can send anything in the param
    // in this case we are using a boolean, but it could be an object
    this.cancelRegister.emit(false);
    console.log('cancelled');
  }

}
