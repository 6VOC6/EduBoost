using EduBoost.Data;
using EduBoost.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduBoost.Controllers
{
    [Authorize]
    public class EvaluacionesController : Controller
    {
        private readonly EduBoostContext _context;

        public EvaluacionesController(EduBoostContext context)
        {
            _context = context;
        }

        // GET: Evaluaciones/Create/5 (id = idCurso)
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Create(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (curso.IdUsuarioAsesor != userId && !User.IsInRole("Administrador")) return Forbid();

            ViewBag.Curso = curso;
            return View();
        }

        // POST: Evaluaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Create([Bind("IdCurso,Titulo,Descripcion")] Evaluacion evaluacion)
        {
            if (ModelState.IsValid)
            {
                evaluacion.FechaCreacion = DateTime.UtcNow;
                _context.Add(evaluacion);
                await _context.SaveChangesAsync();
                return RedirectToAction("Gestionar", new { id = evaluacion.IdEvaluacion });
            }
            return View(evaluacion);
        }

        // GET: Evaluaciones/Gestionar/5
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> Gestionar(int id)
        {
            var evaluacion = await _context.Evaluaciones
                .Include(e => e.Curso)
                .Include(e => e.Preguntas)
                .FirstOrDefaultAsync(e => e.IdEvaluacion == id);

            if (evaluacion == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (evaluacion.Curso?.IdUsuarioAsesor != userId && !User.IsInRole("Administrador")) return Forbid();

            return View(evaluacion);
        }

        // POST: Evaluaciones/AgregarPregunta
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Asesor")]
        public async Task<IActionResult> AgregarPregunta(int idEvaluacion, string enunciado, string a, string b, string c, string correcta)
        {
            var evaluacion = await _context.Evaluaciones.Include(e => e.Curso).FirstOrDefaultAsync(e => e.IdEvaluacion == idEvaluacion);
            if (evaluacion == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (evaluacion.Curso?.IdUsuarioAsesor != userId && !User.IsInRole("Administrador")) return Forbid();

            var pregunta = new Pregunta
            {
                IdEvaluacion = idEvaluacion,
                Enunciado = enunciado,
                OpcionA = a,
                OpcionB = b,
                OpcionC = c,
                RespuestaCorrecta = correcta
            };

            _context.Preguntas.Add(pregunta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Gestionar), new { id = idEvaluacion });
        }

        // GET: Evaluaciones/Rendir/5
        public async Task<IActionResult> Rendir(int id)
        {
            var evaluacion = await _context.Evaluaciones
                .Include(e => e.Curso)
                .Include(e => e.Preguntas)
                .FirstOrDefaultAsync(e => e.IdEvaluacion == id);

            if (evaluacion == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            // Verificar si ya la rindio
            var resultadoPrevio = await _context.ResultadosEvaluacion
                .FirstOrDefaultAsync(r => r.IdUsuario == userId && r.IdEvaluacion == id);

            if (resultadoPrevio != null)
            {
                return RedirectToAction(nameof(Resultado), new { id = resultadoPrevio.IdResultado });
            }

            return View(evaluacion);
        }

        // POST: Evaluaciones/Calificar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calificar(int idEvaluacion, IFormCollection form)
        {
            var evaluacion = await _context.Evaluaciones.Include(e => e.Preguntas).FirstOrDefaultAsync(e => e.IdEvaluacion == idEvaluacion);
            if (evaluacion == null) return NotFound();

            int totalPreguntas = evaluacion.Preguntas.Count;
            if (totalPreguntas == 0) return BadRequest("La evaluación no tiene preguntas.");

            int aciertos = 0;
            foreach (var p in evaluacion.Preguntas)
            {
                string respuestaUsuario = form["p_" + p.IdPregunta].ToString();
                if (respuestaUsuario == p.RespuestaCorrecta)
                {
                    aciertos++;
                }
            }

            decimal calificacion = (decimal)aciertos / totalPreguntas * 10;
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var resultado = new ResultadoEvaluacion
            {
                IdUsuario = userId,
                IdEvaluacion = idEvaluacion,
                Calificacion = calificacion,
                FechaCompletado = DateTime.UtcNow
            };

            _context.ResultadosEvaluacion.Add(resultado);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Resultado), new { id = resultado.IdResultado });
        }

        // GET: Evaluaciones/Resultado/5
        public async Task<IActionResult> Resultado(int id)
        {
            var resultado = await _context.ResultadosEvaluacion
                .Include(r => r.Evaluacion)
                .ThenInclude(e => e.Curso)
                .FirstOrDefaultAsync(r => r.IdResultado == id);

            if (resultado == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (resultado.IdUsuario != userId && !User.IsInRole("Administrador") && !User.IsInRole("Asesor")) return Forbid();

            return View(resultado);
        }
    }
}
