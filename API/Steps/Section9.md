# SECTION 9

## Learning Goals

### 101. Introduction

Implemente os componentes que compõem a interface do usuário em nosso aplicativo cliente e compreenda:

1. Usando tipos TypeScript
2. Usando o Pipe assíncrono
3. Usando Bootstrap para estilização
4. Truques básicos de CSS para melhorar a aparência
5. Usando uma galeria de fotos de terceiros


### 102. Using TypeScript

Relembramos alguns recursos TypeScript, mas apagamos o código.


### 103. Creating the member interface

1. Uso de https://transform.tools/json-to-typescript para converter JSON em TypeScript.

2. 
client\src\app\_models\photo.ts

export interface Photo {
    id: number;
    url: string;
    isMain: boolean;
}


3. 
client\src\app\_models\member.ts

import { Photo } from "./photo"

export interface Member {
    id: number
    userName: string
    photoUrl: string
    age: number
    knownAs: string
    created: string
    lastActive: string
    gender: string
    introduction: string
    lookingFor: string
    interests: string
    city: string
    country: string
    photos: Photo[]
}


### 104. Adding a member service

4. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g --help


5. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g environments
CREATE src/environments/environment.ts (31 bytes)
CREATE src/environments/environment.development.ts (31 bytes)
UPDATE angular.json (3410 bytes)


6. 
client\src\environments\environment.development.ts

export const environment = {
    production: false,
    apiUrl: "https://localhost:5001/api/"
};


7. 
client\src\environments\environment.ts

export const environment = {
    production: true,
    apiUrl: 'api/'
};


8. 
client\src\app\_services\account.service.ts

...
import { environment } from 'src/environments/environment';
...
baseUrl = environment.apiUrl;
...


9. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g s _services\members --skip-tests --dry-run
CREATE src/app/_services/members.service.ts (136 bytes)

NOTE: The "--dry-run" option means no changes were made.


10. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g s _services\members --skip-tests
CREATE src/app/_services/members.service.ts (136 bytes)

11. 
client\src\app\_services\members.service.ts

...
import { Member } from '../_models/member';

...
export class MembersService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'users', this.getHttpOptions());
  }

  getMember(username: string) {
    return this.http.get<Member>(this.baseUrl + 'users/' + username, this.getHttpOptions());
  }

  /*
    nome getHttpOptions() porque vamos passar o token de autorização dentro
    dos cabeçalhos HTTP e precisamos criar algumas opções para fazer isso
  */
  getHttpOptions() {
    const userString = localStorage.getItem('user');  // Obteremos o token do local storage
    if (!userString) return;
    const user = JSON.parse(userString);

    return {
      headers: new HttpHeaders({
        Authorization: 'Bearer ' + user.token
      })
    }
  }
}


### 105. Retrieving the list of members

12. 
client\src\app\members\member-list\member-list.component.ts

import { ..., OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

...
export class MemberListComponent implements OnInit {
  members: Member[] = [];

  constructor(private memberService: MembersService) { }

  ngOnInit() {
    this.loadMembers();
  }

  loadMembers() {
    this.memberService.getMembers().subscribe({
      next: members => this.members = members,
    });
  }
}


13. 
client\src\app\home\home.component.ts

**Removido todo este trecho de código abaixo:**
users: any;
...

constructor(private http: HttpClient) { }

getUsers() {
   this.http.get('https://localhost:5001/api/users').subscribe({
     next: response => this.users = response,
     error: error => console.log(error),
     complete: () => console.log('Request has completed!')
   });
}

ngOnInit() {
   this.getUsers();
}


14. 
client\src\app\members\member-list\member-list.component.html

<div class="row">
    <div class="col-2">
        <p *ngFor="let member of members">{{ member.knownAs }}</p>
    </div>
</div>


### 106. Creating member cards

15. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c members\member-card --skip-tests --dry-run     
CREATE src/app/members/member-card/member-card.component.html (26 bytes)
CREATE src/app/members/member-card/member-card.component.ts (221 bytes)
CREATE src/app/members/member-card/member-card.component.css (0 bytes)
UPDATE src/app/app.module.ts (1935 bytes)

NOTE: The "--dry-run" option means no changes were made.


16. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c members\member-card --skip-tests
CREATE src/app/members/member-card/member-card.component.html (26 bytes)
CREATE src/app/members/member-card/member-card.component.ts (221 bytes)
CREATE src/app/members/member-card/member-card.component.css (0 bytes) 
UPDATE src/app/app.module.ts (1935 bytes)

17. 
client\src\app\members\member-card\member-card.component.ts

import { ..., Input } from '@angular/core';
import { Member } from 'src/app/_models/member';

...
  // MemberCardComponent é filho de MemberListComponent
export class MemberCardComponent {
  @Input() member: Member | undefined;
  /*
    member é inicialmente undefined, só depois torna-se Member


    client\tsconfig.json

    "strictPropertyInitialization": false,
    Check for class properties that are declared but not set in the constructor.

    Evita o erro:
    Property 'member' has no initializer and is not definitely assigned in the constructor.ts(2564)
  */
}


18. 
client\src\app\members\member-card\member-card.component.html

<div
    class="car mb-4"
    *ngIf="member"
>
    <div class="card-img-wrapper">
        <img
            class='card-img-top'
            src="{{member.photoUrl}}"
            alt="{{member.knownAs}}"
        >
    </div>
    <div class="card-body p-1">
        <h6 class="card-title text-center mb-1">
            <i class="fa fa-user me-2"></i>
            {{ member.knownAs }}
        </h6>
        <p class="card-text text-muted text-center">
            {{ member.city }}
        </p>
    </div>
</div>

19. Colocaremos o componente filho MemberCardComponent dentro do pai MemberListComponent:
client\src\app\members\member-list\member-list.component.html

***Ele inteiro ficou assim:***
<div class="row">
    <div
        class="col-2"
        *ngFor="let member of members"
    >
        <app-member-card [member]="member"></app-member-card>
    </div>
</div>


### 107. Adding some style to the cards

20. 
client\src\app\members\member-card\member-card.component.css

.card:hover img {
    transform: scale(1.2, 1.2); 
    transition-duration: 500ms;
    transition-timing-function: ease-out;
    opacity: 0.7;
}

.card img {
    transform: scale(1.0, 1.0);
    transition-duration: 500ms;
    transition-timing-function: ease-out;
}

.card-img-wrapper {
    overflow: hidden;
}


### 108. Adding animated buttons

21. 
client\src\app\members\member-card\member-card.component.html

...
<ul class="list-inline member-icons animate text-center">
    <li class="list-inline-item">
        <button class="btn btn-dark">
            <i class="fa fa-user"></i>
        </button>
    </li>
    <li class="list-inline-item">
        <button class="btn btn-dark">
            <i class="fa fa-heart"></i>
        </button>
    </li>
    <li class="list-inline-item">
        <button class="btn btn-dark">
            <i class="fa fa-envelope"></i>
        </button>
    </li>
</ul>
...


22. 
client\src\app\members\member-card\member-card.component.css

...
.card-img-wrapper {
    ...
    position: relative;
}

.member-icons {
    position: absolute;
    bottom: -30%;
    left: 0;
    right: 0;
    margin-left: auto;
    margin-right: auto;
    opacity: 0;
}

.card-img-wrapper:hover .member-icons {
    bottom: 0;
    opacity: 1;
}

.animate {
    transition: all 0.3s ease-in-out;
}


### 109. Using an interceptor to send the token

23. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g interceptor _interceptors/jwt --skip-tests --dry-run
CREATE src/app/_interceptors/jwt.interceptor.ts (408 bytes)

NOTE: The "--dry-run" option means no changes were made.


24. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g interceptor _interceptors/jwt --skip-tests
CREATE src/app/_interceptors/jwt.interceptor.ts (408 bytes)


25. 
client\src\app\_interceptors\jwt.interceptor.ts

...
import { ..., take } from 'rxjs';
import { AccountService } from '../_services/account.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) { }

  **intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {**
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) {
          request = request.clone({
            setHeaders: {
              Authorization: `Bearer ${user.token}`
            }
          })
        }
      }
    });

    ...
  }
}


26. 
client\src\app\app.module.ts

...
import { JwtInterceptor } from './_interceptors/jwt.interceptor';

@NgModule({
  ...
  providers: [
    ...
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true}
  ],
  ...
})


27. 
client\src\app\_services\members.service.ts

**Código abaixo removido, além das chamadas do método:**
getHttpOptions() {
    const userString = localStorage.getItem('user');  // Obteremos o token do local storage
    if (!userString) return;
    const user = JSON.parse(userString);

    return {
      headers: new HttpHeaders({
        Authorization: 'Bearer ' + user.token
      })
    }
}

**Código ficou assim:**
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'users');
  }

  getMember(username: string) {
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }  
}


### 110. Routing to the detailed page

Adicionar funcionalidade de clicar no botão com ícone de usuário, que aparece sobre o card de usuário, e ser direcionado para uma página detalhada de usuário.

28. 
client\src\app\members\member-detail\member-detail.component.ts

import { ..., OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

...
export class MemberDetailComponent implements OnInit {
  member: Member | undefined;

  constructor(private memberService: MembersService, private route: ActivatedRoute) { }

  ngOnInit() {
    this.loadMember();
  }

  loadMember() {
    const username = this.route.snapshot.paramMap.get('username');

    if (!username) return;

    this.memberService.getMember(username).subscribe({
      next: member => this.member = member,
    });
  }
}


29. 
client\src\app\members\member-detail\member-detail.component.html

<h1 *ngIf="member">{{ member.knownAs }}</h1>


30. 
client\src\app\members\member-card\member-card.component.html

***Neste arquivo, apenas inserimos atributo ROUTERLINK no botão com ícone de usuário. O código nas linhas anteriores e posteriores é só para facilitar a localização.***

<ul class="list-inline member-icons animate text-center">
    <li class="list-inline-item">
        <button ...  routerLink="/members/{{member.userName}}">
            <i class="fa fa-user"></i>
        </button>
    </li>
    ...
</ul>


### 111. Styling the member detailed page

31. 
client\src\app\members\member-detail\member-detail.component.html

<div
    class="row"
    *ngIf="member"
>
    <div class="col-4">
        <!-- col-4: ocupa 1 terço do espaço disponível -->
        <div class="card">
            <img
                class='card-img-top img-thumbnail'
                src="{{member.photoUrl || './assets/user.png'}}"
                alt="{{member.knownAs}}"
            >
            <!-- caso usuário não possua photoUrl, foto padrão user.png -->
            <div class="card-body">
                <div>
                    <strong>Location:</strong>
                    <p>{{member.city}}, {{member.country}}</p>
                </div>
                <div>
                    <strong>Age:</strong>
                    <p>{{member.age}}</p>
                </div>
                <div>
                    <strong>Last active:</strong>
                    <p>{{member.lastActive}}</p>
                </div>
                <div>
                    <strong>Member since:</strong>
                    <p>{{member.created}}</p>
                </div>
            </div>
            <div class="card-footer">
                <div class="btn-group d-flex">
                    <!-- d-flex: display flex -->
                    <button class="btn btn-success">Like</button>
                    <button class="btn btn-dark">Messages</button>
                </div>
            </div>
        </div>
    </div>
</div>


32. 
client\src\app\members\member-detail\member-detail.component.css

.img-thumbnail {
    margin: 25px;
    width: 85%;
    height: 85%;
}

.card-body {
    padding: 0 25px;
}

.card-footer {
    padding: 10px 15px;
    /* background-color: #fff; */
    /* border-top: none; */
}


### 112. Styling the member detailed page part two

33. 
https://valor-software.com/ngx-bootstrap/#/components
https://valor-software.com/ngx-bootstrap/#/components/tabs?tab=overview

client\src\app\_modules\shared.module.ts

...
import { TabsModule } from 'ngx-bootstrap/tabs';

@NgModule({
  ...
  imports: [
    ...
    **BsDropdownModule.forRoot(),**
    TabsModule.forRoot(),
    **ToastrModule.forRoot({**
      ...
    }),
  ],
  exports: [
    ...
    TabsModule,
  ]
})
...


34. 
client\src\app\members\member-detail\member-detail.component.html

<div
    ...
>
    ...

    <div class="col-8">
        <!-- esta segunda div ocupará o restante das 12 colunas (primeira ocupou as 4 iniciais à esquerda) -->
        <tabset class='member-tabset'>
            <tab heading="About {{member.knownAs}}">
                <h4>Description</h4>
                <p>{{member.introduction}}</p>
                <h4>Looking for</h4>
                <p>{{member.lookingFor}}</p>
            </tab>
            <tab heading="Interests">
                <h4>Interests</h4>
                <p>{{member.interests}}</p>
            </tab>
            <tab heading="Photos">
                <p>Photos will go here</p>
            </tab>
            <tab heading="Messages">
                <p>Messages will go here</p>
            </tab>
        </tabset>
    </div>
</div>


35. 
Copiamos o CSS que estava em (pasta fornecida pelo professor Neil):

StudentAssetsAug2023\StudentAssets\snippets\member-tabs-css.txt

Colamos o conteúdo do arquivo acima em:

client\src\styles.css

.tab-panel {
  border: 1px solid #ddd;
  padding: 10px;
  border-radius: 4px;
}

.nav-tabs > li.open, .member-tabset > .nav-tabs > li:hover {
  border-bottom: 4px solid #fbcdcf;
}

.member-tabset > .nav-tabs > li.open > a, .member-tabset > .nav-tabs > li:hover > a {
  border: 0;
  background: none !important;
  color: #333333;
}

.member-tabset > .nav-tabs > li.open > a > i, .member-tabset > .nav-tabs > li:hover > a > i {
  color: #a6a6a6;
}

.member-tabset > .nav-tabs > li.open .dropdown-menu, .member-tabset > .nav-tabs > li:hover .dropdown-menu {
  margin-top: 0px;
}

.member-tabset > .nav-tabs > li.active {
  border-bottom: 4px solid #E95420;
  position: relative;
}

.member-tabset > .nav-tabs > li.active > a {
  border: 0 !important;
  color: #333333;
}

.member-tabset > .nav-tabs > li.active > a > i {
  color: #404040;
}

.member-tabset > .tab-content {
  margin-top: -3px;
  background-color: #fff;
  border: 0;
  border-top: 1px solid #eee;
  padding: 15px 0;
}


### 113. Adding a photo gallery (Angular 16+)

https://ngx-gallery.netlify.app/#/
https://ngx-gallery.netlify.app/#/getting-started/gallery


36. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> npm i ng-gallery @angular/cdk
npm ERR! code ERESOLVE
npm ERR! ERESOLVE unable to resolve dependency tree
npm ERR! 
npm ERR! While resolving: client@0.0.0
npm ERR! Found: @angular/common@16.2.12
npm ERR! node_modules/@angular/common
npm ERR!   @angular/common@"^16.2.0" from the root project
npm ERR!   peer @angular/common@">=16.0.0" from ng-gallery@11.0.0
npm ERR!   node_modules/ng-gallery
npm ERR!     ng-gallery@"*" from the root project
npm ERR! 
npm ERR! Could not resolve dependency:
npm ERR! peer @angular/common@"^17.0.0 || ^18.0.0" from @angular/cdk@17.0.0
npm ERR! node_modules/@angular/cdk
npm ERR!   @angular/cdk@"*" from the root project
npm ERR!   peer @angular/cdk@">=16.0.0" from ng-gallery@11.0.0
npm ERR!   node_modules/ng-gallery
npm ERR!     ng-gallery@"*" from the root project
npm ERR! 
npm ERR! Fix the upstream dependency conflict, or retry
npm ERR! this command with --force or --legacy-peer-deps
npm ERR! to accept an incorrect (and potentially broken) dependency resolution.
npm ERR! 
npm ERR! 
npm ERR! For a full report see:
npm ERR! C:\Users\medve\AppData\Local\npm-cache\_logs\2023-11-08T17_20_21_204Z-eresolve-report.txt

17_20_21_204Z-debug-0.log


37. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> npm i ng-gallery @angular/cdk --legacy-peer-deps

added 3 packages, removed 1 package, and audited 967 packages in 5s

114 packages are looking for funding
  run `npm fund` for details

3 vulnerabilities (2 moderate, 1 critical)

To address all issues, run:
  npm audit fix

Run `npm audit` for details.


38. 
**ng-gallery**
Para usar as galerias desta dependência num componente é necessário que este seja um componente autônomo (standalone).
Faremos isso a seguir:


client\src\app\members\member-detail\member-detail.component.ts

...
import { CommonModule } from '@angular/common';
import { GalleryModule } from 'ng-gallery';
import { TabsModule } from 'ngx-bootstrap/tabs';  

@Component({
  ...,
  standalone: true,
  ...,
  ...,
  imports: [CommonModule, TabsModule, GalleryModule]
})
/*
  Ao definir propriedade "standalone: true" (acima), o componente torna-se autônomo (standalone)
  e não mais precisa ser declarado em @NgModule:

  client\src\app\app.module.ts

  @NgModule({
    declarations: [
      ...
      MemberDetailComponent,  // preciso removê-lo daqui para cessar erro
      ...
    ],
  })
*/


39. 
client\src\app\app.module.ts

@NgModule({
  declarations: [
    ...
    **MemberDetailComponent,**  // preciso removê-lo daqui para cessar erro
    ...
  ],
...
})


40. 
client\src\app\members\member-detail\member-detail.component.ts

Documentação explica como adicionar galeria ao código: 
                                                        https://ngx-gallery.netlify.app/#/gallery

...
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
...

@Component({
  ...
  imports: [..., GalleryModule]
})

**export class MemberDetailComponent implements OnInit {**
  ...
  images: GalleryItem[] = [];

  ...

  loadMember() {
    ...

    **this.memberService.getMember(username).subscribe({**
      **next: member => {**
        ...
        this.getImages();
      },
    });
  }

  getImages() {
    if (!this.member) return;

    for (const photo of this.member?.photos) {
      this.images.push(new ImageItem({ src: photo.url, thumb: photo.url }));
      // thumb (thumbnail = miniatura) -> imagem em miniatura
    }
  }
}


41. 
client\src\app\members\member-detail\member-detail.component.html

...
**<tab**
    **heading="Photos"**
    #photoTab="tab"
**>**
    <gallery
        class="gallery"
        *ngIf="photoTab.active"
        [items]="images"
    ></gallery>
**</tab>**
...


42. 
client\src\styles.css

...
.gallery {
  height: 600px;
}


43. 
Copiar ou mover arquivo:

    StudentAssetsAug2023\StudentAssets\user.png 
                para 
    client\src\assets\user.png


44. 
client\src\app\members\member-card\member-card.component.html

...
**<div class="card-img-wrapper">**
        **<img**
            **class='card-img-top'**
            src="{{member.photoUrl || './assets/user.png'}}"
            **alt="{{member.knownAs}}"**
        **>**
...

45. 
git add .  
git commit -m 'End of section 9'  
Clico em Sync Changes para sincronizar mudanças com repositório GitHub (equivale ao git push). 

### 114. LEGACY (will be removed Nov 23) Adding a photo gallery (Angular 13 and below)

### 115. Section 9 summary