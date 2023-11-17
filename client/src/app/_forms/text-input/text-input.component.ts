import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})
export class TextInputComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() type = 'text';

  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
  }

  writeValue(obj: any): void { }
  registerOnChange(fn: any): void { }
  registerOnTouched(fn: any): void { }

  /*
    get (abaixo)

    Palavra-chave para que, quando tentarmos acessar este controle (get control()), simplesmente o obtenhamos (get).
    O bloco de código interno é o que retorna.
  */
  get control(): FormControl {
    return this.ngControl.control as FormControl;
    /*
      Estamos efetivamente lançando isto em um modo controlado para contornar 
      o erro TypeScript que estava ocorrendo no template (HTML) neste atributo:

        [formControl]="ngControl.control"
      
      dentro do 1° input.
    */
  }
}
