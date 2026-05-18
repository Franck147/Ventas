using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SalesSuite.Domain.Interfaces;
using SalesSuite.Infrastructure.Data;

namespace SalesSuite.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta del repositorio genérico usando EF Core.
///
/// Todas las operaciones son asíncronas para no bloquear el hilo del servidor
/// mientras se espera la respuesta de PostgreSQL (Supabase).
///
/// Esta clase no llama a SaveChanges; eso es responsabilidad del UnitOfWork,
/// lo que permite agrupar varias operaciones en una sola transacción.
/// </summary>
/// <typeparam name="T">Tipo de entidad del dominio.</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    // DbContext compartido dentro del scope de DI (un scope = un request HTTP).
    private readonly SalesDbContext _context;

    // DbSet tipado para T: evita escribir _context.Set<T>() en cada método.
    private readonly DbSet<T> _dbSet;

    public GenericRepository(SalesDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    // ── Consultas ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.AsNoTracking().ToListAsync();
    // AsNoTracking: las entidades solo se leen, no se rastrean por EF.
    // Mejora el rendimiento en consultas de solo lectura (listas, grids).

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync();

    /// <inheritdoc/>
    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);
    // FindAsync busca primero en el cache del contexto antes de ir a la BD.
    // No usa AsNoTracking porque el objeto podría necesitar ser modificado.

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        // Construcción incremental de la consulta (query composition).
        // EF Core traduce toda la cadena a un único SQL con LIMIT/OFFSET.
        IQueryable<T> query = _dbSet.AsNoTracking();

        if (predicate is not null)
            query = query.Where(predicate);

        if (orderBy is not null)
            query = orderBy(query);

        // Skip/Take corresponden a OFFSET/LIMIT en SQL.
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate is null)
            return await _dbSet.CountAsync();

        return await _dbSet.CountAsync(predicate);
    }

    // ── Comandos (escritura) ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task AddAsync(T entity)
        => await _dbSet.AddAsync(entity);
    // El registro queda en estado Added; se escribe en BD con SaveChangesAsync().

    /// <inheritdoc/>
    public void Update(T entity)
        => _dbSet.Update(entity);
    // Marca todas las propiedades como Modified; EF genera un UPDATE completo.
    // Para actualizaciones parciales se podría usar Entry(...).Property(...).IsModified.

    /// <inheritdoc/>
    public void Delete(T entity)
        => _dbSet.Remove(entity);
    // El registro queda en estado Deleted; se ejecuta el DELETE con SaveChangesAsync().
}
