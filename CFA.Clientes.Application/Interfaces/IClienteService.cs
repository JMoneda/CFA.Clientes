using CFA.Clientes.Application.Dtos;
using CFA.Clientes.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CFA.Clientes.Application.Interfaces
{
    public interface IClienteService
    {
        Task<(bool Success, Cliente? Cliente, string? Message)> RegistrarClienteAsync(ClienteCreateDto dto);
        Task<(bool Success, Cliente? Cliente, string? Message)> ActualizarClienteAsync(int clienteId, ClienteUpdateDto dto);
        Task<(bool Success, Cliente? Cliente, string? Message)> EliminarClienteAsync(int clienteId);
        Task<List<Cliente>> ObtenerTodosLosClientesAsync();
        Task<List<Cliente>> BuscarClientesAsync(string? nombre, string? documento);
        Task<List<Cliente>> BuscarPorNumeroDocumentoAsync(string numeroDocumento);
        Task<List<Cliente>> BuscarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
        Task<List<ClienteTelefonosDto>> ObtenerClientesConMultiplesTelefonosAsync();
        Task<List<ClienteDireccionesDto>> ObtenerClientesConMultiplesDireccionesAsync();
    }
}
