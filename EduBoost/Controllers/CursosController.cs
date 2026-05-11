using EduBoost.Data;
using EduBoost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduBoost.Controllers
{
    [Authorize]
    public class CursosController : Controller
    {
        private readonly EduBoostContext _context;

        public CursosController(EduBoostContext context)
        {
            _context = context;
        }

        // GET: Cursos
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var cursos = await _context.Cursos.OrderByDescending(c => c.FechaCreacion).ToListAsync();
            return View(cursos);
        }

        // GET: Cursos/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(m => m.IdCurso == id);
            if (curso == null)
            {
                return NotFound();
            }

            // Verificar si el usuario ya esta inscrito
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                ViewBag.EstaInscrito = await _context.Inscripciones
                    .AnyAsync(i => i.IdUsuario == userId && i.IdCurso == id);
            }

            return View(curso);
        }

        // GET: Cursos/Create
        [Authorize(Roles = "Administrador,Asesor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cursos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Create([Bind("IdCurso,Nombre,Descripcion,Profesor")] Curso curso)
        {
            if (ModelState.IsValid)
            {
                curso.FechaCreacion = DateTime.UtcNow;
                _context.Add(curso);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Cursos/Edit/5
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        // POST: Cursos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Edit(int id, [Bind("IdCurso,Nombre,Descripcion,Profesor,FechaCreacion")] Curso curso)
        {
            if (id != curso.IdCurso)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.IdCurso))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Cursos/Delete/5
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(m => m.IdCurso == id);
            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        // POST: Cursos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null)
            {
                _context.Cursos.Remove(curso);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cursos/Inscribirse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Challenge();

            var userId = int.Parse(userIdStr);
            
            // Verificar si ya esta inscrito
            var yaInscrito = await _context.Inscripciones
                .AnyAsync(i => i.IdUsuario == userId && i.IdCurso == id);

            if (yaInscrito)
            {
                TempData["ErrorMessage"] = "Ya estas inscrito en este curso.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Crear inscripcion
            var inscripcion = new Inscripcion
            {
                IdUsuario = userId,
                IdCurso = id,
                FechaInscripcion = DateTime.UtcNow
            };

            // Crear registro de progreso inicial
            var progreso = new Progreso
            {
                IdUsuario = userId,
                IdCurso = id,
                Porcentaje = 0,
                UltimaActualizacion = DateTime.UtcNow
            };

            _context.Inscripciones.Add(inscripcion);
            _context.Progresos.Add(progreso);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "¡Te has inscrito correctamente!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Cursos/MisCursos
        public async Task<IActionResult> MisCursos()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Challenge();
            var userId = int.Parse(userIdStr);

            var misCursos = await _context.Inscripciones
                .Include(i => i.Curso)
                .Where(i => i.IdUsuario == userId)
                .Select(i => new MisCursosViewModel
                {
                    Curso = i.Curso,
                    Progreso = _context.Progresos.FirstOrDefault(p => p.IdUsuario == userId && p.IdCurso == i.IdCurso)
                })
                .ToListAsync();

            return View(misCursos);
        }

        // GET: Cursos/VerContenido/5
        public async Task<IActionResult> VerContenido(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Challenge();
            var userId = int.Parse(userIdStr);

            // Verificar inscripcion
            var inscripcion = await _context.Inscripciones
                .FirstOrDefaultAsync(i => i.IdUsuario == userId && i.IdCurso == id);

            if (inscripcion == null)
            {
                TempData["ErrorMessage"] = "Debes inscribirte en el curso para ver el contenido.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var curso = await _context.Cursos
                .FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null) return NotFound();

            var materiales = await _context.MaterialCurso
                .Where(m => m.IdCurso == id)
                .ToListAsync();

            ViewBag.Progreso = await _context.Progresos
                .FirstOrDefaultAsync(p => p.IdUsuario == userId && p.IdCurso == id);

            ViewData["Materiales"] = materiales;
            return View(curso);
        }

        // POST: Cursos/CompletarMaterial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarMaterial(int idCurso, int idMaterial)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Challenge();
            var userId = int.Parse(userIdStr);

            // 1. Verificar si ya se marco como completado
            var yaCompletado = await _context.MaterialesCompletados
                .AnyAsync(mc => mc.IdUsuario == userId && mc.IdMaterial == idMaterial);

            if (!yaCompletado)
            {
                var nuevoCompletado = new MaterialCompletado
                {
                    IdUsuario = userId,
                    IdMaterial = idMaterial
                };
                _context.MaterialesCompletados.Add(nuevoCompletado);
                await _context.SaveChangesAsync();
            }

            // 2. Recalcular progreso
            var totalMateriales = await _context.MaterialCurso.CountAsync(m => m.IdCurso == idCurso);
            if (totalMateriales > 0)
            {
                var materialesIds = await _context.MaterialCurso
                    .Where(m => m.IdCurso == idCurso)
                    .Select(m => m.IdMaterial)
                    .ToListAsync();

                var completadosCount = await _context.MaterialesCompletados
                    .CountAsync(mc => mc.IdUsuario == userId && materialesIds.Contains(mc.IdMaterial));

                var nuevoPorcentaje = (int)((double)completadosCount / totalMateriales * 100);

                var progreso = await _context.Progresos
                    .FirstOrDefaultAsync(p => p.IdUsuario == userId && p.IdCurso == idCurso);

                if (progreso != null)
                {
                    progreso.Porcentaje = nuevoPorcentaje;
                    progreso.UltimaActualizacion = DateTime.UtcNow;
                    _context.Update(progreso);
                    await _context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "¡Progreso actualizado!";
            return RedirectToAction(nameof(VerContenido), new { id = idCurso, materialId = idMaterial });
        }

        private bool CursoExists(int id)
        {
            return _context.Cursos.Any(e => e.IdCurso == id);
        }
    }
}
