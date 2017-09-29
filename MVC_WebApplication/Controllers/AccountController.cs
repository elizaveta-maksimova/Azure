using System.Web.Mvc;
using MVC_WebApplication.BL.Account;
using MVC_WebApplication.Models;

namespace MVC_WebApplication.Controllers
{
    /// <summary>
    /// Controller for user authentication and registration
    /// </summary>
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;

        public AccountController()
        {
            _accountService = new AccountService();
        }

        /// <summary>
        /// Gets login page
        /// </summary>
        /// <returns>Login page view</returns>
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Logs user into system
        /// </summary>
        /// <param name="username">User login</param>
        /// <param name="password">User password</param>
        /// <returns>Redirects to main page</returns>
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (_accountService.IsValid(username, password))
            {
                // some additional logic 

                return RedirectToAction("Index", "Home");
            }

            // redirect to error page
            // return RedirectToAction("Error", "Home");

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Registers new user to the system
        /// </summary>
        /// <param name="newUser">New user info</param>
        /// <returns>Redirects to main page</returns>
        [HttpPost]
        public ActionResult Register(NewUser newUser)
        {
            // additional logic here 
            // _accountService.Register(newUser);

            return RedirectToAction("Index", "Home");
        }
    }
}