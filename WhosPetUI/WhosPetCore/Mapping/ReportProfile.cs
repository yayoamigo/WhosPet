using WhosPetCore.DTO.Incoming.Pets;
using WhosPetCore.Domain.Entities;
using AutoMapper;
using WhosPetCore.DTO.Outgoing;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<LostReportDTO, LostPetReport>()
             .ForMember(dest => dest.Id, opt => opt.Ignore()) 
             .ForMember(dest => dest.UserProfile, opt => opt.Ignore())
             .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.PetName))
             .ForMember(dest => dest.latitude, opt => opt.MapFrom(src => src.Latitude))
             .ForMember(dest => dest.longitude, opt => opt.MapFrom(src => src.Longitude))
             .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.ImageUrl))
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
             .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.DateLost))
             .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
             .ForMember(dest => dest.IsFound, opt => opt.MapFrom(src => false)) 
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));
       CreateMap<LostPetReport, LostPetResponseDTO>()
             .ForMember(dest => dest.PetName, opt => opt.MapFrom(src => src.Pet.Name))
             .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
             .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
             .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.City))
             .ForMember(dest => dest.longitude, opt => opt.MapFrom(src => src.longitude))
             .ForMember(dest => dest.latitude, opt => opt.MapFrom(src => src.latitude))
             .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
             .ForMember(dest => dest.IsFound, opt => opt.MapFrom(src => src.IsFound))
             .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
    }
}