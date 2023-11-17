# SECTION 12: Reactive Forms

## Learning Goals

### 142. Introduction

Implement more advanced forms using Reactive Forms in Angular and understand how to:

1. Use Reactive Forms*
2. Use Angular Validation for inputs
3. Implement custom validation
4. Implement reusable form controls
5. Working with Date inputs

* Formulários Reativos são construídos em torno de fluxos de observáveis em que as entradas e os valores dos formulários são fornecidos como fluxos de valores de entrada que podem ser acessados de modo síncrono. E também mais fáceis de testar se incluirmos isso em nosso aplicativo, porque podemos ter certeza de que nossos dados são consistentes e previsíveis quando solicitados, já que a validação é feita no componente. Mias fácil testar o componente que o template (HTML).
**Formulários Reativos são baseados em componentes. Criamos e controlamos o formulário pelo componente e não pelo template.**
São simples e devido à ligação bidirecional, podemos simplesmente adicionar um formulário a um componente sem pensar muito sobre isso.

Sobre Formulários, formulários prontos podem ser muito prolixos. Por isso, iremos ver a implementação de controles de formulários reutilizáveis.
Quando começamos a adicionar validação ao nosso modelo de componentes, qualquer método de formulários que estamos usando, acabamos, acabamos com uma grande quantidade de código padrão/clichê. E o que podemos fazer é criar um controle de formulário reutilizável e isso reduzirá a quantidade de código que estamos digitando em nossos modelos.


### 143. Reactive forms introduction

1. 
client\src\app\app.module.ts

...
import { ..., ReactiveFormsModule } from '@angular/forms';
...

imports: [
    ...
    ReactiveFormsModule,
    ...
  ],

2. 
client\src\app\register\register.component.ts

import { ..., ..., OnInit, ... } from '@angular/core';
...
import { FormControl, FormGroup } from '@angular/forms';

...
registerForm: FormGroup = new FormGroup({});
...

ngOnInit() {
    this.initializeForm();
  }

initializeForm() {
    this.registerForm = new FormGroup({
      username: new FormControl(),
      password: new FormControl(),
      confirmPassword: new FormControl(),
    });
}

register() {
    console.log(this.registerForm?.value);

    // this.accountService.register(this.model).subscribe({
    //   next: _ => {
    //     this.cancel();
    //   },
    //   error: error => {
    //     console.log(error.error.errors);
    //     this.toastr.error(error.error, 'Error');
    //   },
    // });
}
...


3. 
client\src\app\register\register.component.html

**<form**
    [formGroup]="registerForm"
    **(ngSubmit)="register()"**
    **autocomplete='off'**
**>**
...
**<div class="mb-3">**
    **<input**
        **class='form-control'**
        formControlName="username"
        placeholder='Username'
    **>**
**</div>**
**<div class="mb-3">**
    **<input**
        **class='form-control'**
        formControlName="password"
        **type='password'**
        placeholder="Password"
    **>**
**</div>**
<div class="mb-3">
    <input
        class='form-control'
        formControlName="confirmPassword"
        type='password'
        placeholder="Confirm Password"
    >
</div>
...
<p>Form value: {{registerForm.value | json }}</p>
<p>Form status: {{registerForm.status | json }}</p>


### 144. Client side validation

4. 
client\src\app\register\register.component.ts

...
import { ..., ..., Validators } from '@angular/forms';

...
**initializeForm() {**
    **this.registerForm = new FormGroup({**
      **username: new FormControl(**'Me', Validators.required**)**,
      **password: new FormControl(**
        '', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]
      **),**
      **confirmPassword: new FormControl(**'', Validators.required**),**
    **});**
**}**
...


### 145. Adding custom validators

Iremos criar um método para validar o campo Confirm Password. Verificar se ele corresponde ao campo Password.

5. 
client\src\app\register\register.component.ts

...
import { ..., ..., ..., ValidatorFn, ... } from '@angular/forms';
...

**initializeForm() {**
    ...
    /*
      Linha abaixo impede que o usuário tente burlar nossa validação de confirmação de senha e senha.
      Isso porque sem ela o usuário podia preencher a senha, preencher confirma senha e voltar para 
      modificar/atualizar o campo senha sem que a validação da correspondência desses 2 campos funcio-
      asse.
    */
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: _ => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    });
  **}**

macthValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : { notMatching: true }
    }
}


### 146. Validation feedback

Exibiremos mensagens na tela informando ao usuário qual requisito não está sendo preenchido ao entrar com os dados nos campos input do formulário:

6. 
client\src\app\register\register.component.html

...
**<div class="mb-3">**
    **<input**
        [class.is-invalid]="registerForm.get('username')?.errors && registerForm.get('username')?.touched"
        **class='form-control'**
        **formControlName="username"**
        ...
    **>**
    <div class="invalid-feedback">Please enter a username</div>
**</div>**
**<div class="mb-3">**
    **<input**
        [class.is-invalid]="registerForm.get('password')?.errors && registerForm.get('password')?.touched"
        **class='form-control'**
        **formControlName="password"**
        ...
    **>**
    <div
        class="invalid-feedback"
        *ngIf="registerForm.get('password')?.hasError('required')"
    >
        Please enter a password
    </div>
    <div
        class="invalid-feedback"
        *ngIf="registerForm.get('password')?.hasError('minlength')"
    >
        Password must be at least 4 characters
    </div>
    <div
        class="invalid-feedback"
        *ngIf="registerForm.get('password')?.hasError('maxlength')"
    >
        Password must be at most 8 characters
    </div>
**</div>**
**<div class="mb-3">**
    **<input**
        [class.is-invalid]="registerForm.get('confirmPassword')?.errors && registerForm.get('confirmPassword')?.touched"
        **class='form-control'**
        **formControlName="confirmPassword"**
        ...
    **>**
    <div
        class="invalid-feedback"
        *ngIf="registerForm.get('confirmPassword')?.hasError('required')"
    >
        Please enter a confirmation password
    </div>
    <div
        class="invalid-feedback"
        *ngIf="registerForm.get('confirmPassword')?.hasError('notMatching')"
    >
        Confirm password must match password
    </div>
**</div>**
...


### 147. Creating a reusable text input

A abordagem de formulário reativo usada até a lição anterior, 146, está funcionando bem.
Porém, se seguirmos desse modo, nosso template (HTML) ficará muito poluído.
Ainda nem adicionamos os inputs restantes e já está com muito código.
Assim, a partir desta aula veremos uma abordagem melhor.

7. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c _forms/text-input --skip-tests --dry-run
CREATE src/app/_forms/text-input/text-input.component.html (25 bytes)
CREATE src/app/_forms/text-input/text-input.component.ts (217 bytes)
CREATE src/app/_forms/text-input/text-input.component.css (0 bytes)
UPDATE src/app/app.module.ts (2482 bytes)

NOTE: The "--dry-run" option means no changes were made.


8. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c _forms/text-input --skip-tests
CREATE src/app/_forms/text-input/text-input.component.html (25 bytes)
CREATE src/app/_forms/text-input/text-input.component.ts (217 bytes)
CREATE src/app/_forms/text-input/text-input.component.css (0 bytes) 
UPDATE src/app/app.module.ts (2482 bytes)


9. 
client\src\app\_forms\text-input\text-input.component.ts

import { ..., Input, Self } from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

...
**export class TextInputComponent** implements ControlValueAccessor {
  @Input() label = '';
  @Input() type = 'text';

  constructor(@Self() public ngControl: NgControl) { 
    this.ngControl.valueAccessor = this;
  }
  
  writeValue(obj: any) { }
  registerOnChange(fn: any) { }
  registerOnTouched(fn: any) { }

  get control(): FormControl {
    return this.ngControl.control as FormControl;
  }
**}**


10. 
client\src\app\_forms\text-input\text-input.component.html

<div class="mb-3">
    <input
        [class.is-invalid]="control.touched && control.invalid"
        class='form-control'
        [formControl]="control"
        type={{type}}
        placeholder={{label}}
    >
    <!-- 
        As chaves duplas de type e placeholder (acima) têm de ficar coladas ao seu conteúdo interno como estão.
        Qualquer espaço gera erro no código:
        core.mjs:10592 ERROR DOMException: Failed to execute 'setAttribute' on 'Element': '}}' is not a valid attribute name.
    -->
    <div
        class="invalid-feedback"
        *ngIf="control.errors?.['required']"
    >
        Please enter a {{ label }}
    </div>
    <div
        class="invalid-feedback"
        *ngIf="control.errors?.['minlength']"
    >
        {{ label }} must be at least {{ control.errors?.['minlength'].requiredLength }} characters
    </div>
    <div
        class="invalid-feedback"
        *ngIf="control.errors?.['maxlength']"
    >
        {{ label }} must be at most {{ control.errors?.['maxlength'].requiredLength }} characters
    </div>
    <div
        class="invalid-feedback"
        *ngIf="control.errors?.['notMatching']"
    >
        Passwords do not match
    </div>
</div>

11. 
client\src\app\register\register.component.html

**<form**
    ...
**>**
    ...
    <app-text-input
        [formControl]="$any(registerForm.controls['username'])"
        [label]="'Username'"
    >
    </app-text-input>
    <app-text-input
        [formControl]="$any(registerForm.controls['password'])"
        [label]="'Password'"
        [type]="'password'"
    >
    </app-text-input>
    <app-text-input
        [formControl]="$any(registerForm.controls['confirmPassword'])"
        [label]="'Confirm Password'"
        [type]="'password'"
    >
    </app-text-input>

    ...
**</form>**

...


### 148. Using the form builder service

Veremos um serviço fornecido por formulários reativos que simplifica formulários: builder service.

12. 
client\src\app\register\register.component.ts

...
import { ..., FormBuilder, ..., ..., ... } from '@angular/forms';

constructor(
    ..., ..., private formBuilder: FormBuilder
  ) { }

...
**initializeForm() {**
    **this.registerForm =** this.formBuilder.group({
      **username:** ['', Validators.required],
      **password:** [
        '', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]
      ],
      **confirmPassword:** ['', [Validators.required, this.macthValues('password')]],
    });
    ...
}
...


### 149. Expanding the register form

13. 
client\src\app\register\register.component.ts

...

initializeForm() {
    this.registerForm = this.formBuilder.group({
      ...,
      gender: ['male'],  // checkbox
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      ...,
      ...
    });
    ...
}
...


14. 
client\src\app\register\register.component.html

...
**<hr>**
<div class="mb-3">
    <label style='margin-right: 10px'>I am a: </label>
    <label class="form-check-label">
        <input
            type="radio"
            class="form-check-input"
            value='male'
            formControlName='gender'
        >
        Male
    </label>
    <label class="form-check-label">
        <input
            type="radio"
            class="form-check-input ms-3"
            value='female'
            formControlName='gender'
        >
        Female
    </label>
</div>

**<app-text-input**
        **[formControl]="$any(registerForm.controls['username'])"**
        **[label]="'Username'"**
    **>**
    **</app-text-input>**
    <app-text-input
        [formControl]="$any(registerForm.controls['knownAs'])"
        [label]="'Known As'"
    >
    </app-text-input>
    <app-text-input
        [formControl]="$any(registerForm.controls['dateOfBirth'])"
        [label]="'Date Of Birth'"
    >
    </app-text-input>
    <app-text-input
        [formControl]="$any(registerForm.controls['city'])"
        [label]="'City'"
    >
    </app-text-input>
    <app-text-input
        [formControl]="$any(registerForm.controls['country'])"
        [label]="'Country'"
    >
    </app-text-input>
    **<app-text-input**
        **[formControl]="$any(registerForm.controls['password'])"**
        **...**
    **>**
...


### 150. Adding a reusable date input

No input Date Of Birth, apenas a inserção do atributo [type]="'date'" já seria suficiente para a finalidade deste campo.
Porém, como cada navegador possui um estilo específico deste tipo de campo, se é que o possui, nosso app não ficaria consistente.
Daí a ideia de criar um modelo único para esse input, modelo que possua o mesmo estilo independente do navegador.
Para tal usaremos:

**https://valor-software.com/ngx-bootstrap/#/components/datepicker?tab=overview**
**https://valor-software.com/ngx-bootstrap/#/components/datepicker?tab=api**


15. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c _forms/date-picker --skip-tests --dry-run
CREATE src/app/_forms/date-picker/date-picker.component.html (26 bytes)
CREATE src/app/_forms/date-picker/date-picker.component.ts (221 bytes)
CREATE src/app/_forms/date-picker/date-picker.component.css (0 bytes)
UPDATE src/app/app.module.ts (2589 bytes)

NOTE: The "--dry-run" option means no changes were made.


16. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c _forms/date-picker --skip-tests
CREATE src/app/_forms/date-picker/date-picker.component.html (26 bytes)
CREATE src/app/_forms/date-picker/date-picker.component.ts (221 bytes)
CREATE src/app/_forms/date-picker/date-picker.component.css (0 bytes) 
UPDATE src/app/app.module.ts (2589 bytes)


17. 
client\src\app\_modules\shared.module.ts

...
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';

...
 **imports: [**
    ...
    BsDatepickerModule.forRoot(),
  **],**
  **exports: [**
    **...**
    BsDatepickerModule,
  **]**
...


18. 
client\src\app\_forms\date-picker\date-picker.component.ts

***Ao usar Partial<...> diante de um tipo, tornamos cada propriedade dentro desse tipo opcional. Assim, mesmo que tenhamos 100 propriedades que são todas necessárias dentro desse tipo particular de algo, usamos a palavra "Partial" e colocamos a sequência dentro de colchetes angulares:***

***Partial<BsDatepickerConfig>***

import { ..., Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

...
**export class DatePickerComponent** implements ControlValueAccessor {
  @Input() label = '';
  @Input() maxDate: Date | undefined;
  bsConfig: Partial<BsDatepickerConfig> | undefined;

  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
    this.bsConfig = {
      containerClass: 'theme-red',
      dateInputFormat: 'DD MMMM YYYY',
      
    };
  }

  writeValue(obj: any) { }
  registerOnChange(fn: any) { }
  registerOnTouched(fn: any) { }

  get control(): FormControl {
    return this.ngControl.control as FormControl;
  }
**}**


19. 
client\src\app\_forms\date-picker\date-picker.component.html

<div class="mb-3">
    <input
        class="form-control"
        [formControl]="control"
        [class.is-invalid]="control.touched && control.invalid"
        placeholder="{{label}}"
        bsDatepicker
        [bsConfig]="bsConfig"
        [maxDate]="maxDate"
    >
    <!-- 
        bsDatepicker é uma diretiva disponibilizada pelo módulo BsDatepickerModule em shared.module.ts
        https://valor-software.com/ngx-bootstrap/#/components/datepicker?tab=overview
    -->
    <div
        class="invalid-feedback"
        *ngIf="control.errors?.['required']"
    >
        {{ label }} is required
    </div>
</div>


20. 
client\src\app\register\register.component.html

***Estava assim:***
<app-text-input
    [formControl]="$any(registerForm.controls['dateOfBirth'])"
    [label]="'Date Of Birth'"
>
</app-text-input>


***Ficou assim:***
...
<app-date-picker
    [formControl]="$any(registerForm.controls['dateOfBirth'])"
    [label]="'Date Of Birth'"
>
</app-date-picker>
...


21. 
client\src\app\register\register.component.ts

...
**registerForm: FormGroup = new FormGroup({});**
maxDate: Date = new Date();
...

**ngOnInit() {**
    ...
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
**}**
...


22. 
client\src\app\register\register.component.html

...
**<app-date-picker**
    ...
    [maxDate]="maxDate"
**>**
**</app-date-picker>**
...


### 151. Updating the API register method

23. 
API\DTOs\RegisterDto.cs

...

public class RegisterDto
{
    ...

    [Required] public string KnownAs { get; set; }

    [Required] public string Gender { get; set; }

    [Required] public DateOnly? DateOfBirth { get; set; }  // optional to make required work!

    [Required] public string City { get; set; }

    [Required] public string Country { get; set; }

    ...
}


24. 
Fazer mapeamento entre RegisterDto e AppUser:

API\Helpers\AutoMapperProfiles.cs

...

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        ...
        **CreateMap<MemberUpdateDto, AppUser>();**
        ...
        CreateMap<RegisterDto, AppUser>();
    **}**
}


25. 
API\DTOs\UserDto.cs

...
public string KnownAs { get; set; }
...


26. 
API\Controllers\AccountController.cs

...
using AutoMapper;
...

**public class AccountController : BaseApiController**
{
    ...
    private readonly IMapper _mapper;

    public AccountController(..., ..., IMapper mapper)
    {
        ...
        _mapper = mapper;
    }

   **[HttpPost("register")]  // api/account/register**
   **public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)**
   **{**
        ...

        var user = _mapper.Map<AppUser>(registerDto);
        
        ...

        user.**UserName = registerDto.Username.ToLower();**
        user.**PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));**
        user.**PasswordSalt = hmac.Key;**

        ... 
        
        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user),
            KnownAs = user.KnownAs,
        };
    }
    
   **[HttpPost("login")]**
   **public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)**
    {
        ...

        **return new UserDto**
        {
            ...
            KnownAs = user.KnownAs,
        **};**
   **}**

    ...
**}**


27. 
Testes no Postman, Section 12 de DatingApp.


### 152. Client side registration

Uma vez que o usuário houver se cadastrado, vamos considerá-lo logado.
Então o redirecionaremos para a página de membro após registrar-se.

28. 
client\src\app\register\register.component.ts

...
import { Router } from '@angular/router';

...
**export class RegisterComponent implements OnInit {**
  ...
  **model: any = {};** esta propriedade model deve ser APAGADA
  ...
  validationErrors: string[] | undefined;

  **constructor(**
    ...
    private router: Router
  **) { }**

  ...

  **register() {**
  /*
    Precisamos atualizar o método register para usar nosso formulário
    reativo, não mais com this.model, mas sim com **this.registerForm.value**,
    para que possamos obter os valores do nosso formulário.
  */
    this.accountService.register(this.registerForm.value).subscribe({
      **next: _ => {**
        this.router.navigateByUrl('/members');
      **},**
      **error: error => {**
        this.validationErrors = error;
        /*
          Com base em nosso client\src\app\_interceptors\error.interceptor.ts,
          error será uma série de erros de validação recebidos do servidor.
        */
      **},**
    **});**
  **}**

  ...
}


29. 
client\src\app\register\register.component.html

**<app-text-input**
    ...
    **[type]="'password'"**
**>**
**</app-text-input>**

<div
    class="row"
    *ngIf="validationErrors"
>
    <ul class="text-danger">
        <li *ngFor="let error of validationErrors">{{ error }}</li>
    </ul>
</div>
<!-- 
    div acima não costuma ser usada porquanto estamos lidando com erros do lado do cliente
    -->
...


30. 
client\src\app\register\register.component.ts

...

export class RegisterComponent implements OnInit {
  ...

  ngOnInit() {
    ...
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  register() {
    /*
    Precisamos atualizar o método register para usar nosso formulário
    reativo, não mais com this.model, mas sim com this.registerForm.value,
    para que possamos obter os valores do nosso formulário.
  */
    var dateOfBirth = this.getDateOnly(this.registerForm.controls['dateOfBirth'].value);
    var values = { ...this.registerForm.value, dateOfBirth };
    
    **this.accountService.register(**values**).subscribe({**
      ...
    });
  }

  private getDateOnly(dateOfBirthday: string | undefined) {
    /*
      De: 
           Fri Sep 29 1995 10:17:33 GMT-0200 (Hora de verão de Brasília)

      Para:
           1995-09-29
    */
    if (!dateOfBirthday) return;
    let dob = new Date(dateOfBirthday);

    return new Date(
      dob.setMinutes(dob.getMinutes() - dob.getTimezoneOffset())
    ).toISOString().slice(0, 10);
  }
}


31. 
client\src\app\register\register.component.html

```csharp
<form
    ...
    **(ngSubmit)="**registerForm.valid && **register()"**
    ...
>
...
    <div ...>
        <button
            ...
            [disabled]="!registerForm.valid"
        >Register</button>
        <button
            ...
        >Cancel</button>
    </div>
</form>

<p>Form value: {{registerForm.value | json }}</p>
<p>Form status: {{registerForm.status | json }}</p>
```
**OBS 2 últimas linhas acima foram REMOVIDAS**


32. 
client\src\app\members\photo-editor\photo-editor.component.ts

...
```csharp
**this.uploader.onSuccessItem = (item, response, status, headers) => {**
    ...

    if (photo.isMain && this.user && this.member) {
        this.user.photoUrl = photo.url;
        /*
        Ao carregar a primeira foto, ela será automaticamente definida como a foto principal.
        Qualquer outra foto não entrará neste if: por isso photo.isMain && ...
        */
        this.member.photoUrl = photo.url;
        this.accountService.setCurrentUser(this.user);
    }
}
...
```


33. 
git add .  
git commit -m 'End of section 10'  
Clico em Sync Changes para sincronizar mudanças com repositório GitHub (equivale ao git push).

### 153. Section 12 summary
