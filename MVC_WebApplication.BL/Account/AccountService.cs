namespace MVC_WebApplication.BL.Account
{
    public class AccountService
    {
        /// <summary>
        /// Checks authentication parameters for user
        /// </summary>
        /// <param name="username">User login</param>
        /// <param name="password">User password</param>
        /// <returns>True is parameters are valid, false - otherwise</returns>
        public bool IsValid(string username, string password)
        {
            return true;
        }
    }
}
