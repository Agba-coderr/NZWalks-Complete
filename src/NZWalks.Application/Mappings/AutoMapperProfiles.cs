using AutoMapper;
using Microsoft.AspNetCore.Identity;
using NZWalks.Domain.Entities;
using NZWalks.Application.DTOs;
using NZWalks.Application.Regions.Commands;
using NZWalks.Application.Walks.Commands;

namespace NZWalks.Application.Mappings
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            //Region Automappings
            CreateMap<Region, RegionDto>().ReverseMap();
            CreateMap<CreateRegionCommand, Region>().ReverseMap();
            CreateMap<UpdateRegionCommand, Region>().ReverseMap();
            CreateMap<UpdateRegionDto, Region>().ReverseMap();

            //Walk Automappings
            CreateMap<Walk, WalkDto>().ReverseMap();
            CreateMap<UpdateWalkCommand, Walk>().ReverseMap();
            CreateMap<AddWalkRequestDto, Walk>().ReverseMap();
            CreateMap<UpdateWalkDto, Walk>().ReverseMap();

            // Image Automappings
            CreateMap<ImageUploadRequestDto, Image>()
                .ForMember(dest => dest.FileExtension, opt => opt.MapFrom(src => Path.GetExtension(src.FileName)))
                .ForMember(dest => dest.FileSizeInBytes, opt => opt.MapFrom(src => src.FileStream.Length));
        }
    }
}
