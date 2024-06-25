namespace InstagramWebAPI.DTO
{
    public class RegistrationRequestDTO
    {
        public string? EmailOrNumber { get; set; }
        //public string? MobileNumber { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Type { get; set; }
    }
}
