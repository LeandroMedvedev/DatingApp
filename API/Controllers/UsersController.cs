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
}
