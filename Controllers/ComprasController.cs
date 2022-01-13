using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ComercioElectronicoMvc.Data;
using ComercioElectronicoMvc.Models;
using Microsoft.AspNetCore.Http;

namespace ComercioElectronicoMvc.Controllers
{
    public class ComprasController : Controller
    {
        private readonly MercadoContext _context;

        public ComprasController(MercadoContext context)
        {
            _context = context;
        }

        // Muestra los productos de la compra con su info
        public async Task<IActionResult> Details(int? id)
        {
            var session = HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);
            if (session == null)
            {
                HttpContext.Session.Clear();
                TempData["SessionExpired"] = "La sesión expiró, ingresa nuevamente.";
                return RedirectToAction("Login", "Usuarios");
                //return View("~/Views/Usuarios/Login.cshtml");
            }

            int userId = (int)HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);

            if (id == null) return NotFound();
            
            var compra = await _context.Compra.Include(c => c.Rel_Carro_Compras)
                .Include(c => c.Productos).FirstOrDefaultAsync(c => c.compraId == id);

            if (compra == null) return NotFound();

            Usuario usuario = _context.Usuario.Where(u => u.usuarioId == userId).FirstOrDefault();
            if (usuario == null) return NotFound();

            //Lo mismo que el index, modifico que se muestra en el caso de admin o cliente
            if (usuario.esAdministrador) ViewBag.EsAdministrador = true;
            else ViewBag.EsAdministrador = false;

            return View(compra.Rel_Carro_Compras);
        }

        // GET: Compras
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


            Usuario usuario = _context.Usuario.Where(u => u.usuarioId == userId).FirstOrDefault();
            if (usuario == null) return NotFound();

            //Con este boleano modifico que cosas se muestran en el formulario de compra para el admin o cliente
            if (usuario.esAdministrador)
            {
                ViewBag.EsAdministrador = true;

                // Muestro todas las compras realizadas
                return View(await _context.Compra.Include(c => c.usuario).ToListAsync());
            }
            else
            {
                ViewBag.EsAdministrador = false;

                // Filtro las compras que le pertenecen solo a ese cliente
                return View(await _context.Compra.Include(c => c.usuario).Where(c => c.usuarioId == usuario.usuarioId).ToListAsync());
            }
        }

        public async Task<IActionResult> EditPurchaseTotal(int? id)
        {
            if (id == null) return NotFound();

            var compra = await _context.Compra.FirstOrDefaultAsync(c => c.compraId == id);
            if (compra == null) return NotFound();

            return View(compra);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPurchaseTotal(int id, double nuevoTotal)
        {
            var compra = await _context.Compra.FindAsync(id);

            if (id != compra.compraId) return NotFound();

            if (nuevoTotal < 1)
            {
                ViewData["ErrorMessage"] = "Error: La nueva cantidad debe ser mayor a 0.";
                return View(compra);
            }

            if (ModelState.IsValid)
            {
                compra.total = nuevoTotal;
                _context.Update(compra);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(compra);
        }

        public async Task<IActionResult> Create()
        {
            var session = HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);
            if (session == null)
            {
                HttpContext.Session.Clear();
                TempData["SessionExpired"] = "La sesión expiró, ingresa nuevamente.";
                return RedirectToAction("Login", "Usuarios");
            }

            int userId = (int)HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);

            // Busco el carrito del usuario logueado 
            _context.Carro.Include(c => c.Productos).Load();
            _context.Carro.Include(c => c.Rel_Carro_Productos).Load();
            _context.Producto.Include(p => p.categoria).Load();

            Usuario usuario = _context.Usuario.Where(u => u.usuarioId == userId).FirstOrDefault();

            var carro = await _context.Carro.Where(c => c.carroId == usuario.usuarioId).FirstAsync();

            if (carro.Rel_Carro_Productos.Count == 0)
            {
                TempData["ErrorValidation"] = "Error: No se puede finalizar la compra si no hay productos en el carro, agregue algunos.";
                return RedirectToAction("LoadProducts", "Carro");
            }

            //Calculo el total de la compra 
            double totalCompra = 0.0;
           
            foreach (var rcp in carro.Rel_Carro_Productos)
            {
                //Me fijo si hay stock del producto, si no hay, retorno la vista correspondiente 
                if (rcp.producto.cantidad < rcp.cantidad)
                {
                    Producto p = carro.Productos.Where(c => c.productoId == rcp.productoId).First();
                    if (rcp.producto.cantidad == 0)
                    {
                        carro.Productos.Remove(p);
                    }
                    else
                    {
                        rcp.cantidad = rcp.producto.cantidad;
                    }
                    await _context.SaveChangesAsync();
                    TempData["ErrorValidation"] = "Error: Stock insuficiente para efectuar la compra, hemos actualizado la cantidad a la maxima posible en su carro.";
                    return RedirectToAction("LoadProducts", "Carro");
                }

                //Si hay stock sumo el valor al total
                totalCompra += rcp.producto.precio * rcp.cantidad;
            }

            // Si el usuario logueado es empresa se le descuenta un 21% del precio total correspondiente al IVA
            if (usuario.esEmpresa) totalCompra = totalCompra * 0.79;

            // Creo la compra con el importe total, y el id del usuario
            var compra = new Compra
            {
                total = totalCompra,
                usuarioId = usuario.usuarioId,
                Rel_Carro_Compras = new List<Rel_Carro_Compra>()
            };
            _context.Add(compra);

            // Inserto en Rel carro compra un row por cada producto con el id de la compra y el id de producto
            foreach (var rcp in carro.Rel_Carro_Productos)
            {
                double precioProducto = usuario.esEmpresa ? rcp.producto.precio * 0.79 : rcp.producto.precio;

                var rcc = new Rel_Carro_Compra()
                {
                    compra = compra,
                    compraId = compra.compraId,
                    productoId = rcp.productoId,
                    cantidad = rcp.cantidad,
                    precioProducto = precioProducto
                };

                // Bajo la cantidad de cada producto comprado en la tabla de productos
                Producto producto = await _context.Producto.Where(p => p.productoId == rcp.productoId).FirstAsync();
                producto.cantidad -= rcp.cantidad;

                // Añado la relacion a la lista de relaciones en compra
                compra.Rel_Carro_Compras.Add(rcc);
                _context.Add(rcc);
            }

            // Vacio el carrito del usuario
            carro.Rel_Carro_Productos.Clear();
            carro.Productos.Clear();

            // Guardo los cambios 
            await _context.SaveChangesAsync();

            // Retorno la vista con el mensaje correspondiente 
            ViewBag.NombreUsuario = usuario.nombre;
            ViewBag.ApellidoUsuario = usuario.apellido;
            ViewBag.CompraSatisfactoria = "Muchas gracias " + usuario.nombre +", la compra ha sido satisfactoria.";
            return View("~/Views/Carro/Index.cshtml", carro);
        }

       
    }    
}
