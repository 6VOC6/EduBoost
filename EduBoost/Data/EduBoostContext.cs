namespace EduBoost.Data
{
    using EduBoost.Models;
    using Microsoft.EntityFrameworkCore;

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
        public DbSet<Evaluacion> Evaluaciones { get; set; }
        public DbSet<Pregunta> Preguntas { get; set; }
        public DbSet<ResultadoEvaluacion> ResultadosEvaluacion { get; set; }

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
                entity.Property(c => c.Nombre).HasMaxLength(150).IsRequired();
                entity.Property(c => c.Descripcion).HasMaxLength(500);
                entity.Property(c => c.Profesor).HasMaxLength(100);
                entity.Property(c => c.FechaCreacion).HasDefaultValueSql("GETDATE()");
                entity.HasOne(c => c.Asesor).WithMany().HasForeignKey(c => c.IdUsuarioAsesor).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MaterialCurso>(entity =>
            {
                entity.ToTable("MaterialCurso");
                entity.HasKey(m => m.IdMaterial);
                entity.Property(m => m.Titulo).HasMaxLength(200).IsRequired();
                entity.Property(m => m.TipoMaterial).HasMaxLength(50).IsRequired();
                entity.Property(m => m.UrlVideo).HasMaxLength(500);
                entity.HasOne(m => m.Curso).WithMany(c => c.Materiales).HasForeignKey(m => m.IdCurso).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Inscripcion>(entity =>
            {
                entity.ToTable("Inscripciones");
                entity.HasKey(i => i.IdInscripcion);
                entity.Property(i => i.FechaInscripcion).HasDefaultValueSql("GETDATE()");
                entity.HasOne(i => i.Usuario).WithMany().HasForeignKey(i => i.IdUsuario).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(i => i.Curso).WithMany().HasForeignKey(i => i.IdCurso).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Progreso>(entity =>
            {
                entity.ToTable("Progreso");
                entity.HasKey(p => p.IdProgreso);
                entity.Property(p => p.Porcentaje).HasDefaultValue(0);
                entity.HasOne(p => p.Usuario).WithMany().HasForeignKey(p => p.IdUsuario).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(p => p.Curso).WithMany().HasForeignKey(p => p.IdCurso).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<MaterialCompletado>(entity =>
            {
                entity.ToTable("MaterialesCompletados");
                entity.HasKey(mc => mc.Id);
                entity.Property(mc => mc.FechaCompletado).HasDefaultValueSql("GETDATE()");
                entity.HasOne<Usuario>().WithMany().HasForeignKey(mc => mc.IdUsuario).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Asesoria>(entity =>
            {
                entity.ToTable("Asesorias");
                entity.HasKey(a => a.IdAsesoria);
                entity.Property(a => a.Estado).HasDefaultValue("Pendiente");
                entity.HasOne(a => a.Estudiante).WithMany().HasForeignKey(a => a.IdEstudiante).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(a => a.Asesor).WithMany().HasForeignKey(a => a.IdAsesor).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(a => a.Curso).WithMany().HasForeignKey(a => a.IdCurso).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Evaluacion>(entity =>
            {
                entity.ToTable("Evaluaciones");
                entity.HasKey(e => e.IdEvaluacion);
                entity.Property(e => e.Titulo).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Curso).WithMany().HasForeignKey(e => e.IdCurso).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Pregunta>(entity =>
            {
                entity.ToTable("Preguntas");
                entity.HasKey(p => p.IdPregunta);
                entity.Property(p => p.Enunciado).IsRequired();
                entity.Property(p => p.OpcionA).HasMaxLength(255).IsRequired();
                entity.Property(p => p.OpcionB).HasMaxLength(255).IsRequired();
                entity.Property(p => p.OpcionC).HasMaxLength(255).IsRequired();
                entity.Property(p => p.RespuestaCorrecta).HasMaxLength(1).IsRequired();
                entity.HasOne(p => p.Evaluacion).WithMany(e => e.Preguntas).HasForeignKey(p => p.IdEvaluacion).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ResultadoEvaluacion>(entity =>
            {
                entity.ToTable("ResultadosEvaluacion");
                entity.HasKey(re => re.IdResultado);
                entity.Property(re => re.Calificacion).HasColumnType("decimal(5,2)").IsRequired();
                entity.Property(re => re.FechaCompletado).HasDefaultValueSql("GETDATE()");
                entity.HasOne(re => re.Usuario).WithMany().HasForeignKey(re => re.IdUsuario).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(re => re.Evaluacion).WithMany().HasForeignKey(re => re.IdEvaluacion).OnDelete(DeleteBehavior.Cascade);
            });

            // Configurar filtros de consulta global para Soft Delete
            modelBuilder.Entity<Usuario>().HasQueryFilter(u => u.Activo);
            modelBuilder.Entity<Curso>().HasQueryFilter(c => c.Activo);
            modelBuilder.Entity<MaterialCurso>().HasQueryFilter(m => m.Activo);
            modelBuilder.Entity<Asesoria>().HasQueryFilter(a => a.Activo);
            modelBuilder.Entity<Evaluacion>().HasQueryFilter(e => e.Activo);
            modelBuilder.Entity<Pregunta>().HasQueryFilter(p => p.Activo);
        }

        public override int SaveChanges()
        {
            ApplySoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplySoftDelete()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Deleted)
                {
                    // Comprobar si la entidad tiene la propiedad 'Activo'
                    var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Activo");
                    if (property != null)
                    {
                        entry.State = EntityState.Modified;
                        property.CurrentValue = false;
                    }
                }
            }
        }
    }
}
