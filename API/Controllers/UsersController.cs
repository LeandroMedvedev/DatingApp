using API.DTOs;
using API.Entities;
using API.Extensions;
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
    private readonly IPhotoService _photoService;

    public UsersController(IUSerRepository userRepository, IMapper mapper, IPhotoService photoService)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _photoService = photoService;
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
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        /*
            Não haveria necessidade de verificar se user == null porque estamos obtendo usuário pelo nome do usuário do token. E usuário só consegue acessar esta rota autenticado, ou seja, user == null será sempre false; fizemos só por fazer mesmo. No método de deleção abaixo, não fizemos.
        */
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
            Isso porque o mapeamento, AutoMapper, não detectou nunhuma alteração entre Member-
            UpdateDto e AppUser.
        */

        return BadRequest("Failed to update user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

        if (user == null) return NotFound();

        var result = await _photoService.AddPhotoAsync(file);

        if (result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId
        };

        /*
            Se for 1° carregamento de foto do usuário, colocaremos esta como principal.
        */
        if (user.Photos.Count == 0) photo.IsMain = true;

        user.Photos.Add(photo);

        if (await _userRepository.SaveAllAsync())
        {
            return CreatedAtAction(
                nameof(GetUser), new { username = user.UserName }, _mapper.Map<PhotoDto>(photo)
            );
        }

        return BadRequest("Problem adding photo");
    }

    [HttpPut("set-main-photo/{photoId}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        if (user == null) return NotFound();

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("this is already your main photo");

        var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMain != null) currentMain.IsMain = false;  // defino foto principal atual como false
        photo.IsMain = true; // defino foto que será a principal, a partir de agora, como true
        // isso garante que haverá sempre 1 foto principal somente, as demais serão { isMain: false }

        if (await _userRepository.SaveAllAsync()) return NoContent();

        return BadRequest("Problem setting main photo");
    }

    [HttpDelete("delete-photo/{photoId}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

        if (photo == null) return NotFound();

        if (photo.IsMain) return BadRequest("You cannot delete your main photo");

        if (photo.PublicId != null)
        {
            /*
                Esta verificação é feita porque há imagens que não possuem PublicId em nosso db.
                E, se elas não possuem PublicId, então é uma ds imagens que nós semeamos.
                E não precisamos apagar essas de Cloudinary porque simplesmente não estão na nuvem de qualquer modo.
            */
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);

        if (await _userRepository.SaveAllAsync()) return Ok();

        return BadRequest("Problem deleting photo");
    }
}
