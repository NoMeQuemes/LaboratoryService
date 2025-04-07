using LaboratoryService_Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace LaboratoryService_Api.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<LabRegistroDetalle> LabRegistroDetalles { get; set; }
        public DbSet<LaboratorioPracticas> LaboratorioPracticas { get; set; }
        public DbSet<LaboratorioRegistro> LaboratorioRegistro { get; set; }
        public DbSet<LabGrupoPractica> LabGrupoPracticas { get; set; }
        public DbSet<Pacientes> Pacientes { get; set; }
        public DbSet<Prestadores> Prestadores { get; set; }
        public DbSet<Instituciones> Instituciones { get; set; }
        public DbSet<Sexo> Sexo { get; set; }
        public DbSet<Internaciones> Internaciones { get; set; }
        public DbSet<Turnos> Turnos { get; set; }
        public DbSet<Habitaciones_Hospital> Habitaciones_Hospital { get; set; }
        public DbSet<Servicios> Servicios { get; set; }
        public DbSet<LabRegistroXEstado> LabRegistroXEstados { get; set; }

        //Relación sin necesidad de una FK en LaboratorioRegistro. Esto usa el potencial de EF sin usar consultas toscas
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LaboratorioRegistro>()
                .HasMany(l => l.LabRegistroDetalle)
                .WithOne(d => d.LaboratorioRegistro)
                .HasForeignKey(d => d.LaboratorioRegistroID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar la relación entre LaboratorioRegistro y LabRegistroXEstado 
            modelBuilder.Entity<LaboratorioRegistro>()
                .HasMany(l => l.LabRegistroXEstado)
                .WithOne(d => d.LaboratorioRegistro)
                .HasForeignKey(d => d.LaboratorioRegistroID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar la relación entre LabGrupoPractica y LaboratorioPracticas (grupo)
            modelBuilder.Entity<LabGrupoPractica>()
                .HasOne(lgp => lgp.LaboratorioPracticasGrup)
                .WithMany()
                .HasForeignKey(lgp => lgp.LaboratorioPracticasIDGrupo)
                .OnDelete(DeleteBehavior.Restrict); // O el comportamiento que prefieras

            //// Configurar la relación entre LabGrupoPractica y LaboratorioPracticas (práctica)
            //modelBuilder.Entity<LabGrupoPractica>()
            //    .HasOne(lgp => lgp.LaboratorioPracticasPrac)
            //    .WithMany()
            //    .HasForeignKey(lgp => lgp.LaboratorioPracticasID)
            //    .OnDelete(DeleteBehavior.Restrict); // O el comportamiento que prefieras
        }


    }
}