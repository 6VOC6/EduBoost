namespace EduBoost.Controllers
{
    using EduBoost.Models;
    using Microsoft.AspNetCore.Mvc;
    using EduBoost.Data;

    public class UsuariosController : Controller
    {
        private readonly EduBoostContext _context;

        public UsuariosController(EduBoostContext context)
        {
            _context = context;
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(usuario);
        }

        public IActionResult Login()
        {
            return View();
        }
    }
}
