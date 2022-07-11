using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComercioElectronicoMvc.Data;

namespace ComercioElectronicoMvc.Models
{
    public class CategoriasController : Controller
    {
        private readonly MercadoContext _context;

        public CategoriasController(MercadoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Categoria.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            Categoria category = FindCategoryById(id);
            if (category == null) {
                TempData["ErrorMessage"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }
            return View (category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("categoriaId,nombre,deprecado")] Categoria category)
        {
            Categoria optionalCategory = FindCategoryByName(category.nombre);
            
            if (optionalCategory != null)
            {
                ViewBag.ErrorMessage = "El nombre de la categoría ya existe.";
                return View(category);
            }

            if (ModelState.IsValid)
            {
                _context.Categoria.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = String.Format("La categoría {0} se agregó correctamente.", category.nombre);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            Categoria category = FindCategoryById(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("categoriaId,nombre,deprecado")] Categoria category)
        {
            if (ModelState.IsValid)
            {
                _context.Categoria.Update(category);
                await _context.SaveChangesAsync();
                ViewBag.SuccessMessage = String.Format("Categoría actualizada correctamente.", category.nombre);
                return View(category);
            }
            return View(category);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            Categoria category = FindCategoryById(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categoria.Include(category => category.productos)
               .FirstOrDefaultAsync(category => category.categoriaId == id);

            //Elimino los productos asosiados a esa categoria
            if (category.productos != null)
            {
                category.productos.ForEach(producto =>
                {
                    if (!producto.deprecado) producto.deprecado = true;
                    _context.Producto.Update(producto);
                });
            }

            category.deprecado = true;
            _context.Categoria.Update(category);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = String.Format("La categoría {0} se elimino correctamente.", category.nombre);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Enable(int? id)
        {
            Categoria category = FindCategoryById(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            category.deprecado = false;
            _context.Categoria.Update(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = String.Format("La categoría {0} se habilitó correctamente.", category.nombre);
            return RedirectToAction(nameof(Index));
        }

        private Categoria FindCategoryByName(string categoryName)
        {
            return _context.Categoria.Where(category => category.nombre.ToUpper().Equals(categoryName.ToUpper())).FirstOrDefault();
        }

        private Categoria FindCategoryById(int? categoryId)
        {
            if (categoryId == null) return null;
            Categoria categoria = _context.Categoria.Find(categoryId);
            if (categoria == null) return null;
            else return categoria;
        }
    }
}
