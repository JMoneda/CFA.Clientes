

using System.Text.Json.Serialization;

namespace CFA.Clientes.Domain.Entities
{
    public class Telefono
    {
        public int TelefonoId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        [JsonIgnore]
        public Cliente? Cliente { get; set; }
    }
}
