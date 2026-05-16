using EduBoost.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduBoost.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly EduBoostContext _context;

        public AdminController(EduBoostContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsuarios = await _context.Usuarios.CountAsync();
            var totalEstudiantes = await _context.Usuarios.CountAsync(u => u.Rol == "Estudiante");
            var totalAsesores = await _context.Usuarios.CountAsync(u => u.Rol == "Asesor");
            var totalCursos = await _context.Cursos.CountAsync();
            
            var asesoriasTotales = await _context.Asesorias.IgnoreQueryFilters().CountAsync();
            var asesoriasPendientes = await _context.Asesorias.CountAsync(a => a.Estado == "Pendiente");
            
            var totalInscripciones = await _context.Inscripciones.CountAsync();

            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.TotalEstudiantes = totalEstudiantes;
            ViewBag.TotalAsesores = totalAsesores;
            ViewBag.TotalCursos = totalCursos;
            ViewBag.AsesoriasTotales = asesoriasTotales;
            ViewBag.AsesoriasPendientes = asesoriasPendientes;
            ViewBag.TotalInscripciones = totalInscripciones;

            // Obtener cursos recientes
            ViewBag.CursosRecientes = await _context.Cursos
                .Include(c => c.Asesor)
                .OrderByDescending(c => c.FechaCreacion)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
