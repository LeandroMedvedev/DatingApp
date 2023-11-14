using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /*
        Método de extensão para obter nome de usuário a partir do token
    */
    public static string GetUsername(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    /*
        Debugger:
        this: / User: / Claims [IEnumerable]: / Visualização dos Resultados: / [0] [Claim]:
        [0] [Claim] {http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: lisa}
        value [string]: "lisa"

        ?.Value -> encadeamento opcional (acima) para o caso do username ser null
        Afinal, FindFirst pode resultar em ArgumentNullException. Assim evita-se isso.
    */
}
