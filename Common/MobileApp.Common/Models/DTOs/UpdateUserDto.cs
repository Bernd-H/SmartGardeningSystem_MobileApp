namespace MobileApp.Common.Models.DTOs {
    public class UpdateUserDto {

        public string Username { get; set; }

        public string Password { get; set; }

        public string NewPassword { get; set; }

        public string NewUsername { get; set; }

        public UpdateUserDto() {

        }
    }
}
