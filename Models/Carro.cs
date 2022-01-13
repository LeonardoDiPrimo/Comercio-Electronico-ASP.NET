using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ComercioElectronicoMvc.Models
{
    public class Carro
    {
        public int carroId { get; set; }

        public Usuario usuario { get; set; }

        public int usuarioId { get; set; }

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

        public List<Rel_Carro_Producto> Rel_Carro_Productos { get; set; }

        public Carro() { }

    }
}
