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
        
        public string apiKeyFirebase { get; set; }
        public string userAuthFirebase { get; set; }
        public string passAuthFurebase { get; set; }
        public string bucket { get; set; }
        public string userFtpConfia { get; set; }
        public string passFtpConfia { get; set; }
        public string userFtpOpor { get; set; }
        public string passFtpOpor { get; set; }
        public string userFtpCrece { get; set; }
        public string passFtpCrece { get; set; }
        public string userFtpGyt { get; set; }
        public string passFtpGyt { get; set; }
    }
}
