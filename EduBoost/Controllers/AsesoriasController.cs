using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduBoost.Data;
using EduBoost.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EduBoost.Controllers
{
    [Authorize]
    public class AsesoriasController : Controller
    {
        private readonly EduBoostContext _context;

        public AsesoriasController(EduBoostContext context)
        {
            _context = context;
        }

        // Listar asesorías (según rol)
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var rol = User.FindFirstValue(ClaimTypes.Role);

            IQueryable<Asesoria> query = _context.Asesorias
                .Include(a => a.Estudiante)
                .Include(a => a.Asesor)
                .Include(a => a.Curso);

            if (rol == "Estudiante")
            {
                query = query.Where(a => a.IdEstudiante == userId);
            }
            else if (rol == "Asesor")
            {
                // El asesor ve las que tiene asignadas o las que están pendientes (si quisiera auto-asignarse)
                // Por ahora, solo las asignadas a él
                query = query.Where(a => a.IdAsesor == userId || a.Estado == "Pendiente");
            }

            var asesorias = await query.OrderByDescending(a => a.FechaHora).ToListAsync();
            return View(asesorias);
        }

        // GET: Solicitar Asesoría
        public async Task<IActionResult> Solicitar()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Solo dejamos solicitar asesoría de cursos en los que están inscritos
            var misCursos = await _context.Inscripciones
                .Where(i => i.IdUsuario == userId)
                .Include(i => i.Curso)
                .Select(i => i.Curso)
                .ToListAsync();

            ViewBag.Cursos = misCursos;
            return View();
        }

        // POST: Solicitar Asesoría
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Solicitar(Asesoria asesoria)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Forzamos el estudiante al usuario logueado
            asesoria.IdEstudiante = userId;
            asesoria.Estado = "Pendiente";

            if (ModelState.IsValid)
            {
                _context.Asesorias.Add(asesoria);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Solicitud de asesoría enviada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Si falla, recargar cursos
            var misCursos = await _context.Inscripciones
                .Where(i => i.IdUsuario == userId)
                .Include(i => i.Curso)
                .Select(i => i.Curso)
                .ToListAsync();
            ViewBag.Cursos = misCursos;
            
            return View(asesoria);
        }

        // GET: Detalle / Gestionar (para Asesor)
        public async Task<IActionResult> Gestionar(int id)
        {
            var asesoria = await _context.Asesorias
                .Include(a => a.Estudiante)
                .Include(a => a.Curso)
                .FirstOrDefaultAsync(a => a.IdAsesoria == id);

            if (asesoria == null) return NotFound();

            return View(asesoria);
        }

        // POST: Aceptar Asesoría
        [HttpPost]
        [Authorize(Roles = "Asesor,Administrador")]
        public async Task<IActionResult> Aceptar(int id, string enlace)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var asesoria = await _context.Asesorias.FindAsync(id);

            if (asesoria != null)
            {
                asesoria.IdAsesor = userId;
                asesoria.Estado = "Aceptada";
                asesoria.EnlaceReunion = enlace;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Has aceptado la asesoría.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Finalizar Asesoría
        [HttpPost]
        [Authorize(Roles = "Asesor,Administrador")]
        public async Task<IActionResult> Finalizar(int id)
        {
            var asesoria = await _context.Asesorias.FindAsync(id);
            if (asesoria != null)
            {
                asesoria.Estado = "Completada";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Asesoría marcada como completada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
