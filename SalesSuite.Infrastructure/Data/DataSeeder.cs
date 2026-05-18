using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SalesSuite.Domain.Entities;

namespace SalesSuite.Infrastructure.Data;

/// <summary>
/// Pobla la base de datos con datos iniciales de forma idempotente.
/// Usa métodos async para compatibilidad con el pooler de Supabase
/// en modo transacción (puerto 6543).
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(SalesDbContext context, UserManager<IdentityUser>? userManager = null)
    {
        // ── Productos ─────────────────────────────────────────────────────────
        if (!await context.Productos.AnyAsync())
        {
            var productos = new List<Producto>
            {
                new() { Nombre = "Laptop HP Pavilion 15\"",     CodigoBarras = "7501234560001", Precio = 899.99m,  Stock = 15, StockMinimo = 3,  Activo = true },
                new() { Nombre = "Mouse Inalámbrico Logitech",  CodigoBarras = "7501234560002", Precio = 29.99m,   Stock = 50, StockMinimo = 10, Activo = true },
                new() { Nombre = "Teclado Mecánico RGB",        CodigoBarras = "7501234560003", Precio = 79.99m,   Stock = 30, StockMinimo = 5,  Activo = true },
                new() { Nombre = "Monitor 27\" Full HD",         CodigoBarras = "7501234560004", Precio = 299.99m,  Stock = 10, StockMinimo = 2,  Activo = true },
                new() { Nombre = "Auriculares Sony WH-1000XM4", CodigoBarras = "7501234560005", Precio = 249.99m,  Stock = 20, StockMinimo = 4,  Activo = true },
                new() { Nombre = "Webcam Logitech C920 HD",     CodigoBarras = "7501234560006", Precio = 89.99m,   Stock = 25, StockMinimo = 5,  Activo = true },
                new() { Nombre = "SSD 1TB Samsung 870 EVO",     CodigoBarras = "7501234560007", Precio = 119.99m,  Stock = 40, StockMinimo = 8,  Activo = true },
                new() { Nombre = "Memoria RAM 16GB DDR4",       CodigoBarras = "7501234560008", Precio = 64.99m,   Stock = 35, StockMinimo = 7,  Activo = true },
                new() { Nombre = "Mousepad XL Gaming",          CodigoBarras = "7501234560009", Precio = 19.99m,   Stock = 60, StockMinimo = 12, Activo = true },
                new() { Nombre = "Hub USB-C 7 Puertos",         CodigoBarras = "7501234560010", Precio = 39.99m,   Stock = 45, StockMinimo = 9,  Activo = true },
            };
            await context.Productos.AddRangeAsync(productos);
            await context.SaveChangesAsync();
        }

        // ── Clientes ──────────────────────────────────────────────────────────
        if (!await context.Clientes.AnyAsync())
        {
            var clientes = new List<Cliente>
            {
                new() { DocumentoIdentidad = "12345678A", Nombre = "Juan",   Apellido = "Pérez",     Email = "juan.perez@email.com",   Telefono = "555-0101" },
                new() { DocumentoIdentidad = "87654321B", Nombre = "María",  Apellido = "García",    Email = "maria.garcia@email.com",  Telefono = "555-0102" },
                new() { DocumentoIdentidad = "11223344C", Nombre = "Carlos", Apellido = "Rodríguez", Email = "carlos.rod@email.com",    Telefono = "555-0103" },
            };
            await context.Clientes.AddRangeAsync(clientes);
            await context.SaveChangesAsync();
        }

        // ── Usuario de prueba ─────────────────────────────────────────────────
        if (userManager is not null)
        {
            const string email    = "admin@salessuite.com";
            const string password = "Admin123!";

            if (await userManager.FindByEmailAsync(email) is null)
            {
                var user = new IdentityUser
                {
                    UserName       = email,
                    Email          = email,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, password);
            }
        }
    }
}
