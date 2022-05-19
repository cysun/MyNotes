using AutoMapper;
using MyNotes.Models;

namespace MyNotes.Services;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Note, NoteInputModel>();
        CreateMap<NoteInputModel, Note>().ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<Models.File, FileInputModel>();
        CreateMap<FileInputModel, Models.File>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ParentId, opt => opt.Ignore())
            .ForMember(dest => dest.IsFolder, opt => opt.Ignore());

        CreateMap<Models.File, FileHistory>().ForMember(h => h.FileId, opt => opt.MapFrom(f => f.Id));
    }
}
