using LaboratoryService_Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace LaboratoryService_Api.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :base(options)
        {

        }

        public DbSet<LabRegistroDetalle> LabRegistroDetalles { get; set;}
        public DbSet<LaboratorioPracticas> LaboratorioPracticas { get; set;}
        public DbSet<LaboratorioRegistro> LaboratorioRegistro { get; set;}
        public DbSet<Pacientes> Pacientes { get; set;}
        public DbSet<Prestadores> Prestadores { get; set;}
        public DbSet<Instituciones> Instituciones { get; set;}
        public DbSet<Sexo> Sexo { get; set;}
        public DbSet<Internaciones> Internaciones { get; set;}

        //Relación sin necesidad de una FK en LaboratorioRegistro. Esto usa el potencial de EF sin usar consultas toscas
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LaboratorioRegistro>()
                .HasMany(l => l.LabRegistroDetalle)
                .WithOne(d => d.LaboratorioRegistro)
                .HasForeignKey(d => d.LaboratorioRegistroID)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
