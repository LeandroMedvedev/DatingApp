using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUSerRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
        // .FirstOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users
            .Include(p => p.Photos)  // para que objeto retornado inclua entidade Photo e não "photos": []
            .ToListAsync();
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

    public async Task<IEnumerable<MemberDto>> GetMembersAsync()
    {
        return await _context.Users
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}
