using System.Security.Claims;
using API.DTOs;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUSerRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUSerRepository repository, IMapper mapper)
    {
        _userRepository = repository;
        _mapper = mapper;
    }

    /*
        --MÉTODO SÍNCRONO--

        [HttpGet]
        public ActionResult<IEnumerable<AppUser>> GetUsers()
        {
            var users = _context.Users.ToList();

            return users;
        }

        --Método Assíncrono--
        1. async
        2. Task
        3. await
        4. método assíncrono
    */
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
    {
        var users = await _userRepository.GetMembersAsync();

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        return await _userRepository.GetMemberByUsernameAsync(username);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        /*
            Obteremos o nome de usuário do token, con-
            siderando que esta rota é autenticada, pois 
            ele só poderá atualizar seu próprio perfil
        */
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        /*
            Debugger:
            this: / User: / Claims [IEnumerable]: / Visualização dos Resultados: / [0] [Claim]:
            [0] [Claim] {http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: lisa}
            value [string]: "lisa"

            ?.Value -> encadeamento opcional (acima) para o caso do username ser null
            Afinal, FindFirst pode resultar em ArgumentNullException. Assim evita-se isso.
        */
        var user = await _userRepository.GetUserByUsernameAsync(username);

        if (user == null) return NotFound();
        /*
            Agora podemos usar AutoMapper para atualizar propriedades.
            De MemberUpdateDto para User.
            Quando buscamos "user" do repositório, EF está rastreando este usuário e qual-
            quer atualização para ele será rastreada por EF.

            _mapper.Map(memberUpdateDto, user);
            Esta linha de código está mapeando e atualizando todas as propriedades de Member-
            UpdateDto e substituindo pelos novos dados/propriedades passados pelo usuário.
            Nada ainda, porém, foi salvo no DB.
        */
        _mapper.Map(memberUpdateDto, user);

        if (await _userRepository.SaveAllAsync()) return NoContent();
        /*
            Caso tente enviar dados no corpo da requisição iguais aos de uma requisição ante-
            rior, que já sejam os dados da entidade/perfil sem qualquer atualização de fato,
            o fluxo do código seguirá para a linha de baixo e retornará bad request.
            Isso porque o mapeamento, AtuoMapper, não detectou nunhuma alteração entre Member-
            UpdateDto e AppUser.
        */

        return BadRequest("Failed to update user");
    }
}
