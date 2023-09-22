using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TwatterDemo.Models;

namespace TwatterDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBConnector _context;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, DBConnector context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        // Loads/displays index.cshtml and if it recives a post from popupPost in index it then adds it to the db table twats.
        public async Task<IActionResult> Index([FromForm] TwatsDTO request)
        {

            if (request.content != null)
            {
                // Gets cookies userName, and userImg.
                string? userName = Request.Cookies["userName"];
                string? userImg = Request.Cookies["userImg"];

                // Create a new twat object with the provided details
                var newTwat = new Twats
                {
                    username = userName,
                    userImg = userImg,
                    timeOfPost = DateTime.Now,
                    content = request.content
                };

                // Add the new user to the database.
                _context.twats.Add(newTwat);
                _context.SaveChanges();
            }

            DateTime startOfMonth, endOfMonth;
            DateTime currentDateTime = DateTime.Now;

            // Set the startOfMonth to the first day of the current month at 00:00:00.
            startOfMonth = new DateTime(currentDateTime.Year, currentDateTime.Month, 1, 0, 0, 0);

            // To get the endOfMonth, add one month to startOfMonth and subtract one second.
            endOfMonth = startOfMonth.AddMonths(1).AddSeconds(-1);

            // Call the stored procedure and retrieve the results.
            var twats = await _context.twats.FromSqlRaw("CALL MainFeed({0}, {1})", startOfMonth, endOfMonth).ToListAsync();

            // Gets token if its set.
            string? token = HttpContext.Session.GetString("AuthToken");

            // Create a view model to hold both twats and token.
            var viewModel = new TwatsTokenViewModel
            {
                Twats = twats,
                Token = token
            };

            _logger.LogInformation(viewModel.Token);
            return View(viewModel);


        }

        // Loads/displays Register.cshtml and if it recives a post from the form there it then adds a new user to db table users.
        public IActionResult Register([FromForm] UserDTO request)
        {
            if (request.userName != null)
            {
                // Handle the form submission and user registration logic.
                _logger.LogInformation("Register POST request received.");
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);

                // Create a new User object with the provided details.
                var newUser = new User
                {
                    userName = request.userName,
                    passwordHashed = passwordHash,
                    userImg = "No Img",
                    creationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // Add the new user to the database.
                _context.users.Add(newUser);
                _context.SaveChanges();

                // Redirect to index where you will still be asked to login.
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogInformation("Register GET request received.");
                return View();
            }
        }

        // Loads/displays login.cshtml and if it recives a post from the form and if it is a real user and the password matches
        // it then runs CreateToken and adds a sesstion-cookie with the token, and redirect to index but logged in this time.
        public async Task<ActionResult<User>> Login([FromForm] UserDTO request)
        {
            try
            {
                if (request.userName != null)
                {
                    // Retrieve user data from the database based on the provided username.
                    _logger.LogInformation("Login POST request received.");
                    var userlist = await _context.users.FromSqlRaw("CALL GetUser({0})", request.userName).ToListAsync();
                    var user = userlist.FirstOrDefault(); // Use FirstOrDefault() instead of [0].

                    if (user == null)
                    {
                        throw new InvalidLoginException("Username or password is incorrect.");
                    }

                    // Verify the password hash.
                    if (!BCrypt.Net.BCrypt.Verify(request.password, user.passwordHashed))
                    {
                        throw new InvalidLoginException("Username or password is incorrect.");
                    }

                    // call a method that creates a token.
                    string token = CreateToken(user);

                    // sets cookies userName, and userImg.
                    Response.Cookies.Append("userName", user.userName);
                    Response.Cookies.Append("userImg", user.userImg);

                    //store token in session (gonna be used to control ability to post and what not).
                    HttpContext.Session.SetObjectAsJson("AuthToken", token);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _logger.LogInformation("Login GET request received.");
                    return View();
                }
            }
            catch (InvalidLoginException ex)
            {
                // Handle the custom exception and return a 400 (Bad Request) response with the error message.
                _logger.LogError(ex, "Login failed.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Handle any other exceptions here, log them, and return 404 as response
                _logger.LogError(ex, "An error occurred during login.");
                return StatusCode(404, "An error occurred during login."); // 404 (Not Found)
            }
        }


        private string CreateToken(User user)
        {
            List<Claim> Claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.userName)};

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JWT:SecretKey").Value!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: Claims,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        // Loads Profile.cshtml not alot function on it not even a feed atm.
        public IActionResult Profile()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}