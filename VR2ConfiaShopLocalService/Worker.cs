using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PushoverClient;
using RestSharp;

namespace estadosCBService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IOptions<appSettings> _config;

        public Worker(ILogger<Worker> logger, IOptions<appSettings> config)
        {
            _logger = logger;
            _config = config;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            string mensaje = string.Format("El servicio se ha detenido...");
            _logger.LogInformation(mensaje);
            EnviaPush("Mensaje Informativo", mensaje);
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
            DateTime fhUltimaActualizacion = DateTime.MinValue;
            DateTime fhUltimaInsercionOAviso = DateTime.Now;
            List<Autorizacion> Autorizaciones = new List<Autorizacion>();
            string mensaje = string.Format("El servicio se ha iniciado...");
            _logger.LogInformation(mensaje);
            EnviaPush("Mensaje Informativo", mensaje);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {

                    _logger.LogInformation("Rutina iniciada a las: {time}", DateTimeOffset.Now);

                    List<XrbIds> xrbIds = new List<XrbIds>();

                    //Solicitudes en estado en espera de consulta de buro
                    List<Solicitud>  solicitudes = await FireStore.GetSolicitudesFromFireStore();

                    foreach (Solicitud item in solicitudes)
                    {
                        if(item.CB_idXRB > 0)
                        {
                            xrbIds.Add(new XrbIds { XrbId = item.CB_idXRB });
                        }               
                    }
                    
                    if(xrbIds.Count > 0)
                    {
                        using (SqlConnection connection = new SqlConnection(_config.Value.cadenaConexionSql))
                        {

                            DataTable dt = new DataTable("Prueba");
                            dt.Columns.Add("id", typeof(int));

                            foreach (XrbIds value in xrbIds)
                            {
                                DataRow dr = dt.NewRow();
                                dr["id"] = value.XrbId;
                                dt.Rows.Add(dr);
                            }
                            IDataReader idr = dt.CreateDataReader();

                            SqlParameter[] Parameters =
                            {
                                new SqlParameter("@ids", idr) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.ttIdXRBConsulta"},
                            };

                            DataTable exec = SqlHelper.ExecuteDataTable(connection, CommandType.StoredProcedure, "wsSolicitudesBuroAtendidas", Parameters, 1);

                            foreach (DataRow dataRow in exec.Rows)
                            {
                                if (dataRow[7].ToString() == "True" && dataRow[4].ToString() == "OK.")
                                {
                                    String url = "";
                                    url = await GetUrlPdfBuro(dataRow[1].ToString(), dataRow[0].ToString(), int.Parse(dataRow[13].ToString()));

                                    //status 9
                                    if(url != "") 
                                    {
                                        if (await FireStore.ActualizaStatusConsulta(dataRow[0].ToString(), dataRow[4].ToString(), 9))
                                        {
                                            _logger.LogInformation("Solicitud Relacionada a {0} actualizada a 'POR AUTORIZAR'", dataRow[0].ToString());
                                        }
                                        else
                                        {
                                            _logger.LogError("ERROR Solicitud Relacionada a {0} NO actualizada a status 9", dataRow[0].ToString());
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogError("ERROR: No se pudo obtener la url de la Solicitud Buro a {0}", dataRow[0].ToString());
                                    }
                                }
                                else if (dataRow[7].ToString() == "True" && dataRow[8].ToString() == "3")
                                {
                                    //status 10 y mensaje
                                    if (await FireStore.ActualizaStatusConsulta(dataRow[0].ToString(), dataRow[4].ToString(), 10))
                                    {
                                        _logger.LogInformation("Solicitud Relacionada a {0} actualizada a 'ERROR EN CONSULTA DE BURO'", dataRow[0].ToString());
                                    }
                                    else
                                    {
                                        _logger.LogError("ERROR Solicitud Relacionada a {0} NO actualizada a status 10", dataRow[0].ToString());
                                    }
                                }
                                else
                                {
                                    _logger.LogInformation("Solicitud Relacionada a {0} sin consulta realizada", dataRow[0].ToString());
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("EMPTY --> Sin solicitudes con ids de consulta de buro por revisar");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en la clase ExecuteAsync");
                    EnviaPush("Error", ex.Message);
                }
                finally
                {
                     await Task.Delay(1000 * _config.Value.segundosActualizacion, stoppingToken);
                }

            }
        }

        private async Task<String> GetUrlPdfBuro(String _cveCli, String _xrbID, int sistema)
        {
            String url = "";
            _logger.LogInformation("Consultando URL solicitud Buro {0}", _xrbID);

            try
            {
                String cadConn = GetCadenaSistema(sistema);

                using (SqlConnection connection = new SqlConnection(cadConn))
                {
                    SqlParameter[] Parameters =
                    {
                        new SqlParameter("@Usu", SqlDbType.Int) {SqlValue = 0},
                        new SqlParameter("@SoloPendientes", SqlDbType.Bit) { SqlValue = 0 },
                        new SqlParameter("@fhInicio", SqlDbType.DateTime) { SqlValue = "01/01/01"},
                        new SqlParameter("@fhFinal", SqlDbType.DateTime) { SqlValue = "01/01/01"},
                        new SqlParameter("@cvecli", SqlDbType.VarChar) { SqlValue = _cveCli},
                        new SqlParameter("@distribuidorID", SqlDbType.BigInt) { SqlValue = 0},
                    };

                    DataTable exec = SqlHelper.ExecuteDataTable(connection, CommandType.StoredProcedure, "ModListaClientesBuro", Parameters, 2);
                    foreach (DataRow dataRow in exec.Rows)
                    {
                        if(_xrbID == dataRow[0].ToString())
                        {
                            _logger.LogInformation("Lolcat");
                            //url = dataRow[8].ToString();
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error en la clase ExecuteAsync");
            }
            
            return url;
        }

        private String GetCadenaSistema(int sistema)
        {
            _logger.LogInformation("Obteniendo conexion al sistema {0}", sistema);
            switch (sistema)
            {
                case 1:
                    return _config.Value.cadenaConexionSqlC;
                case 2:
                    return _config.Value.cadenaConexionSqlOPOR;
                    case 3:
                    return _config.Value.cadenaConexionSqlCR;
                case 4:
                    return _config.Value.cadenaConexionSqlGYT;
                default:
                    return "";
            }
        }


        private void EnviaPush(string titulo, string mensaje)
        {
            Pushover pclient = new Pushover(_config.Value.pushoverApiKey);
            PushResponse resultado = pclient.Push(
                          String.Format("{0} - {1}",_config.Value.appName,  titulo),
                          mensaje,
                          _config.Value.pushoverApiKeyUser
                      );
        }

    }
}
