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
            if (id == null) return NotFound();

            var curso = await _context.Cursos
                .Include(c => c.Materiales)
                .FirstOrDefaultAsync(m => m.IdCurso == id);
            
            if (curso == null) return NotFound();

            // Buscar la evaluación del curso
            var evaluacion = await _context.Evaluaciones.FirstOrDefaultAsync(e => e.IdCurso == id);
            ViewBag.Evaluacion = evaluacion;

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr != null)
            {
                var userId = int.Parse(userIdStr);
                ViewBag.EstaInscrito = await _context.Inscripciones
                    .AnyAsync(i => i.IdUsuario == userId && i.IdCurso == id);
                
                // Verificar si es el dueño
                ViewBag.EsDuenio = curso.IdUsuarioAsesor == userId;

                // Si hay evaluación, ver si ya la rindió
                if (evaluacion != null)
                {
                    ViewBag.ResultadoEvaluacion = await _context.ResultadosEvaluacion
                        .FirstOrDefaultAsync(r => r.IdUsuario == userId && r.IdEvaluacion == evaluacion.IdEvaluacion);
                }
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
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion")] Curso curso)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Challenge();

            if (ModelState.IsValid)
            {
                var userId = int.Parse(userIdStr);
                var userName = User.Identity?.Name;

                curso.IdUsuarioAsesor = userId;
                curso.Profesor = userName; 
                curso.FechaCreacion = DateTime.UtcNow;
                
                _context.Add(curso);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Curso creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Cursos/Edit/5
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (curso.IdUsuarioAsesor != userId && !User.IsInRole("Administrador"))
            {
                return Forbid();
            }

            return View(curso);
        }

        // POST: Cursos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Edit(int id, [Bind("IdCurso,Nombre,Descripcion,Profesor,IdUsuarioAsesor,FechaCreacion")] Curso curso)
        {
            if (id != curso.IdCurso) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(curso);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.IdCurso)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(curso);
        }

        // GET: Cursos/Delete/5
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var curso = await _context.Cursos.FirstOrDefaultAsync(m => m.IdCurso == id);
            if (curso == null) return NotFound();

            return View(curso);
        }

        // POST: Cursos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso != null) _context.Cursos.Remove(curso);

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

            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            if (curso.IdUsuarioAsesor == userId)
            {
                TempData["ErrorMessage"] = "Eres el asesor de este curso, no puedes inscribirte como alumno.";
                return RedirectToAction(nameof(Details), new { id });
            }
            
            var yaInscrito = await _context.Inscripciones.AnyAsync(i => i.IdUsuario == userId && i.IdCurso == id);
            if (yaInscrito)
            {
                TempData["ErrorMessage"] = "Ya estás inscrito en este curso.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Inscripciones.Add(new Inscripcion { IdUsuario = userId, IdCurso = id, FechaInscripcion = DateTime.UtcNow });
            _context.Progresos.Add(new Progreso { IdUsuario = userId, IdCurso = id, Porcentaje = 0, UltimaActualizacion = DateTime.UtcNow });
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "¡Te has inscrito correctamente!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Cursos/AgregarMaterial
        [HttpPost]
        [Authorize(Roles = "Administrador,Asesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarMaterial(int idCurso, string titulo, string tipo, string url)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var curso = await _context.Cursos.FindAsync(idCurso);
            
            if (curso == null) return NotFound();
            if (curso.IdUsuarioAsesor != userId && !User.IsInRole("Administrador")) return Forbid();

            var material = new MaterialCurso
            {
                IdCurso = idCurso,
                Titulo = titulo,
                TipoMaterial = tipo,
                UrlVideo = url
            };

            _context.MaterialCurso.Add(material);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Material añadido correctamente.";
            return RedirectToAction(nameof(Details), new { id = idCurso });
        }

        // POST: Cursos/EliminarMaterial
        [HttpPost]
        [Authorize(Roles = "Administrador,Asesor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMaterial(int id)
        {
            var material = await _context.MaterialCurso.Include(m => m.Curso).FirstOrDefaultAsync(m => m.IdMaterial == id);
            if (material == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (material.Curso?.IdUsuarioAsesor != userId && !User.IsInRole("Administrador")) return Forbid();

            var idCurso = material.IdCurso;
            _context.MaterialCurso.Remove(material);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Material eliminado.";
            return RedirectToAction(nameof(Details), new { id = idCurso });
        }

        // GET: Cursos/MisCursos
        public async Task<IActionResult> MisCursos()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Challenge();
            var userId = int.Parse(userIdStr);

            // Cursos en los que estoy inscrito (como alumno)
            var misCursos = await _context.Inscripciones
                .Include(i => i.Curso)
                .Where(i => i.IdUsuario == userId)
                .Select(i => new MisCursosViewModel
                {
                    Curso = i.Curso,
                    Progreso = _context.Progresos.FirstOrDefault(p => p.IdUsuario == userId && p.IdCurso == i.IdCurso)
                })
                .ToListAsync();

            // Cursos que yo administro (como asesor)
            ViewBag.CursosAdministrados = await _context.Cursos
                .Where(c => c.IdUsuarioAsesor == userId)
                .ToListAsync();

            return View(misCursos);
        }

        public async Task<IActionResult> VerContenido(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Challenge();
            var userId = int.Parse(userIdStr);

            var inscripcion = await _context.Inscripciones.FirstOrDefaultAsync(i => i.IdUsuario == userId && i.IdCurso == id);
            var curso = await _context.Cursos.Include(c => c.Materiales).FirstOrDefaultAsync(c => c.IdCurso == id);

            if (curso == null) return NotFound();

            if (inscripcion == null && curso.IdUsuarioAsesor != userId)
            {
                TempData["ErrorMessage"] = "Debes inscribirte en el curso para ver el contenido.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Progreso = await _context.Progresos.FirstOrDefaultAsync(p => p.IdUsuario == userId && p.IdCurso == id);
            ViewBag.MaterialesCompletados = await _context.MaterialesCompletados.Where(mc => mc.IdUsuario == userId).Select(mc => mc.IdMaterial).ToListAsync();
            ViewData["Materiales"] = curso.Materiales.ToList();
            
            return View(curso);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompletarMaterial(int idCurso, int idMaterial)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return Challenge();
            var userId = int.Parse(userIdStr);

            var yaCompletado = await _context.MaterialesCompletados.AnyAsync(mc => mc.IdUsuario == userId && mc.IdMaterial == idMaterial);
            if (!yaCompletado)
            {
                _context.MaterialesCompletados.Add(new MaterialCompletado { IdUsuario = userId, IdMaterial = idMaterial, FechaCompletado = DateTime.UtcNow });
                await _context.SaveChangesAsync();
            }

            var totalMateriales = await _context.MaterialCurso.CountAsync(m => m.IdCurso == idCurso);
            if (totalMateriales > 0)
            {
                var materialesIds = await _context.MaterialCurso.Where(m => m.IdCurso == idCurso).Select(m => m.IdMaterial).ToListAsync();
                var completadosCount = await _context.MaterialesCompletados.CountAsync(mc => mc.IdUsuario == userId && materialesIds.Contains(mc.IdMaterial));
                var nuevoPorcentaje = (int)((double)completadosCount / totalMateriales * 100);

                var progreso = await _context.Progresos.FirstOrDefaultAsync(p => p.IdUsuario == userId && p.IdCurso == idCurso);
                if (progreso != null)
                {
                    progreso.Porcentaje = nuevoPorcentaje;
                    progreso.UltimaActualizacion = DateTime.UtcNow;
                    _context.Update(progreso);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(VerContenido), new { id = idCurso });
        }

        private bool CursoExists(int id) => _context.Cursos.Any(e => e.IdCurso == id);
    }
}
