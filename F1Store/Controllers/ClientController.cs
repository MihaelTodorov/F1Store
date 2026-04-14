using F1Store.Infrastructure.Data;
using F1Store.Infrastructure.Data.Domain;
using F1Store.Models.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace F1Store.Controllers
{
    public class ClientController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ClientController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            this._userManager = userManager;
            this._context = context;
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index()
        {
            var rawUsers = this._userManager.Users.ToList();

            var allUsers = rawUsers
                .Select(u => new ClientIndexVM
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Address = u.Address,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                })
                .ToList();

            var adminIds = (await _userManager.GetUsersInRoleAsync("Administrator"))
                .Select(a => a.Id).ToList();

            foreach (var user in allUsers)
            {
                user.IsAdmin = adminIds.Contains(user.Id);
            }

            var users = allUsers.Where(x => x.IsAdmin == false)
                .OrderBy(x => x.UserName).ToList();

            return this.View(users);
        }

        // GET: Client/Delete/id
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(string id)
        {
            var user = this._userManager.Users.FirstOrDefault(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            ClientDeleteVM userToDelete = new ClientDeleteVM()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserName = user.UserName
            };

            return View(userToDelete);
        }

        // POST: Client/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(ClientDeleteVM bidingModel)
        {
            string id = bidingModel.Id;
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // 1. ПРОВЕРКА ЗА ПОРЪЧКИ
            bool hasOrders = _context.Orders.Any(o => o.UserId == id);

            if (hasOrders)
            {
                // Показваме специалния изглед за забрана, който създадохме
                return View("DeleteDenied");
            }

            // 2. АКО НЯМА ПОРЪЧКИ - ТРИЕМ
            IdentityResult result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // Пренасочваме към Success изгледа в същата папка
                return RedirectToAction(nameof(Success));
            }

            return BadRequest("Възникна грешка при изтриването.");
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Success()
        {
            return View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult DeleteDenied()
        {
            return View();
        }

        // Празни методи за детайли, създаване и редакция (могат да се развият по-късно)
        public ActionResult Details(int id) => View();
        public ActionResult Create() => View();
        public ActionResult Edit(int id) => View();
    }
}