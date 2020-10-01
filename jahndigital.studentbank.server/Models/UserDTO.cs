using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.Models
{
    public class UserDTO
    {
        /// <summary>
        /// The user's ID number.
        /// </summary>
        public long? Id {get; private set;}

        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email {get; set;} = string.Empty;

        /// <summary>
        /// The user's password.
        /// </summary>
        public string Password {private get; set;} = string.Empty;

        /// <summary>
        /// Get the user's password.
        /// </summary>
        /// <remarks>Prevents the property from being deserialized but still allows access.</remarks>
        /// <returns></returns>
        public string GetPassword() => Password;

        /// <summary>
        /// The ID number of the user's role.
        /// </summary>
        public long? RoleId {get; set;}

        /// <summary>
        /// Convert an entity into a new DTO
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static UserDTO FromEntity(User user)
        {
            return new UserDTO {
                Id = user.Id,
                Email = user.Email,
                RoleId = user.RoleId
            };
        }

        /// <summary>
        /// Convert a DTO to an entity.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static User ToEntity(UserDTO user)
        {
            var entity = new User {
                Email = user.Email,
                RoleId = user.RoleId!.Value
            };
            
            if (user.Id != null) {
                entity.Id = user.Id.Value;
            }

            if (!string.IsNullOrEmpty(user.Password)) {
                entity.Password = user.Password;
            }

            return entity;
        }
    }
}
