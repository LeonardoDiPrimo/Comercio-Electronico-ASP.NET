using ComercioElectronicoMvc.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace ComercioElectronicoMvc.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new MercadoContext(
                 serviceProvider.GetRequiredService<
                     DbContextOptions<MercadoContext>>()))
            {
                //  Buscar cualquier usuario.
                if (context.Usuario.Any())
                {
                    return;   // DB con registros
                }

                context.Usuario.AddRange(
                    new Usuario
                    {
                        dni = 40748616,
                        cuilCuit = 20407486162L,
                        nombre = "Roberto Leonardo",
                        apellido = "Di Primo",
                        mail = "leodiprimo@gmail.com",
                        password = Utilities.GetStringSha256Hash("1234567"),
                        esAdministrador = true,
                        esEmpresa = false,
                        deprecado = false,
                        carro = new Carro()
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
