using System;
using System.Collections.Generic;
using System.Text;

namespace estadosCBService
{
    public class appSettings
    {

        public int segundosActualizacion { get; set; }
        public string apiURI { get; set; }
        public string servicioObtenerPagos { get; set; }
        public string apiKey { get; set; }
        public string cadenaConexionSql { get; set; }
        public string cadenaConexionSqlC { get; set; }
        public string cadenaConexionSqlCR { get; set; }
        public string cadenaConexionSqlOPOR { get; set; }
        public string cadenaConexionSqlGYT { get; set; }

        public string formatoFechasQueryString { get; set; }
        public int minutosSinInsercionesParaAvisar { get; set; }
        public string pushoverApiKey { get; set; }
        public string pushoverApiKeyUser { get; set; }

        public string appName { get; set; }

    }
}
