using CFA.Clientes.Application.Dtos;
using CFA.Clientes.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CFA.Clientes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var clientes = await _clienteService.ObtenerTodosLosClientesAsync();
            return Ok(clientes);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ClienteCreateDto clienteDto)
        {
            var (success, cliente, message) = await _clienteService.RegistrarClienteAsync(clienteDto);

            if (!success || cliente == null)
                return BadRequest(message);

            return CreatedAtAction(nameof(Get), new { id = cliente.ClienteId }, cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ClienteUpdateDto clienteDto)
        {
            var (success, cliente, message) = await _clienteService.ActualizarClienteAsync(id, clienteDto);

            if (!success || cliente == null)
                return BadRequest(message);

            return Ok(cliente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, cliente, message) = await _clienteService.EliminarClienteAsync(id);

            if (!success || cliente == null)
                return NotFound(message);

            return Ok(new { Message = message, Cliente = cliente });
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string? nombre, [FromQuery] string? documento)
        {
            var clientes = await _clienteService.BuscarClientesAsync(nombre, documento);

            if (!clientes.Any())
                return NotFound("No se encontraron clientes con los filtros proporcionados.");

            return Ok(clientes);
        }

        [HttpGet("buscar/documento")]
        public async Task<IActionResult> BuscarPorDocumento([FromQuery] string numeroDocumento)
        {
            var clientes = await _clienteService.BuscarPorNumeroDocumentoAsync(numeroDocumento);
            return Ok(clientes);
        }

        [HttpGet("buscar/rango-fechas")]
        public async Task<IActionResult> BuscarPorRangoFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            var clientes = await _clienteService.BuscarPorRangoFechasAsync(fechaInicio, fechaFin);
            return Ok(clientes);
        }

        [HttpGet("multiples-telefonos")]
        public async Task<IActionResult> ClientesConMultiplesTelefonos()
        {
            var resultado = await _clienteService.ObtenerClientesConMultiplesTelefonosAsync();
            return Ok(resultado);
        }

        [HttpGet("multiples-direcciones")]
        public async Task<IActionResult> ClientesConMultiplesDirecciones()
        {
            var resultado = await _clienteService.ObtenerClientesConMultiplesDireccionesAsync();
            return Ok(resultado);
        }
    }
}

