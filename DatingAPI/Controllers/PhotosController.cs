using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingAPI.Data;
using DatingAPI.Dtos;
using DatingAPI.Helpers;
using DatingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingAPI.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudSettings;
        private readonly Cloudinary _cloudinary;
        public PhotosController( IDatingRepository repo, IMapper mapper,
            IOptions<CloudinarySettings> cloudinaryConfig)
        
        {
            _repo = repo;
            _mapper = mapper;
            _cloudSettings = cloudinaryConfig;

            Account acc = new Account(
                cloudinaryConfig.Value.CloudName,
                cloudinaryConfig.Value.ApiKey,
                cloudinaryConfig.Value.ApiSecret

            );
            _cloudinary = new Cloudinary(acc);
        }
        [HttpGet("{id}", Name ="GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photoToReturn = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photoToReturn);
        }


        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]photoForCreationDto photoCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var UserFromRepo = await _repo.GetUser(userId);
           var file = photoCreationDto.File;
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }
            photoCreationDto.Url = uploadResult.Url.ToString();
            photoCreationDto.PublicId = uploadResult.PublicId;
            var photo = _mapper.Map<Photo>(photoCreationDto);

            if (!UserFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;
            UserFromRepo.Photos.Add(photo);
            if(await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photoToReturn.Id }, photoToReturn);
            }
            return BadRequest("could not add photo");


        }
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var UserFromRepo = await _repo.GetUser(userId);
            if(!UserFromRepo.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }
            var photoFromRepo = await _repo.GetPhoto(id);
            if (photoFromRepo.IsMain)
            {
                return BadRequest("This is already a main photo");
            }
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;
            if (await _repo.SaveAll())
                return NoContent();
            return BadRequest("could not set photo to main photo!");
            
        }
        [HttpDelete("{photoId}")]
        public async Task<IActionResult> DeletePhoto(int userId, int photoId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            var UserFromRepo = await _repo.GetUser(userId);
            if (!UserFromRepo.Photos.Any(p => p.Id == photoId))
            {
                return Unauthorized();
            }
            var photoFromRepo = await _repo.GetPhoto(photoId);
            if (photoFromRepo.IsMain)
            {
                return BadRequest("Yo can't delete your main photo");
            }
            if(photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);
                if (result.Result == "ok")
                {
                    _repo.Delete(photoFromRepo);
                }
            }
            if(photoFromRepo.PublicId == null)
            {
                _repo.Delete(photoFromRepo);
            }
            
            if(await _repo.SaveAll())
            {
                return Ok();
            }
            return BadRequest("Yo can't delete your main photo");
        }
        
    }
}