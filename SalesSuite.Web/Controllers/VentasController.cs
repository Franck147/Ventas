using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesSuite.Domain.Entities;
using SalesSuite.Domain.Interfaces;
using SalesSuite.Infrastructure.Data;
using SalesSuite.Web.DTOs;
using X.PagedList.Extensions;

namespace SalesSuite.Web.Controllers;

[Authorize]
public class VentasController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly SalesDbContext _db;

    // El DbContext se inyecta directamente para las consultas complejas con Include
    // que el repositorio genérico no puede expresar sin extensiones adicionales.
    public VentasController(IUnitOfWork uow, IMapper mapper, SalesDbContext db)
    {
        _uow    = uow;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /Ventas ───────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(int pagina = 1)
    {
        const int tamanoPagina = 10;

        var ventas = await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();

        var dtos     = _mapper.Map<IEnumerable<VentaDTO>>(ventas);
        var paginado = dtos.ToPagedList(pagina, tamanoPagina);

        return View(paginado);
    }

    // ── GET /Ventas/Details/5 ─────────────────────────────────────────────────
    public async Task<IActionResult> Details(int id)
    {
        var venta = await _db.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta is null) return NotFound();

        return View(_mapper.Map<VentaDetalleDTO>(venta));
    }

    // ── GET /Ventas/Create ────────────────────────────────────────────────────
    public async Task<IActionResult> Create()
    {
        // Cargar catálogo activo con stock > 0 y lista de clientes para el formulario.
        var clientes  = await _uow.Repository<Cliente>().GetAllAsync();
        var productos = await _uow.Repository<Producto>().GetAsync(
            p => p.Activo && p.Stock > 0);

        ViewBag.Clientes  = _mapper.Map<IEnumerable<ClienteDTO>>(clientes);
        ViewBag.Productos = _mapper.Map<IEnumerable<ProductoDTO>>(productos);

        return View();
    }

    // ── POST /Ventas/Create (AJAX JSON) ──────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VentaCreateRequest request)
    {
        // Validación básica del modelo recibido.
        if (!ModelState.IsValid || request.Items.Count == 0)
            return BadRequest(new { error = "La solicitud no es válida o el carrito está vacío." });

        await _uow.BeginTransactionAsync();
        try
        {
            var detalles = new List<DetalleVenta>();

            // ── Validar y descontar stock para cada línea del carrito ──────────
            foreach (var item in request.Items)
            {
                // Se obtiene el producto rastreado por EF para poder modificar su Stock.
                var producto = await _db.Productos.FindAsync(item.ProductoId);

                if (producto is null)
                    throw new InvalidOperationException(
                        $"El producto con ID {item.ProductoId} no existe.");

                if (!producto.Activo)
                    throw new InvalidOperationException(
                        $"El producto '{producto.Nombre}' no está disponible para la venta.");

                // Validación de stock real en base de datos (no se confía en la UI).
                if (producto.Stock < item.Cantidad)
                    throw new InvalidOperationException(
                        $"Stock insuficiente para '{producto.Nombre}'. " +
                        $"Disponible: {producto.Stock}, solicitado: {item.Cantidad}.");

                // Descuento de stock.
                producto.Stock -= item.Cantidad;

                detalles.Add(new DetalleVenta
                {
                    ProductoId     = producto.Id,
                    Cantidad       = item.Cantidad,
                    PrecioUnitario = producto.Precio,                    // precio histórico snapshot
                    Subtotal       = producto.Precio * item.Cantidad
                });
            }

            // ── Crear la cabecera de la venta ─────────────────────────────────
            var venta = new Venta
            {
                ClienteId  = request.ClienteId,
                FechaVenta = DateTime.UtcNow,
                Total      = detalles.Sum(d => d.Subtotal), // calculado en backend
                Detalles   = detalles
            };

            await _db.Ventas.AddAsync(venta);

            // CommitTransactionAsync llama internamente a SaveChangesAsync antes del commit.
            await _uow.CommitTransactionAsync();

            return Ok(new { ventaId = venta.Id, mensaje = "Venta registrada correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            await _uow.RollbackTransactionAsync();
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            await _uow.RollbackTransactionAsync();
            return StatusCode(500, new { error = "Error interno al procesar la venta." });
        }
    }
}
