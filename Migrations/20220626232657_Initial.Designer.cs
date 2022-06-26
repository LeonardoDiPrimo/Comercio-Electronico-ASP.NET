﻿// <auto-generated />
using ComercioElectronicoMvc.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ComercioElectronicoMvc.Migrations
{
    [DbContext(typeof(MercadoContext))]
    [Migration("20220626232657_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.11")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Carro", b =>
                {
                    b.Property<int>("carroId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<int>("usuarioId")
                        .HasColumnType("integer");

                    b.HasKey("carroId");

                    b.HasIndex("usuarioId")
                        .IsUnique();

                    b.ToTable("Carro");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Categoria", b =>
                {
                    b.Property<int>("categoriaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<bool>("deprecado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("nombre")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("categoriaId");

                    b.ToTable("Categoria");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Compra", b =>
                {
                    b.Property<int>("compraId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<double>("total")
                        .HasColumnType("double precision");

                    b.Property<int>("usuarioId")
                        .HasColumnType("integer");

                    b.HasKey("compraId");

                    b.HasIndex("usuarioId");

                    b.ToTable("Compra");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Producto", b =>
                {
                    b.Property<int>("productoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<int>("cantidad")
                        .HasColumnType("integer");

                    b.Property<int>("categoriaId")
                        .HasColumnType("integer");

                    b.Property<bool>("deprecado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("nombre")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("precio")
                        .HasColumnType("double precision");

                    b.HasKey("productoId");

                    b.HasIndex("categoriaId");

                    b.ToTable("Producto");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Rel_Carro_Compra", b =>
                {
                    b.Property<int>("compraId")
                        .HasColumnType("integer");

                    b.Property<int>("productoId")
                        .HasColumnType("integer");

                    b.Property<int>("cantidad")
                        .HasColumnType("integer");

                    b.Property<double>("precioProducto")
                        .HasColumnType("double precision");

                    b.HasKey("compraId", "productoId");

                    b.HasIndex("productoId");

                    b.ToTable("Rel_Carro_Compra");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Rel_Carro_Producto", b =>
                {
                    b.Property<int>("carroId")
                        .HasColumnType("integer");

                    b.Property<int>("productoId")
                        .HasColumnType("integer");

                    b.Property<int>("cantidad")
                        .HasColumnType("integer");

                    b.HasKey("carroId", "productoId");

                    b.HasIndex("productoId");

                    b.ToTable("Rel_Carro_Producto");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Usuario", b =>
                {
                    b.Property<int>("usuarioId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                    b.Property<string>("apellido")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("cuilCuit")
                        .HasColumnType("bigint");

                    b.Property<bool>("deprecado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<int>("dni")
                        .HasColumnType("integer");

                    b.Property<bool>("esAdministrador")
                        .HasColumnType("boolean");

                    b.Property<bool>("esBloqueado")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("esEmpresa")
                        .HasColumnType("boolean");

                    b.Property<string>("mail")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("nombre")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("reitentosBloqueo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.HasKey("usuarioId");

                    b.ToTable("Usuario");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Carro", b =>
                {
                    b.HasOne("ComercioElectronicoMvc.Models.Usuario", "usuario")
                        .WithOne("carro")
                        .HasForeignKey("ComercioElectronicoMvc.Models.Carro", "usuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("usuario");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Compra", b =>
                {
                    b.HasOne("ComercioElectronicoMvc.Models.Usuario", "usuario")
                        .WithMany()
                        .HasForeignKey("usuarioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("usuario");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Producto", b =>
                {
                    b.HasOne("ComercioElectronicoMvc.Models.Categoria", "categoria")
                        .WithMany("productos")
                        .HasForeignKey("categoriaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("categoria");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Rel_Carro_Compra", b =>
                {
                    b.HasOne("ComercioElectronicoMvc.Models.Compra", "compra")
                        .WithMany("Rel_Carro_Compras")
                        .HasForeignKey("compraId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ComercioElectronicoMvc.Models.Producto", "producto")
                        .WithMany("Rel_Carro_Compras")
                        .HasForeignKey("productoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("compra");

                    b.Navigation("producto");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Rel_Carro_Producto", b =>
                {
                    b.HasOne("ComercioElectronicoMvc.Models.Carro", "carro")
                        .WithMany("Rel_Carro_Productos")
                        .HasForeignKey("carroId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ComercioElectronicoMvc.Models.Producto", "producto")
                        .WithMany("Rel_Carro_Productos")
                        .HasForeignKey("productoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("carro");

                    b.Navigation("producto");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Carro", b =>
                {
                    b.Navigation("Rel_Carro_Productos");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Categoria", b =>
                {
                    b.Navigation("productos");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Compra", b =>
                {
                    b.Navigation("Rel_Carro_Compras");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Producto", b =>
                {
                    b.Navigation("Rel_Carro_Compras");

                    b.Navigation("Rel_Carro_Productos");
                });

            modelBuilder.Entity("ComercioElectronicoMvc.Models.Usuario", b =>
                {
                    b.Navigation("carro");
                });
#pragma warning restore 612, 618
        }
    }
}
