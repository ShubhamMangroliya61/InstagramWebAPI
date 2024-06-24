﻿using AutoMapper;
using InstagramWebAPI.DAL.Models;
using InstagramWebAPI.DTO;

namespace InstagramWebAPI
{
    public class MappingConfig :Profile
    {
        public MappingConfig()
        {
            CreateMap<User, UserDTO>().ReverseMap();
            CreateMap<UserDTO, User>();
        }
    }
}
