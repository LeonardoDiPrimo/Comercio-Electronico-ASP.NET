using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ComercioElectronicoMvc.Models;

namespace ComercioElectronicoMvc.Data
{
    public class MercadoContext : DbContext
    {
        public MercadoContext(DbContextOptions<MercadoContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categoria { get; set; }

        public DbSet<Producto> Producto { get; set; }

        public DbSet<Usuario> Usuario { get; set; }

        public DbSet<Carro> Carro { get; set; }

        public DbSet<Compra> Compra { get; set; }

        public const String loggedInUserIdKey = "loggedInUserId";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();

            modelBuilder.Entity<Categoria>(cat =>
            {
                cat.ToTable("Categoria").HasKey(c => c.categoriaId); ;
            });

            modelBuilder.Entity<Categoria>(
                cat =>
                {
                    cat.Property(c => c.categoriaId).IsRequired(true);
                    cat.Property(c => c.nombre).IsRequired(true);
                    cat.Property(c => c.deprecado).IsRequired(true);
                    cat.Property(c => c.deprecado).HasDefaultValue(false);
                });

            modelBuilder.Entity<Producto>(prod =>
            {
                prod.ToTable("Producto");
                prod.HasOne(p => p.categoria).WithMany(c => c.productos).HasForeignKey(p => p.categoriaId);
            });

            modelBuilder.Entity<Producto>(
               prod =>
               {
                   prod.Property(p => p.productoId).IsRequired(true);
                   prod.Property(p => p.nombre).IsRequired(true);
                   prod.Property(p => p.precio).IsRequired(true);
                   prod.Property(p => p.cantidad).IsRequired(true);
                   prod.Property(p => p.deprecado).IsRequired(true);
                   prod.Property(p => p.deprecado).HasDefaultValue(false);
               });

            modelBuilder.Entity<Usuario>(user =>
                {
                    user.ToTable("Usuario").HasKey(u => u.usuarioId);
                    user.HasOne(u => u.carro).WithOne(c => c.usuario).HasForeignKey<Carro>(c => c.usuarioId);
                });

            modelBuilder.Entity<Usuario>(
                    usr =>
                    {
                        usr.Property(u => u.usuarioId).IsRequired(true);
                        usr.Property(u => u.dni).IsRequired(true);
                        usr.Property(u => u.cuilCuit).IsRequired(true);
                        usr.Property(u => u.nombre).IsRequired(true);
                        usr.Property(u => u.apellido).IsRequired(true);
                        usr.Property(u => u.mail).IsRequired(true);
                        usr.Property(u => u.password).IsRequired(true);
                        usr.Property(u => u.esAdministrador).IsRequired(true);
                        usr.Property(u => u.esEmpresa).IsRequired(true);
                        usr.Property(u => u.deprecado).IsRequired(true);
                        usr.Property(u => u.deprecado).HasDefaultValue(false);
                        usr.Property(u => u.esBloqueado).IsRequired(true);
                        usr.Property(u => u.esBloqueado).HasDefaultValue(false);
                        usr.Property(u => u.reitentosBloqueo).IsRequired(true);
                        usr.Property(u => u.reitentosBloqueo).HasDefaultValue(0);
                    });

            modelBuilder.Entity<Carro>(
                   carro =>
                   {
                       carro.ToTable("Carro");
                       carro.HasKey(c => c.carroId);
                       carro.HasOne(c => c.usuario).WithOne(u => u.carro).HasForeignKey<Carro>(c => c.usuarioId);
                       carro.Property(c => c.carroId).IsRequired(true);
                       carro.Property(c => c.usuarioId).IsRequired(true);
                   });

            modelBuilder.Entity<Compra>(compra =>
            {
                compra.ToTable("Compra");
                compra.HasKey(c => c.compraId);
                compra.Property(c => c.total).IsRequired(true);
            });

            //DEFINICIÓN DE LA RELACIÓN MANY TO MANY Carro <-> Producto
            modelBuilder.Entity<Carro>()
                .HasMany(c => c.Productos)
                .WithMany(p => p.Carros)
                .UsingEntity<Rel_Carro_Producto>(
                    rcp => rcp.HasOne(cp => cp.producto).WithMany(p => p.Rel_Carro_Productos).HasForeignKey(rcp => rcp.productoId),
                    rcp => rcp.HasOne(cp => cp.carro).WithMany(c => c.Rel_Carro_Productos).HasForeignKey(rcp => rcp.carroId),
                    rcp => rcp.HasKey(k => new { k.carroId, k.productoId })
            );

            //DEFINICIÓN DE LA RELACIÓN MANY TO MANY Compra <-> Producto
            modelBuilder.Entity<Compra>()
                .HasMany(c => c.Productos)
                .WithMany(p => p.Compras)
                .UsingEntity<Rel_Carro_Compra>(
                    rcc => rcc.HasOne(cc => cc.producto).WithMany(p => p.Rel_Carro_Compras).HasForeignKey(rcc => rcc.productoId),
                    rcc => rcc.HasOne(cc => cc.compra).WithMany(c => c.Rel_Carro_Compras).HasForeignKey(rcc => rcc.compraId),
                    rcc => rcc.HasKey(k => new { k.compraId, k.productoId })
            );

            modelBuilder.Ignore<ErrorViewModel>();
        }
    }
}
