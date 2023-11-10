# SECTION 3: Building a Walking Skeleton Part Two

## Learning Goals
Complete the walking skeleton and have an introductory understanding of:

1. Using the Angular CLI
2. How to create a new Angular app
3. The Angular project files
4. The Angular bootstrap process
5. Using the Angular HTTP Client Service
6. Running an Angular app over HTTPS
7. How to add packages using NPM

1. C:\Users\medve\source\repos\udemy\DatingApp> ng new client

### 24 - Making HTTP Requests in Angular

2. 
client\src\app\app.module.ts

import { HttpClientModule } from "@angular/common/http";

@NgModule({
  ...
  imports: [
    ...
    HttpClientModule
  ],
  ...
})
export class AppModule { }

3. 
client\src\app\app.component.ts

import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  ...
})
export class AppComponent implements OnInit {
  title = 'Dating App';
  users: any;

  constructor(private http: HttpClient) { }

  ngOnInit(): void {
    this.http.get('https://localhost:5001/api/users').subscribe({
      next: response => this.users = response,
      error: error => console.log(error),
      complete: () => console.log('Request has completed!')
    });
  }
}

### 25 - Adding CORS support in the API

4. 
API\Program.cs

...
builder.Services.AddCors();
...

// Configure the HTTP request pipeline.
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200"));

### 26 - Displaying the fetched users in the browser

5. 
client\src\app\app.component.html

<h1>{{ title }}</h1>
<ul>
    <li *ngFor="let user of users">{{ user.id }} - {{ user.userName }}</li>
</ul>

### 27 - Adding bootstrap and font-awesome

6. C:\Users\medve\source\repos\udemy\DatingApp\client> ng add ngx-bootstrap
Using package manager: npm
✔ Found compatible package version: ngx-bootstrap@11.0.2.
✔ Package information loaded.

The package ngx-bootstrap@11.0.2 will be installed and executed.
Would you like to proceed? Yes
...

7. C:\Users\medve\source\repos\udemy\DatingApp\client> npm install font-awesome

8. 
client\angular.json

"styles": [
    ...
    "./node_modules/font-awesome/css/font-awesome.min.css",
    ...
],

### 28 - Adding HTTPS to Angular using mkcert

9. Abra Powershell com permissão de administrador e execute o comando:
  PS C:\Windows\system32> mkcert --version

Caso mkcert não esteja instalado:
  PS C:\Windows\system32> choco install mkcert

Crie um diretório ssl dentro de client:
  PS C:\Users\medve\source\repos\udemy\DatingApp\client> mkdir ssl

Crie um novo certificado local:
  PS C:\Users\medve\source\repos\udemy\DatingApp\client\ssl> mkcert -install

Crie um certificado válido para localhost:
  PS C:\Users\medve\source\repos\udemy\DatingApp\client\ssl> mkcert localhost

client\angular.json
...
"build": {
  ...
},
"serve": {
    "options": {
      "ssl": true,
      "sslCert": "./ssl/localhost.pem",
      "sslKey": "./ssl/localhost-key.pem"
    },
    ...
}

### 29 - LEGACY (will be removed Nov 23) Using HTTPS in angular - MAC
Aula alternativa a anterior.

### 30 - LEGACY (will be removed Nov 23) Using HTTPS in angular - WINDOWS
Aula alternativa a aula 28.

### 31 - Saving into source control

10. Adicionar s em http:
API\Program.cs

...
app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));
...

### 32 - Section 3 summary