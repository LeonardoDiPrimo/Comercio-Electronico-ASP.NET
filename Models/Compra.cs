using System;
using System.Collections.Generic;
using System.Text;

namespace ComercioElectronicoMvc.Models
{
    public class Compra
    {
        public int compraId { get; set; }

        public int usuarioId { get; set; }

        public double total { get; set; }

        public ICollection<Producto> Productos { get; set; } = new List<Producto>();

        public List<Rel_Carro_Compra> Rel_Carro_Compras { get; set; }

        public Usuario usuario { get; set; }

        public Compra() { }
    }
}
