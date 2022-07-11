using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ComercioElectronicoMvc.Models
{
    public class Producto
    {
        public int productoId { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayName("Nombre")]
        public String nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Range(1, 100000.00, ErrorMessage = "El precio debe estar entre 1 y 100000")]
        [DisplayName("Precio")]
        public double precio { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [Range(1, 1000, ErrorMessage = "La cantidad debe estar entre 1 y 1000")]
        [DisplayName("Cantidad")]
        public int cantidad { get; set; }

        [DisplayName("Eliminado")]
        public bool deprecado { get; set; }

        [DisplayName("Categoria")]
        public Categoria categoria { get; set; }

        [Required]
        [DisplayName("Categoria")]
        public int categoriaId { get; set; }

        public ICollection<Carro> Carros { get; set; } = new List<Carro>();

        public List<Rel_Carro_Producto> Rel_Carro_Productos { get; set; }

        public ICollection<Compra> Compras  { get; set; } = new List<Compra>();

        public List<Rel_Carro_Compra> Rel_Carro_Compras { get; set; }

        public Producto() {}
    }
}
