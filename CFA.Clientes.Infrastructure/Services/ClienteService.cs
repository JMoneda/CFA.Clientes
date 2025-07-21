using CFA.Clientes.Application.Dtos;
using CFA.Clientes.Application.Interfaces;
using CFA.Clientes.Domain.Entities;
using CFA.Clientes.Infrastructure.Repositories;
using System.Text.RegularExpressions;

namespace CFA.Clientes.Infrastructure.Services
{
    public class ClienteService : IClienteService
    {
        private readonly ClienteRepository _repository;

        public ClienteService(ClienteRepository repository)
        {
            _repository = repository;
        }

        // ✅ Registrar Cliente
        public async Task<(bool Success, Cliente? Cliente, string? Message)> RegistrarClienteAsync(ClienteCreateDto dto)
        {
            var cliente = MapDtoToCliente(dto);

            if (!ValidarMinimoTelefonosDirecciones(dto.Telefonos, dto.Direcciones, out var mensaje))
                return (false, null, mensaje);

            if (!ValidarTipoDocumentoPorEdad(cliente.TipoDocumento, CalcularEdad(cliente.FechaNacimiento)))
                return (false, null, "El tipo de documento no es válido para la edad.");

            if (!ValidarEmail(cliente.Email))
                return (false, null, "El email ingresado no tiene un formato válido.");

            var existentes = await _repository.GetAllAsync();
            if (existentes.Any(c => c.NumeroDocumento == cliente.NumeroDocumento))
                return (false, null, "El número de documento ya está registrado.");

            int clienteId = await _repository.InsertarClienteSPAsync(cliente);
            if (clienteId <= 0)
                return (false, null, "Error al insertar el cliente.");

            cliente.ClienteId = clienteId;

            if (cliente.Telefonos.Any())
            {
                foreach (var tel in cliente.Telefonos)
                    tel.ClienteId = clienteId;

                await _repository.InsertarTelefonosAsync(cliente.Telefonos);
            }

            if (cliente.Direcciones.Any())
            {
                foreach (var dir in cliente.Direcciones)
                    dir.ClienteId = clienteId;

                await _repository.InsertarDireccionesAsync(cliente.Direcciones);
            }

            var clienteCompleto = await _repository.GetByIdAsync(clienteId);
            return (true, clienteCompleto, "Cliente registrado correctamente.");
        }

        // ✅ Actualizar Cliente
        public async Task<(bool Success, Cliente? Cliente, string? Message)> ActualizarClienteAsync(int clienteId, ClienteUpdateDto dto)
        {
            var clienteExistente = await _repository.GetByIdAsync(clienteId);
            if (clienteExistente == null)
                return (false, null, "Cliente no encontrado.");

            if (!ValidarMinimoTelefonosDirecciones(dto.Telefonos, dto.Direcciones, out var mensaje))
                return (false, null, mensaje);

            if (!ValidarTipoDocumentoPorEdad(dto.TipoDocumento, CalcularEdad(dto.FechaNacimiento)))
                return (false, null, $"El tipo de documento {dto.TipoDocumento} no es válido para la edad.");

            if (!ValidarEmail(dto.Email))
                return (false, null, "El email ingresado no tiene un formato válido.");

            var existentes = await _repository.GetAllAsync();
            if (existentes.Any(c =>
                c.NumeroDocumento == dto.NumeroDocumento && c.ClienteId != clienteId))
                return (false, null, "El número de documento ya está registrado por otro cliente.");

            clienteExistente.TipoDocumento = dto.TipoDocumento;
            clienteExistente.NumeroDocumento = dto.NumeroDocumento;
            clienteExistente.Nombres = dto.Nombres;
            clienteExistente.Apellido1 = dto.Apellido1;
            clienteExistente.Apellido2 = dto.Apellido2;
            clienteExistente.Genero = dto.Genero;
            clienteExistente.FechaNacimiento = dto.FechaNacimiento;
            clienteExistente.Email = dto.Email;

            ActualizarTelefonos(clienteExistente, dto.Telefonos);
            ActualizarDirecciones(clienteExistente, dto.Direcciones);

            await _repository.UpdateAsync(clienteExistente);

            var clienteActualizado = await _repository.GetByIdAsync(clienteId);
            return (true, clienteActualizado, "Cliente actualizado correctamente.");
        }

        // ✅ Eliminar Cliente
        public async Task<(bool Success, Cliente? Cliente, string? Message)> EliminarClienteAsync(int clienteId)
        {
            var cliente = await _repository.GetByIdAsync(clienteId);
            if (cliente == null)
                return (false, null, "Cliente no encontrado.");

            await _repository.DeleteAsync(cliente);
            return (true, cliente, "Cliente eliminado correctamente.");
        }

        // ✅ Obtener todos los clientes
        public async Task<List<Cliente>> ObtenerTodosLosClientesAsync()
        {
            return await _repository.GetAllAsync();
        }

        // ✅ Buscar por nombre y documento
        public async Task<List<Cliente>> BuscarClientesAsync(string? nombre, string? documento)
        {
            var clientes = await _repository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(nombre))
                clientes = clientes.Where(c => c.Nombres.Contains(nombre, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(documento))
                clientes = clientes.Where(c => c.NumeroDocumento.ToString().Contains(documento)).ToList();

            return clientes
            .OrderBy(c => $"{c.Nombres} {c.Apellido1} {c.Apellido2}".ToUpper())
            .ToList();
        }

        // ✅ Buscar por número de documento
        public async Task<List<Cliente>> BuscarPorNumeroDocumentoAsync(string numeroDocumento)
        {
            var clientes = await _repository.GetAllAsync();

            return clientes
                .Where(c => c.NumeroDocumento.ToString().Contains(numeroDocumento))
                .OrderByDescending(c => c.NumeroDocumento)
                .ToList();
        }

        // ✅ Buscar por rango de fechas
        public async Task<List<Cliente>> BuscarPorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            var clientes = await _repository.GetAllAsync();

            return clientes
                .Where(c => c.FechaNacimiento >= fechaInicio && c.FechaNacimiento <= fechaFin)
                .OrderBy(c => c.FechaNacimiento)
                .ThenBy(c => c.Nombres)
                .ToList();
        }

        // ✅ Clientes con múltiples teléfonos
        public async Task<List<ClienteTelefonosDto>> ObtenerClientesConMultiplesTelefonosAsync()
        {
            var datos = await _repository.GetAllAsync();

            return datos
                .Where(c => c.Telefonos.Count > 1)
                .Select(c => new ClienteTelefonosDto
                {
                    NombreCompleto = $"{c.Nombres} {c.Apellido1} {c.Apellido2}",
                    CantidadTelefonos = c.Telefonos.Count
                })
                .ToList();
        }

        // ✅ Clientes con múltiples direcciones
        public async Task<List<ClienteDireccionesDto>> ObtenerClientesConMultiplesDireccionesAsync()
        {
            var datos = await _repository.GetAllAsync();

            return datos
                .Where(c => c.Direcciones.Count > 1)
                .Select(c => new ClienteDireccionesDto
                {
                    NombreCompleto = $"{c.Nombres} {c.Apellido1} {c.Apellido2}",
                    PrimeraDireccion = c.Direcciones.First().Descripcion
                })
                .ToList();
        }


        // 🔥 Métodos auxiliares
        private Cliente MapDtoToCliente(ClienteCreateDto dto)
        {
            return new Cliente
            {
                TipoDocumento = dto.TipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                Nombres = dto.Nombres,
                Apellido1 = dto.Apellido1,
                Apellido2 = dto.Apellido2,
                Genero = dto.Genero,
                FechaNacimiento = dto.FechaNacimiento,
                Email = dto.Email,
                Direcciones = dto.Direcciones.Select(d => new Direccion
                {
                    Descripcion = d.Descripcion
                }).ToList(),
                Telefonos = dto.Telefonos.Select(t => new Telefono
                {
                    Numero = t.Numero,
                    Tipo = t.Tipo
                }).ToList()
            };
        }

        private void ActualizarTelefonos(Cliente cliente, List<TelefonoDto> telefonosDto)
        {
            cliente.Telefonos.Clear();
            cliente.Telefonos.AddRange(telefonosDto.Select(t => new Telefono
            {
                Numero = t.Numero,
                Tipo = t.Tipo,
                ClienteId = cliente.ClienteId
            }));
        }

        private void ActualizarDirecciones(Cliente cliente, List<DireccionDto> direccionesDto)
        {
            cliente.Direcciones.Clear();
            cliente.Direcciones.AddRange(direccionesDto.Select(d => new Direccion
            {
                Descripcion = d.Descripcion,
                ClienteId = cliente.ClienteId
            }));
        }

        private bool ValidarMinimoTelefonosDirecciones(List<TelefonoDto> telefonos, List<DireccionDto> direcciones, out string? mensaje)
        {
            if (telefonos == null || !telefonos.Any())
            {
                mensaje = "Debe registrar al menos un teléfono.";
                return false;
            }
            if (direcciones == null || !direcciones.Any())
            {
                mensaje = "Debe registrar al menos una dirección.";
                return false;
            }
            mensaje = null;
            return true;
        }

        private bool ValidarTipoDocumentoPorEdad(string tipoDoc, int edad)
        {
            return (edad <= 7 && tipoDoc == "RC") ||
                   (edad >= 8 && edad <= 17 && tipoDoc == "TI") ||
                   (edad >= 18 && tipoDoc == "CC");
        }

        private bool ValidarEmail(string email)
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento > hoy.AddYears(-edad)) edad--;
            return edad;
        }
    }
}
