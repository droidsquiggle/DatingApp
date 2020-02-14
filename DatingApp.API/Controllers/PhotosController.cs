using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _repo = repo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            // in constructor activate the cloudinare account
            // store settings from config helper
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName, 
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            // send account information to cloundinary
            _cloudinary = new Cloudinary(acc);
        }

        // get photo via photo id
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            // first check the user id against the token to make sure the user logged in is user uploading photos
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userFromRepo = await _repo.GetUser(userId);

            // using a dto to send photos from our api to cloudinary
            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            // make sure theres a photo to upload and then stream it to cloudinary
            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    // set upload params, we will transform it to be a square image and focus on face only
                    // using cloudinary settings
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    // upload photo to cloudinary
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            // after upload we now have the public url and id to view photo with
            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            // use the auto mapper to set the dto object to a photo object
            var photo = _mapper.Map<Photo>(photoForCreationDto);

            // if the user has not uploaded any photos yet, set the first one they upload to "main"
            if (!userFromRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;

            // set the photo to the user
            userFromRepo.Photos.Add(photo);

            // save the repo
            if (await _repo.SaveAll())
            {
                // since the photo id doesnt exist until the phot is saved in the sql lite db
                // we dont map until after the save all has succeded
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                // we need to use created at route to return the photo, so we use the below method
                // param 1 calls the http get function we define above
                // param 2 is a new object with the params
                // param 3 is the mapped photo object
                return CreatedAtRoute("GetPhoto", new {userId = userId, id = photo.Id}, photoToReturn);
            }

            return BadRequest("Could not add photo.");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            // first check the user id against the token to make sure the user logged in is user uploading photos
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            // if the id we're passing in does not assign to any photo id in the users photo return unauthorized
            if(!user.Photos.Any(p => p.Id == id))
                return Unauthorized();
            

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo");

            // grab old main photo
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;
            
            // set new photo to main
            photoFromRepo.IsMain = true;

            // save all changes
            if (await _repo.SaveAll())
                return NoContent();
            

            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            // first check the user id against the token to make sure the user logged in is user uploading photos
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);

            // if the id we're passing in does not assign to any photo id in the users photo return unauthorized
            if(!user.Photos.Any(p => p.Id == id))
                return Unauthorized();
            
            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo.IsMain)
                return BadRequest("You cannot delete your main photo");

            // if the photo is stored in cloudinary then it has a publicid so delete from cloudinary as well
            if (photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                // result should return "ok"
                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                    _repo.Delete(photoFromRepo);
            }
            
            // if photo object doesn'thave a public id then it is stored locally in db so just delete at the repo level
            if (photoFromRepo.PublicId==null)
            {
                _repo.Delete(photoFromRepo);
            }

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to delete photo");
        }
    }
}