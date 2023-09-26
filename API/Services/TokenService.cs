using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace API.Services;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    /*
        SymmetricSecurityKey
        A mesma chave é usada para criptografar e descriptografar os dados.
        Em nosso caso, usamos uma chave de segurança simétrica porque esta ficará no servidor e nunca irá para o cliente porquanto o cliente não precisa descriptografar esta chave.

        Em HTTPS e SSL há:
            * chave privada - permanece no servidor.
            * chave pública - pode ser usada para decifrar dados.
    */

    public TokenService(IConfiguration config)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }

    public string CreateToken(AppUser user)
    {
        /*
            CLAIM (reivindicação/afirmação)
            Poderia afirmar que meu aniversário é tal dia, que meu time é tal... Todas seriam afirmações/reivindicações sobre mim mesmo.
            Uma reivindicação é um pouco de informação que um usuário requer. Neste caso, definimos as reivindicações para o nome de usuário.
            Usamos List porque pode haver mais de 1 e é desejável que haja (implementaremos posteriormente).
            Usuário terá um token que afirma que seu username é o que está definido dentro do token.
        */
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.NameId, user.UserName)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(7),
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
