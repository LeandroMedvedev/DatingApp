# THE REPOSITORY PATTERN

A repository mediates between the domain and data mapping layers, acting like an in-memory domain object collection.
Martin Fowler - Patterns of Enterprise Architecture

A estrutura que temos no momento nesta aplicação é:

**Web Server -> Controller -> DbContext -> DB**
**Web Server <- Controller <- DbContext <- DB**

A estrutura que teremos será:

**Web Server -> Controller -> Repository -> DbContext -> DB**
**Web Server <- Controller <- Repository <- DbContext <- DB**

Haverá adição de uma camada de abstração. 
Alguns acham isso desnecessário porque o próprio contexto do DB não atua como um repositório como tal, mas como outro padrão do qual falaremos mais tarde.
A unidade de trabalho e os conjuntos de DB que criamos dentro do contexto do DB são como repositórios e esse é um argumento justo. Mas o que veremos é porque fazer esta camada de abstração adicional do que já são consideradas as unidades de trabalho e o padrão de repositório em ação dentro do contexto do DB.
Resumindo, há razões para usar o padrão de repositório, mas não é essencial.

**Motivos para usar:**

**I. Encapsulates the logic**

***DbContext***
I support the following methods:
User.First()
Users.FirstOrDefault()
Users.SingleOrDefault()
Users.Include(x => x.Thing).FirstOrDefault()
+ another 10000 methods

***Repository***
I support the following 4 methods:
GetUser()
GetUsers()
UpdateUser()
SaveAll()


**II. Reduces duplicate query logic**
***DbContext***
***UsersController***                   ***MessageController***                 ***LikesController***
var user =                              var user =                              var user =
context.Users.Include(x =>              context.Users.Include(x =>              context.Users.Include(x =>
x.Thing).FirstOrDefault(x =>            x.Thing).FirstOrDefault(x =>            x.Thing).FirstOrDefault(x => 
x.UserName == username);                x.UserName == username);                x.UserName == username);

***Repository***
public User GetUser(string username) {
    return context.Users.Include(x =>
    x.Thing).FirstOrDefault(x =>
    x.User.UserName == username);
}

**III. Promotes testability**
                         RepositoryClass
                                |
WebServer -> Controller -> IRepository -> DbContext-> DB
UnitTest -> Controller -> MockRepository

É mais fácil testar uma interface de repositório do que um contexto de DB.
O repositório terá interfaces e teremos uma classe de implmentação para o repositório.
O que injetamos em nossos controladores será a interface do repositório e então teremos uma classe de implementação que contém toda a lógica real.
Se fôssemos introduzir teste, não precisaríamos implementar métodos dentro do repositório, dentro dos testes unitários. Poderíamos simular o repositório, usar uma estrutura de simulação:

var user = new User{ UserName = "bob" };
var mockRepo = MockRepository.GenerateMock<IRepository>();

mockRepo.Expect(x => x.GetUser("bob")).Return(user);

Simular o repositório permite escrever menos código.
Se estivéssemos usando/codificando diretamente no contexto de DB em nossos controladores, isso seria mais difícil.

**VANTAGENS DO REPOSITORY PATTERN:**

***I. Minimiza a lógica de consulta duplicada***
***II. Desacopla o aplicativo da estrutura de persistência***
***III. Centraliza todas as consultas ao DB, não as espalha pelo aplicativo***
***IV. Permite-nos alterar facilmente o mapeamento relacional de objetos (ORM)***
***V. Promove a testabilidade***
      Podemos facilmente simular uma interface de repositório, testar no DbContext é mais difícil.

**DESVANTAGENS DO REPOSITORY PATTERN:**

***I. Abstração de uma abstração***
      Entity Framework é uma abstração de um DB. 
      Um repositório é uma abstração de um Entity Framework.
***II. Cada entidade raiz deve ter seu próprio repositório, o que significa mais código***
***III. Também precisamos implementar o padrão UnitOfWork para controlar transações***
        Isso porque serão injetadas diferentes instâncias do contexto de dados em diferentes repositórios.
        Mais tarde precisaremos introduzir um padrão para controlar as transações que estão ocorrendo.
      




*******************************************************************************************************************
# AUTOMAPPER

AutoMapper irá fazer o mapeamento automático em cada propriedade de 2 classes, origem e destino ou destino e origem:

    API\Entities\AppUser.cs     x    API\DTOs\MemberDto.cs
    API\Entities\Photo.cs       x    API\DTOs\PhotoDto.cs

Irá mapear que ambas possuem Id, UserName... Ou seja, todas as propriedades entre as classes acima em que o tipo e o nome correspondem serão mapeadas automaticamente por AutoMapper. Irá mapear que AppUser possui o método GetAge() e MemberDto possui a propriedade Age. Ele é inteligente para saber usar o método para calcular a idade, mas para isso o nome precisa ser este: GetAge. Caso contrário, o mapeamento para esta propriedade falhará. 