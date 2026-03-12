namespace EduBoost.Data
{
    using EduBoost.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;

    public class EduBoostContext : DbContext
    {
        public EduBoostContext(DbContextOptions<EduBoostContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Curso> Cursos { get; set; }

        public DbSet<MaterialCurso> MaterialCurso { get; set; }
    }
}
