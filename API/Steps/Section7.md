# SECTION 7: Error Handling

A ideia desta sessão será lidar com erros num nível mais alto, justamente para evitar usar try catch em cada método nos controllers. Evitar poluir o código desse modo. Assim, criaremos um middleware personalizado para lidar com os erros.

## Learning Goals

Implement global error handling in both the API and the Angular Application. Also to have an understanding of:

1. API Middleware
2. Angular Interceptors
3. Troubleshooting exceptions

### 75. Introduction

Implement global error handling in both the API and the Angular application. Also to have an understanding of:

1. API Middleware
2. Angular Interceptors
3. Troubleshooting exceptions

Angular Interceptors -> interceptam tanto requisição que sai do aplicativo Angular quanto as respostas que voltam do servidor API.

### 76. Creating an error controller for testing errors

BuggyController -> classe controller criada para devolver respostas de erro HTTP ao cliente.

1. 
API\Controllers\BuggyController.cs

using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BuggyController : BaseApiController
{
    private readonly DataContext _context;
    public BuggyController(DataContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpGet("auth")]
    public ActionResult<string> GetSecret()
    {
        return "secret text";
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        var thing = _context.Users.Find(-1);

        if (thing == null) return NotFound();

        return thing;
    }

    [HttpGet("server-error")]
    public ActionResult<string> GetServerError()
    {
        /*
            1. thing retornará null
            2. null não pode ser convertido para string; uma exceção será lançada
        */
        var thing = _context.Users.Find(-1);

        var thingToReturn = thing.ToString();

        return thingToReturn;
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("This was not a good request");
    }
}


### 77. Handling server errors

Em .NET 5 (ou até .NET 5, não sei ao certo), veríamos o seguinte trecho de código em Program:

API\Program.cs

...
var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
...


### 78. Exception handling middleware

2. 
*A classe ApiException conterá a resposta a ser enviada de volta ao cliente em caso de exceção.*

API\Errors\ApiException.cs

namespace API.Errors;

public class ApiException
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }

    public ApiException(int statusCode, string message, string details)
    {
        StatusCode = statusCode;
        Message = message;
        Details = details;
    }
}

3. 
API\Middleware\ExceptionMiddleware.cs

using API.Errors;
using System.Net;
using System.Text.Json;

namespace API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env
    )
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
            /*
                Se houver uma exceção em qualquer lugar da aplicação e níveis 
                inferiores não a capturarem, este middleware irá tratá-la.
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = _env.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                : new ApiException(context.Response.StatusCode, ex.Message, "Internal Server Error");

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}


4. 
***Para conseguir usar o middleware acima, preciso acrescentar a seguinte linha:***

API\Program.cs

...
app.UseMiddleware<ExceptionMiddleware>();
...

***Ela precisará ser posta exatamente como no código abaixo (logo acima das solicitações de requisição HTTP)***

...
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithOrigins("https://localhost:4200")
);
...


### 79. Testing errors in the client

5. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c errors/test-error --skip-tests --dry-run
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c errors/test-error --skip-tests

6. 
client\src\app\errors\test-error\test-error.component.ts

import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-test-error',
  templateUrl: './test-error.component.html',
  styleUrls: ['./test-error.component.css']
})
export class TestErrorComponent {
  baseUrl = 'https://localhost:5001/api/';
  validationErrors: string[] = [];

  constructor(private http: HttpClient) { }

  get404Error() {
    this.http.get(this.baseUrl + 'buggy/not-found').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get400Error() {
    this.http.get(this.baseUrl + 'buggy/bad-request').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get500Error() {
    this.http.get(this.baseUrl + 'buggy/server-error').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get401Error() {
    this.http.get(this.baseUrl + 'buggy/auth').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    });
  }

  get400ValidationError() {
    this.http.post(this.baseUrl + 'account/register', {}).subscribe({
      next: response => console.log(response),
      error: error => {
        console.log(error);
        this.validationErrors = error;
      }
    });
  }
}


7. 
client\src\app\errors\test-error\test-error.component.html

<ng-container>
    <button
        class="btn btn-outline-danger me-3"
        (click)="get500Error()"
    >Test 500 Error</button>
    
    <button
        class="btn btn-outline-danger me-3"
        (click)="get404Error()"
    >Test 404 Error</button>

    <button
        class="btn btn-outline-danger me-3"
        (click)="get401Error()"
    >Test 401 Error</button>

    <button
        class="btn btn-outline-danger me-3"
        (click)="get400Error()"
    >Test 400 Error</button>

    <button
        class="btn btn-outline-danger me-3"
        (click)="get400ValidationError()"
    >Test 400 Validation Error</button>
</ng-container>


8. 
client\src\app\app-routing.module.ts

...
const routes: Routes = [
  ...
  { path: 'errors', component: TestErrorComponent },
  ...
];
...

9. 
client\src\app\nav\nav.component.html

...
<li class="nav-item">
    <a
        class="nav-link"
        routerLink="/errors"
        routerLinkActive='active'
    >Errors</a>
</li>
...


### 80. Adding an error interceptor

Angular fornece um recurso sofisticado para lidar com erros. Este permite interceptar requisições quando saem ou retornam para a API.

10. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g interceptor _interceptors/error --skip-tests --dry-run
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g interceptor _interceptors/error --skip-tests

11. 
client\src\app\_interceptors\error.interceptor.ts

...
constructor(private router: Router, private toastr: ToastrService) { }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error) {
          switch (error.status) {
            case 400:
              if (error.error.errors) {
                const modelStateErrors = [];
                for (const key in error.error.errors) {
                  if (error.error.errors[key]) {
                    modelStateErrors.push(error.error.errors[key]);
                  }
                }
                throw modelStateErrors.flat();
              } else {
                this.toastr.error(error.error, error.status.toString());
              }
              break;
            case 401:
              this.toastr.error('Unauthorized', error.status.toString());
              break;
            case 404:
              this.router.navigateByUrl('/not-found');
              break;
            case 500:
              const navigationExtras: NavigationExtras = { state: { error: error.error } };
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              this.toastr.error('Something unexpected went wrong');
              console.log(error);
              break;
          }
        }
        throw error;
      })
    );
  }
...

12. 
client\src\app\app.module.ts

...
import { HTTP_INTERCEPTORS, ... } from "@angular/common/http";
import { ErrorInterceptor } from './_interceptors/error.interceptor';
...

@NgModule({
    ...
    providers: [
      { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true }
    ],
    ...
})

### 81. Validation errors

13. 
client\src\app\errors\test-error\test-error.component.html

...
<div
class="row mt-5"
*ngIf="validationErrors.length > 0"
>
<ul class="text-danger">
    <li *ngFor="let error of validationErrors">
        {{ error }}
    </li>
</ul>
</div>
</ng-container>


### 82. Handling not found

14. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c errors/not-found --skip-tests

15. 
PS C:\Users\medve\source\repos\udemy\DatingApp\client> ng g c errors/server-error --skip-tests

16. 
client\src\app\errors\not-found\not-found.component.html

<div class="container">
    <h1>Not found</h1>
    <button
        class="btn btn-info btn-lg"
        routerLink="/"
    >Return to home page</button>
</div>

17. 
client\src\app\app-routing.module.ts

const routes: Routes = [
  ...
  {
    ...
  },
  ...
  { path: 'not-found', component: NotFoundComponent },
  { path: 'server-error', component: ServerErrorComponent },
  { path: '**', component: NotFoundComponent, pathMatch: 'full' },
];


18. 
client\src\app\nav\nav.component.ts

this.accountService.login(this.model).subscribe({
    next: _ => this.router.navigateByUrl('/members'),
    ***error: error => this.toastr.error(error.error, 'Error')*** **LINHA REMOVIDA**
});


### 83. Adding a server error page

19. 
client\src\app\errors\server-error\server-error.component.ts

...
error: any;

constructor(private router: Router) { 
  const navigation = this.router.getCurrentNavigation();
  this.error = navigation?.extras?.state?.['error'];
}
...


20. 
client\src\app\errors\server-error\server-error.component.html

<h4>Internal Server Error - refreshing the page will make the error disappear</h4>
<!-- Refrescar/atualizar a página em versões Angular a partir da 15 ainda preservará o conteúdo da página -->
<ng-container *ngIf="error">
    <h5 class="text-danger">Error: {{ error.message }}</h5>
    <p class="font-weight-bold">Note: if you are seeing this then Angular is propbably not to blame</p>
    <p>What to do next?</p>
    <ol>
        <li>Open Chrome Dev Tools! Then check the failing request in the network tab.</li>
        <li>Examine the request URL - what API endpoint are you requesting?</li>
        <li>Reproduce the problem in Postman - if we get same error Angular is 100% not the problem.</li>
        <!-- Se conseguir reproduzir o problema no Postman, significa que o problema está no backend (.NET), não em Angular -->
        <p style='font-weight: bold'>Following is the stack trace - the first few lines will tell you which line of code
            caused the issue in the API.</p>
        <code
            class='mt-5'
            style='background-color: whitesmoke'
        >
            {{ error.details }}</code>
    </ol>
</ng-container>

21. 
git add .
git commit -m 'End of section 7'
Clico em Sync Changes para sincronizar mudanças com repositório GitHub (equivale ao git push).


### 84. Section 7 summary
