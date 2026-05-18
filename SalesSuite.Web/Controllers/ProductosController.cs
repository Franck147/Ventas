using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSuite.Domain.Entities;
using SalesSuite.Domain.Interfaces;
using SalesSuite.Web.DTOs;
using X.PagedList.Extensions;

namespace SalesSuite.Web.Controllers;

/// <summary>
/// Controlador CRUD de productos. Protegido con [Authorize]: solo usuarios
/// autenticados pueden acceder a la gestión del catálogo.
/// </summary>
[Authorize]
public class ProductosController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProductosController(IUnitOfWork uow, IMapper mapper)
    {
        _uow    = uow;
        _mapper = mapper;
    }

    // ── GET /Productos ────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? busqueda, int pagina = 1)
    {
        const int tamanoPagina = 10;

        // Filtra por nombre o código de barras si el usuario envió un término.
        var productos = string.IsNullOrWhiteSpace(busqueda)
            ? await _uow.Repository<Producto>().GetAllAsync()
            : await _uow.Repository<Producto>().GetAsync(
                p => p.Nombre.Contains(busqueda) ||
                     (p.CodigoBarras != null && p.CodigoBarras.Contains(busqueda)));

        var dtos = _mapper.Map<IEnumerable<ProductoDTO>>(productos);

        // X.PagedList convierte IEnumerable en una página in-memory.
        var paginado = dtos.ToPagedList(pagina, tamanoPagina);

        ViewBag.Busqueda = busqueda;
        return View(paginado);
    }

    // ── GET /Productos/Create ─────────────────────────────────────────────────
    public IActionResult Create() => View(new ProductoInputModel());

    // ── POST /Productos/Create ────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductoInputModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var producto = _mapper.Map<Producto>(model);
        await _uow.Repository<Producto>().AddAsync(producto);
        await _uow.SaveChangesAsync();

        TempData["Exito"] = $"Producto '{producto.Nombre}' creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Productos/Edit/5 ─────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _uow.Repository<Producto>().GetByIdAsync(id);
        if (producto is null) return NotFound();

        return View(_mapper.Map<ProductoInputModel>(producto));
    }

    // ── POST /Productos/Edit/5 ────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductoInputModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        var producto = await _uow.Repository<Producto>().GetByIdAsync(id);
        if (producto is null) return NotFound();

        _mapper.Map(model, producto); // actualiza las propiedades del objeto rastreado
        _uow.Repository<Producto>().Update(producto);
        await _uow.SaveChangesAsync();

        TempData["Exito"] = $"Producto '{producto.Nombre}' actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Productos/Delete/5 ───────────────────────────────────────────────
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _uow.Repository<Producto>().GetByIdAsync(id);
        if (producto is null) return NotFound();

        return View(_mapper.Map<ProductoDTO>(producto));
    }

    // ── POST /Productos/Delete/5 ──────────────────────────────────────────────
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _uow.Repository<Producto>().GetByIdAsync(id);
        if (producto is null) return NotFound();

        try
        {
            _uow.Repository<Producto>().Delete(producto);
            await _uow.SaveChangesAsync();
            TempData["Exito"] = $"Producto '{producto.Nombre}' eliminado.";
        }
        catch (Exception)
        {
            // DeleteBehavior.Restrict lanza excepción si el producto tiene ventas asociadas.
            TempData["Error"] = "No se puede eliminar un producto que tiene ventas registradas.";
        }

        return RedirectToAction(nameof(Index));
    }
}
