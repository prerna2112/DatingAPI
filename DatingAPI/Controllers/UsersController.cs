﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingAPI.Dtos;

namespace DatingAPI.Controllers
{   [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController :ControllerBase
    {
        private readonly IMapper _impapper;
        private  readonly IDatingRepository _datingRepository;
        public UsersController(IDatingRepository datingRepository, IMapper imapper)
        {
            _datingRepository = datingRepository;
            _impapper = imapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _datingRepository.GetUsers();
            var usersToReturn = _impapper.Map<IEnumerable<UserForListDto>>(users);
            return Ok (usersToReturn);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingRepository.GetUser(id);
            var uerToReturn = _impapper.Map<UerForDetailedDto>(user);
            return Ok(uerToReturn);
        }
    }
}