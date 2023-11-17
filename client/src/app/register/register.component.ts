import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  /*
    @Input() indica que informações virão do componente pai, home.
    Para passar essas informações para o filho, componente register,
    lá em home.component.html uso o atributo em app-register:

        client\src\app\home\home.component.html

        <app-register [usersFromHomeComponent]="users"></app-register>
  */

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

  registerForm: FormGroup = new FormGroup({});
  maxDate: Date = new Date();
  validationErrors: string[] | undefined;

  constructor(
    private accountService: AccountService,
    private toastr: ToastrService,
    private formBuilder: FormBuilder,
    private router: Router
  ) { }

  ngOnInit() {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm() {
    this.registerForm = this.formBuilder.group({
      username: ['', Validators.required],
      gender: ['male'],  // checkbox
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: [
        '', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]
      ],
      confirmPassword: ['', [Validators.required, this.macthValues('password')]],
    });
    /*
      Linha abaixo impede que o usuário tente burlar nossa validação de confirmação de senha e senha.
      Isso porque sem ela o usuário podia preencher a senha, preencher confirma senha e voltar para 
      modificar/atualizar o campo senha sem que a validação da correspondência desses 2 campos funcio-
      asse.
    */
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: _ => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    });
  }

  macthValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl) => {
      return control?.value === control?.parent?.get(matchTo)?.value
        ? null
        : { notMatching: true };
    }
  }

  register() {
    /*
    Precisamos atualizar o método register para usar nosso formulário
    reativo, não mais com this.model, mas sim com this.registerForm.value,
    para que possamos obter os valores do nosso formulário.
  */
    var dateOfBirth = this.getDateOnly(this.registerForm.controls['dateOfBirth'].value);
    var values = { ...this.registerForm.value, dateOfBirth };
    
    this.accountService.register(values).subscribe({
      next: _ => {
        this.router.navigateByUrl('/members');
      },
      error: error => {
        this.validationErrors = error;
        /*
          Com base em nosso client\src\app\_interceptors\error.interceptor.ts,
          error será uma série de erros de validação recebidos do servidor.
        */
      },
    });
  }

  cancel() {  // sai do formulário no componente app-register e retorna para componente HOME
    this.cancelRegister.emit(false);
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
