using System;
using System.Collections.Generic;
using System.Text;

namespace VR2ConfiaShopLocalService
{
    public class appSettings
    {

        public int segundosActualizacion { get; set; }
        public string apiURI { get; set; }
        public string servicioObtenerPagos { get; set; }
        public string apiKey { get; set; }
        public string cadenaConexionSql { get; set; }
        public string formatoFechasQueryString { get; set; }
        public int minutosSinInsercionesParaAvisar { get; set; }
        public string pushoverApiKey { get; set; }
        public string pushoverApiKeyUser { get; set; }

        public string appName { get; set; }

    }
}
