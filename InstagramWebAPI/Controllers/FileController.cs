using InstagramWebAPI.Common;
using InstagramWebAPI.Helpers;
using InstagramWebAPI.Interface;
using InstagramWebAPI.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InstagramWebAPI.Controllers
{
    [Route("api/File")]
    [ApiController]
    public class FileController: ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly IUserService _userService;
        private readonly ResponseHandler _responseHandler;

        public FileController(IValidationService validationService, ResponseHandler responseHandler, IUserService userService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _userService = userService;
        }

        [HttpGet("ProfilePhoto")]
        public IActionResult GetProfileImage(long userId, string imageName)
        {
            List<ValidationError> errors = _validationService.ValidateGetUserById(userId);
            if (errors.Any())
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExitsUser, errors));
            }

            int index = imageName.IndexOf('.') + 1;
            string extension = imageName[index..];
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId.ToString(), "ProfilePhoto", imageName);
            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPath, CustomErrorMessage.PathNotExits, imageName));
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

            string fileType = _userService.GetContentType(extension);

            return this.File(imageBytes, fileType);
        }

        [HttpGet("Post")]
        public IActionResult GetPost(long userId, string postName)
        {
            List<ValidationError> errors = _validationService.ValidateGetUserById(userId);
            if (errors.Any())
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExitsUser, errors));
            }

            int index = postName.IndexOf('.') + 1;
            string extension = postName[index..];
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId.ToString(), "Post", postName);
            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPath, CustomErrorMessage.PathNotExits, postName));
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

            string fileType = _userService.GetContentType(extension);

            return this.File(imageBytes, fileType);
        }

        [HttpGet("Reel")]
        public IActionResult GetReel(long userId, string reelName)
        {
            List<ValidationError> errors = _validationService.ValidateGetUserById(userId);
            if (errors.Any())
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExitsUser, errors));
            }

            int index = reelName.IndexOf('.') + 1;
            string extension = reelName[index..];
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId.ToString(), "Reel", reelName);
            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPath, CustomErrorMessage.PathNotExits, reelName));
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

            string fileType = _userService.GetContentType(extension);

            return this.File(imageBytes, fileType);
        }

        [HttpGet("Story")]
        public IActionResult GetStory(long userId, string storyName)
        {
            List<ValidationError> errors = _validationService.ValidateGetUserById(userId);
            if (errors.Any())
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExitsUser, errors));
            }

            int index = storyName.IndexOf('.') + 1;
            string extension = storyName[index..];
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId.ToString(), "Reel", storyName);
            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPath, CustomErrorMessage.PathNotExits, storyName));
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

            string fileType = _userService.GetContentType(extension);

            return this.File(imageBytes, fileType);
        }
    }
}
