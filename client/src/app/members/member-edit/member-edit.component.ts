import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { User } from 'src/app/_models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm: NgForm | undefined;
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
  member: Member | undefined;
  user: User | null = null;

  constructor(
    private accountService: AccountService, private memberService: MembersService, private toastr: ToastrService
  ) {
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
}
