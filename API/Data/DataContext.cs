using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions options) : base(options)
    {        
    }

    public DbSet<AppUser> Users { get; set; }
    /*
        Até poder-se-ia criar criar public DbSet<Photo> Photos { get; set; },
        mas preciso pensar:

        1. Quando um usuário adicionar uma foto, ela será adicionada a ele especificamente, não a outros.
        2. Em nenhum momento quero que uma foto não esteja associada a um usuário.
        3. Não será necessário consultar as fotos diretamente. Ou seja, consultar o DB em busca de uma fo-
           to para um usuário aleatório.

        Logo, não preciso criar um conjunto DB para as fotos porquanto não irei usar esta entidade para 
        consultar o DB diretamente em busca dela.
        Como Photo está vinculada à entidade AppUser, qualquer consulta a Photo será feita por AppUser.
    */
}
