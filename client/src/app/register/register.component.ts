import { Component, EventEmitter, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
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

  model: any = {};

  constructor(private accountService: AccountService) { }

  register() {
    this.accountService.register(this.model).subscribe({
      next: _ => {
        this.cancel();
      },
      error: error => console.log(error),

    });
  }

  cancel() {  // sai do formulário no componente app-register e retorna para componente HOME
    this.cancelRegister.emit(false);
  }
}
