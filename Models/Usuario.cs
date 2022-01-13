using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ComercioElectronicoMvc.Models
{
    public class Usuario
    {
        public int usuarioId { get; set; }

        [Required]
        [DisplayName("DNI")]
        [Range(1, 99999999, ErrorMessage = "El DNI debe ser mayor a 0")]
        public int dni { get; set; }

        [Required]
        [DisplayName("CUIL/CUIT")]
        [Range(1, 99999999999, ErrorMessage = "El CUIL o CUIT debe ser mayor a 0")]
        public Int64 cuilCuit  { get; set; }

        [Required]
        [DisplayName("Nombre")]
        public String nombre { get; set; }

        [Required]
        [DisplayName("Apellido")]
        public String apellido { get; set; }

        [Required]
        [DisplayName("Mail")]
        public String mail { get; set; }

        [Required]
        [DisplayName("Contraseña")]
        public String password { get; set; }

        [Required]
        [DisplayName("Administrador")]
        public bool esAdministrador { get; set; }

        [Required]
        [DisplayName("Empresa")]
        public bool esEmpresa { get; set; }

        [DisplayName("Eliminado")]
        public bool deprecado { get; set; }

        [DisplayName("Bloqueado")]
        public bool esBloqueado { get; set; }

        public int reitentosBloqueo { get; set; }

        public Carro carro { get; set; }

        public Usuario() { }
    }
}
