using System;
using System.Collections.Generic;
using System.Text;

namespace VR2ConfiaShopLocalService
{
    class Pago
    {
        public long pagoId { get; set; }
        public long clienteId { get; set; }
        public string clienteNombre { get; set; }
        public short corresponsalId { get; set; }
        public string corresponsal { get; set; }
        public Guid uid { get; set; }
        public string corresponsalTransaccionId { get; set; }
        public byte pagoEstatusId { get; set; }
        public string referencia { get; set; }
        public decimal importe { get; set; }
        public decimal comision { get; set; }
        public decimal impuestos { get; set; }
        //public long movimientoIdTransfPago { get; set; }
        //public long movimientoIdTransfDeposito { get; set; }
        public decimal importeDisponible { get; set; }
        public DateTime fhTransaccion { get; set; }
        public DateTime fhRegistro { get; set; }
        public string hostRegistro { get; set; }
        public string corresponsalAutorizacion { get; set; }
        public string corresponsalSucursalRef { get; set; }
        public string corresponsalSucursal { get; set; }
        public DateTime fhConcentracion { get; set; }
        public DateTime fhUltimaActualizacion { get; set; }
    }

}