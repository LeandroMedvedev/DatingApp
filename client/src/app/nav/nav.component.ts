import { Component, OnInit } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};

  constructor(public accountService: AccountService) { }

  ngOnInit() { }

  login() {
    /*
      Requisições HTTP cancelam automaticamente a inscrição (subscribe).
      Por isso não preciso desinscrever-me. Mas em outros casos há neces-
      sidade, para que não haja "vazamento" de memória.
      Um modo excelente de certificar-me de que a inscrição foi cancela-
      da é usar Async Pipe (usa-se no HTML).
    */
    this.accountService.login(this.model).subscribe({
      next: response => {
        console.log(response);
      },
      error: error => console.log(error)
    });
  }

  logout() {
    this.accountService.logout();  // remover item user do localStorage
  }
}
