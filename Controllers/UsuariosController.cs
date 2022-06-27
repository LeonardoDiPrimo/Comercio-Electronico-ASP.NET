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

            var usuario = await _context.Usuario.FirstOrDefaultAsync(m => m.usuarioId == id);
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
                if (repeatUser != null)
                {
                    this.ViewData["ErrorMessage"] = "Error: DNI o CUIL/CUIT duplicado";
                    return View(usuario);
                }

                //Un usuario no deberia poder ser administrador y empresa al mismo tiempo
                if (usuario.esAdministrador && usuario.esEmpresa)
                {
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
            if (ModelState.IsValid)
            {
                //Valido que no existan 2 usuarios con el mismo email.
                Usuario repeatEmail = FindUserByEmail(usuario.mail);
                if (repeatEmail != null)
                {
                    this.ViewData["ErrorMessage"] = "Ya existe una cuenta con el correo electrónico ingresado.";
                    return View(usuario);
                }

                usuario.password = Utilities.GetStringSha256Hash(usuario.password);
                usuario.carro = new Carro();
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["AccountCreated"] = "Cuenta creada exitosamente.";
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

            if (usuario.deprecado)
            {
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

            if (TempData["ErrorValidation"] != null) this.ViewData["ErrorMessage"]   = TempData["ErrorValidation"];
            if (TempData["AccountCreated"] != null)  this.ViewData["AccountCreated"] = TempData["AccountCreated"];

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FindAccount(string mail, string password)
        {
            if (ModelState.IsValid)
            {
                Usuario usuario = FindUserByEmail(mail);

                if (usuario == null || usuario.deprecado)
                {
                    TempData["ErrorValidation"] = "El correo ingresado no existe.";
                    return RedirectToAction(nameof(Login));
                }

                if (usuario.esBloqueado)
                {
                    TempData["ErrorValidation"] = "La cuenta está bloqueada, contacte a soporte.";
                    return RedirectToAction(nameof(Login));
                }

                password = Utilities.GetStringSha256Hash(password);

                if (!usuario.password.Equals(password))
                {
                    usuario.reitentosBloqueo += 1;

                    if (usuario.reitentosBloqueo == 3)
                    {
                        usuario.esBloqueado = true;
                        _context.Usuario.Update(usuario);
                        await _context.SaveChangesAsync();
                        TempData["ErrorValidation"] = "La cuenta está bloqueada, contacte a soporte.";
                        return RedirectToAction(nameof(Login));
                    }
                    else
                    {
                        _context.Usuario.Update(usuario);
                        await _context.SaveChangesAsync();
                        int pendingRetries = 3 - usuario.reitentosBloqueo;
                        TempData["ErrorValidation"] = string.Format("Contraseña incorrecta. Quedan {0} reintentos antes que se bloquee la cuenta.", pendingRetries);
                        return RedirectToAction(nameof(Login));
                    }
                }

                //Restablecer los intentos
                if (usuario.reitentosBloqueo > 0)
                {
                    usuario.reitentosBloqueo = 0;
                    _context.Usuario.Update(usuario);
                    await _context.SaveChangesAsync();
                }

                // guardo el usuario logueado en el mercado context
                HttpContext.Session.SetInt32(MercadoContext.loggedInUserIdKey, usuario.usuarioId);

                if (usuario.esAdministrador) return RedirectToAction("Index", "Categorias");
                else return RedirectToAction("LoadProducts", "Carro");
            }
            return RedirectToAction(nameof(Login));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.usuarioId == id);
        }

        private Usuario FindUserByEmail(string email)
        {
            return _context.Usuario.Where(user => user.mail == email).FirstOrDefault();
        }

        private Usuario RepeatUser(int dni, Int64 cuilCuit)
        {
            return _context.Usuario.Where(u => u.dni == dni || u.cuilCuit == cuilCuit).FirstOrDefault();
        }
    }
}
