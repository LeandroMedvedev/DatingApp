# SECTION 11: Adding Photo Upload Functionality

## Learning Goals

### 126. Introduction

Implement photo upload functionality int the application and gain an understanding of the following:

1. Photo storage options
2. Adding related entities
3. Using a 3rd party API
4. Using the Debugger (again)
5. Updating and deleting resources
6. What to return when creating resources in a REST based API

**Photo Storage Options**

**I.** DB como grandes objetos binários? ***NO***
**II.** Usar o sistema de arquivos em nosso servidor? ***NO***
**III.** Usar um serviço em nuvem? ***YES***

Usaremos Cloudinary. Este fornece um nível gratuito de 10 gigabytes e um modo de transformar as imagens carregadas no servidor backend, sem exigência de cartão de crédito.

**Image Upload in Our App**

O usuário autenticado apenas poderá arrastar e soltar uma imagem em nosso app Angular.

**Step 1** Client uploads photo to API with JWT                       CLIENT     -> API/SERVER
**Step 2** Server uploads the photo to Cloudinary                     API/SERVER -> CLOUDINARY
**Step 3** Cloudinary stores photo, sends response                    CLOUDINARY -> API/SERVER
**Step 4** API saves photo URL and Public ID to DB                    API/SERVER -> DB
**Step 5** Saved in DB and given auto generated ID                    DB         -> API/SERVER
**Step 6** 201 Created Response sent to client with location header   API/SERVER -> CLIENT

**CLIENT -1> API/SERVER                       -2> CLOUDINARY**
**CLIENT <6- API/SERVER <5- DB <4- API/SERVER <3- CLOUDINARY**


### 127. Cloudinary Account

https://cloudinary.com/
(conta do Infnet que fiz inscrição pelo Google)


### 128. Configuring cloudinary in the API

https://www.nuget.org/packages/CloudinaryDotNet/1.20.0

1. 
PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet add package CloudinaryDotNet --version 1.20.0


2. 

Em meu Dashboard em Cloudinary, obtenho as informações que precisarei pôr no arquivo abaixo:
CloudName
ApiKey
ApiSecret

API\appsettings.json

...
**"AllowedHosts": "*",**
  "CloudinarySettings": {
    "CloudName": "...",
    "ApiKey": "...",
    "ApiSecret": "..."
}


3. 
API\Helpers\CloudinarySettings.cs

namespace API.Helpers;

public class CloudinarySettings
{
    public string CloudName { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
}


4. 
Acrescento CloudinarySettings em meu serviço de extensão ApplicationServiceExtensions:

API\Extensions\ApplicationServiceExtensions.cs

...
**services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());**
services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));


### 129. Adding a photo service

Para upload e exclusão de fotos iremos criar um serviço. Serviço neste caso é o ideal porquanto qualquer coisa fora do acesso do DB que possamos precisar usar em várias partes diferentes do nosso app. Serviço é normalmente criado como singleton e permite injeção de dependência em qualquer parte. Criaremos uma interface 1°:

5. 
API\Interfaces\IPhotoService.cs

using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
    
}


6. 
API\Services\PhotoService.cs

using API.Helpers;
using API.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> config)
    {
        var acc = new Account
        (
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(acc);
    }

    public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face"),
                Folder = "dating-app"
                /*
                    "Crop" (cortar) para transformar a imagem; por exemplo, uma imagem retangular em quadrada.
                    "Gravity" para focar em preservar, por exemplo, a face/rosto ao cortar.
                    "Folder" especifica a pasta em que as imagens ficarão armazenadas.
                */
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    public async Task<DeletionResult> DeletePhotoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        return await _cloudinary.DestroyAsync(deleteParams);
    }
}


7. 
Uma vez criados novo serviço e nova interface, preciso adicioná-los ao método de extensão ApplicationServiceExtensions. Isso os tornará disponíveis para ser injetados em outras classes.

API\Extensions\ApplicationServiceExtensions.cs

...
**services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));**
services.AddScoped<IPhotoService, PhotoService>();


### 130. Updating the users controller

8. 
API\Extensions\ClaimsPrincipalExtensions.cs

using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUsername(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    /*
        Debugger:
        this: / User: / Claims [IEnumerable]: / Visualização dos Resultados: / [0] [Claim]:
        [0] [Claim] {http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: lisa}
        value [string]: "lisa"

        ?.Value -> encadeamento opcional (acima) para o caso do username ser null
        Afinal, FindFirst pode resultar em ArgumentNullException. Assim evita-se isso.
    */
}


9. 
API\Controllers\UsersController.cs

...
private readonly IPhotoService _photoService;

public UsersController(..., ..., IPhotoService photoService)
{
    ...
    _photoService = photoService;
}
...

[HttpPost("add-photo")]
public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
{
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

    if (user == null) return NotFound();

    var result = await _photoService.AddPhotoAsync(file);

    if (result.Error != null) return BadRequest(result.Error.Message);

    var photo = new Photo
    {
        Url = result.SecureUrl.AbsoluteUri,
        PublicId = result.PublicId
    };

    /*
        Se for 1° carregamento de foto do usuário, colocaremos esta como principal.
    */
    if (user.Photos.Count == 0) photo.IsMain = true;

    user.Photos.Add(photo);

    if (await _userRepository.SaveAllAsync()) return _mapper.Map<PhotoDto>(photo);

    return BadRequest("Problem adding photo");
}


### 131. Testing the photo upload

Apenas testes no Postman.


### 132. Using the Created At Route method

10. 
API\Controllers\UsersController.cs

...
**[HttpPost("add-photo")]**
**public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)**
{
    ...

   **if (await _userRepository.SaveAllAsync())**
   {
       return CreatedAtAction(
           nameof(GetUser), new { username = user.UserName }, _mapper.Map<PhotoDto>(photo)
       );
   }

   **return BadRequest("Problem adding photo");**
}


### 133. Adding a photo editor component

11. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c members\photo-editor --skip-tests --dry-run
CREATE src/app/members/photo-editor/photo-editor.component.html (27 bytes)
CREATE src/app/members/photo-editor/photo-editor.component.ts (225 bytes)
CREATE src/app/members/photo-editor/photo-editor.component.css (0 bytes) 
UPDATE src/app/app.module.ts (2333 bytes)

NOTE: The "--dry-run" option means no changes were made.


12. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c members\photo-editor --skip-tests
CREATE src/app/members/photo-editor/photo-editor.component.html (27 bytes)
CREATE src/app/members/photo-editor/photo-editor.component.ts (225 bytes)
CREATE src/app/members/photo-editor/photo-editor.component.css (0 bytes)
UPDATE src/app/app.module.ts (2333 bytes)


13. 
client\src\app\members\photo-editor\photo-editor.component.ts

import { ..., Input } from '@angular/core';
import { Member } from 'src/app/_models/member';

...
**export class PhotoEditorComponent {**
  @Input() member: Member | undefined;
  /*
    PhotoEditorComponent é filho de MemberEditComponent

    @Input() member: Member | undefined; COMPONENTE PAI PASSA DADO "MEMBER" PARA COMPONENTE FILHO
    member é inicialmente undefined, só depois torna-se Member
  */
**}**


14. 
client\src\app\members\photo-editor\photo-editor.component.html

<div
    class="row"
    *ngIf="member"
>
    <div
        class="col-2"
        *ngFor="let photo of member.photos"
    >
        <img
            class='img-thumbnail mb-1'
            src="{{photo.url}}"
            alt="photo of user profile"
        >
        <div class="text-center">
            <button class='btn btn-sm'>Main</button>
            <button class='btn btn-sm btn-danger'>
                <i class="fa fa-trash"></i>
            </button>
        </div>
    </div>
</div>


15. 
client\src\app\members\member-edit\member-edit.component.html

***Este trecho de código abaixo estava assim:***

...
    <tab heading="Edit Photos">
        <p>Photos edit will go here</p>
    </tab>
</tabset>
...

***Ficou assim:***

...
    <tab heading="Edit Photos">
        <app-photo-editor [member]="member"></app-photo-editor>
    </tab>
</tabset>
...


### 134. Adding a photo uploader

Adicionar a capacidade dos usuários carregarem fotos.
Não codificaremos esta parte do zero. Usaremos ng2-file-upload:

https://valor-software.com/ng2-file-upload/

https://github.com/valor-software/ng2-file-upload


16. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> npm i ng2-file-upload --legacy-peer-deps

added 1 package, and audited 969 packages in 2s

114 packages are looking for funding
  run `npm fund` for details

3 vulnerabilities (2 moderate, 1 critical)

To address all issues, run:
  npm audit fix

Run `npm audit` for details.


17. 
Documentação de como usar ng2-file-ipload:

https://valor-software.com/ng2-file-upload/

No 1° trecho de código que aparece, mais de 100 linhas, copiamos da linha 17 até a 116 e colamos no arquivo abaixo:

client\src\app\members\photo-editor\photo-editor.component.html

...

<div class="row mb-5">
    <div class="col-md-3">
        <h3>Select files</h3>
        <div
            ng2FileDrop
            [ngClass]="{'nv-file-over': hasBaseDropZoneOver}"
            (fileOver)="fileOverBase($event)"
            [uploader]="uploader"
            class="well my-drop-zone"
        >
            Base drop zone
        </div>
        <div
            ng2FileDrop
            [ngClass]="{'another-file-over-class': hasAnotherDropZoneOver}"
            (fileOver)="fileOverAnother($event)"
            [uploader]="uploader"
            class="well my-drop-zone"
        >
            Another drop zone
        </div>
        Multiple
        <input
            type="file"
            ng2FileSelect
            [uploader]="uploader"
            multiple
        /><br />
        Single
        <input
            type="file"
            ng2FileSelect
            [uploader]="uploader"
        />
    </div>
    <div
        class="col-md-9"
        style="margin-bottom: 40px"
    >
        <h3>Upload queue</h3>
        <p>Queue length: {{ uploader?.queue?.length }}</p>
        <table class="table">
            <thead>
                <tr>
                    <th width="50%">Name</th>
                    <th>Size</th>
                    <th>Progress</th>
                    <th>Status</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>
                <tr *ngFor="let item of uploader.queue">
                    <td><strong>{{ item?.file?.name }}</strong></td>
                    <td
                        *ngIf="uploader.options.isHTML5"
                        nowrap
                    >{{ item?.file?.size/1024/1024 | number:'.2' }} MB</td>
                    <td *ngIf="uploader.options.isHTML5">
                        <div
                            class="progress"
                            style="margin-bottom: 0;"
                        >
                            <div
                                class="progress-bar"
                                role="progressbar"
                                [ngStyle]="{ 'width': item.progress + '%' }"
                            ></div>
                        </div>
                    </td>
                    <td class="text-center">
                        <span *ngIf="item.isSuccess"><i class="glyphicon glyphicon-ok"></i></span>
                        <span *ngIf="item.isCancel"><i class="glyphicon glyphicon-ban-circle"></i></span>
                        <span *ngIf="item.isError"><i class="glyphicon glyphicon-remove"></i></span>
                    </td>
                    <td nowrap>
                        <button
                            type="button"
                            class="btn btn-success btn-xs"
                            (click)="item.upload()"
                            [disabled]="item.isReady || item.isUploading || item.isSuccess"
                        >
                            <span class="glyphicon glyphicon-upload"></span> Upload
                        </button>
                        <button
                            type="button"
                            class="btn btn-warning btn-xs"
                            (click)="item.cancel()"
                            [disabled]="!item.isUploading"
                        >
                            <span class="glyphicon glyphicon-ban-circle"></span> Cancel
                        </button>
                        <button
                            type="button"
                            class="btn btn-danger btn-xs"
                            (click)="item.remove()"
                        >
                            <span class="glyphicon glyphicon-trash"></span> Remove
                        </button>
                    </td>
                </tr>
            </tbody>
        </table>
        <div>
            <div>
                Queue progress:
                <div
                    class="progress"
                    style=""
                >
                    <div
                        class="progress-bar"
                        role="progressbar"
                        [ngStyle]="{ 'width': uploader.progress + '%' }"
                    ></div>
                </div>
            </div>
            <button
                type="button"
                class="btn btn-success btn-s mt-3 me-2"
                (click)="uploader?.uploadAll()"
                [disabled]="!uploader?.getNotUploadedItems()?.length"
            >
                <span class="fa fa-upload"></span> Upload all
            </button>
            <button
                type="button"
                class="btn btn-warning btn-s mt-3 me-2"
                (click)="uploader?.cancelAll()"
                [disabled]="!uploader?.isUploading"
            >
                <span class="fa fa-ban"></span> Cancel all
            </button>
            <button
                type="button"
                class="btn btn-danger btn-s mt-3"
                (click)="uploader?.clearQueue()"
                [disabled]="!uploader?.queue?.length"
            >
                <span class="fa fa-trash"></span> Remove all
            </button>
        </div>
    </div>
</div>


18. 
client\src\app\_modules\shared.module.ts

...
import { FileUploadModule } from 'ng2-file-upload';

...
  imports: [
    ...
    NgxSpinnerModule,
    FileUploadModule,
  ],
  exports: [
    ...
    FileUploadModule,
  ]
...


19. 
client\src\app\members\photo-editor\photo-editor.component.ts

import { ..., ..., OnInit } from '@angular/core';
...
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { take } from 'rxjs';

...
export class PhotoEditorComponent implements OnInit {
  ...
  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  user: User | undefined;

  constructor(private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) this.user = user;
      }
    });
  }

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(event: any) {
    this.hasBaseDropZoneOver = event;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      /*
        Como isso está fora das solicitações HTTP do Angular, não usaremos o HTTP Inter-
        ceptor. Então precisaremos especificar nosso token de autenticação aqui dentro.
      */
      authToken: 'Bearer ' + this.user?.token,  // ?. optional chaining (encadeamento opcional)
      isHTML5: true,
      allowedFileType: ['image'], // permite todos os tipos de imagem: jpeg, png...
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024  // Cloudinary permite arquivos de no máximo 10 megas
    })

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false
      /*
        Se não fizermos isso, precisaremos ajustar nossa configuração do CORS e não queremos isso.
      */
    }

    /*
      A seguir, o que queremos fazer depois que o arquivo for carregado com sucesso?
    */
    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo = JSON.parse(response);
        this.member?.photos.push(photo);
      }
    }
  }
}


### 135. Adding a photo uploader part two

20. 
client\src\app\members\photo-editor\photo-editor.component.html

***A maior parte deste arquivo é formada pelo código que copiamos da documentação do ng2-upload-file (está no passo 17). Abaixo está o código do passo 17, mas com as devidas modifiações para funcionar em nosso app:***

<div
    class="row mb-5"
    *ngIf="member"
>
    <div
        class="col-2 mb-3"
        *ngFor="let photo of member.photos"
    >
        <img
            class='img-thumbnail mb-1'
            src="{{photo.url}}"
            alt="photo of user profile"
        >
        <div class="text-center">
            <button class='btn btn-sm'>Main</button>
            <button class='btn btn-sm btn-danger'>
                <i class="fa fa-trash"></i>
            </button>
        </div>
    </div>
</div>

<!- Código copiado da documentação inicia aqui (está modificado para funcionar aqui)->
<div class="row">
    <div class="col-3">
        <h3>Add Photos</h3>
        <div
            ng2FileDrop
            [ngClass]="{'nv-file-over': hasBaseDropZoneOver}"
            (fileOver)="fileOverBase($event)"
            [uploader]="uploader"
            class="card bg-faded p-3 text-center mb-3 my-drop-zone"
        >
            <i class='fa fa-upload fa-3x'></i>
            Drop photos here
        </div>
        Multiple
        <input
            type="file"
            ng2FileSelect
            [uploader]="uploader"
            multiple
        /><br />
        Single
        <input
            type="file"
            ng2FileSelect
            [uploader]="uploader"
        />
    </div>
    <div
        class="col-9"
        style="margin-bottom: 40px"
        *ngIf="uploader?.queue?.length"
    >
        <h3>Upload queue</h3>
        <p>Queue length: {{ uploader?.queue?.length }}</p>
        <table class="table">
            <thead>
                <tr>
                    <th width="50%">Name</th>
                    <th>Size</th>
                </tr>
            </thead>
            <tbody>
                <tr *ngFor="let item of uploader?.queue">
                    <td><strong>{{ item?.file?.name }}</strong></td>
                    <td
                        *ngIf="uploader?.options?.isHTML5"
                        nowrap
                    >{{ item?.file?.size/1024/1024 | number:'.2' }} MB</td>
                </tr>
            </tbody>
        </table>
        <div>
            <div>
                Queue progress:
                <div class="progress">
                    <div
                        class="progress-bar"
                        role="progressbar"
                        [ngStyle]="{ 'width': uploader?.progress + '%' }"
                    ></div>
                </div>
            </div>
            <button
                type="button"
                class="btn btn-success btn-s mt-3 me-2"
                (click)="uploader?.uploadAll()"
                [disabled]="!uploader?.getNotUploadedItems()?.length"
            >
                <span class="fa fa-upload"></span> Upload all
            </button>
            <button
                type="button"
                class="btn btn-warning btn-s mt-3 me-2"
                (click)="uploader?.cancelAll()"
                [disabled]="!uploader?.isUploading"
            >
                <span class="fa fa-ban"></span> Cancel all
            </button>
            <button
                type="button"
                class="btn btn-danger btn-s mt-3"
                (click)="uploader?.clearQueue()"
                [disabled]="!uploader?.queue?.length"
            >
                <span class="fa fa-trash"></span> Remove all
            </button>
        </div>
    </div>
</div>


21. 
client\src\app\members\photo-editor\photo-editor.component.css

/* 
Classe da zona sobre a qual arrastamos arquivos.
Estava sem estilo algum aplicado. Pairávamos com
o arquivo sobre ela e nada ocorria para ao menos
sinalizar que o arquivo estava sobre ela, ainda
que não fosse solto.
*/
.nv-file-over {
    border: dotted 3px #f00;
}

input[type=file] {
    color: transparent; /* ocultar texto "nenhum ficheiro selecionado" */
}


### 136. Setting the main photo in the API

22. 
API\Controllers\UsersController.cs

...

[HttpPut("set-main-photo/{photoId}")]
public async Task<ActionResult> SetMainPhoto(int photoId)
{
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
    if (user == null) return NotFound();

    var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

    if (photo == null) return NotFound();

    if (photo.IsMain) return BadRequest("this is already your main photo");

    var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
    if (currentMain != null) currentMain.IsMain = false;  // defino foto principal atual como false
    photo.IsMain = true; // defino foto que será a principal, a partir de agora, como true
    // isso garante que haverá sempre 1 foto principal somente, as demais serão { isMain: false }

    if (await _userRepository.SaveAllAsync()) return NoContent();

    return BadRequest("Problem setting main photo");
}
...


### 137. Adding the main photo image to the nav bar

23. 
API\DTOs\UserDto.cs

...
public string PhotoUrl { get; set; }
...


24. 
API\Controllers\AccountController.cs

...
**[HttpPost("login")]**
**public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)**
{
    **var user = await _context.Users**
            .Include(p => p.Photos)
            **.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);**

   **return new UserDto**
   {
       ...,
       PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
   **};**
}


25. 
client\src\app\_models\user.ts

...
photoUrl: string;


26. 
client\src\app\_services\account.service.ts

...
**login(model: any) {**
    **return this.http.post<User>(`${this.baseUrl}account/login`, model).pipe(**
    ...
        **if (user) {**
          this.setCurrentUser(user);
        **}**
    ...
    );
  }

  **register(model: any) {**
    **return this.http.post<User>(`${this.baseUrl}account/register`, model).pipe(**
      ...
        **if (user) {**
          this.setCurrentUser(user);
        **}**
      ...
    );
  }

  **setCurrentUser(user: User) {**
    localStorage.setItem('user', JSON.stringify(user));
    **this.currentUserSource.next(user);**
  **}**


27. 
client\src\app\nav\nav.component.html

...
**<div**
    ...
    **dropdown**
**>**
    <img
        src="{{user.photoUrl  || './assets/user.png'}}"
        alt="user photo"
        class='me-2'
    >
    **<a**
        ...
    **>Welcome, {{ user.username | titlecase }}</a>**


28. 
client\src\app\nav\nav.component.css

...
img {
    max-height: 50px;
    border: 2px solid #fff;
    display: inline;
}


### 138. Setting the main photo in the client

29. 
client\src\app\_services\members.service.ts

...
setMainPhoto(photoId: number) {
   return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
}


30. 
client\src\app\members\photo-editor\photo-editor.component.ts

import { MembersService } from 'src/app/_services/members.service';

...
**constructor(...,** private memberService: MembersService) **{**
    ...
  }

...

setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo.id).subscribe({
      /* "_" porque setMainPhoto não retorna nada */
      next: _ => {
        if (this.user && this.member) {
          this.user.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);
          this.member.photoUrl = photo.url;
          this.member.photos.forEach(p => {
            if (p.isMain) p.isMain = false;
            if (p.id === photo.id) p.isMain = true;
          });
        }
      }
    });
}
...


31. 
client\src\app\members\photo-editor\photo-editor.component.html

...
**<div class="text-center">**
    **<button**
        ...
        [disabled]="photo.isMain"
        (click)="setMainPhoto(photo)"
        [ngClass]="photo.isMain ? 'btn-success active' : 'btn-outline-success'"
    **>Main</button>**
    <button class='... btn-outline-danger ms-2'>
        ...
    **</button>**
...

### 139. Deleting photos - API

32. 
API\Controllers\UsersController.cs

...
[HttpDelete("delete-photo/{photoId}")]
public async Task<ActionResult> DeletePhoto(int photoId)
{
    var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
    var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

    if (photo == null) return NotFound();

    if (photo.IsMain) return BadRequest("You cannot delete your main photo");

    if (photo.PublicId != null)
    {
        /*
            Esta verificação é feita porque há imagens que não possuem PublicId em nosso db.
            E, se elas não possuem PublicId, então é uma das imagens que nós semeamos.
            E não precisamos apagar essas de Cloudinary porque simplesmente não estão na nuvem de qualquer modo.
        */
        var result = await _photoService.DeletePhotoAsync(photo.PublicId);
        if (result.Error != null) return BadRequest(result.Error.Message);
    }

    user.Photos.Remove(photo);

    if (await _userRepository.SaveAllAsync()) return Ok();

    return BadRequest("Problem deleting photo");
}
...

### 140. Deleting photos - Client

33. 
client\src\app\_services\members.service.ts
...

deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
}
...


34. 
client\src\app\members\photo-editor\photo-editor.component.ts

...
deletePhoto(photoId: number) {
    this.memberService.deletePhoto(photoId).subscribe({
      next: _ => {
        if (this.member) {
          this.member.photos = this.member.photos.filter(x => x.id !== photoId);
        }
      }
    });
}
...


35. 
client\src\app\members\photo-editor\photo-editor.component.html

**<button**
    **class='btn btn-sm btn-outline-danger ms-2'**
    [disabled]="photo.isMain"
    (click)="deletePhoto(photo.id)"
**>**
    **<i class="fa fa-trash"></i>**
**</button>**


36. 
git add .  
git commit -m 'End of section 10'  
Clico em Sync Changes para sincronizar mudanças com repositório GitHub (equivale ao git push).


### 141. Section 11 summary
