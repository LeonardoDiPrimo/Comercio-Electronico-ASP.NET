using System;
using System.Collections.Generic;
using System.Text;

namespace ComercioElectronicoMvc.Models
{
    public class Rel_Carro_Producto
    {
        public Carro carro { get; set; }

        public int carroId { get; set; }

        public Producto producto { get; set; }

        public int productoId { get; set; }

        public int cantidad { get; set; }

        public Rel_Carro_Producto() { }

        public Rel_Carro_Producto(int carroId, int productoId, int cantidad) {
            this.carroId = carroId;
            this.productoId = productoId;
            this.cantidad = cantidad;
        }
    }
}
