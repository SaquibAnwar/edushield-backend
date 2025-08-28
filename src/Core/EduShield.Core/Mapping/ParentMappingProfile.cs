using AutoMapper;
using EduShield.Core.Dtos;
using EduShield.Core.Entities;

namespace EduShield.Core.Mapping;

/// <summary>
/// AutoMapper profile for Parent entity mappings
/// </summary>
public class ParentMappingProfile : Profile
{
    public ParentMappingProfile()
    {
        // Parent entity to ParentResponse DTO
        CreateMap<Parent, ParentResponse>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.DateOfBirth)))
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));

        // CreateParentRequest to Parent entity
        CreateMap<CreateParentRequest, Parent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // UpdateParentRequest to Parent entity
        CreateMap<UpdateParentRequest, Parent>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());

        // Student entity to ParentChildInfo DTO
        CreateMap<Student, ParentChildInfo>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => CalculateAge(src.DateOfBirth)))
            .ForMember(dest => dest.IsEnrolled, opt => opt.MapFrom(src => src.IsEnrolled));
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
