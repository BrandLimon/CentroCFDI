using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using APBox.Context;
using API.Catalogos;
using System;
using API.Operaciones.OperacionesProveedores;

namespace APBox.Controllers.Catalogos
{
    public class ConfiguracionesRelController : Controller
    {

        #region Variables

        private readonly APBoxContext _db = new APBoxContext();

        #endregion

        // GET: ConfiguracionesRel
        public ActionResult Index()
        {
            ViewBag.Controller = "configuraciones";
            ViewBag.Action = "Index";
            ViewBag.ActionES = "Index";
            ViewBag.Button = "Modificar";
            ViewBag.Title = "Modifica las configuraciones";

            var sucursalId = ObtenerSucursal();
            if (sucursalId == 0) // o cualquier otro valor no válido
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Sucursal no válida.");
            }

            var configuraciones = _db.ConfiguracionesDR.Where(d => d.SucursalId == sucursalId).ToList();
            return View(configuraciones);

        }


        // GET: ConfiguracionesRel/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var configuracion = _db.ConfiguracionesDR.Find(id);
            if (configuracion == null)
            {
                return HttpNotFound();
            }
            return View(configuracion);
        }


        // GET: ConfiguracionesRel/Create
        // public ActionResult Create()
        // {
        //     return View();
        // }

        // POST: ConfiguracionesRel/Create
        // [HttpPost]
        // public ActionResult Create(FormCollection collection)
        // {
        //     try
        //    {
        // TODO: Add insert logic here
        //      return RedirectToAction("Index");
        //      }
        // catch
        //   {
        //          return View();
        //          }
        // }

        // GET: ConfiguracionesRel/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ConfiguracionesDR configuraciones = _db.ConfiguracionesDR.Find(id);
            if (configuraciones == null)
            {
                return HttpNotFound();
            }
            ViewBag.Controller = "Configuracion";
            ViewBag.Action = "Edit";
            ViewBag.ActionES = "Editar";
            ViewBag.Title = "sistema";
            return View(configuraciones);
        }

        // POST: ConfiguracionesRel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult Edit(ConfiguracionesDR configuraciones)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(configuraciones).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(configuraciones);
        }

        // GET: ConfiguracionesRel/Delete/5
        // public ActionResult Delete(int id)
        // {
        //    return View();
        // }

        // POST: ConfiguracionesRel/Delete/5
        // [HttpPost]
        // public ActionResult Delete(int id, FormCollection collection)
        // {
        //     try
        //     {
        //         // TODO: Add delete logic here

        //         return RedirectToAction("Index");
        // }
        //     catch
        //     {
        //         return View();
        // }
        // }
        #region PopulaForma

        private int ObtenerSucursal()
        {
            if (Session["SucursalId"] == null)
            {
                throw new InvalidOperationException("La sucursal no está definida en la sesión.");
            }
            return Convert.ToInt32(Session["SucursalId"]);
        }


        #endregion
    }
}
