# SECTION 8: Extending the API

## Learning Goals

### 85. Introduction

Implement further functionality into our API and gain an understanding of:

1. Entity Framework Relationships
2. Entity Framework Conventions
3. Seeding Data into the Database
4. The repository pattern
5. Using AutoMapper



### 86. Extending the user entity

1. 
API\Entities\AppUser.cs

...
public DateOnly DateOfBirth { get; set; }
public string KnownAs { get; set; }
public DateTime Created { get; set; } = DateTime.UtcNow;
public DateTime LastActive { get; set; } = DateTime.UtcNow;
public string Gender { get; set; }
public string Introduction { get; set; }
public string LookingFor { get; set; }
public string Interests { get; set; }
public string City { get; set; }
public string Country { get; set; }
public List<Photo> Photos { get; set; } = new();
...

2. 
API\Entities\Photo.cs

using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

[Table("Photos")]
/*
    Table permite que minha classe seja mapeada no DB no
    plural, por exemplo, conquanto esteja no singular.
*/
public class Photo
{
    public int Id { get; set; }
    public string Url { get; set; }
    public bool IsMain { get; set; }
    public string PublicId { get; set; }
}


### 87. Adding a DateTime extension to calculate age

3. 
API\Entities\AppUser.cs

...
public int GetAge()
{
    return DateOfBirth.CalculateAge();
}
...

4. 
API\Extensions\DateTimeExtensions.cs

namespace API.Extensions;

/*
    métodos de extensão sempre são ESTÁTICOS
*/
static public class DateTimeExtensions
{
    /*
        Por ser um método de extensão, preciso especificar o que estou estendendo -> this DateOnly dateOfBirth.
    */
    static public int CalculateAge(this DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddDays(-age)) age--;  // caso ainda não haja feito aniversário este ano
        /*
            Este não é um cálculo de idade superpreciso porquanto não contabilizamos anos bissextos
        */

        return age;
    }
}


### 88. Entity Framework relationships

Uma vez criada uma nova tabela/entidade, preciso decidir se esta será adicionada como autopropriedade da classe DataContext, como um public DbSet<NovaEntidade> NomePropriedade { get; set; }.
No momento, por exemplo, possuía somente a entidade AppUser. Daí criamos há pouco Photo, adiciono ou não a DataContext?
Até poder-se-ia criar criar public DbSet<Photo> Photos { get; set; }, mas preciso pensar:

**I.** Quando um usuário adicionar uma foto, ela será adicionada a ele especificamente, não a outros.
**II.** Em nenhum momento quero que uma foto não esteja associada a um usuário.
**III.** Não será necessário consultar as fotos diretamente. Ou seja, consultar o DB em busca de uma fo-
    to para um usuário aleatório.

Logo, não preciso criar um conjunto DB para as fotos porquanto não irei usar esta entidade para 
consultar o DB diretamente em busca dela.
Como Photo está vinculada à entidade AppUser, qualquer consulta a Photo será feita por AppUser.

5. 
API\Entities\Photo.cs

...
/*
    Estas 2 propriedades (abaixo) impedem que possa criar uma foto que não esteja vinculada a um usuário.
    Com elas agora, há obrigatoriedade de vinculação.
    Se olhar na migração (quando criá-la) verá esta linha (80):

        AppUserId = table.Column<int>(type: "INTEGER", nullable: false)
    
    Repare em nullable = false. Não poderá haver uma foto vinculada a um usuário nulo.
    Já na linha 90 há:

        onDelete: ReferentialAction.Cascade);
    
    Efeito cascata fará com que, caso um usuário seja removido, todas as suas entidades relacionadas tb o serão.
*/
public int AppUserId { get; set; }
public AppUser AppUser { get; set; }
...


6. 
PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef migrations add ExtendedUserEntity

7. 
PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef database update

8. 
Ctrl Shift P
SQLite: Open Database

### 89. Generating seed data

9. 
https://json-generator.com/
Ferramenta para gerar dados aleatórios.

Neil disponibilizou, na pasta StudentAssetsAug2023, uma arquivo jsongenerator.txt.
Copiamos o conteúdo dele e colamos na coluna à esquerda do json-generator.com.
Geramos 5 mulheres e 5 homens.
Criamos este arquivo e inserimos o veror com 10 JSONs gerados:

API\Data\UserSeedData.json

[
    {
        ...
    }
]

Ou pode-se copiar e colar o conteúdo de:

StudentAssetsAug2023\StudentAssets\UserSeedData.json


### 90. Seeding data part one

10. 
API\Data\Seed.cs

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
    public static async Task SeedUsers(DataContext context)
    {
        if (await context.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        var options = new JsonSerializerOptions{PropertyNameCaseInsensitive = true};
        /*
            O arquivo Data/UserSeedData.json possui JSON com nomes de chave em PascalCase.
            Ainda que algumas delas esteja em camelCase, a linha acima permitirá que não 
            haja erro.
        */

        var users = JsonSerializer.Deserialize<List<AppUser>>(userData, options);

        // gerar a mesma senha (facilitar) para cada usuário na lista
        foreach(var user in users)
        {
            using var hmac = new HMACSHA512();

            user.UserName = user.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Pa$$w0rd"));
            user.PasswordSalt = hmac.Key;

            context.Users.Add(user);
        }

        await context.SaveChangesAsync();
    }
}


11. 
Preciso rodar comando de .NET cli para rodar migrações usando o código. Isso é feito em Program.cs, o 1° a ser lido:

API\Program.cs

...
**app.MapControllers();**
using var scope = app.Services.CreateScope();  // dá acesso a todos os serviços dentro da classe Program
var services = scope.ServiceProvider;
/*
    O middleware de tratamento de exceções que criamos, ExceptionMiddleware, trata exceções de requisições HTTP.
    Como não é o caso aqui, precisamos usar try/catch.
*/
try
{
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    /*
        MigrateAsync
        Aplica de forma assíncrona quaisquer migrações pendentes do contexto ao banco de dados. Criará o banco de
        dados se ele ainda não existir. Ou seja, se eu reiniciar a aplicação e a execução do programa perceber que o db não existe, ele será criado, tabelas, colunas, dados.
    */
    await Seed.SeedUsers(context);
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}
**app.Run();**


### 91. Seeding data part two

12. 
PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet ef database drop
Build started...
Build succeeded.
Are you sure you want to drop the database 'main' on server 'datingapp.db'? (y/N)
y
Dropping database 'main' on server 'datingapp.db'.
Successfully dropped database 'main'.

13. 
PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet watch --no-hot-reload
Ao rodar a aplicação, as migrações são aplicadas, já que havia apagado o db.


### 92. The repository pattern

A repository mediates between the domain and data mapping layers, acting like an in-memory domain object collection.
Martin Fowler - Patterns of Enterprise Architecture

A estrutura que temos no momento nesta aplicação é:

Web Server -> Controller -> DbContext -> DB
Web Server <- Controller <- DbContext <- DB

A estrutura que teremos será:

Web Server -> Controller -> Repository -> DbContext -> DB
Web Server <- Controller <- Repository <- DbContext <- DB

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


### 93. Creating a repository

14. 
API\Interfaces\IUserRepository.cs

using API.Entities;

namespace API.Interfaces;

public interface IUSerRepository
{
    void Update(AppUser user);
    Task<bool> SaveAllAsync();
    Task<IEnumerable<AppUser>> GetUsersAsync();
    Task<AppUser> GetUserByIdAsync(int id);
    Task<AppUser> GetUserByUsernameAsync(string username);
}

15. 
API\Data\UserRepository.cs

using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUSerRepository
{
    private readonly DataContext _context;

    public UserRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.SingleOrDefaultAsync(x => x.UserName == username);
     // return await _context.Users.FirstOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
        /*
            Entry

            Informa ao EF Tracker que uma entidade fora atualizada.
            Fornece acesso a informações e operações de controle de alterações para a entidade.
            Na verdade, é discutível se há necessidade disso porquanto, ao alterar uma entidade
            em qualquer um dos métodos, EF automaticamente rastreia qualquer mudança. Mas inclui-
            remos isso independentemente caso haja uma razão que nos leve a precisar usar.
        */
    }
}


16. 
API\Extensions\ApplicationServiceExtensions.cs

...
**services.AddScoped<ITokenService, TokenService>();**
/*
    Poderia parametrizar AddScoped somente com TokenService, sem a interface:
        builder.Services.AddScoped<TokenService>();
    
    Mas usar a interface isola melhor o código e facilita ao implementar testes.
*/
services.AddScoped<IUSerRepository, UserRepository>();

**return services;**
...


### 94. Updating the users controller

17. 
API\Controllers\UsersController.cs

using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

...

**public class UsersController : BaseApiController**
{
    private readonly IUSerRepository _userRepository;

    public UsersController(IUSerRepository repository)
    {
        _userRepository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        return Ok(await _userRepository.GetUsersAsync());
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<AppUser>> GetUser(string username)
    {
        return await _userRepository.GetUserByUsernameAsync(username);
    }
}

18. 
***Após modificação abaixo, haverá erro re REFERÊNCIA CIRCULAR. A ser corrigido na aula 95.***

API\Data\UserRepository.cs

...
public async Task<AppUser> GetUserByUsernameAsync(string username)
{
    **return await _context.Users**
        .Include(p => p.Photos)
        **.SingleOrDefaultAsync(x => x.UserName == username);**
        // .FirstOrDefaultAsync(x => x.UserName == username);
}

public async Task<IEnumerable<AppUser>> GetUsersAsync()
{
    **return await _context.Users**
        .Include(p => p.Photos)  // para que objeto retornado inclua entidade Photo e não "photos": []
        **.ToListAsync();**
}
...


### 95. Adding a DTO for Members

Criaremos DTO de usuário e foto. De usuário para ocultar algumas propriedades como PasswordHash, PasswordSalt. Para retornar idade e não data de nascimento. Além disso, DTO ajudará a evitar erro de referência circular.
**OBS** Compare lado a lado a difereça entre:

    API\Entities\AppUser.cs     x    API\DTOs\MemberDto.cs
    API\Entities\Photo.cs       x    API\DTOs\PhotoDto.cs

19. 
API\DTOs\MemberDto.cs

namespace API.DTOs;

public class MemberDto
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public int Age { get; set; }
    public string KnownAs { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastActive { get; set; }
    public string Gender { get; set; }
    public string Introduction { get; set; }
    public string LookingFor { get; set; }
    public string Interests { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public List<PhotoDto> Photos { get; set; }
}

20. 
API\DTOs\PhotoDto.cs

namespace API.DTOs
{
    public class PhotoDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
    }
}


### 96. Adding AutoMapper

Iremos adicionar uma ferramenta para auxiliar no mapeamento de uma entidade para um DTO e vice-versa:

https://www.nuget.org/packages/AutoMapper.Extensions.Microsoft.DependencyInjection/12.0.0

21. 
PS C:\Users\medve\source\repos\udemy\DatingApp\API> dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.0jection --version 12.0.0

AutoMapper irá fazer o mapeamento automático em cada propriedade de 2 classes, origem e destino ou destino e origem:

    API\Entities\AppUser.cs     x    API\DTOs\MemberDto.cs
    API\Entities\Photo.cs       x    API\DTOs\PhotoDto.cs

Irá mapear que ambas possuem Id, UserName... Ou seja, todas as propriedades entre as classes acima em que o tipo e o nome correspondem serão mapeadas automaticamente por AutoMapper. Irá mapear que AppUser possui o método GetAge() e MemberDto possui a propriedade Age. Ele é inteligente para saber usar o método para calcular a idade, mas para isso o nome precisa ser este: GetAge. Caso contrário, o mapeamento para esta propriedade falhará. 

22. 
API\Helpers\AutoMapperProfiles.cs

using API.DTOs;
using API.Entities;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    /*
        AutoMapper permite especificar o caminho DE ONDE -> PARA ONDE
        DE ONDE: AppUser -> PARA ONDE: MemberDto

        Faço isso com CreateMap:
        Creates a mapping configuration from the TSource type to the TDestination type

        Devoluções:
        Mapping expression for more configuration options        
    */
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>();
        CreateMap<Photo, PhotoDto>();
    }
}


23. 
API\Extensions\ApplicationServiceExtensions.cs

...
**services.AddScoped<IUSerRepository, UserRepository>();**
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

**return services;**
...


### 97. Using AutoMapper

24. 
API\Controllers\UsersController.cs

...
private readonly IMapper _mapper;

    public UsersController(..., IMapper mapper)
    {
        ...
        _mapper = mapper;
    }

[HttpGet]
public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
{
    var users = await _userRepository.GetUsersAsync();
    var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);

    return Ok(usersToReturn);
}

[HttpGet("{username}")]
public async Task<ActionResult<MemberDto>> GetUser(string username)
{
    var user = await _userRepository.GetUserByUsernameAsync(username);
    var userToReturn = _mapper.Map<MemberDto>(user);

    return userToReturn;
}
...


### 98. Configuring AutoMapper

25. 
API\DTOs\MemberDto.cs

...
public string PhotoUrl { get; set; }
...

26. 
API\Helpers\AutoMapperProfiles.cs

...
CreateMap<AppUser, MemberDto>()
    .ForMember(
        dest => dest.PhotoUrl,
        opt => opt.MapFrom(
            src => src.Photos.FirstOrDefault(
                x => x.IsMain).Url
        )
    );
/*
    ForMember configura um mapeamento individual para uma propriedade 
    individual que o mapeador automático, AutoMapper, não entende.
    Ele precisa que especifiquemos onde encontrar a foto principal,
    na popriedade Photos, obter a primeira URL.
*/    
...


### 99. Using AutoMapper queryable extensions

27. 
API\Data\UserRepository.cs

...
public async Task<MemberDto> GetMemberByUsernameAsync(string username)
{
    return await _context.Users
        .Where(x => x.UserName == username)
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        .SingleOrDefaultAsync();
        /*
            AutoMapper vem com extensões variáveis, que permite-nos projetar em algo.
            Usamos "ProjectTo" para dizer o que queremos projetar, MemberDto, especifi-
            camos o "_mapper" e passamos o provedor de configuração, ConfigurationPro-
            vider, para que ele saiba onde encontrar nossos perfis de mapeamento, que
            obtém do serviço que adicionamos às nossas extensões de serviço.
        */
}
...

28. 
API\Controllers\UsersController.cs

[HttpGet("{username}")]
public async Task<ActionResult<MemberDto>> GetUser(string username)
{
    return await _userRepository.GetMemberByUsernameAsync(username);
}

29. 
API\Entities\AppUser.cs

...
// public int GetAge()
// {
//     return DateOfBirth.CalculateAge();
// }
...

30. 
API\Helpers\AutoMapperProfiles.cs

...
CreateMap<AppUser, MemberDto>()
    .ForMember(...)
    .ForMember(
        dest => dest.Age,
        opt => opt.MapFrom(
            src => src.DateOfBirth.CalculateAge()
        )
    );
...

31. 
API\Data\UserRepository.cs

...
**public async Task<IEnumerable<MemberDto>> GetMembersAsync()**
{
    return await _context.Users
        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
}
...

32. 
API\Controllers\UsersController.cs

...
[HttpGet]
**public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()**
{
    var users = await _userRepository.GetMembersAsync();

    return Ok(users);
}
...

33. 
git add .  
git commit -m 'End of section 8'  
Clico em Sync Changes para sincronizar mudanças com repositório GitHub (equivale ao git push).  

### 100. Section 8 summary
