﻿using API.CatalogosCartaPorte;
using API.Enums;
using API.Enums.CartaPorteEnums;
using API.Relaciones;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Catalogos
{
    [Table("cat_clientes")]
    public class Cliente
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Informacion
        [DisplayName("Teléfono 1")]
        public String Telefono1 { get; set; }

        [DisplayName("Teléfono 2")]
        public String Telefono2 { get; set; }

        [DisplayName("E-Mail")]
        public String Email { get; set; }
        
        [DisplayName("Fecha de Alta")]
        public DateTime FechaAlta { get; set; }

        public Status Status { get; set; }

        public String Observaciones { get; set; }

        //Facturacion
        [DisplayName("Razón Social")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        public String RazonSocial { get; set; }

        public String Rfc { get; set; }

        [NotMapped]
        [DisplayName("RFC - Razón Social")]
        public String RfcRazonSocial { get { return String.Format("{0} - {1}", Rfc, RazonSocial); } }

        [DisplayName("Domicilio Fiscal")]
        [RegularExpression("[\\s]{0,3}([0-9]{5})[\\s]{0,3}", ErrorMessage = "El código postal tiene que conformarse de 5 caracteres numéricos")]
        public String CodigoPostal { get; set; }

        [DisplayName("Uso Cfdi")]
        public c_UsoCfdiCP UsoCfdi { get; set; }
        
        [DisplayName("País")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        public c_Pais Pais { get; set; }

        [DisplayName("Numero Registro Identificación Tributaria")]
        public string NumRegIdTrib { get; set; }

        
        /*[DisplayName("Domicilio Fiscal")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        public string DomicilioFiscal { get; set; }*/
        
        [DisplayName("Regimen Fiscal")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        public int? RegimenFiscalId { get; set; }
        [ForeignKey("RegimenFiscalId")]
        public virtual RegimenFiscal RegimenFiscal { get; set; }

        [NotMapped]
        public virtual BancoCliente Banco { get; set; }
        public virtual List<BancoCliente> Bancos { get; set; }

        #region Grupo

        [Required(ErrorMessage = "Campo Obligatorio")]
        [DisplayName("Sucursal")]
        public int SucursalId { get; set; }
        [ForeignKey("SucursalId")]
        public virtual Sucursal Sucursal { get; set; }

        #endregion
    }
}
