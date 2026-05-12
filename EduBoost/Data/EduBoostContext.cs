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
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<Progreso> Progresos { get; set; }
        public DbSet<MaterialCompletado> MaterialesCompletados { get; set; }
        public DbSet<Asesoria> Asesorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(u => u.IdUsuario);
                entity.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Correo).HasMaxLength(100).IsRequired();
                entity.Property(u => u.Password).HasMaxLength(255).IsRequired();
                entity.Property(u => u.Rol).HasMaxLength(50).IsRequired();
                entity.Property(u => u.FechaRegistro).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(u => u.Correo).IsUnique();
            });

            modelBuilder.Entity<Curso>(entity =>
            {
                entity.ToTable("Cursos");
                entity.HasKey(c => c.IdCurso);
                entity.Property(c => c.Nombre).HasMaxLength(200).IsRequired();
                entity.Property(c => c.Descripcion).HasMaxLength(1000).IsRequired();
                entity.Property(c => c.Profesor).HasMaxLength(150).IsRequired();
                entity.Property(c => c.FechaCreacion).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<MaterialCurso>(entity =>
            {
                entity.ToTable("MaterialCurso");
                entity.HasKey(m => m.IdMaterial);
                entity.Property(m => m.Titulo).HasMaxLength(200).IsRequired();
                entity.Property(m => m.TipoMaterial).HasMaxLength(50).IsRequired();
                entity.Property(m => m.UrlVideo).HasMaxLength(500);
                entity.HasOne<Curso>()
                    .WithMany()
                    .HasForeignKey(m => m.IdCurso)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Inscripcion>(entity =>
            {
                entity.ToTable("Inscripciones");
                entity.HasKey(i => i.IdInscripcion);
                entity.Property(i => i.FechaInscripcion).HasDefaultValueSql("GETDATE()");
                entity.HasOne(i => i.Usuario)
                    .WithMany()
                    .HasForeignKey(i => i.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.Curso)
                    .WithMany()
                    .HasForeignKey(i => i.IdCurso)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Progreso>(entity =>
            {
                entity.ToTable("Progreso");
                entity.HasKey(p => p.IdProgreso);
                entity.Property(p => p.Porcentaje).HasDefaultValue(0);
                entity.HasOne(p => p.Usuario)
                    .WithMany()
                    .HasForeignKey(p => p.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Curso)
                    .WithMany()
                    .HasForeignKey(p => p.IdCurso)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MaterialCompletado>(entity =>
            {
                entity.ToTable("MaterialesCompletados");
                entity.HasKey(mc => mc.Id);
                entity.Property(mc => mc.FechaCompletado).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Asesoria>(entity =>
            {
                entity.ToTable("Asesorias");
                entity.HasKey(a => a.IdAsesoria);
                entity.Property(a => a.Estado).HasDefaultValue("Pendiente");
                entity.HasOne(a => a.Estudiante)
                    .WithMany()
                    .HasForeignKey(a => a.IdEstudiante)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(a => a.Asesor)
                    .WithMany()
                    .HasForeignKey(a => a.IdAsesor)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(a => a.Curso)
                    .WithMany()
                    .HasForeignKey(a => a.IdCurso)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
