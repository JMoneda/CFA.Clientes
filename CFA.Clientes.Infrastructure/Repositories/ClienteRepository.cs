using CFA.Clientes.Domain.Entities;
using CFA.Clientes.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CFA.Clientes.Infrastructure.Repositories
{
    public class ClienteRepository
    {
        private readonly AppDbContext _context;

        public ClienteRepository(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Obtener todos los clientes (con teléfonos y direcciones)
        public async Task<List<Cliente>> GetAllAsync()
        {
            return await _context.Clientes
                .Include(c => c.Telefonos)
                .Include(c => c.Direcciones)
                .ToListAsync();
        }

        // ✅ Obtener cliente por ID
        public async Task<Cliente?> GetByIdAsync(int id)
        {
            return await _context.Clientes
                .Include(c => c.Telefonos)
                .Include(c => c.Direcciones)
                .FirstOrDefaultAsync(c => c.ClienteId == id);
        }

        // ✅ Insertar cliente usando SP
        public async Task<int> InsertarClienteSPAsync(Cliente cliente)
        {
            var clienteIdParam = new SqlParameter
            {
                ParameterName = "@ClienteId",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC InsertarCliente @TipoDocumento, @NumeroDocumento, @Nombres, @Apellido1, @Apellido2, @Genero, @FechaNacimiento, @Email, @ClienteId OUTPUT",
                new SqlParameter("@TipoDocumento", cliente.TipoDocumento),
                new SqlParameter("@NumeroDocumento", cliente.NumeroDocumento),
                new SqlParameter("@Nombres", cliente.Nombres),
                new SqlParameter("@Apellido1", cliente.Apellido1),
                new SqlParameter("@Apellido2", cliente.Apellido2 ?? (object)DBNull.Value),
                new SqlParameter("@Genero", cliente.Genero),
                new SqlParameter("@FechaNacimiento", cliente.FechaNacimiento),
                new SqlParameter("@Email", cliente.Email),
                clienteIdParam
            );

            return (int)clienteIdParam.Value;
        }
        
        // ✅ Insertar teléfonos
        public async Task InsertarTelefonosAsync(IEnumerable<Telefono> telefonos)
        {
            await _context.Telefonos.AddRangeAsync(telefonos);
            await _context.SaveChangesAsync();
        }

        // ✅ Insertar direcciones
        public async Task InsertarDireccionesAsync(IEnumerable<Direccion> direcciones)
        {
            await _context.Direcciones.AddRangeAsync(direcciones);
            await _context.SaveChangesAsync();
        }

        // ✅ Actualizar cliente
        public async Task UpdateAsync(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        // ✅ Eliminar cliente (cascada configurada en EF)
        public async Task DeleteAsync(Cliente cliente)
        {
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }

        // 🔥 Devuelve solo nombre completo y cantidad de teléfonos
        public async Task<List<(string NombreCompleto, int CantidadTelefonos)>> ObtenerClientesConMultiplesTelefonosAsync()
        {
            var datos = await _context.Clientes
                .Include(c => c.Telefonos)
                .Where(c => c.Telefonos.Count > 1)
                .Select(c => new
                {
                    NombreCompleto = c.Nombres + " " + c.Apellido1 + " " + (c.Apellido2 ?? ""),
                    CantidadTelefonos = c.Telefonos.Count
                })
                .ToListAsync();

            // Ahora convierte a tuplas en memoria
            return datos
                .Select(x => (x.NombreCompleto, x.CantidadTelefonos))
                .ToList();
        }



        // 🔥 Devuelve nombre completo y primera dirección
        public async Task<List<(string NombreCompleto, string PrimeraDireccion)>> ObtenerClientesConMultiplesDireccionesAsync()
        {
            var datos = await _context.Clientes
                .Include(c => c.Direcciones)
                .Where(c => c.Direcciones.Count > 1)
                .Select(c => new
                {
                    NombreCompleto = c.Nombres + " " + c.Apellido1 + " " + (c.Apellido2 ?? ""),
                    PrimeraDireccion = c.Direcciones.OrderBy(d => d.DireccionId).FirstOrDefault().Descripcion
                })
                .ToListAsync();

            // Convierte a tuplas en memoria
            return datos
                .Select(x => (x.NombreCompleto, x.PrimeraDireccion))
                .ToList();
        }


    }
}
