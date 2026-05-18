using System.ComponentModel.DataAnnotations;

namespace SalesSuite.Domain.Entities;

/// <summary>
/// Encabezado de una transacción de venta (agregado raíz del proceso de venta).
/// Contiene los datos globales y la referencia al cliente; los artículos
/// vendidos se almacenan en la colección de <see cref="DetalleVenta"/>.
/// </summary>
public class Venta
{
    // ── Clave primaria ────────────────────────────────────────────────────────
    public int Id { get; set; }

    // ── Datos temporales ──────────────────────────────────────────────────────

    /// <summary>
    /// Fecha y hora exacta en que se registró la venta (UTC recomendado).
    /// Se asigna automáticamente al crear la venta desde el controlador.
    /// </summary>
    public DateTime FechaVenta { get; set; }

    // ── Relación con Cliente ──────────────────────────────────────────────────

    /// <summary>FK hacia la tabla Clientes.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
    public int ClienteId { get; set; }

    // ── Resumen económico ─────────────────────────────────────────────────────

    /// <summary>
    /// Suma total de todos los subtotales de <see cref="DetalleVenta"/>.
    /// Se calcula y persiste en el backend; nunca se confía en el valor enviado
    /// desde el cliente (seguridad contra manipulación de precios).
    /// </summary>
    [Range(0.01, double.MaxValue, ErrorMessage = "El total de la venta debe ser mayor a 0.")]
    public decimal Total { get; set; }

    // ── Propiedades de navegación ─────────────────────────────────────────────

    /// <summary>Cliente que realizó la compra (cargado vía Include en las consultas).</summary>
    public Cliente Cliente { get; set; } = null!;

    /// <summary>Líneas de artículos de esta venta. Relación uno-a-muchos con DetalleVenta.</summary>
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
