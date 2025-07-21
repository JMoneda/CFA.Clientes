using System;
using System.Collections.Generic;

namespace CFA.Clientes.Application.Dtos
{
    public class ClienteCreateDto
    {
        public string TipoDocumento { get; set; } = string.Empty;
        public long NumeroDocumento { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellido1 { get; set; } = string.Empty;
        public string? Apellido2 { get; set; }
        public string Genero { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Email { get; set; } = string.Empty;
        public List<DireccionDto> Direcciones { get; set; } = new();
        public List<TelefonoDto> Telefonos { get; set; } = new();
    }
}
