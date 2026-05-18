namespace SalesSuite.Domain.Interfaces;

/// <summary>
/// Contrato de la Unidad de Trabajo (Unit of Work).
///
/// Centraliza dos responsabilidades clave:
///  1. Fábrica de repositorios: un único punto de acceso a todos los repos
///     sin tener que inyectarlos individualmente en cada servicio.
///  2. Coordinación transaccional: agrupa múltiples operaciones de escritura
///     en una sola transacción de base de datos (commit / rollback atómico).
///
/// Hereda IDisposable para garantizar la liberación del DbContext y la
/// conexión de base de datos cuando el scope de DI finaliza.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // ── Fábrica de repositorios ───────────────────────────────────────────────

    /// <summary>
    /// Devuelve (o crea si no existe) el repositorio genérico para la entidad T.
    /// Los repositorios se cachean por tipo para no crear instancias redundantes
    /// dentro del mismo scope de request.
    ///
    /// Ejemplo de uso en un controlador o servicio:
    ///   var productos = await _uow.Repository&lt;Producto&gt;().GetAllAsync();
    /// </summary>
    IGenericRepository<T> Repository<T>() where T : class;

    // ── Persistencia ──────────────────────────────────────────────────────────

    /// <summary>
    /// Persiste todos los cambios rastreados por el DbContext en la base de datos.
    /// Devuelve el número de filas afectadas.
    /// </summary>
    Task<int> SaveChangesAsync();

    // ── Control de transacciones ──────────────────────────────────────────────

    /// <summary>
    /// Inicia una transacción explícita en la base de datos.
    /// Úsala cuando un caso de uso requiera múltiples operaciones atómicas
    /// (ej: descontar stock y registrar venta simultáneamente).
    /// </summary>
    Task BeginTransactionAsync();

    /// <summary>
    /// Confirma la transacción activa, haciendo permanentes todos los cambios.
    /// Solo debe llamarse después de BeginTransactionAsync().
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Revierte la transacción activa, deshaciendo todos los cambios.
    /// Se llama en el bloque catch cuando alguna operación falla.
    /// </summary>
    Task RollbackTransactionAsync();
}
