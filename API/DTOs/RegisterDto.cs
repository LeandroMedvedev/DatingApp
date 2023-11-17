using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class RegisterDto
{
    [Required] public string Username { get; set; }

    [Required] public string KnownAs { get; set; }

    [Required] public string Gender { get; set; }

    [Required] public DateOnly? DateOfBirth { get; set; }  // optional to make required work!
    /*
        Por que o uso de [Required] com Nullable Types ("?", operador opcional)?

        [Required] geralmente é usado para exigir que um valor seja passado para a propriedade, para que não seja aceitável receber null. Então por que usar Nullable Types?
        Sem [Required] e sem o operador de interrogação (Nullable Type) o valor passado automaticamente seria o valor padrão do tipo DateOnly, 0001-01-01.
        Esse é um comportamento que não desejamos. Desejamos que o usuário passe um valor de data no input DateOfBirth.

        1. sem required e sem ?: caso nenhum valor seja passado, assumirá valor padrão 0001-01-01;
        2. sem required e com ?: aceita valor null;
        3. com required e sem ?: obriga usuário a passar um valor;
        4. com required e com ?: obriga usuário a passar valor e aceita valor nulo (acho eu).
    */    

    [Required] public string City { get; set; }

    [Required] public string Country { get; set; }

    [Required]
    [StringLength(8, MinimumLength = 4)]
    public string Password { get; set; }
}
