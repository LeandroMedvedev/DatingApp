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

    /*
        Estas 2 propriedades (abaixo) impedem que possa criar uma foto que não esteja vinculada a um usuário.
        Com elas agora, há obrigatoriedade de vinculação. Não poderá haver uma foto vinculada a um usuário nulo.

        Se olhar na migração (quando criá-la) verá esta linha (80):

            AppUserId = table.Column<int>(type: "INTEGER", nullable: false)
        
        Repare em nullable = false. Não poderá haver uma foto vinculada a um usuário nulo.
        Já na linha 90 há:

            onDelete: ReferentialAction.Cascade);
        
        Efeito cascata fará com que, caso um usuário seja removido, todas as suas entidades relacionadas tb o serão.
    */
    public int AppUserId { get; set; }
    public AppUser AppUser { get; set; }
}
