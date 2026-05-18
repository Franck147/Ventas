using Microsoft.EntityFrameworkCore.Storage;
using SalesSuite.Domain.Interfaces;
using SalesSuite.Infrastructure.Data;

namespace SalesSuite.Infrastructure.Repositories;

/// <summary>
/// Implementación del patrón Unit of Work usando EF Core.
///
/// Ciclo de vida en DI: Scoped (un UnitOfWork por request HTTP).
/// Esto garantiza que todos los repositorios creados en el mismo request
/// compartan el mismo DbContext y, por tanto, la misma transacción.
///
/// Flujo típico de uso transaccional:
///   await _uow.BeginTransactionAsync();
///   try {
///       // operaciones con repositorios...
///       await _uow.SaveChangesAsync();
///       await _uow.CommitTransactionAsync();
///   } catch {
///       await _uow.RollbackTransactionAsync();
///       throw;
///   }
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly SalesDbContext _context;

    // Cache de repositorios: evita crear múltiples instancias del mismo tipo
    // dentro del mismo scope de request.
    // Clave = tipo de entidad (ej: typeof(Producto)), Valor = instancia del repo.
    private readonly Dictionary<Type, object> _repositories = new();

    // Transacción activa de base de datos (null cuando no hay transacción abierta).
    private IDbContextTransaction? _transaction;

    // Controla si el objeto ya fue liberado (patrón Dispose seguro).
    private bool _disposed;

    public UnitOfWork(SalesDbContext context)
    {
        _context = context;
    }

    // ── Fábrica de repositorios ───────────────────────────────────────────────

    /// <inheritdoc/>
    public IGenericRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);

        // Si ya existe un repo para este tipo en el cache, lo devuelve.
        if (_repositories.TryGetValue(type, out var existingRepo))
            return (IGenericRepository<T>)existingRepo;

        // Si no existe, crea uno nuevo, lo guarda en el cache y lo retorna.
        var newRepo = new GenericRepository<T>(_context);
        _repositories[type] = newRepo;
        return newRepo;
    }

    // ── Persistencia ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    // ── Control de transacciones ──────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task BeginTransactionAsync()
    {
        // Solo abre una nueva transacción si no hay una activa ya.
        // Esto previene transacciones anidadas accidentales.
        if (_transaction is null)
            _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync()
    {
        try
        {
            // Primero persiste los cambios en EF, luego confirma en la BD.
            await _context.SaveChangesAsync();
            await _transaction!.CommitAsync();
        }
        finally
        {
            // Siempre limpia la transacción al terminar (éxito o fallo).
            await DisposeTransactionAsync();
        }
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_transaction is not null)
                await _transaction.RollbackAsync();
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    // ── Liberación de recursos ────────────────────────────────────────────────

    /// <summary>
    /// Libera la transacción activa y resetea el campo a null
    /// para que pueda iniciarse una nueva si es necesario.
    /// </summary>
    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        // Suprime el finalizador para evitar doble liberación.
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Implementación estándar del patrón Dispose con flag de seguridad.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Libera la transacción si quedó abierta (caso de error no controlado).
                _transaction?.Dispose();
                // El DbContext también se libera aquí, aunque DI lo gestiona automáticamente
                // al finalizar el scope. Esta línea es una salvaguarda extra.
                _context.Dispose();
            }
            _disposed = true;
        }
    }
}
