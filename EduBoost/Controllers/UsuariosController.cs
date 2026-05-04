namespace EduBoost.Controllers
{
    using EduBoost.Models;
    using Microsoft.AspNetCore.Mvc;
    using EduBoost.Data;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Claims;
    using System.Security.Cryptography;

    public class UsuariosController : Controller
    {
        private readonly EduBoostContext _context;

        public UsuariosController(EduBoostContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Registro(RegistroViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            model.Correo = model.Correo?.Trim().ToLowerInvariant() ?? string.Empty;
            model.Rol = model.Rol?.Trim() ?? "Estudiante";

            if (_context.Usuarios.Any(u => u.Correo == model.Correo))
            {
                ModelState.AddModelError(nameof(RegistroViewModel.Correo), "Ya existe una cuenta con este correo.");
            }

            if (ModelState.IsValid)
            {
                var usuario = new Usuario
                {
                    Nombre = model.Nombre.Trim(),
                    Correo = model.Correo,
                    Password = HashPassword(model.Password),
                    Rol = model.Rol,
                    FechaRegistro = DateTime.UtcNow
                };

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Cuenta creada correctamente. Inicia sesion para continuar.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            model.Correo = model.Correo?.Trim().ToLowerInvariant() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == model.Correo);
            if (usuario is null || !VerifyPassword(model.Password, usuario.Password))
            {
                ModelState.AddModelError(string.Empty, "Correo o contrasena incorrectos.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = model.Recordarme });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedValue)
        {
            var parts = storedValue.Split(':', 2);
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt;
            byte[] expectedHash;
            try
            {
                salt = Convert.FromBase64String(parts[0]);
                expectedHash = Convert.FromBase64String(parts[1]);
            }
            catch (FormatException)
            {
                return false;
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] inputHash = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(inputHash, expectedHash);
        }
    }
}
