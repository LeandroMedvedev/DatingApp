import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { delay, finalize, Observable } from 'rxjs';
import { BusyService } from '../_services/busy.service';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {

  constructor(private busyService: BusyService) { }

  /*
    Se estivermos chamando o método intercept() significa que uma solicitação HTTP está em andamento.
    Portanto, antes da instrução de retorno, invocamos this.busyService.busy(). Este irá incrementar
    a contagem de solicaitações ocupadas (busy) e em seguida retornar.
  */
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.busyService.busy();

    return next.handle(request).pipe(
      delay(1000),
      finalize(() => {
        this.busyService.idle();
      }),
      /* uso de "delay" para simular um atraso numa requisição HTTP, algo que 
         não ocorre agora por estarmos executando a solicitação em localhost.

         uso de "finalize" para mostrar o que fazer após solicitação ser concluída:
         invocar this.busyService.idle() para desativar spinner.
      */
    );
  }
}
