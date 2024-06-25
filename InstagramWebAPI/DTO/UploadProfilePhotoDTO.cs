namespace InstagramWebAPI.DTO
{
    public class UploadProfilePhotoDTO
    {
        public long UserId { get; set; }
        public IFormFile? ProfilePhoto { get; set; }
        
    }
}
