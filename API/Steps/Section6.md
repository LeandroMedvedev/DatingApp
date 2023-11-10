# SECTION 6: Routing in Angular

## Learning Goals

Implement routing our Angular app and have an understanding of:

1. Angular routing
2. Adding a bootstrap theme
3. Using Angular route guards*
4. Using a Shared Module

* O route guards (ou rota de guarda), faz a verificação se o usuário pode ou não acessar determinada rota.

### 65. Introduction

### 66. Creating some more components

1. PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c members/member-list --skip-tests

2. PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c members/member-detail --skip-tests

3. PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c lists --skip-tests

4. PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c messages --skip-tests

5. 
client\src\app\app-routing.module.ts

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { ListsComponent } from './lists/lists.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MessagesComponent } from './messages/messages.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'members', component: MemberListComponent },
  { path: 'members/:username', component: MemberDetailComponent },
  { path: 'lists', component: ListsComponent },
  { path: 'messages', component: MessagesComponent },
  { path: '**', component: HomeComponent, pathMatch: 'full' },
];
/*
  '**' representam qq coisa que não esteja nesta lista
*/

...

6. 
client\src\app\app.component.html

<app-nav></app-nav>
<div class="container" style='margin-top: 100px'>
    <router-outlet></router-outlet>
</div>


### 67. Adding the nav links

7. 
client\src\app\nav\nav.component.html

...
<a
    ...
    routerLink="/"
    routerLinkActive="active"
>Dating App</a>
<ul
    ...
><!-- Se currentUser$ não for nulo, está logado -->
    <li ...>
        <a
            ...
            routerLink="/members"
            routerLinkActive='active'
        >Matches</a>
    </li>
    <li ...>
        <a
            ...
            routerLink="/lists"
            routerLinkActive='active'
        >Lists</a>
    </li>
    <li ...>
        <a
            ...
            routerLink="/messages"
            routerLinkActive='active'
        >Messages</a>
    </li>
</ul>
...


### 68. Routing in code

8. 
client\src\app\nav\nav.component.ts

...
import { Router } from '@angular/router';

@Component({
  ...
})
export class NavComponent implements OnInit {
  ...

  constructor(..., private router: Router) { }

  ...

  login() {
    this.accountService.login(this.model).subscribe({
      next: _ => this.router.navigateByUrl('/members'),
      ...
    });
  }

  logout() {
    ...
    this.router.navigateByUrl('/');
  }
}


### 69. Adding a toast service for notifications

https://ngx-toastr.vercel.app/

https://github.com/scttcper/ngx-toastr

***Sigo os passos do link do github acima para instalar e usar ngx-toastr:***

9. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> npm i ngx-toastr@17

**OBS** versão 17 é a compatível com Angular 16.

10. 
client\angular.json

"styles": [
    ...
    "node_modules/ngx-toastr/toastr.css",
    "src/styles.css"
],

11. 
client\src\app\app.module.ts

...
import { ToastrModule } from 'ngx-toastr';

@NgModule({
 ...
  imports: [
    ...
    ToastrModule.forRoot({
      easeTime: 500,
      progressBar: true,
      positionClass: 'toast-bottom-left',
      iconClasses: {
        error: 'toast-error',
        info: 'toast-info',
        success: 'toast-success',
        warning: 'toast-warning',
      }
    })
  ],
  ...
})

12. 
client\src\app\nav\nav.component.ts

...
import { ToastrService } from 'ngx-toastr';

@Component({
  ...
})
export class NavComponent implements OnInit {
  ...

  constructor(..., private toastr: ToastrService) { }

  ...

  login() {
    this.accountService.login(this.model).subscribe({
      ...
      error: error => this.toastr.error(error.error, 'Error')
    });
  }
  ...
}


13. 
client\src\app\register\register.component.ts

...
import { ToastrService } from 'ngx-toastr';

@Component({
  ...
})
export class RegisterComponent {
  ...

  constructor(..., private toastr: ToastrService) { }

  register() {
    this.accountService.register(this.model).subscribe({
      ...
      error: error => {
        console.log(error.error.errors);
        this.toastr.error(error.error, 'Error');  // Retorna Object object; resolveremos na sessão seguinte
      },
    });
  }
  ...
}


### 70. Adding an Angular route guard

Adicionar segurança de roteamento para não deixar usuário adicionar rotas manualmente das quais não possui autorização.
Por exemplo, consigo acessar rotas que só seriam acessíveis estando logado.
A segurança para isso, de fato, só é feita realmente no backend, mas usaremos recursos Angular (frontend) para esconder tais erros.
**LEMBRE-SE:** 
***segurança do lado do cliente é somente um recurso de interface do usuário; segurança, de fato, só no backend***
Angular oferece Route Guard. Um método que controla a navegação para uma rota solicitada em um aplicativo de roteamento. Os guardas determinam se uma rota pode ser ativada ou desativada e se um módulo carregado lentamente pode ser carregado.

https://angular.io/guide/glossary#route-guard

https://angular.io/guide/router#preventing-unauthorized-access

14. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g guard _guards/auth --skip-tests --dry-run
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g guard _guards/auth --skip-tests

? Which type of guard would you like to create? CanActivate
CREATE src/app/_guards/auth.guard.ts (128 bytes)

15. 
client\src\app\_guards\auth.guard.ts

import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  /*
    Como authGuard é const e não class, não 
    há constructor para injetar dependência.
    Mas Angular provê função inject para isso.
  */
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  return accountService.currentUser$.pipe(
    map(user => {
      if (user) return true;
      toastr.error('you shall not pass!', 'Error');
      return false;
    })
  );
};


16. 
client\src\app\app-routing.module.ts

import { authGuard } from './_guards/auth.guard';

const routes: Routes = [
  ...
  { path: 'members', component: MemberListComponent, canActivate: [authGuard] },
  ...
];


### 71. Adding a dummy route

17. 
client\src\app\app-routing.module.ts

...
import { authGuard } from './_guards/auth.guard';

const routes: Routes = [
  ...
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [authGuard],
    children: [
      { path: 'members', component: MemberListComponent },
      { path: 'members/:username', component: MemberDetailComponent },
      { path: 'lists', component: ListsComponent },
      { path: 'messages', component: MessagesComponent },
    ]
  },
  ...
];
...

<ng-container></ngcontainer> não é visível no HTML, não é adicionado ao DOM. É como uma tag HTML vazia não visível na tela.

18. 
No arquivo abaixo, as tags "a" foram inseridas dentro de ng-container. Antes estavam somente dentro da tag "ul".

client\src\app\nav\nav.component.html

<ul
    class="navbar-nav me-auto mb-2 mb-md-0"
    *ngIf="accountService.currentUser$ | async"
><!-- Se currentUser$ não for nulo, está logado -->

    <ng-container *ngIf="accountService.currentUser$ | async">
        <li class="nav-item">
            <a
                class="nav-link"
                routerLink="/members"
                routerLinkActive='active'
            >Matches</a>
        </li>
        <li class="nav-item">
            <a
                class="nav-link"
                routerLink="/lists"
                routerLinkActive='active'
            >Lists</a>
        </li>
        <li class="nav-item">
            <a
                class="nav-link"
                routerLink="/messages"
                routerLinkActive='active'
            >Messages</a>
        </li>
    </ng-container>
</ul>

### 72. Adding a new theme

19. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> npm i bootswatch@5.2.3

https://bootswatch.com/
https://bootswatch.com/united/  (tema que adicionamos foi o "united")

20. 
client\angular.json

"styles": [
  "node_modules/ngx-bootstrap/datepicker/bs-datepicker.css",
  "node_modules/bootstrap/dist/css/bootstrap.min.css",
  "node_modules/bootswatch/dist/united/bootstrap.min.css",  (adicionei esta linha apenas)
  "node_modules/font-awesome/css/font-awesome.min.css",
  "node_modules/ngx-toastr/toastr.css",
  "src/styles.css"
],

21. 
client\src\app\nav\nav.component.html

<div
    ...
    *ngIf="(accountService.currentUser$ | async) as user" 
    ...
>
    <a
        ...        
    >Welcome, {{ user.username | titlecase }}</a>
    <div
        ...
    >
        <a ...>Edit Profile</a>
        <a
            ...
        >Logout</a>
    </div>
</div>

### 73. Tidying up the app module by using a shared module

**Objetivo:** deixar app.module.ts somente com importações relacionadas à Angular. Criar um módulo compartilhado para isso, alocando nele componentes de terceiros.

22. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g m _modules/shared --flat --dry-run
CREATE src/app/_modules/shared.module.ts (192 bytes)

--flat -> evita que seja criada esta estrutura de diretórios:
                              src/app/_modules/shared/shared.module.ts

PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g m _modules/shared --flat
CREATE src/app/_modules/shared.module.ts (192 bytes)

***O módulo é criado com esta estrutura inicial:***

client\src\app\_modules\shared.module.ts

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [],
  imports: [
    CommonModule
  ]
})
export class SharedModule { }

23. 
client\src\app\_modules\shared.module.ts

...
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ToastrModule } from 'ngx-toastr';

@NgModule({
  ...,
  imports: [
    ...,
    BsDropdownModule.forRoot(),
    ToastrModule.forRoot({
      easeTime: 500,
      progressBar: true,
      positionClass: 'toast-bottom-left',
      iconClasses: {
        error: 'toast-error',
        info: 'toast-info',
        success: 'toast-success',
        warning: 'toast-warning',
      }
    }),
  ],
  exports: [
    BsDropdownModule,
    ToastrModule,
  ]
})
...

24. 
client\src\app\app.module.ts

...
import { SharedModule } from './_modules/shared.module';

@NgModule({
  declarations: [
    ...
  ],
  imports: [
    ...,
    SharedModule,
  ],
  ...
})
...

25. 
git add .
git commit -m 'End of section 6'
Clico em Sync Changes para sincronizar mudanças com repositório GitHub (equivale ao git push).


### 74. Section 6 summary