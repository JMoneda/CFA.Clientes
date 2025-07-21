namespace CFA.Clientes.Domain.Entities
{
    public class Cliente
    {
        public int ClienteId { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public long NumeroDocumento { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellido1 { get; set; } = string.Empty;
        public string? Apellido2 { get; set; }
        public string Genero { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Email { get; set; } = string.Empty;

        public List<Direccion> Direcciones { get; set; } = new();
        public List<Telefono> Telefonos { get; set; } = new();
    }
}
