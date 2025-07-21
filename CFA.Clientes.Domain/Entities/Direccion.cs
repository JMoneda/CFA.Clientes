

using System.Text.Json.Serialization;

namespace CFA.Clientes.Domain.Entities
{
    public class Direccion
    {
        public int DireccionId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        [JsonIgnore]
        public Cliente? Cliente { get; set; }
    }
}