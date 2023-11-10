# SECTION 10: Updating Resources

## Learning Goals

### 116. Introduction

Implement persistence when updating resources in the API and gaining an understanding of:

1. Angular Template Forms
2. The CanDeactivate Route Guard
3. The @ViewChild Decorator
4. Persisting Changes to the API
5. Adding Loading Indicators to the Client App
6. Caching Data in Angular Services


### 117. Creating a member edit component

**Objetivo** Ao clicar na barra de navegação na opção do lado superior direito em Edit Profile, poder atualizar dados do perfil do usuário.

1. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c members\member-edit --skip-tests --dry-run
CREATE src/app/members/member-edit/member-edit.component.html (26 bytes)
CREATE src/app/members/member-edit/member-edit.component.ts (221 bytes)
CREATE src/app/members/member-edit/member-edit.component.css (0 bytes)
UPDATE src/app/app.module.ts (2157 bytes)

NOTE: The "--dry-run" option means no changes were made.


2. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c members\member-edit --skip-tests
CREATE src/app/members/member-edit/member-edit.component.html (26 bytes)
CREATE src/app/members/member-edit/member-edit.component.ts (221 bytes)
CREATE src/app/members/member-edit/member-edit.component.css (0 bytes) 
UPDATE src/app/app.module.ts (2157 bytes)


3. 
client\src\app\app-routing.module.ts

import { MemberEditComponent } from './members/member-edit/member-edit.component';

...
{ path: 'member/edit', component: MemberEditComponent },
...


4. 
client\src\app\nav\nav.component.html

...
<a
    **class="dropdown-item"**
    routerLink="/member/edit"
>Edit Profile</a>
...


5. 
Ao clicar em Edit Profile, seremos redirecionados para o componente MemberEditComponent.
Preciso buscar o usuário pelo "username" para trazer os dados do perfil dele e atualizá-los.
Posso usar (injeção de dependência) AccountService para buscar por "username".

client\src\app\members\member-edit\member-edit.component.ts

import { ..., OnInit } from '@angular/core';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

...
export class MemberEditComponent implements OnInit {
  member: Member | undefined;
  user: User | null = null;

  constructor(private accountService: AccountService, private memberService: MembersService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user,
    });
  }

  ngOnInit() { 
    this.loadMember();
  }

  loadMember() {
    if (!this.user) return;

    this.memberService.getMember(this.user.username).subscribe({
      next: member => this.member = member,
    });
  }
}


6. 
client\src\app\members\member-edit\member-edit.component.html

<h1 *ngIf="member">{{ member.userName }}</h1>


### 118. Creating the edit template form

7. 
Copiamos todo o conteúdo de 
            **member.detail.component.html** 
e colamos em 
            **member.edit.component.html**
, mas com as devidas edições:

client\src\app\members\member-edit\member-edit.component.html

<div
    class="row"
    *ngIf="member"
>
    <div class="col-4">
        <h1>Your profile</h1>
    </div>
    <div class="col-8">
        <div class="alert alert-info pb-0">
            <p><strong>Information: </strong> You have made changes. Any unsaved changes will be lost.</p>
        </div>
    </div>
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
                <button class="btn btn-dark col-12">Save changes</button>
            </div>
        </div>
    </div>
    <div class="col-8">
        <!-- esta segunda div ocupará o restante das 12 colunas (primeira ocupou as 4 iniciais à esquerda) -->
        <tabset class='member-tabset'>
            <tab heading="About {{member.knownAs}}">
                <form>
                    <h4 class='mt-2'>Description</h4>
                    <textarea
                        class='form-control'
                        [(ngModel)]="member.introduction"
                        name='introduction'
                        rows='6'                        
                    ></textarea>
                    <h4 class='mt-2'>Looking for</h4>
                    <textarea
                        class='form-control'
                        [(ngModel)]="member.lookingFor"
                        name='lookingFor'
                        rows='6'
                    ></textarea>
                    <h4 class='mt-2'>Interests</h4>
                    <textarea
                        class='form-control'
                        [(ngModel)]="member.interests"
                        name='interests'
                        rows='6'
                    ></textarea>
                    <h4 class='mt-2'>Location details:</h4>
                    <div class="d-flex flex-row align-items-center">
                        <label for="city">City</label>
                        <input
                            id='city'
                            [(ngModel)]='member.city'
                            name='city'
                            class='form-control mx-2'
                        >
                        <!-- mx-2: margem esquerda e direita (creio que 'x' seja de eixo horizontal) -->
                        <label for="country">Country</label>
                        <input
                            id='country'
                            [(ngModel)]='member.country'
                            name='country'
                            class='form-control mx-2'
                        >
                    </div>
                </form>
            </tab>
            <tab heading="Edit Photos">
                <p>Photos edit will go here</p>
            </tab>
        </tabset>
    </div>
</div>


8. 
Copiamos todo o conteúdo de 
            **member-detail.component.css**
e colamos em 
            **member-edit.component.css**:

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


### 119. Adding the update functionality

9. 
client\src\app\members\member-edit\member-edit.component.html

...
**<div**
    **class="alert alert-info pb-0"**
    *ngIf="editForm.dirty"
**>**
    **<p><strong>Information: </strong> You have made changes. Any unsaved changes will be lost.</p>**
**</div>**
...
**<div class="card-footer">**
  **<button**
      **class="btn btn-success col-12"**
      type='submit'
      form='editForm'
      [disabled]="!editForm.dirty"
  **>Save changes</button>**
...
<form
    id='editForm'
    #editForm='ngForm'
    (ngSubmit)="updateMember()"
>
...


10. 
client\src\app\members\member-edit\member-edit.component.ts

import { ToastrService } from 'ngx-toastr';
import { ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';

**export class MemberEditComponent implements OnInit {**
 @ViewChild('editForm') editForm: NgForm | undefined;
...
 constructor(
    ..., ..., private toastr: ToastrService
  ) {
    ...
  }

  ngOnInit() {
    ...
  }

  loadMember() {
    ...
  }

  updateMember() {
    console.log(this.member);
    this.toastr.success('Profile updated successfully!');
    this.editForm?.reset(this.member);
  }
...


### 120. Adding a Can Deactivate route guard

Para evitar que o usuário, por exemplo, preencha um formulário como na lição anterior e clique em um link/botão para navegar para outra página sem que haja salvado os dados do formulário editado, emitiremos um aviso alertando se ele tem certeza de que é isso o que deseja.
Inicialmente, este aviso será o modalzinho feinho acionado pela função JS confirm(). Posteriormente, iremos melhorar isso.

11. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g --help


12. 
**Antes de selecionar opção:**
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g guard _guards/prevent-unsaved-changes --skip-tests --dry-run
? Which type of guard would you like to create? (Press <space> to select, <a> to toggle all, <i> to invert selection, and <enter> to proceed)
>( ) CanActivate
 ( ) CanActivateChild
 (*) CanDeactivate
 ( ) CanMatch

 **PRESSIONE TECLA DE ESPAÇO PARA DESMARCAR OPÇÃO INDESEJADA OU TECLA DE ESPAÇO PARA MARCAR NOVA OPÇÃO DESEJADA**

**Depois de selecionar opção:**
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g guard _guards/prevent-unsaved-changes --skip-tests --dry-run
? Which type of guard would you like to create? CanDeactivate      
CREATE src/app/_guards/prevent-unsaved-changes.guard.ts (194 bytes)

NOTE: The "--dry-run" option means no changes were made.

13. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g guard _guards/prevent-unsaved-changes --skip-tests
? Which type of guard would you like to create? CanDeactivate      
CREATE src/app/_guards/prevent-unsaved-changes.guard.ts (194 bytes)


14. 
client\src\app\_guards\prevent-unsaved-changes.guard.ts

...
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

export const preventUnsavedChangesGuard: CanDeactivateFn<MemberEditComponent> = (component) => {
  if (component.editForm?.dirty)
    return confirm('Are you sure you want to continue? Any unsaved changes will be lost.');

  **return true;**
};


15. 
client\src\app\app-routing.module.ts

import { preventUnsavedChangesGuard } from './_guards/prevent-unsaved-changes.guard';

...
**{ path: 'member/edit', component: MemberEditComponent,** canDeactivate: [preventUnsavedChangesGuard] },
...

***Os passos acima impedirão o usuário de clicar em um link/botão na barra de navegação e perder dados que houvesse digitado para atualizar seu perfil, mas não impediriam um Alt Tab ou digitar uma URL diferente diretamente na barra de endereços do navegador. Para evitar isso é necessário mais implementação (abaixo):***

16. 
client\src\app\members\member-edit\member-edit.component.ts

import { ..., HostListener, ..., ... } from '@angular/core';

...
**@ViewChild('editForm') editForm: NgForm | undefined;**
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event: any) {
    if (this.editForm?.dirty) {
      $event.returnValue = true;
      /*
          $event.returnValue = true;
          Isso ativa a capacidade do navegador de impedir que saiamos de onde estávamos.
          Se o formulário estiver sujo, abrir-se-á um prompt no navegador e perguntará se usuário deseja continuar. 
      */
    }
  };
  **member: Member | undefined;**


### 121. Persisting the changes in the API

17. 
API\DTOs\MemberUpdateDto.cs

namespace API.DTOs;

public class MemberUpdateDto
{
    public string Introduction { get; set; }
    public string LookingFor { get; set; }
    public string Interests { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}


18. 
API\Helpers\AutoMapperProfiles.cs

...
CreateMap<MemberUpdateDto, AppUser>();
/*
    Como as propriedades de MemberUpdateDto correspondem exatamente às 
    propriedades de AppUser, é desnecessário qualquer lógica adicional.
*/


19. 
API\Controllers\UsersController.cs

using System.Security.Claims;
...

[HttpPut]
public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
{
    /*
        Obteremos o nome de usuário do token, con-
        siderando que esta rota é autenticada, pois 
        ele só poderá atualizar seu próprio perfil
    */
    var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    /*
        ?.Value -> encadeamento opcional (acima) para o caso do username ser null
        Afinal, FindFirst pode resultar em ArgumentNullException. Assim evita-se isso.
    */
    var user = await _userRepository.GetUserByUsernameAsync(username);

    if (user == null) return NotFound();
    /*
        Agora podemos usar AutoMapper para atualizar propriedades.
        De MemberUpdateDto para User.
        Quando buscamos "user" do repositório, EF está rastreando este usuário e qual-
        quer atualização para ele será rastreada por EF.

        _mapper.Map(memberUpdateDto, user);
        Esta linha de código está mapeando e atualizando todas as propriedades de Member-
        UpdateDto e substituindo pelos novos dados/propriedades passados pelo usuário.
        Nada ainda, porém, foi salvo no DB.
    */
    _mapper.Map(memberUpdateDto, user);

    if (await _userRepository.SaveAllAsync()) return NoContent();
    /*
        Caso tente enviar dados no corpo da requisição iguais aos de uma requisição ante-
        rior, que já sejam os dados da entidade/perfil sem qualquer atualização de fato,
        o fluxo do código seguirá para a linha de baixo e retornará bad request.
        Isso porque o mapeamento, AtuoMapper, não detectou nunhuma alteração entre Member-
        UpdateDto e AppUser.
    */

    return BadRequest("Failed to update user");
}


### 122. Updating the user in the client app

20. 
client\src\app\_services\members.service.ts

...
updateMember(member: Member) {
    return this.http.put<Member>(this.baseUrl + 'users', member);
}


21. 
client\src\app\members\member-edit\member-edit.component.ts

...
updateMember() {
    this.memberService.updateMember(this.editForm?.value).subscribe({
      next: _ => {
        /*
          updateMember de memberService retorna void, por isso o "_"
        */
        this.toastr.success('Profile updated successfully!');
        this.editForm?.reset(this.member);
      }
    });
}
...


### 123. Adding loading indicators

Veja modelos de giradores (spinners) para página solicitada em carregamento:

https://napster2210.github.io/ngx-spinner/

Passo a passo (Neil fez um pouco diferente, criou um interceptor):

https://github.com/Napster2210/ngx-spinner


22. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> npm i ngx-spinner --save --legacy-peer-deps

**OBS** usei --legacy-peer-deps porquanto houve erro na instalação sem esta flag.


23. 
client\angular.json

...
"styles": [
    ...
    **"node_modules/ngx-toastr/toastr.css",**
    "node_modules/ngx-spinner/animations/ball-atom.css",
    **"src/styles.css"**
],
...


24. 
client\src\app\_modules\shared.module.ts

import { NgxSpinnerModule } from 'ngx-spinner';

...
imports: [
    ...
    NgxSpinnerModule,
],
exports: [
    ...
    NgxSpinnerModule,
  ]
...


25. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g s _services\busy --skip-tests
CREATE src/app/_services/busy.service.ts (133 bytes)


26. 
client\src\app\_services\busy.service.ts

...
import { NgxSpinnerService } from 'ngx-spinner';

...
**export class BusyService {**
  busyRequestCount = 0;

  constructor(private spinnerService: NgxSpinnerService) { }

  busy() {
    this.busyRequestCount++;
    this.spinnerService.show(undefined, {
      // bdColor: 'rgba(255,255,255,0)',
    });
  }

  idle() {
    this.busyRequestCount--;
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide();
    }
  }
**}**


27. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g interceptor _interceptors/loading --skip-tests
CREATE src/app/_interceptors/loading.interceptor.ts (412 bytes)


28. 
client\src\app\_interceptors\loading.interceptor.ts

...
import { delay, finalize, ... } from 'rxjs';
import { BusyService } from '../_services/busy.service';

...
**export class LoadingInterceptor implements HttpInterceptor {**

  **constructor**(private busyService: BusyService) { }

  /*
    Se estivermos chamando o método intercept() significa que uma solicitação HTTP está em andamento.
    Portanto, antes da instrução de retorno, invocamos this.busyService.busy(). Este irá incrementar
    a contagem de solicaitações ocupadas (busy) e em seguida retornar.
  */
  **intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {**
    this.busyService.busy();

    return next.handle(request).pipe(
      delay(1000),
      finalize(() => {
        this.busyService.idle();
      }),
      /* uso de "delay" para simular um atraso numa requisição HTTP, algo que 
         não ocorre agora por estarmos executando a solicitação em localhost.

         uso de "finalize" para mostrar o que fazer após solicitação ser concluída:
         invocar this.busyService.idle() para desativar spinner.
      */
    );
  }
}

29. 
client\src\app\app.module.ts

import { LoadingInterceptor } from './_interceptors/loading.interceptor';

providers: [
    ...
    { provide: HTTP_INTERCEPTORS, useClass: LoadingInterceptor, multi: true },
],


30. 
client\src\app\app.component.html

<ngx-spinner type="ball-atom">
    <h3>Loading...</h3>
</ngx-spinner>


31. 
client\src\app\app.component.css

h3 {
    color: #ffffffc9;
}


### 124. Using the service to store state

Na aula anterior, vimos algo desnecessário em nossa aplicação.
Ao clicar no botãozinho do usuário, dentro do próprio card do usuário, estamos fazendo uma requisição HTTP para buscar os dados do perfil, dados que já temos, pois o usuário já está logado.
Ou seja, isso seria necessário para visualizar perfis de outros usuários, mas não o que está logado.

Quando mudo de um componente para outro, o componente e os dados nele são destruídos.
Há serviços de memória disponíveis em uma aplicação Angular. Estes não são perdidos.
Assim, um modo de corrigir o problema é armazenar os usuários, não na variável members em MemberListComponent, destruído toda vez que dele saímos, mas em algum lugar em MembersService. Isso permitira inclusive usá-los em diferentes lugares da aplicação sem precisar requisitá-los toda vez do servidor.
Eis exatamente o que iremos fazer nos passos seguintes:

32. 
client\src\app\_services\members.service.ts

...
import { map, of } from 'rxjs';

...
export class MembersService {
  ...
  members: Member[] = [];

  ...

  /*
    Não precisamos passar token nos métodos abaixo porquanto este está sendo provido em nível superior pelo JwtInterceptor
  */
  **getMembers() {**
    if (this.members.length) return of(this.members);
    /*
      Como é preciso retornar um Observable no if acima, não podemos retornar diretamente members, que é do tipo Members[]. Logo, usamos "of" de rxjs que retornará Observable<Member[]>.
      Uma vez que os membros tenham sido retornados na requisição, posso atribuí-los à variável/propriedade members.
      Ao chamar getMembers(), se não houver nada na propriedade members, daí sim faço a requisição da API (return abaixo).

      Após buscar usuários da API, caso necessário, usamos o método "pipe" com o método "map" dentro. Para projetar o
      que recebemos da API, a lista de membros. 
      Como nosso componente está utilizando esta lista com propriedade members, precisamos retorná-los também como "return" 
      aninhado abaixo.
    */
   **return this.http.get<Member[]>(this.baseUrl + 'users')**.pipe(
      map(members => {
        this.members = members;
        return members;
      })
    );
  **}**

  **getMember(username: string) {**
    const member = this.members.find(x => x.userName == username);
    if (member) return of(member);

   **return this.http.get<Member>(this.baseUrl + 'users/' + username);**
  **}**

  **updateMember(member: Member) {**
    **return this.http.put<Member>(this.baseUrl + 'users', member)**.pipe(
      map(_ => {
        const index = this.members.indexOf(member);
        this.members[index] = { ...this.members[index], ...member };
      })
    );
  **}**
**}**


33. 
client\src\app\members\member-list\member-list.component.ts

...
import { Observable } from 'rxjs';
import { Member } from 'src/app/_models/member';
...

export class MemberListComponent implements OnInit {
  members$: Observable<Member[]> | undefined;
  // $ representa um Observable

  **constructor(private memberService: MembersService) { }**

  **ngOnInit() {**
    this.members$ = this.memberService.getMembers();
  **}**
}


34. 
client\src\app\members\member-list\member-list.component.html

...
<div
    ...
    *ngFor="let member of members$ | async"
>
...


35. 
git add .  
git commit -m 'End of section 10'  
Clico em Sync Changes para sincronizar mudanças com repositório GitHub (equivale ao git push). 

### 125. Section 10 summary
