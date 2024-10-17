using boardCtrl.Models;
using Microsoft.EntityFrameworkCore;

namespace boardCtrl.DATA
{
    // Contexto de la base de datos que hereda de DbContext
    public class boardCtrlContext : DbContext
    {
        // Constructor que acepta opciones de configuracion para el contexto de base de datos
        public boardCtrlContext(DbContextOptions<boardCtrlContext> options) : base(options)
        {
        }
        // Configuracion del modelo utilizando el ModelBuilder
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configura la entidad Role
            modelBuilder.Entity<Role>()
                .HasKey(r => r.roleId); // Define RoleId como la clave primaria de Role

            // Configura la entidad User
            modelBuilder.Entity<User>()
                .HasKey(u => u.userId); // Define UserId como la clave primaria de User

            // Configura la relación uno-a-muchos entre Role y User
            modelBuilder.Entity<Role>()
                .HasMany(r => r.Users) // Un Role tiene muchos Users
                .WithOne(u => u.Role) // Cada User tiene un Role
                .HasForeignKey(u => u.roleId); // La clave foranea en User que hace referencia a Role

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role) // Un User tiene un Role
                .WithMany(r => r.Users) // Un Role tiene muchos Users
                .HasForeignKey(u => u.roleId); // La clave foranea en User que hace referencia a Role

            // Configura los campos opcionales en User
            modelBuilder.Entity<User>()
                .Property(u => u.createdUserBy)
                .IsRequired(false); // Permite valores nulos

            modelBuilder.Entity<User>()
                .Property(u => u.editedUserBy)
                .IsRequired(false); // Permite valores nulos
        }
        /* Configura la cadena de conexion a la base de datos SQL Server
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=SantiagoOsuna;Database=BoardsCtrl;Trusted_Connection=true;TrustServerCertificate=true",
                sqlServerOptions => sqlServerOptions
                    .EnableRetryOnFailure()); // Habilita reintentos en caso de fallo de conexion
        }
        */
        // Define los DbSet para cada tabla en la base de datos
        public DbSet<Role> Roles { get; set; } // Representa la tabla Roles
        public DbSet<User> Users { get; set; } // Representa la tabla Users
        public DbSet<Category> Categories { get; set; } // Representa la tabla Categories
        public DbSet<Board> Boards { get; set; } // Representa la tabla Boards
        public DbSet<Slide> Slides { get; set; } // Representa la tabla Slides
    }
}