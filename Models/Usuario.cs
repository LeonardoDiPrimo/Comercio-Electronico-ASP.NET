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

        [Required(ErrorMessage = "El campo DNI es obligatorio.")]
        [DisplayName("DNI")]
        [Range(1000000, 99999999, ErrorMessage = "El DNI debe ingresado debe contener entre 7 y 8 digitos.")]
        public int dni { get; set; }

        [Required(ErrorMessage = "El campo CUIL/CUIT es obligatorio.")]
        [DisplayName("CUIL/CUIT")]
        [Range(10000000000, 99999999999, ErrorMessage = "El CUIL o CUIT ingresado debe contener 11 digitos exactos.")]
        public Int64 cuilCuit  { get; set; }

        [Required(ErrorMessage = "El campo Nombre es obligatorio.")]
        [DisplayName("Nombre")]
        public String nombre { get; set; }

        [Required(ErrorMessage = "El campo Apellido es obligatorio.")]
        [DisplayName("Apellido")]
        public String apellido { get; set; }

        [Required(ErrorMessage = "El campo Email es obligatorio.")]
        [DisplayName("Email")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*",
            ErrorMessage = "Dirección de Correo electrónico incorrecta.")]
        [StringLength(100, ErrorMessage = "Longitud máxima 100")]
        public String mail { get; set; }

        [Required(ErrorMessage = "El campo Contraseña es obligatorio.")]
        [DisplayName("Contraseña")]
        [StringLength(20, ErrorMessage = "La longitud del campo {0} debe estar entre {2} y {1} digitos.", MinimumLength = 7)]
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
