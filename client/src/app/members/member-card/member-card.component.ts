import { Component, Input } from '@angular/core';
import { Member } from 'src/app/_models/member';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css'],
  // encapsulation: ViewEncapsulation.None 
  /*
    encapsulation: ViewEncapsulation.None 
    Caso quisesse que o CSS definido neste componente fosse aplicado globalmente.
    O padrão é ViewEncapsultation.Emulated.
  */
})
export class MemberCardComponent {
  @Input() member: Member | undefined;
  /*
    MemberCardComponent é filho de MemberListComponent

    @Input() member: Member | undefined; COMPONENTE PAI PASSA DADO "MEMBER" PARA COMPONENTE FILHO
    member é inicialmente undefined, só depois torna-se Member

    client\src\app\members\member-list\member-list.component.html
    ...
      <app-member-card [member]="member"></app-member-card>
    ...

    client\tsconfig.json

    "strictPropertyInitialization": false,
    Check for class properties that are declared but not set in the constructor.

    Evita o erro:
    Property 'member' has no initializer and is not definitely assigned in the constructor.ts(2564)
  */
}
