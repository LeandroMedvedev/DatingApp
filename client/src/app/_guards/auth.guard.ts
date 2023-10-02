import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { map } from 'rxjs';

export const authGuard: CanActivateFn = (route, state) => {
  /*
    Como authGuard é const e não class, não
    há constructor para injetar dependência. 
    Mas Angular provê função inject para isso.
  */
  const accountService = inject(AccountService);
  const toastr = inject(ToastrService);

  return accountService.currentUser$.pipe(
    map(user => {
      if (user) return true;
      toastr.error('you shall not pass!', 'Error');
      return false;
    })
  );
};
