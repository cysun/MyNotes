using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MyNotes.Models;

namespace MyNotes.Services
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Note, NoteInputModel>();
            CreateMap<NoteInputModel, Note>().ForMember(i => i.Id, opt => opt.Ignore());

            CreateMap<File, FileInputModel>();
            CreateMap<FileInputModel, File>()
                .ForMember(i => i.Id, opt => opt.Ignore())
                .ForMember(i => i.ParentId, opt => opt.Ignore())
                .ForMember(i => i.IsFolder, opt => opt.Ignore());

            CreateMap<File, FileHistory>().ForMember(h => h.FileId, opt => opt.MapFrom(f => f.Id));
        }
    }
}
