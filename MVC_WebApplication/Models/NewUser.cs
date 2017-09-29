namespace MVC_WebApplication.Models
{
    /// <summary>
    /// Model for user registration
    /// </summary>
    public class NewUser
    {
        /// <summary>
        /// Gets or sets user login
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets user email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets user password
        /// </summary>
        public string Password { get; set; }
    }
}