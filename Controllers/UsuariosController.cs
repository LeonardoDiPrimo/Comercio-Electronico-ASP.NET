using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ComercioElectronicoMvc.Data;
using Microsoft.AspNetCore.Http;

namespace ComercioElectronicoMvc.Models
{
    public class UsuariosController : Controller
    {
        private readonly MercadoContext _context;

        public UsuariosController(MercadoContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            if (TempData["ErrorValidation"] != null) this.ViewData["ErrorMessage"] = TempData["ErrorValidation"];
            return View(await _context.Usuario.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(m => m.usuarioId == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        //Detalle del usuario desde Mis Datos
        public async Task<IActionResult> DetailsUser()
        {
            //chequeo si hay una sesion 
            if (HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey) == null)
            {
                this.ViewData["ErrorMessage"] = "La sesión expiró, ingresa nuevamente.";
                return View("Login");
            }
            
            int id = (int)HttpContext.Session.GetInt32(MercadoContext.loggedInUserIdKey);

            if (id == 0) return NotFound();
            
            var usuario = await _context.Usuario .FirstOrDefaultAsync(m => m.usuarioId == id);
            if (usuario == null) return NotFound();
            
            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("usuarioId,dni,cuilCuit,nombre,apellido,mail,password,esAdministrador,esEmpresa,deprecado")] Usuario usuario)
        {
            

            if (ModelState.IsValid)
            {
                //Valido que no exista un usuario con el mismo DNI o Cuit/Cuil
                Usuario repeatUser = RepeatUser(usuario.dni, usuario.cuilCuit);
                if (repeatUser != null) {
                    this.ViewData["ErrorMessage"] = "Error: DNI o CUIL/CUIT duplicado";
                    return View(usuario);
                }

                //Un usuario no deberia poder ser administrador y empresa al mismo tiempo
                if (usuario.esAdministrador && usuario.esEmpresa) {
                    this.ViewData["ErrorMessage"] = "Error: Un usuario no puede ser administrador y empresa a la vez";
                    return View(usuario);
                }

                usuario.password = Utilities.GetStringSha256Hash(usuario.password);

                usuario.carro = new Carro();
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // Este form tiene oculto el booleano administrador y hace rollback al login en el caso que sea exitoso la creacion de usuario desde el login
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([Bind("usuarioId,dni,cuilCuit,nombre,apellido,mail,password,esEmpresa,deprecado")] Usuario usuario)
        {
            usuario.password = Utilities.GetStringSha256Hash(usuario.password);

            if (ModelState.IsValid)
            {
                //Valido que no exista un usuario con el mismo DNI o Cuit/Cuil
                Usuario repeatUser = RepeatUser(usuario.dni, usuario.cuilCuit);
                if (repeatUser != null)
                {
                    this.ViewData["ErrorMessage"] = "Error: DNI o CUIL/CUIT duplicado";
                    return View(usuario);
                }

                usuario.carro = new Carro();
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Login));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.deprecado)
            {
                TempData["ErrorValidation"] = "Error: No se puede modificar un usuario eliminado";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("usuarioId,dni,cuilCuit,nombre,apellido,mail,password,esAdministrador,esEmpresa,deprecado,esBloqueado,reitentosBloqueo")] Usuario usuario)
        {
            if (id != usuario.usuarioId)
            {
                return NotFound();
            }

            //Un usuario no deberia poder ser administrador y empresa al mismo tiempo
            if (usuario.esAdministrador && usuario.esEmpresa)
            {
                this.ViewData["ErrorMessage"] = "Error: Un usuario no puede ser administrador y empresa a la vez";
                return View(usuario);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.usuarioId))
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
            return View(usuario);
        }

        // Este metodo se invoca cuando se quiere modificar el usuario desde los formularios del carro (Mis Datos)
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [Bind("usuarioId,dni,cuilCuit,nombre,apellido,mail,password,esAdministrador,esEmpresa,deprecado,esBloqueado,reitentosBloqueo")] Usuario usuario)
        {
            if (id != usuario.usuarioId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.usuarioId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(DetailsUser));
            }
            return View(usuario);
        }

        public async Task<IActionResult> UpdatePassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(int id, String contraseñaActual, String nuevaContraseña, String repetirContraseña)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (id != usuario.usuarioId) return NotFound();
            //Validaciones del formulario
            contraseñaActual = Utilities.GetStringSha256Hash(contraseñaActual);
            if (!usuario.password.Equals(contraseñaActual))
            {
                ViewData["ErrorMessage"] = "Error: La contraseña actual es incorrecta.";
                return View(usuario);
            }

            if (!nuevaContraseña.Equals(repetirContraseña))
            {
                ViewData["ErrorMessage"] = "Error: El campo Nueva Contraseña y Repetir Contraseña son distintos.";
                return View(usuario);
            }

            if (contraseñaActual.Equals(Utilities.GetStringSha256Hash(nuevaContraseña)))
            {
                ViewData["ErrorMessage"] = "Error: La nueva contraseña no puede ser igual a la actual.";
                return View(usuario);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    usuario.password = Utilities.GetStringSha256Hash(nuevaContraseña);
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.usuarioId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";
                return RedirectToAction(nameof(DetailsUser));
            }
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(m => m.usuarioId == id);
            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.deprecado)
            {
                TempData["ErrorValidation"] = "Error: El usuario ya se encuentra eliminado";
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            usuario.deprecado = true;
            _context.Usuario.Update(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UnlockUser(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.usuarioId == id);
            if (usuario == null) return NotFound();

            if (usuario.deprecado) {
                TempData["ErrorValidation"] = "Error: No se puede modificar un usuario eliminado";
                return RedirectToAction(nameof(Index));
            }

            if (usuario.esBloqueado)
            {
                usuario.reitentosBloqueo = 0;
                usuario.esBloqueado = false;
                _context.Usuario.Update(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["ErrorValidation"] = "Error: El usuario no se encuentra bloqueado";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Login()
        {
            HttpContext.Session.Clear();

            if (TempData["ErrorValidation"] != null) this.ViewData["ErrorMessage"] = TempData["ErrorValidation"];            

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FindAccount(int dni, string password)
        {
            password = Utilities.GetStringSha256Hash(password);
            if (ModelState.IsValid)
            {
                Usuario usuario = FindUserByDni(dni);

                if (usuario == null || usuario.deprecado)
                {
                    TempData["ErrorValidation"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Login));
                }

                else if (usuario.esBloqueado)
                {
                    TempData["ErrorValidation"] = "El usuario está bloqueado";
                    return RedirectToAction(nameof(Login));
                }

                else if (!usuario.password.Equals(password))
                {
                    usuario.reitentosBloqueo += 1;

                    if (usuario.reitentosBloqueo == 3) usuario.esBloqueado = true;

                    _context.Usuario.Update(usuario);
                    await _context.SaveChangesAsync();
                    TempData["ErrorValidation"] = "La contraseña ingresada es incorrecta";
                    return RedirectToAction(nameof(Login));
                }

                else if (usuario.esAdministrador) {
                    
                    // Si el usuario ingreso la contraseña mal, pero despues bien antes que se bloquee la cuenta entonces devuelvo el contador a 0. Tienen que ser 3 fallos consecutivos
                    if (usuario.reitentosBloqueo > 0) {
                        usuario.reitentosBloqueo = 0;
                        _context.Usuario.Update(usuario);
                        await _context.SaveChangesAsync();

                    }
                    // guardo el admin logueado en el mercado context
                    //MercadoContext.loggedInUserId = usuario.usuarioId;
                    HttpContext.Session.SetInt32(MercadoContext.loggedInUserIdKey, usuario.usuarioId);
                    return RedirectToAction("Index", "Categorias");
                }

                else  {

                    // Lo mismo que con el administrador
                    if (usuario.reitentosBloqueo > 0)
                    {
                        usuario.reitentosBloqueo = 0;
                        _context.Usuario.Update(usuario);
                        await _context.SaveChangesAsync();
                    }

                    // guardo el usuario logueado en el mercado context
                    HttpContext.Session.SetInt32(MercadoContext.loggedInUserIdKey, usuario.usuarioId);
                    //MercadoContext.loggedInUserId = usuario.usuarioId;
                    return RedirectToAction("LoadProducts", "Carro");
                }
            }

            return RedirectToAction(nameof(Login));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.usuarioId == id);
        }
        
        private Usuario FindUserByDni(int dni)
        {
            return _context.Usuario.Where(u => u.dni == dni).FirstOrDefault();
        }

        private Usuario RepeatUser(int dni, Int64 cuilCuit)
        {
            return _context.Usuario.Where(u => u.dni == dni || u.cuilCuit == cuilCuit).FirstOrDefault();
        }
    }
}
