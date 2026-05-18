using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesSuite.Domain.Entities;
using SalesSuite.Domain.Interfaces;
using SalesSuite.Web.DTOs;
using X.PagedList.Extensions;

namespace SalesSuite.Web.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ClientesController(IUnitOfWork uow, IMapper mapper)
    {
        _uow    = uow;
        _mapper = mapper;
    }

    // ── GET /Clientes ─────────────────────────────────────────────────────────
    public async Task<IActionResult> Index(string? busqueda, int pagina = 1)
    {
        const int tamanoPagina = 10;

        var clientes = string.IsNullOrWhiteSpace(busqueda)
            ? await _uow.Repository<Cliente>().GetAllAsync()
            : await _uow.Repository<Cliente>().GetAsync(
                c => c.Nombre.Contains(busqueda)    ||
                     c.Apellido.Contains(busqueda)  ||
                     c.DocumentoIdentidad.Contains(busqueda));

        var dtos     = _mapper.Map<IEnumerable<ClienteDTO>>(clientes);
        var paginado = dtos.ToPagedList(pagina, tamanoPagina);

        ViewBag.Busqueda = busqueda;
        return View(paginado);
    }

    // ── GET /Clientes/Create ──────────────────────────────────────────────────
    public IActionResult Create() => View(new ClienteInputModel());

    // ── POST /Clientes/Create ─────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteInputModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Verificar que el documento de identidad no exista ya.
        var existe = await _uow.Repository<Cliente>().GetAsync(
            c => c.DocumentoIdentidad == model.DocumentoIdentidad);

        if (existe.Any())
        {
            ModelState.AddModelError(nameof(model.DocumentoIdentidad),
                "Ya existe un cliente con ese documento de identidad.");
            return View(model);
        }

        var cliente = _mapper.Map<Cliente>(model);
        await _uow.Repository<Cliente>().AddAsync(cliente);
        await _uow.SaveChangesAsync();

        TempData["Exito"] = $"Cliente '{cliente.Nombre} {cliente.Apellido}' registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Clientes/Edit/5 ──────────────────────────────────────────────────
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _uow.Repository<Cliente>().GetByIdAsync(id);
        if (cliente is null) return NotFound();

        return View(_mapper.Map<ClienteInputModel>(cliente));
    }

    // ── POST /Clientes/Edit/5 ─────────────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClienteInputModel model)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        // Verificar duplicado de documento (excluyendo el propio cliente).
        var duplicado = await _uow.Repository<Cliente>().GetAsync(
            c => c.DocumentoIdentidad == model.DocumentoIdentidad && c.Id != id);

        if (duplicado.Any())
        {
            ModelState.AddModelError(nameof(model.DocumentoIdentidad),
                "Otro cliente ya usa ese documento de identidad.");
            return View(model);
        }

        var cliente = await _uow.Repository<Cliente>().GetByIdAsync(id);
        if (cliente is null) return NotFound();

        _mapper.Map(model, cliente);
        _uow.Repository<Cliente>().Update(cliente);
        await _uow.SaveChangesAsync();

        TempData["Exito"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // ── GET /Clientes/Delete/5 ────────────────────────────────────────────────
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _uow.Repository<Cliente>().GetByIdAsync(id);
        if (cliente is null) return NotFound();

        return View(_mapper.Map<ClienteDTO>(cliente));
    }

    // ── POST /Clientes/Delete/5 ───────────────────────────────────────────────
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _uow.Repository<Cliente>().GetByIdAsync(id);
        if (cliente is null) return NotFound();

        try
        {
            _uow.Repository<Cliente>().Delete(cliente);
            await _uow.SaveChangesAsync();
            TempData["Exito"] = "Cliente eliminado correctamente.";
        }
        catch (Exception)
        {
            TempData["Error"] = "No se puede eliminar un cliente que tiene ventas registradas.";
        }

        return RedirectToAction(nameof(Index));
    }
}
