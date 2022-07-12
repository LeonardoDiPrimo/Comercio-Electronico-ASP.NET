using System;
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

        public async Task<IActionResult> Index(int? categoriaId, string sortOrder, string searchProductByName)
        {
            IQueryable<Producto> productsList = _context.Producto.Include(product => product.categoria).AsQueryable();

            //Filtro Categoria
            if (categoriaId != null)
            {
                productsList = productsList.Where(p => p.categoriaId == categoriaId);
                ViewBag.SelectedCategory = _context.Categoria.Where(category => category.categoriaId == categoriaId).FirstOrDefault();

                //Como en SelectedCategory ya la tengo seleccionada la categoria la saco de la lista para que no se duplique en el filtro.
                ViewBag.Categories = _context.Categoria.Where(category => !category.deprecado && category.categoriaId != categoriaId).ToList();
            }
            else
            {
                //Cargo todas las categorias ya que ninguna está selecionada en el filtro
                ViewBag.Categories = _context.Categoria.Where(category => !category.deprecado).ToList();
            }

            //Filtro Buscar Producto por Nombre
            if (!String.IsNullOrEmpty(searchProductByName))
            {
                productsList = productsList.Where(p => p.nombre.ToUpper().Contains(searchProductByName.ToUpper()));
                ViewBag.SearchProductByName = searchProductByName;
            }

            if (String.IsNullOrEmpty(sortOrder)) sortOrder = "name_asc";

            ViewBag.ProductNameSort = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewBag.ProductPriceSort = sortOrder == "price_asc" ? "price_desc" : "price_asc";

            //Ordeno ascendente y descendente el nombre de los productos y el precio
            switch (sortOrder)
            {
                case "name_asc":
                    productsList = productsList.OrderBy(p => p.nombre);
                    break;
                case "name_desc":
                    productsList = productsList.OrderByDescending(p => p.nombre);
                    break;
                case "price_asc":
                    productsList = productsList.OrderBy(p => p.precio);
                    break;
                case "price_desc":
                    productsList = productsList.OrderByDescending(p => p.precio);
                    break;
            }

            return View(await productsList.ToListAsync());
        }

        public IActionResult Details(int? id)
        {
            Producto product = FindProductById(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = LoadCategoryFilter(null);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("productoId,nombre,precio,cantidad,deprecado,categoriaId")] Producto product)
        {
            Producto optionalProduct = FindProductyByName(product.nombre);

            if (optionalProduct != null)
            {
                ViewBag.ErrorMessage = "El nombre del producto ya existe.";
                ViewBag.Categories = LoadCategoryFilter(product.categoriaId);
                return View(product);
            }

            if (ModelState.IsValid)
            {
                _context.Producto.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = String.Format("El producto {0} se agregó correctamente.", product.nombre);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = LoadCategoryFilter(product.categoriaId);
            return View(product);
        }

        public IActionResult Edit(int? id)
        {
            Producto product = FindProductById(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = LoadCategoryFilter(product.categoriaId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("productoId,nombre,precio,cantidad,deprecado,categoriaId")] Producto product)
        {
            if (ModelState.IsValid)
            {
                _context.Producto.Update(product);
                await _context.SaveChangesAsync();
                ViewBag.SuccessMessage = "Producto actualizado correctamente.";
                ViewBag.Categories = LoadCategoryFilter(product.categoriaId);
                return View(product);
            }

            ViewBag.Categories = LoadCategoryFilter(product.categoriaId);
            return View(product);
        }

        public IActionResult Delete(int? id)
        {
            Producto product = FindProductById(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Producto product = await _context.Producto.FindAsync(id);
            product.deprecado = true;
            _context.Producto.Update(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = String.Format("El producto {0} se elimino correctamente.", product.nombre);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Enable(int? id)
        {
            Producto product = FindProductById(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            product.deprecado = false;
            _context.Producto.Update(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = String.Format("El producto {0} se habilitó correctamente.", product.nombre);
            return RedirectToAction(nameof(Index));
        }

        private Producto FindProductyByName(string productName)
        {
            return _context.Producto.Where(product => product.nombre.ToUpper().Equals(productName.ToUpper())).FirstOrDefault();
        }

        private Producto FindProductById(int? productId)
        {
            if (productId == null) return null;
            Producto product = _context.Producto.Include(product => product.categoria).Where(product => product.productoId == productId).FirstOrDefault();
            if (product == null) return null;
            else return product;
        }

        private SelectList LoadCategoryFilter(int? categoryId)
        {
            //Filtro todas las categorias no eliminadas
            if (categoryId == null) return new SelectList(_context.Categoria.Where(c => !c.deprecado), "categoriaId", "nombre");
            //Ademas selecciono la actual
            return new SelectList(_context.Categoria.Where(c => !c.deprecado), "categoriaId", "nombre", categoryId);
        }
    }
}
