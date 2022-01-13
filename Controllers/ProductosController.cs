using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ComercioElectronicoMvc.Data;

namespace ComercioElectronicoMvc.Models
{
    public class ProductosController : Controller
    {
        private readonly MercadoContext _context;

        public ProductosController(MercadoContext context)
        {
            _context = context;
        }

        // GET: Productos
        public async Task<IActionResult> Index(int? categoriaId, string sortOrder, string searchProduct)
        {
            //Validaciones de error que se le pasan a la vista
            if (TempData["ErrorValidation"] != null) this.ViewData["ErrorMessage"] = TempData["ErrorValidation"];

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "Precio" ? "price_desc" : "Precio";

            var mercadoContext = _context.Producto.Include(p => p.categoria).AsQueryable();

            //Filtro Categoria
            if (categoriaId != null) mercadoContext = mercadoContext.Where(p => p.categoriaId == categoriaId);

            //Filtro Buscar Producto por Nombre
            if (!String.IsNullOrEmpty(searchProduct)) mercadoContext = mercadoContext.Where(p => p.nombre.ToUpper().Contains(searchProduct.ToUpper()));

            //Ordeno ascendente y descendente el nombre de los productos y el precio
            switch (sortOrder)
            {
                case "name_desc":
                    mercadoContext = mercadoContext.OrderByDescending(p => p.nombre);
                    break;
                case "Precio":
                    mercadoContext = mercadoContext.OrderBy(p => p.precio);
                    break;
                case "price_desc":
                    mercadoContext = mercadoContext.OrderByDescending(p => p.precio);
                    break;
                default:
                    mercadoContext = mercadoContext.OrderBy(p => p.nombre);
                    break;
            }

            //Cargo el Filtro de categorias que no estan deprecadas
            ViewData["categorias"] = _context.Categoria.Where(c => !c.deprecado).ToList();

            return View(await mercadoContext.ToListAsync());
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Producto
                .Include(p => p.categoria)
                .FirstOrDefaultAsync(m => m.productoId == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        public IActionResult Create()
        {
            //Filtro las categorias que no estan eliminadas
            ViewData["categoriaId"] = new SelectList(_context.Categoria.Where(c => !c.deprecado), "categoriaId", "nombre");
            return View();
        }

        // POST: Productos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("productoId,nombre,precio,cantidad,deprecado,categoriaId")] Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["categoriaId"] = new SelectList(_context.Categoria, "categoriaId", "nombre", producto.categoriaId);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Producto.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            if (producto.deprecado)
            {
                TempData["ErrorValidation"] = "Error: No se puede modificar un producto eliminado";
                return RedirectToAction(nameof(Index));
            }

            ViewData["categoriaId"] = new SelectList(_context.Categoria.Where(c => !c.deprecado), "categoriaId", "nombre", producto.categoriaId);
            return View(producto);
        }

        // POST: Productos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("productoId,nombre,precio,cantidad,deprecado,categoriaId")] Producto producto)
        {
            if (id != producto.productoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.productoId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["categoriaId"] = new SelectList(_context.Categoria, "categoriaId", "nombre", producto.categoriaId);
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Producto
                .Include(p => p.categoria)
                .FirstOrDefaultAsync(m => m.productoId == id);
            if (producto == null)
            {
                return NotFound();
            }

            if (producto.deprecado)
            {
                TempData["ErrorValidation"] = "Error: El producto ya se encuentra eliminado";
                return RedirectToAction(nameof(Index));
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            producto.deprecado = true;
            _context.Producto.Update(producto);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Producto.Any(e => e.productoId == id);
        }

    }
}
