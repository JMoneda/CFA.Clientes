namespace CFA.Clientes.Application.Dtos
{
    public class TelefonoDto
    {
        public int? TelefonoId { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }
}
