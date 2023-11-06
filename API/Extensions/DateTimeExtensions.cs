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
