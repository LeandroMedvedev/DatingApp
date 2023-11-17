using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    /*
        AutoMapper permite especificar o caminho DE ONDE -> PARA ONDE
        DE ONDE: AppUser -> PARA ONDE: MemberDto

        Faço isso com CreateMap:
        Creates a mapping configuration from the TSource type to the TDestination type

        Devoluções:
        Mapping expression for more configuration options        
    */
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                opt => opt.MapFrom(
                    src => src.Photos.FirstOrDefault(
                        x => x.IsMain).Url
                )
            )
            .ForMember(
                dest => dest.Age,
                opt => opt.MapFrom(
                    src => src.DateOfBirth.CalculateAge()
                )
            );
        /*
            ForMember configura um mapeamento individual para uma propriedade 
            individual que o mapeador automático, AutoMapper, não entende.
            Ele precisa que especifiquemos onde encontrar a foto principal,
            na propriedade Photos, obter a primeira URL.
        */
        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        /*
            Como as propriedades de MemberUpdateDto correspondem exatamente às
            propriedades de AppUser, é desnecessário qualquer lógica adicional.
            O mesmo para RegisterDto e AppUser (abaixo).
        */
        CreateMap<RegisterDto, AppUser>();
    }
}
