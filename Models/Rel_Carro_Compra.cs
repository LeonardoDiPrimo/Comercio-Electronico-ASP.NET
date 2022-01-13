using System;
using System.Collections.Generic;
using System.Text;

namespace ComercioElectronicoMvc.Models
{
    public class Rel_Carro_Compra
    {
        public Compra compra { get; set; }

        public int compraId { get; set; }

        public Producto producto { get; set; }

        public int productoId { get; set; }

        public int cantidad { get; set; }

        public double precioProducto { get; set; }

        public Rel_Carro_Compra() { }
    }
}
