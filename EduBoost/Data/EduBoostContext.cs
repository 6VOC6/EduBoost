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
        }
    }
}
