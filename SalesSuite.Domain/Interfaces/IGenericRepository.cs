using System.Linq.Expressions;

namespace SalesSuite.Domain.Interfaces;

/// <summary>
/// Contrato genérico de acceso a datos para cualquier entidad T.
/// Sigue el patrón Repository para desacoplar la lógica de negocio
/// de la tecnología de persistencia (EF Core, Dapper, etc.).
///
/// Al ser genérico, evita duplicar operaciones CRUD en cada repositorio
/// específico; solo se crean repositorios especializados cuando se
/// necesitan consultas propias de la entidad (ej: búsqueda por CodigoBarras).
/// </summary>
/// <typeparam name="T">Tipo de entidad que gestiona este repositorio.</typeparam>
public interface IGenericRepository<T> where T : class
{
    // ── Consultas ─────────────────────────────────────────────────────────────

    /// <summary>Devuelve todas las entidades sin filtro. Úsalo solo en catálogos pequeños.</summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Devuelve las entidades que cumplan la condición expresada en el predicado.
    /// Ejemplo: repo.GetAsync(p => p.Activo && p.Stock > 0)
    /// </summary>
    Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Devuelve una entidad por su clave primaria (int).
    /// Retorna null si no existe — el llamador decide si lanzar 404 o manejar el caso.
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Devuelve una página de resultados, opcionalmente filtrada.
    /// Útil para las tablas paginadas de la UI con X.PagedList.
    /// </summary>
    /// <param name="pageNumber">Número de página (base 1).</param>
    /// <param name="pageSize">Registros por página.</param>
    /// <param name="predicate">Filtro opcional.</param>
    /// <param name="orderBy">Criterio de ordenamiento opcional.</param>
    Task<IEnumerable<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    /// <summary>
    /// Cuenta los registros que cumplen un predicado (o todos si es null).
    /// Se usa junto con GetPagedAsync para calcular el total de páginas.
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // ── Comandos (escritura) ──────────────────────────────────────────────────

    /// <summary>
    /// Agrega una nueva entidad al contexto.
    /// Los cambios se persisten al llamar IUnitOfWork.SaveChangesAsync().
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Marca la entidad como modificada en el contexto.
    /// Los cambios se persisten al llamar IUnitOfWork.SaveChangesAsync().
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Marca la entidad para eliminación en el contexto.
    /// Los cambios se persisten al llamar IUnitOfWork.SaveChangesAsync().
    /// </summary>
    void Delete(T entity);
}
