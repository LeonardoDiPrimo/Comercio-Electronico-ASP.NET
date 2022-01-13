using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ComercioElectronicoMvc.Models
{
    public class Categoria
    {

        public int categoriaId { get; set; }

        [Required]
        [DisplayName("Nombre")]
        public string nombre { get; set; }

        [DisplayName("Eliminado")]
        public bool deprecado { get; set; }

        public List<Producto> productos { get; } = new List<Producto>();

        public Categoria() { }
    }
}
