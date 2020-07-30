using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DatingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DatingAPI.Dtos;
using System.Security.Claims;
using DatingAPI.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingAPI.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMapper _impapper;
        private readonly IDatingRepository _datingRepository;
        public UsersController(IDatingRepository datingRepository, IMapper imapper)
        {
            _datingRepository = datingRepository;
            _impapper = imapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams)
        {
            var loggedInUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _datingRepository.GetUser(loggedInUserId);
            userParams.UserId = loggedInUserId;
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = userFromRepo.Gender == "male" ? "female" : "male";
            }
            var users = await _datingRepository.GetUsers(userParams);
            var usersToReturn = _impapper.Map<IEnumerable<UserForListDto>>(users);
            

            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(usersToReturn);
        }
        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingRepository.GetUser(id);
            var uerToReturn = _impapper.Map<UerForDetailedDto>(user);
            return Ok(uerToReturn);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id,UserForUpdate userUpdate)
        {
            if(id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();

            }
            var UserFromRepo = await _datingRepository.GetUser(id);
            _impapper.Map(userUpdate, UserFromRepo);
            if (await _datingRepository.SaveAll())
            {
                return NoContent();
            }
            throw new Exception("Failed on update");
        }
    }
}