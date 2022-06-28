
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComercioElectronicoMvc.Data;
using Microsoft.AspNetCore.Http;
using ComercioElectronicoMvc.Models;
using System.Linq;
using System;


namespace ComercioElectronicoMvc.Controllers
{
    public class CarroController : Controller
    {

        private readonly MercadoContext _context;
        public CarroController(MercadoContext context)
        {
            _context = context;
        }

        // GET: CarroController
        public async Task<IActionResult> Index()
        {
            var session = HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);
            if (session == null)
            {
                HttpContext.Session.Clear();
                TempData["SessionExpired"] = "La sesión expiró, ingresa nuevamente.";
                return RedirectToAction("Login", "Usuarios");
            }

            int userId = (int)HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);

            _context.Carro.Include(c => c.Productos).Include(c => c.Rel_Carro_Productos).Load();
            _context.Producto.Include(p => p.categoria).Load();

            Usuario usuario = _context.Usuario.Where(u => u.usuarioId == userId).FirstOrDefault();

            ViewBag.NombreUsuario = usuario.nombre;
            ViewBag.ApellidoUsuario = usuario.apellido;
            ViewBag.EsEmpresa = usuario.esEmpresa;

            //Filtro solo el carro del usuario logueado 
            var carro = await _context.Carro.Where(c => c.carroId == usuario.usuarioId).FirstAsync();

            //Calculo el precio total de la compra
            double precioTotalDeCompra = 0;
            foreach (var rcp in carro.Rel_Carro_Productos)
            {
                precioTotalDeCompra += rcp.producto.precio * rcp.cantidad;
            }

            //Si es empresa le descuento IVA
            if (usuario.esEmpresa) precioTotalDeCompra = precioTotalDeCompra * 0.79;

            //paso el total a la vista
            ViewBag.TotalCompra = precioTotalDeCompra;
            return View(carro);
        }

        public async Task<IActionResult> LoadProducts(int? categoriaId, string sortOrder, string searchProduct)
        {
            var session = HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);
            if (session == null)
            {
                HttpContext.Session.Clear();
                TempData["SessionExpired"] = "La sesión expiró, ingresa nuevamente.";
                return RedirectToAction("Login", "Usuarios");
            }
            int userId = (int) HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);

            //Validaciones de error que se le pasan a la vista
            if (TempData["ErrorValidation"] != null) this.ViewData["ErrorMessage"] = TempData["ErrorValidation"];

            var mercadoContext = _context.Producto.Include(p => p.categoria).Where(p => !p.deprecado && !p.categoria.deprecado && p.cantidad > 0);
            Usuario usuario = _context.Usuario.Where(u => u.usuarioId == userId).FirstOrDefault();
            ViewBag.EsEmpresa = usuario.esEmpresa;

            //Cargo el Filtro de categorias que no estan deprecadas
            ViewData["categorias"] = _context.Categoria.Where(c => !c.deprecado).ToList();

            //Filtro Categoria
            if (categoriaId != null) mercadoContext = mercadoContext.Where(p => p.categoriaId == categoriaId);

            //Filtro Buscar Producto por Nombre
            ViewData["SearchProduct"] = searchProduct;
            if (!String.IsNullOrEmpty(searchProduct)) mercadoContext = mercadoContext.Where(p => p.nombre.ToUpper().Contains(searchProduct.ToUpper()));

            //Ordeno ascendente y descendente el nombre de los productos y el precio
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSort"] = sortOrder == "Precio" ? "price_desc" : "Precio";

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

            //Cargo los productos en el carro
            _context.Carro.Include(c => c.Productos).Load();
            _context.Carro.Include(c => c.Rel_Carro_Productos).Load();
            var carro = await _context.Carro.Where(c => c.carroId == usuario.usuarioId).FirstAsync();
            ViewBag.Rel_Carro_Productos = carro.Rel_Carro_Productos;

            return View(await mercadoContext.ToListAsync());
        }

        public async Task<IActionResult> AgregarAlCarro(int id)
        {
            var session = HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);
            if (session == null)
            {
                HttpContext.Session.Clear();
                TempData["SessionExpired"] = "La sesión expiró, ingresa nuevamente.";
                return RedirectToAction("Login", "Usuarios");
            }

            int userId = (int)HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);

            _context.Carro.Include(c => c.Productos).Load();
            _context.Carro.Include(c => c.Rel_Carro_Productos).Load();
            var carro = await _context.Carro.Where(c => c.carroId == userId).ToArrayAsync();
            // busco el producto a agregar al carro
            foreach (var rcp in carro.First().Rel_Carro_Productos)
            {
                // me fijo si ya está en el carro o no
                if (rcp.productoId == id)
                {
                    //Valido stock para agregar al carro (se valida solo acá porque si es un nuevo producto deberia tener minimo 1 de stock en el catalogo)
                    if (rcp.cantidad < rcp.producto.cantidad)
                    {

                        //si está lo modifico y retorno la vista edit con la nueva cantidad
                        rcp.cantidad++;
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(LoadProducts));
                        //return View("Add", rcp);
                    }
                    else {
                        // Stock insuficiente
                        TempData["ErrorValidation"] = "Error: Stock insuficiente para agregar al carro.";
                        return RedirectToAction(nameof(LoadProducts));
                    }
                }
            }

            //si actualmente no está en el carro, lo agrego y retorno la vista edit con la cantidad
            Rel_Carro_Producto newRcp = new Rel_Carro_Producto(carro.First().carroId, id, 1);
            _context.Add(newRcp);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(LoadProducts));
            //return View("Add", newRcp);
        }

        public async Task<IActionResult> RestarDelCarro(int id)
        {
            var session = HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);
            if (session == null)
            {
                HttpContext.Session.Clear();
                TempData["SessionExpired"] = "La sesión expiró, ingresa nuevamente.";
                return RedirectToAction("Login", "Usuarios");
            }

            int userId = (int)HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);

            _context.Carro.Include(c => c.Productos).Load();
            _context.Carro.Include(c => c.Rel_Carro_Productos).Load();
            var carro = await _context.Carro.Where(c => c.carroId == userId).ToArrayAsync();

            //Verifico si el producto está en el carro o no
            foreach (var rcp in carro.First().Rel_Carro_Productos)
            {
                // me fijo si ya está en el carro o no
                if (rcp.productoId == id)
                {
                    //si está lo modifico y retorno la vista edit con la nueva cantidad
                    if (rcp.cantidad > 0)
                    {
                        if (rcp.cantidad == 1)
                        {
                            _context.Remove(rcp);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(LoadProducts));
                            //return View("Removed");
                        }
                        else
                        {
                            rcp.cantidad--;
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(LoadProducts));
                            //return View("Remove", rcp);
                        }
                    }
                }
            }

            //Si el producto no se encuentra en el carro redirecciono al usuario a la vista correspondiente
            ViewBag.productoId = id;
            return View("WantsToAdd");
        }

        public async Task<IActionResult> EliminarDelCarro(int id)
        {
            var session = HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);
            if (session == null)
            {
                HttpContext.Session.Clear();
                TempData["SessionExpired"] = "La sesión expiró, ingresa nuevamente.";
                return RedirectToAction("Login", "Usuarios");
            }

            int userId = (int)HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);

            _context.Carro.Include(c => c.Productos).Load();
            _context.Carro.Include(c => c.Rel_Carro_Productos).Load();
            var carro = await _context.Carro.Where(c => c.carroId == userId).ToArrayAsync();

            //Verifico si el producto está en el carro o no
            foreach (var rcp in carro.First().Rel_Carro_Productos)
            {
                // me fijo si ya está en el carro o no
                if (rcp.productoId == id)
                {
                   _context.Remove(rcp);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(LoadProducts));
                            //return View("Removed");                        
                }
            }
            //Si el producto no se encuentra en el carro redirecciono al usuario a la vista correspondiente
            ViewBag.productoId = id;
            return View("WantsToAdd");
        }


            // POST: CarroController/Edit/5
            [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CarroController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CarroController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

    }

}
