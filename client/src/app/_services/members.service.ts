import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { map, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];

  constructor(private http: HttpClient) { }

  /*
    Não precisamos passar token nos métodos abaixo porquanto este está sendo provido em nível superior pelo JwtInterceptor
  */
  getMembers() {
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
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.members = members;
        return members;
      })
    );
  }

  getMember(username: string) {
    const member = this.members.find(x => x.userName == username);
    if (member) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put<Member>(this.baseUrl + 'users', member).pipe(
      map(_ => {
        const index = this.members.indexOf(member);
        this.members[index] = { ...this.members[index], ...member };
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }
}
