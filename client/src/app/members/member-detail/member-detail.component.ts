import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
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
*/
export class MemberDetailComponent implements OnInit {
  member: Member | undefined;
  images: GalleryItem[] = [];

  constructor(private memberService: MembersService, private route: ActivatedRoute) { }

  ngOnInit() {
    this.loadMember();
  }

  loadMember() {
    const username = this.route.snapshot.paramMap.get('username');
    if (!username) return;

    this.memberService.getMember(username).subscribe({
      next: member => {
        this.member = member;
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

/*
  CICLO DE VIDA DOS COMPONENTES

  1° a classe do componente é construída   (constructor)
  2° a classe do componente é inicializada (ngOnInit)
  Neste ponto, loadMember buscará dados da API, mas o layout (elementos) também será construído.
  No momento em que for construído, teremos o membro.
*/