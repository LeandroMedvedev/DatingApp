# SECTION 5

## Learning Goals

Implement the login and register (client side) functionality into the app as well as understanding:

1. Creating components using the Angular CLI
2. Using Angular Template forms
3. Using Angular services
4. Understanding Observables
5. Using Angular structural directives to conditionally display elements on a page
6. Component communication from parent to child
7. Component communication from child to parent
  

### 49. Introduction


### 50. Creating a nav bar

1. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g --help

2. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c nav --skip-tests --dry-run
Mostra o que será feito, mas não cria e altera arquivos.

3. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c nav --skip-tests

4. 
client\src\app\app.component.html

<app-nav></app-nav>
<div class="container" style='margin-top: 100px'>
    <ul>
        <li *ngFor="let user of users">{{ user.id }} - {{ user.userName }}</li>
    </ul>
</div>

5. 
Copiar barra de navegação deste link e fazer algumas adaptações, como abaixo:
        https://getbootstrap.com/docs/5.3/examples/carousel/

client\src\app\nav\nav.component.html

<nav class="navbar navbar-expand-md navbar-dark fixed-top bg-dark">
    <div class="container">
        <a class="navbar-brand" href="#">Dating App</a>
        <ul class="navbar-nav me-auto mb-2 mb-md-0">
            <li class="nav-item">
                <a class="nav-link" href="#">Matches</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" href="#">Lists</a>
            </li>
            <li class="nav-item">
                <a class="nav-link" aria-disabled="true">Messages</a>
            </li>
        </ul>
        <form class="d-flex">
            <input class="form-control me-2" placeholder="Username">
            <input class="form-control me-2" type="password" placeholder="Password">
            <button class="btn btn-outline-success" type="submit">Login</button>
        </form>
    </div>
</nav>

### 51. Introduction to Angular template forms

6. 
client\src\app\app.module.ts

  imports: [
    ...
    FormsModule
  ],

7. 
client\src\app\nav\nav.component.ts

import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};

  ngOnInit(): void {
      
  }

  login() {
    console.log(this.model);
  }
}

8. 
client\src\app\nav\nav.component.html

...
<form
    #loginForm='ngForm'
    ...
    (ngSubmit)="login()"
    autocomplete='off'
>
    <input
        name='username'
        [(ngModel)]='model.username'
        ...
    >
    <input
        name='password'
        [(ngModel)]='model.password'        
        ...
    >
    ...
</form>
...

### 52. Introduction to Angular services

9. 
Crio diretório _services em:
        client\src\app\_services 

10. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g s _services\account --skip-tests --dry-run

11. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g s _services\account --skip-tests

12. 
client\src\app\_services\account.service.ts

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = 'https://localhost:5001/api/';

  constructor(private http: HttpClient) { }

  login(model: any) {
    return this.http.post(`${this.baseUrl}account/login`, model);
  }
}


### 53. Injecting services into components

13. 
client\src\app\nav\nav.component.ts

...
loggedIn = false;

  constructor(private accountService: AccountService) { }

  ...

  login() {
    this.accountService.login(this.model).subscribe({
      next: response => {
        console.log(response);
        this.loggedIn = true;
      },
      error: error => console.log(error)
    });
  }
...

### 54. Using conditionals to show and remove content

14. 
client\src\app\nav\nav.component.ts

...
logout() {
    this.loggedIn = false;
  }
...

15. 
client\src\app\nav\nav.component.html

...
<ul ... *ngIf="loggedIn">
...
    <li class="nav-item" (click)="logout()">
        <a
        class="nav-link"
        href='#'
        >Logout</a>
    </li>
</ul>
<div class="dropdown" *ngIf="loggedIn">
    <a class="dropdown-toggle text-light">Welcome, user</a>
    <div class="dropdown-menu">
        <a class="dropdown-item">Edit Profile</a>
        <a
            class="dropdown-item"
            (click)="logout()"
        >Logout</a>
    </div>
</div>
<form
    *ngIf="!loggedIn"
    ...
>
...
</form>    
...


### 55. Using the angular bootstrap components - dropdown

16. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng add ngx-bootstrap --component dropdowns

17. 
client\src\app\nav\nav.component.html

REMOVA:
<li class="nav-item" (click)="logout()">
    <a
        class="nav-link"
        href='#'
    >Logout</a>
</li>

...
ADICIONE AS DIRETIVAS dropdown, dropdownToggle e *dropdownMenu, além de text-decoration-none:
<div class="dropdown" ... dropdown>
    <a class="dropdown-toggle text-light text-decoration-none" dropdownToggle>Welcome, user</a>
    <div class="dropdown-menu" *dropdownMenu>
        <a class="dropdown-item">Edit Profile</a>
        <a
            class="dropdown-item"
            ...
        >Logout</a>
    </div>
</div>
...

18. 
client\src\app\nav\nav.component.css

.dropdown-toggle, .dropdown-item {
    cursor: pointer;
}


### 56. Introduction to observables

##### *What are Observables?*

New standard for managing async data included in ES7 (ES2016)
Introduced in Angular v2
Observables are lazy collections of multiple values over time
You can think of observables like a newsletter
  * Only subscribers of the newsletter receive the newsletter
  * I no-one subscribes to the newsletter it probably will not be printed

**PROMISE**                                   **OBSERVABLE**
Provides a single future value                Emits multiple values over time
Not lazy                                      Lazy
Can not cancel                                Able to cancel
                                              Can use with map, filter, reduce and other operators

##### *Observables and Angular*

**Angular Service**                                   **Angular Component OnInit()**

getMembers(): Observable<Object> {                    getMembers() {
  return this.http.get('api/users');                    this.service.getMembers(); 
}                                                     }
Se quiser fazer algo com esse observable, 
preciso assiná-lo.

Caso queira TRANSFORMAR dados antes de passá-los para o subscriber (inscrito), use método PIPE de rxjs:

getMembers() {
  return this.http.get('api/users').**pipe**(
    map(members => {
      console.log(member.id)
      return member.id
    })
  )
}

No exemplo acima, o método map aplicará uma função aos itens da matriz de membros e retornará somente seus ids, não o objeto inteiro.

##### *Getting data from observables*

**Subscribe**
getMembers() {
  this.service.getMembers().subscribe(
    members => {
      this.members = members
  }, error => {                   // 2. o que fazer se houver erro
    console.log(error);
  }, () => {                      // 3. o que fazer se completar
    console.log('completed');
  })
}

Depois de assinar (subscribe), normalmente há 3 partes para o que fazer a seguir na assinatura.


**TopPromise()**
Outro modo de obter dados de Obervables sem nos inscrevermos neles é enviá-los para uma Promise JS normal.
Isso não dá nenhuma das vantagens dos Obervables.
Mas o que podemos fazer se enviarmos uma Promise, ativamos essa requisição e ela faz algo.
Tendo a Promise, posso lidar com ela de modo normal em meu código, sem problemas.

getMembers() {
  return this.http.get('api/users').toPromise();
}

**Async Pipe**
Outro modo de obter dados dos Observables é a função Angular Async Pipe:

    <li *ngFor="let member of service.getMembers() | async">{{ member.username }}</li>

Automatically subscribes/unsubscribes from the Observable.
A vantagem de usar o Pipe Assíncrono é que ele inscreve-se e desinscreve-se automaticamente do Observable.
Quando uso o Observable no modo tradicional (ilustrado acima em **Subscribe**), a inscrição no Observable não é cancelada. Seria preciso fazer isso de modo diferente. 
Mas há uma ressalva aqui também. Se estivermos fazendo uma requisição HTTP, elas normalmente serão concluídas. Portanto, não preciso cancelar a inscrição no Observable.

### 57. Persisting the login

19. 
client\src\app\_models\user.ts

export interface User {
    username: string;
    token: string;
}

20. 
client\src\app\_services\account.service.ts

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  ...
  private currentUserSource = new BehaviorSubject<User | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  ...

  login(model: any) {
    return this.http.post<User>(`${this.baseUrl}account/login`, model).pipe(
      map((response: User) => {
        const user = response;
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }

  setCurrentUser(user: User) {
    this.currentUserSource.next(user);
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}

21. 
client\src\app\app.component.ts

...
import { AccountService } from './_services/account.service';
import { User } from './_models/user';

@Component({
  ...
})
export class AppComponent implements OnInit {
  ...

  constructor(..., private accountService: AccountService) { }

  ngOnInit(): void {
    this.getUsers();
    this.setCurrentUser();
  }

  getUsers() {
    this.http.get('https://localhost:5001/api/users').subscribe({
      next: response => this.users = response,
      error: error => console.log(error),
      complete: () => console.log('Request has completed!')
    });
  }

  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
    const user: User = JSON.parse(userString);
    this.accountService.setCurrentUser(user);
  }
}

22. 
client\src\app\nav\nav.component.ts

...
@Component({
 ...
})
export class NavComponent implements OnInit {  
  ...

  ngOnInit() {
    this.getCurrentUser();
  }

  getCurrentUser() {
    this.accountService.currentUser$.subscribe({
      next: user => this.loggedIn = Boolean(user),
      error: error => console.log(error),
    });
  }

  ...

  logout() {
    this.accountService.logout();  // remover item user do localStorage
    ...
  }
}


### 58. Using the async pipe

23. 
client\src\app\nav\nav.component.ts

...
import { User } from '../_models/user';
import { Observable, of } from 'rxjs';

@Component({
  ...
})
export class NavComponent implements OnInit {
  ...
  currentUser$: Observable<User | null> = of(null);  // currentUser$ é Observable de (of) null

  constructor(private accountService: AccountService) { }

  ngOnInit() {
    this.currentUser$ = this.accountService.currentUser$;
  }

  login() {
    this.accountService.login(this.model).subscribe({
      next: response => {
        console.log(response);        
      },
      error: error => console.log(error)
    });
  }

  logout() {
    this.accountService.logout();
  }
}

24. 
client\src\app\nav\nav.component.html

<ul
    class="navbar-nav me-auto mb-2 mb-md-0"
    *ngIf="currentUser$ | async"
><!-- Se currentUser$ não for nulo, está logado -->
    ...
</ul>
<div
    class="dropdown"
    *ngIf="currentUser$ | async"
    ...
>
...
<form
    *ngIf="!(currentUser$ | async)"
    ...
>

25. 
client\src\app\nav\nav.component.ts

...
@Component({
  ...
})
export class NavComponent implements OnInit {
  ...  

  constructor(**public** accountService: AccountService) { }

  ngOnInit() {
    
  }

  ...
}

26. 
client\src\app\nav\nav.component.html

<ul
    class="navbar-nav me-auto mb-2 mb-md-0"
    *ngIf="accountService.currentUser$ | async"
><!-- Se currentUser$ não for nulo, está logado -->
    ...
</ul>
<div
    class="dropdown"
    *ngIf="accountService.currentUser$ | async"
    ...
>
...
<form
    *ngIf="!(accountService.currentUser$ | async)"
    ...
>


### 59. Adding a home page

27. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c home --skip-tests --dry-run
CREATE src/app/home/home.component.html (19 bytes)
CREATE src/app/home/home.component.ts (194 bytes)
CREATE src/app/home/home.component.css (0 bytes)
UPDATE src/app/app.module.ts (879 bytes)

NOTE: The "--dry-run" option means no changes were made.

28. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c home --skip-tests

29. 
client\src\app\home\home.component.ts
import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  registerMode = false;

  registerToggle() {
    this.registerMode = !this.registerMode;
  }
}

30. 
client\src\app\home\home.component.html

<div class="container mt-5">
    <div
        *ngIf="!registerMode"
        style='text-align: center'
    >
        <h1>Find your match</h1>
        <p class="lead">Come on into view your matches... all you need to do is sign up!</p>
        <div class="text-center">
            <button
                (click)="registerToggle()"
                class="btn btn-primary btn-lg me-2"
            >Register</button>
            <button class="btn btn-secondary btn-lg">Learn more</button>
        </div>
    </div>
    <div
        *ngIf="registerMode"
        class="container"
    >
        <div class="row justify-content-center">
            <div class="col-4">
                <p>Register form goes here</p>
            </div>
        </div>
    </div>
</div>

31. 
client\src\app\app.component.html

<app-nav></app-nav>
<div class="container" style='margin-top: 100px'>
    <app-home></app-home>
</div>

### 60. Adding a register form

32. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c register --skip-tests

33. 
client\src\app\register\register.component.ts

import { Component } from '@angular/core';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  model: any = {};

  register() {
    console.log(this.model);
  }

  cancel() {
    console.log('cancelled');
  }
}

34. 
client\src\app\register\register.component.html

<form
    #registerForm='ngForm'
    (ngSubmit)="register()"
    autocomplete='off'
>
    <h2 class='text-center text-primary'>Sign up</h2>
    <hr>
    <div class="mb-3">
        <input
            class='form-control'
            name='username'
            [(ngModel)]="model.username"
            placeholder='Username'
        >
    </div>
    <div class="mb-3">
        <input
            class='form-control'
            name='password'
            [(ngModel)]="model.password"
            placeholder='Password'
        >
    </div>
    <div class="text-center">
        <button
            class="btn btn-success me-2"
            type='submit'
        >Register</button>
        <button
            class="btn btn-default"
            type='button'
            (click)="cancel()"
        >Cancel</button>
    </div>
</form>

35. 
client\src\app\home\home.component.html

<div class="container mt-5">
    ...
        <div class="row justify-content-center">
            <div ...>
                <app-register></app-register>
            </div>
        </div>
    </div>
</div>


### 61. Parent to child communication (@Input)

O componente home é pai do componente register. register é o formulário de registro do usuário contido em home.
Como passar informações do componente pai, home, para o componente filho, register?
Como passar a lista de usuários, users, de home para register?

O fluxo será basicamente o seguinte:
**I.**
client\src\app\home\home.component.ts
users: any;

**II.**
client\src\app\home\home.component.html
<app-register [usersFromHomeComponent]="users"></app-register>

**III.**
client\src\app\register\register.component.ts
@Input() usersFromHomeComponent: any;

**IV.**
client\src\app\register\register.component.html
<option
    *ngFor="let user of usersFromHomeComponent"
    [value]="user.userName"
>
    {{ user.userName }}
</option>

Veja os passos 36-40 a seguir que ilustram a representação dos 4 passos acima:

36. 
client\src\app\app.component.ts

import { Component, OnInit } from '@angular/core';
import { AccountService } from './_services/account.service';
import { User } from './_models/user';

@Component({
  ...
})
export class AppComponent implements OnInit {
  ...  

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {    
    this.setCurrentUser();
  }

  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
    const user: User = JSON.parse(userString);
    this.accountService.setCurrentUser(user);
  }
}

37. 
client\src\app\home\home.component.ts

import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  ...
})
export class HomeComponent implements OnInit {
  ...
  users: any;

  constructor(private http: HttpClient) { }

  ngOnInit() {
    this.getUsers();
  }

  getUsers() {
    this.http.get('https://localhost:5001/api/users').subscribe({
      next: response => this.users = response,
      error: error => console.log(error),
      complete: () => console.log('Request has completed!')
    });
  }
}

38. 
client\src\app\home\home.component.html

<div class="container mt-5">
    ...
        <div class="row justify-content-center">
            <div ...>
                <app-register [usersFromHomeComponent]="users"></app-register>
            </div>
        </div>
    </div>
</div>

39. 
client\src\app\register\register.component.ts

import { Component, Input } from '@angular/core';

@Component({
  ...
})
export class RegisterComponent {
  model: any = {};
  @Input() usersFromHomeComponent: any;
  /*
    @Input() indica que informações virão do componente pai, home.
    Para passar essas informações para o filho, componente register,
    lá em home.component.html uso o atributo em app-register:

        client\src\app\home\home.component.html

        <app-register [usersFromHomeComponent]="users"></app-register>
  */

  ...
}

40. 
client\src\app\register\register.component.html

<form
    ...
>
    ...
    <div class="mb-3">
        <label>Who is your favourite user?</label>
        <select class="form-select">
            <option
                *ngFor="let user of usersFromHomeComponent"
                [value]="user.userName"
            >
                {{ user.userName }}
            </option>
        </select>
    </div>
    ...
</form>


### 62. Child to parent communication (@Output)

Quero fazer a comunicação entre o componente filho REGISTER e seu pai HOME.
Em REGISTER há um botão Cancel dentro de um formulário. Quero clicar neste botão Cancel e ocultar o componente REGISTER, e exibir novamente apenas o componente pai, HOME. A seguir:

41. 
client\src\app\register\register.component.ts

import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  ...
})
export class RegisterComponent {
  /*
    @Input() indica que informações virão do componente pai, home.
    Para passar essas informações para o filho, componente register,
    lá em home.component.html uso o atributo em app-register:

        client\src\app\home\home.component.html

        <app-register [usersFromHomeComponent]="users"></app-register>
  */
  @Input() usersFromHomeComponent: any;
  @Output() cancelRegister = new EventEmitter();
  /*
    Uso a propriedade de saída, cancelRegister, para emitir um evento
    deste componente filho, REGISTER, para seu pai, HOME.
    No método cancel() (abaixo), a propriedade de saída cancelRegister,
    que agora pode emitir coisas, emitirá o valor false:

        cancel() {
          this.cancelRegister.emit(false);
        }
    
    Usarei este valor, false, para desativar o modo de registro de usuário,
    ou seja, deixar de renderizar o componente REGISTER e exibir só o com-
    ponente pai HOME.
  */
  ...

  cancel() {
    this.cancelRegister.emit(false);
  }
}

42. 
client\src\app\home\home.component.html

...
<div class="row justify-content-center">
  <div class="col-4">
      <app-register
          [usersFromHomeComponent]="users"
          (cancelRegister)="cancelRegisterMode($event)"
      ></app-register>
      <!--
          [] -> app-register está recebendo algo (@Input)
          () -> app-register está enviando algo  (@Output)
      -->
  </div>
</div>
...

43. 
client\src\app\home\home.component.ts

...

@Component({
  ...
})
export class HomeComponent implements OnInit {
  ...

  cancelRegisterMode(event: boolean) {  // event será o valor false emitido do componente filho, REGISTER
    this.registerMode = event;
  }
}


### 63. Hooking up the register method to the service

44. 
client\src\app\_services\account.service.ts

...

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  ...

  register(model: any) {
    return this.http.post<User>(`${this.baseUrl}account/register`, model).pipe(
      map(user => {
        if (user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user);
        }
      })
    );
  }
  ...
}

45. 
client\src\app\register\register.component.ts

...

@Component({
  ...
})
export class RegisterComponent {
  ***// @Output já estava aqui. Única alteração agora foi a remoção de @Input() usersFromHomeComponent: any;***
  @Output() cancelRegister = new EventEmitter();
}

46. 
client\src\app\register\register.component.html

**REMOVIDO o trecho de código abaixo:**
<div class="mb-3">
    <label>Who is your favourite user?</label>
    <select class="form-select">
        <option
            *ngFor="let user of usersFromHomeComponent"
            [value]="user.userName"
        >
            {{ user.userName }}
        </option>
    </select>
</div>

47. 
client\src\app\home\home.component.html

**REMOVIDO o trecho de código abaixo:**
[usersFromHomeComponent]="users"

48. 
client\src\app\register\register.component.ts

...

@Component({
  ...
})
export class RegisterComponent {
  ...

  register() {
    this.accountService.register(this.model).subscribe({
      next: _ => {
        **REMOVIDO um console.log(response) daqui e o response na linha acima onde agora há _**
        this.cancel();
      },
      error: error => console.log(error),

    });
  }

  ...
}

49. 
PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef database drop
Build started...
Build succeeded.
Are you sure you want to drop the database 'main' on server 'datingapp.db'? (y/N)
y
Dropping database 'main' on server 'datingapp.db'.
Successfully dropped database 'main'.

50. 
PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef database update

51. 
Ctrl Shift p
SQLite: Open Database
API\datingapp.db

52. 
git add .
git commit -m 'End of section 5'
Clico em Sync Changes para sincronizar mudanças com repositório GitHub (equivale ao git push).


### 64. Section 5 summary