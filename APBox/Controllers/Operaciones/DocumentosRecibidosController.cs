﻿using APBox.Context;
using API.Catalogos;
using API.Enums;
using API.Models.DocumentosRecibidos;
using API.Models.Dto;
using API.Operaciones.OperacionesProveedores;
using Aplicacion.LogicaPrincipal.DocumentosRecibidos;
using Aplicacion.LogicaPrincipal.Facturas;
using Newtonsoft.Json;
using SW.Services.Authentication;
using SW.Services.Validate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Utilerias.LogicaPrincipal;

namespace APBox.Controllers.Operaciones
{
    public class DocumentosRecibidosController : Controller
    {
        private readonly APBoxContext _db = new APBoxContext();
        private readonly ProcesaDocumentoRecibido _procesaDocumentoRecibido = new ProcesaDocumentoRecibido();
        private readonly Decodificar _decodifica = new Decodificar();
        // GET: DocumentosRecibidos
        public ActionResult Index()
        {
            ViewBag.Controller = "DocumentosRecibidos";
            ViewBag.Action = "Index";
            ViewBag.ActionES = "Index";
            ViewBag.Button = "CargaDocumentoRecibido";
            ViewBag.NameHere = "proveedor";
            //get usaurio
            var usuario = _db.Usuarios.Find(ObtenerUsuario());
            if (usuario.esProveedor)
            {
                ViewBag.isProveedor = "Proveedor";
            }
            else
            {
                ViewBag.isProveedor = "Usuario";
            }
             

            var documentosRecibidosModel = new DocumentosRecibidosModel();
            var fechaInicial = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1, 0, 0, 0);
            var fechaFinal = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
            documentosRecibidosModel.FechaInicial = fechaInicial;
            documentosRecibidosModel.FechaFinal = fechaFinal;
                if (usuario.esProveedor)
                {
                    documentosRecibidosModel.DocumentosRecibidos = _procesaDocumentoRecibido.Filtrar(fechaInicial, fechaFinal, usuario.Id, (int)usuario.SocioComercialID);
                }
                else
                {
                    documentosRecibidosModel.DocumentosRecibidos = _procesaDocumentoRecibido.Filtrar(fechaInicial, fechaFinal, usuario.Id, null);
                }
            return View(documentosRecibidosModel);
        }
        [HttpPost]
        public ActionResult Index(DocumentosRecibidosModel documentosRecibidosModel)
        {
            ViewBag.Controller = "DocumentosRecibidos";
            ViewBag.Action = "Index";
            ViewBag.ActionES = "Index";
            ViewBag.Button = "CargaDocumentoRecibido";
            ViewBag.NameHere = "proveedor";
            //get usaurio
            var usuario = _db.Usuarios.Find(ObtenerUsuario());
            if (usuario.esProveedor)
            {
                ViewBag.isProveedor = "Proveedor";
            }
            else
            {
                ViewBag.isProveedor = "Usuario";
            }
            DateTime fechaI = documentosRecibidosModel.FechaInicial;
            DateTime fechaF = documentosRecibidosModel.FechaFinal;
            
            var fechaInicial = new DateTime(fechaI.Year, fechaI.Month, fechaI.Day, 0, 0, 0);
            var fechaFinal = new DateTime(fechaF.Year, fechaF.Month, fechaF.Day, 23, 59, 59);
                if (usuario.esProveedor)
                {
                    documentosRecibidosModel.DocumentosRecibidos = _procesaDocumentoRecibido.Filtrar(fechaInicial, fechaFinal, usuario.Id, (int)usuario.SocioComercialID);
                }
                else
                {
                    documentosRecibidosModel.DocumentosRecibidos = _procesaDocumentoRecibido.Filtrar(fechaInicial, fechaFinal, usuario.Id, null);
                }
            
            return View(documentosRecibidosModel);
        }

        public ActionResult CargaCfdi()
        {
            ViewBag.Controller = "DocumentosRecibidos";
            ViewBag.Action = "CargaCfdi";
            ViewBag.NameHere = "proveedor";
            //ViewBag.ActionES = "CargaCfdi";

            //get usaurio
            var usuario = _db.Usuarios.Find(ObtenerUsuario());
            if (usuario.esProveedor)
            {
                ViewBag.isProveedor = "Proveedor";
            }
            else
            {
                ViewBag.isProveedor = "Usuario";
            }
            DocumentosRecibidosDR documentoRecibidoDr = new DocumentosRecibidosDR()
            {
                Validaciones = new ValidacionesDR()
            };
            
            documentoRecibidoDr.Procesado = false;
            return View(documentoRecibidoDr);
        }
        [HttpPost]
        public ActionResult CargaCfdi(DocumentosRecibidosDR documentoRecibidoDr)
        {
            ViewBag.Controller = "DocumentosRecibidos";
            ViewBag.Action = "CargaCfdi";
            ViewBag.NameHere = "proveedor";
            //ViewBag.ActionES = "Index";
            //get usaurio
            var usuario = _db.Usuarios.Find(ObtenerUsuario());
            if (usuario.esProveedor)
            {
                ViewBag.isProveedor = "Proveedor";
            }
            else
            {
                ViewBag.isProveedor = "Usuario";
            }
            PathArchivosDto archivo;
            ValidateXmlResponse responseValidacion =  new ValidateXmlResponse();
            AuthResponse responseAutenticacion = new AuthResponse();
            ComprobanteCFDI cfdi = new ComprobanteCFDI();
            documentoRecibidoDr.DetalleArrays = new List<String>();

            try
            {
                archivo = SubeArchivo();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", String.Format("No se pudo cargar el archivo: {0}", ex.Message));
                return View(documentoRecibidoDr);
            }
            try
            {
                //Deserealiza XML
                cfdi = _procesaDocumentoRecibido.DecodificaXML(archivo.PathDestinoXml);
                var timbreFiscalDigital = _decodifica.DecodificarTimbre(cfdi,null);
                var sucursal = _db.Sucursales.Find(ObtenerSucursal());
                var socioComercial = new Cliente();
                
                documentoRecibidoDr.Validaciones = new ValidacionesDR();
                if (usuario.esProveedor)
                {
                    socioComercial = _db.Clientes.Where(s => s.Rfc == cfdi.Emisor.Rfc && s.SucursalId == sucursal.Id).FirstOrDefault();
                    var existUUID = _db.DocumentoRecibidoDr.Where(dr =>dr.CfdiRecibidos_UUID == timbreFiscalDigital.UUID).FirstOrDefault();
                    if (usuario.SocioComercial.Rfc != cfdi.Emisor.Rfc)
                    {
                        throw new Exception("Error Validación : El archivo cargado no coincide con el Rfc emisor al socio comercial");

                    }
                    if(sucursal.Rfc != cfdi.Receptor.Rfc)
                    {
                        throw new Exception("Error Validación : El archivo cargado no coincide con el Rfc receptor ala empresa asignada");
                    }
                    if(existUUID != null)
                    {
                        throw new Exception("Error Validación : El archivo ya se encuentra cargado en el sistema");
                    }
                }
                else
                {
                    if (cfdi.Receptor.Rfc == "XEXX010101000" || cfdi.Receptor.Rfc == "XAXX010101000")
                    {
                        socioComercial = _db.Clientes.Where(s => s.Rfc == cfdi.Receptor.Rfc && s.RazonSocial == cfdi.Receptor.Nombre && s.SucursalId == sucursal.Id).FirstOrDefault();
                    }
                    else
                    {
                        socioComercial = _db.Clientes.Where(s => s.Rfc == cfdi.Receptor.Rfc && s.SucursalId == sucursal.Id).FirstOrDefault();
                    }

                }
                if (socioComercial == null)
                {
                    throw new Exception("Error Validación : El socio comercial no esta registrado en la BD");
                }
                //autenticacion
                responseAutenticacion = _procesaDocumentoRecibido.GetToken();
                if (responseAutenticacion.data.token != null)
                {
                    //Valida CFDI
                    responseValidacion = _procesaDocumentoRecibido.ValidaCfdi(responseAutenticacion.data.token,archivo.PathDestinoXml);
                    if (responseValidacion == null) { throw new Exception("Error response validación CFDI : null"); }
                    if (responseValidacion.status == "success")
                    {
                        documentoRecibidoDr.EstadoComercial = c_EstadoComercial.Aprobado;
                        documentoRecibidoDr.Procesado = true;
                        //Para iterar la lista sobre la validacion estructura
                        List<Detail> detail1 = responseValidacion.detail;
                        StringBuilder sb = new StringBuilder();
                        var count = detail1.Count();
                        var limite = 0;
                        foreach (var detalle in detail1)
                        {
                            limite++;
                            var limiteDetail = 0;
                            foreach (var nodedetalle in detalle.detail)
                            {
                                limiteDetail++;
                                sb.AppendLine(limite + "." + limiteDetail + " " + detalle.section + ": " + nodedetalle.message + " - " + nodedetalle.messageDetail);
                                //add validaciones
                                if (limite < count)
                                {
                                    documentoRecibidoDr.DetalleArrays.Add(limite + "." + limiteDetail + " " + detalle.section + ": " + nodedetalle.message + " - " + nodedetalle.messageDetail + "\r\n");
                                }
                                else { documentoRecibidoDr.DetalleArrays.Add(limite + "." + limiteDetail + " " + detalle.section + ": " + nodedetalle.message + " - " + nodedetalle.messageDetail); }
                                
                            }
                            
                        }
                        documentoRecibidoDr.Validaciones.Detalle = sb.ToString();
                        documentoRecibidoDr.Validaciones_Detalle = sb.ToString();
                        documentoRecibidoDr.Validaciones.Fecha = DateTime.Now;
                    }
                }

                //addd socio comercial
                documentoRecibidoDr.SocioComercial_Id = socioComercial.Id;
                //add usuario
                documentoRecibidoDr.Usuario_Id = usuario.Id;
                documentoRecibidoDr.CfdiRecibidos_Serie = cfdi.Serie;
                documentoRecibidoDr.CfdiRecibidos_Folio = cfdi.Folio;
                documentoRecibidoDr.Moneda_Id = cfdi.Moneda;
                
                documentoRecibidoDr.FechaComprobante = Convert.ToDateTime(cfdi.Fecha);
                documentoRecibidoDr.CfdiRecibidos_UUID = timbreFiscalDigital.UUID;
                documentoRecibidoDr.FechaEntrega = DateTime.Now;
                documentoRecibidoDr.TipoDocumentoRecibido = c_TipoDocumentoRecibido.CFDI;
                documentoRecibidoDr.Monto = cfdi.Total;
                //add data validacion correcta
                
                documentoRecibidoDr.PathArchivoXml = archivo.PathDestinoXml;
                documentoRecibidoDr.PathArchivoPdf = archivo.PathDestinoPdf;
                
            }
            catch (Exception ex)
            {
                var errores = ex.Message.Split('|');
                foreach (var error in errores)
                {
                    ModelState.AddModelError("", error);
                }
                //delete Files
                if (archivo.PathDestinoXml != null)
                {
                    System.IO.File.Delete(archivo.PathDestinoXml);
                }
                if(archivo.PathDestinoPdf != null)
                {
                    System.IO.File.Delete(archivo.PathDestinoPdf);
                }
                documentoRecibidoDr.Procesado = false;
                documentoRecibidoDr.DetalleArrays = null;
            }

            return View(documentoRecibidoDr);
        }
        // GET: DocumentosRecibidos/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }


        // GET: DocumentosRecibidos/Create
        public ActionResult Create()
        {
            ViewBag.NameHere = "proveedor";
            //get usaurio
            var usuario = _db.Usuarios.Find(ObtenerUsuario());
            if (usuario.esProveedor)
            {
                ViewBag.isProveedor = "Proveedor";
            }
            else
            {
                ViewBag.isProveedor = "Usuario";
            }
            return View();
        }

        // POST: DocumentosRecibidos/Create
        [HttpPost]
        public ActionResult Create(DocumentosRecibidosDR documentoRecibidoDr)
        {
            //get usaurio
            var usuario = _db.Usuarios.Find(ObtenerUsuario());
            ComprobanteCFDI cfdi = new ComprobanteCFDI();
            ViewBag.NameHere = "proveedor";
            if (usuario.esProveedor)
            {
                ViewBag.isProveedor = "Proveedor";
            }
            else
            {
                ViewBag.isProveedor = "Usuario";
            }
            try
            {

                cfdi = _procesaDocumentoRecibido.DecodificaXML(documentoRecibidoDr.PathArchivoXml);
                var sucursal = _db.Sucursales.Where(s => s.Rfc == cfdi.Emisor.Rfc).FirstOrDefault();
                
                var timbreFiscalDigital = _decodifica.DecodificarTimbre(cfdi, null);

                documentoRecibidoDr.RecibidosXml = new RecibidosXMLDR();
                documentoRecibidoDr.RecibidosPdf = new RecibidosPDFDR(); 
                //insert files
                byte[] xmlFile = System.IO.File.ReadAllBytes(documentoRecibidoDr.PathArchivoXml);
                documentoRecibidoDr.RecibidosXml.Archivo = xmlFile;
                if(documentoRecibidoDr.PathArchivoPdf != null)
                {
                    byte[] pdfFile = System.IO.File.ReadAllBytes(documentoRecibidoDr.PathArchivoPdf);
                    documentoRecibidoDr.RecibidosPdf.Archivo = pdfFile;
                }
                else { documentoRecibidoDr.RecibidosPdf = null; documentoRecibidoDr.CfdiRecibidos_PDF_Id = null; }
                //table validaciones
                if (documentoRecibidoDr.Validaciones == null)
                {
                    documentoRecibidoDr.Validaciones = null; documentoRecibidoDr.Validaciones_Id = null;
                }
                // table recibidosComprobante
                documentoRecibidoDr.RecibidosComprobante = new RecibidosComprobanteDR()
                {
                    Sucursal_Id = sucursal.Id,
                    SocioComercial_Id = (int)documentoRecibidoDr.SocioComercial_Id,
                    Fecha = documentoRecibidoDr.FechaComprobante,
                    Serie = cfdi.Serie,
                    Folio = cfdi.Folio,
                    TipoComprobante = cfdi.TipoDeComprobante,
                    Version = cfdi.Version,
                    FormaPago = cfdi.FormaPago,
                    Moneda = cfdi.Moneda,
                    TipoCambio = (double)cfdi.TipoCambio,
                    LugarExpedicion = cfdi.LugarExpedicion,
                    MetodoPago = cfdi.MetodoPago,
                    Descuento = (double)cfdi.Descuento,
                    Subtotal = (double)cfdi.SubTotal,
                    Total = (double)cfdi.Total,
                    TotalImpuestosTrasladados = (double)cfdi.Impuestos.TotalImpuestosTrasladados,
                    TotalImpuestosRetenidos = (double)cfdi.Impuestos.TotalImpuestosTrasladados
                };
                documentoRecibidoDr.CfdiRecibidos_Id = null;
                // table solicitudes
                documentoRecibidoDr.Solicitudes = null;
                documentoRecibidoDr.Solicitud_Id = null;
                documentoRecibidoDr.Pagos = null;
                documentoRecibidoDr.Pagos_Id = null;
                _db.DocumentoRecibidoDr.Add(documentoRecibidoDr);
                _db.SaveChanges();
                return Json(new { success = true, redirectTo = Url.Action("Index", "DocumentosRecibidos") });
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return Json(new { error = true, redirectTo = Url.Action("CargaCfdi", "DocumentosRecibidos") });
           
        }

        // GET: DocumentosRecibidos/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DocumentosRecibidos/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DocumentosRecibidos/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DocumentosRecibidos/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        private PathArchivosDto SubeArchivo()
        {
            PathArchivosDto pathArchivos = new PathArchivosDto();
            if (Request.Files.Count > 0)
            {
                var operacionesStreams = new OperacionesStreams();
                for (var r = 0; r< Request.Files.Count;r ++) {
                    var archivo = Request.Files[r];
                    var extencion = Path.GetExtension(archivo.FileName);
                    if(extencion == ".xml" || extencion == ".XML")
                    {
                        var pathDestinoXml = Path.Combine(Server.MapPath("~/Archivos/DocumentosProveedores/"), archivo.FileName);
                        Stream fileStream = archivo.InputStream;
                        var mStreamer = new MemoryStream();
                        mStreamer.SetLength(fileStream.Length);
                        fileStream.Read(mStreamer.GetBuffer(), 0, (int)fileStream.Length);
                        mStreamer.Seek(0, SeekOrigin.Begin);
                        operacionesStreams.StreamArchivo(mStreamer, pathDestinoXml);

                        pathArchivos.PathDestinoXml = pathDestinoXml;
                    }
                    if(extencion == ".pdf" || extencion == ".PDF")
                    {
                        var pathDestinoPdf = Path.Combine(Server.MapPath("~/Archivos/DocumentosProveedores/"), archivo.FileName);
                        Stream fileStream = archivo.InputStream;
                        var mStreamer = new MemoryStream();
                        mStreamer.SetLength(fileStream.Length);
                        fileStream.Read(mStreamer.GetBuffer(), 0, (int)fileStream.Length);
                        mStreamer.Seek(0, SeekOrigin.Begin);
                        operacionesStreams.StreamArchivo(mStreamer, pathDestinoPdf);

                        pathArchivos.PathDestinoPdf = pathDestinoPdf;
                    }
                    
                }
                
                return pathArchivos;
            }
            throw new Exception("Favor de cargar por lo menos un archivo");
        }

        public static string FechaFormat(string fecha)
        {
            DateTime fechaConvertDate = Convert.ToDateTime(fecha);
            string fechaFormat = fechaConvertDate.ToString("dd/MM/yyyy");
            return fechaFormat;
        }
        private int ObtenerGrupo()
        {

            return Convert.ToInt32(Session["GrupoId"]);
        }
        private int ObtenerSucursal()
        {
            return Convert.ToInt32(Session["SucursalId"]);
        }

        private int ObtenerUsuario()
        {
            return Convert.ToInt32(Session["UsuarioId"]);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
