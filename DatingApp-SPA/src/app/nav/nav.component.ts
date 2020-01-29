import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(private authService: AuthService) { }

  ngOnInit() {
  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      console.log('Logged in successfully');
    }, error => {
      console.log('Failed to login');
    });
  }

  loggedIn() {
    // if you look at the application tab when logging in you see that the local storage contains a key 'token'
    // with login token, so we're just pulling to see if we're logged in
    const token = localStorage.getItem('token');
    // !! means if something is in token, it returns true otherwise false
    return !!token;
  }

  logout(){
    localStorage.removeItem('token');
    console.log('logged out');
  }
}
