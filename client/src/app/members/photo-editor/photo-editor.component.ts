import { Component, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { take } from 'rxjs';
import { Photo } from 'src/app/_models/photo';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member | undefined;
  /*
    PhotoEditorComponent é filho de MemberEditComponent

    @Input() member: Member | undefined; COMPONENTE PAI PASSA DADO "MEMBER" PARA COMPONENTE FILHO
    member é inicialmente undefined, só depois torna-se Member
  */
  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  user: User | undefined;

  constructor(private accountService: AccountService, private memberService: MembersService) {
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

  deletePhoto(photoId: number) {
    this.memberService.deletePhoto(photoId).subscribe({
      next: _ => {
        if (this.member) {
          this.member.photos = this.member.photos.filter(x => x.id !== photoId);
        }
      }
    });
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
        Se não fizermos isso, precisaremos ajustar nossa configuração do CORS, o que não queremos.
      */
    }

    /*
      A seguir, o que queremos fazer depois que o arquivo (foto) for carregado com sucesso?
    */
    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo = JSON.parse(response);
        this.member?.photos.push(photo);

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
    }
  }
}
