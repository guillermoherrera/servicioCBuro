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

namespace VR2ConfiaShopLocalService
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

        //public override Task StartAsync(CancellationToken cancellationToken)
        //{


        //}

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
            Int64 wsvcCanjeAppId = 0;
            Int64 NoCda = 0;
            List<Respuesta> RespuestaAutorizacion;
            List<Autorizacion> Autorizaciones = new List<Autorizacion>();
            string id_empresa = "";
            string id_ticket1 = "";
            string id_forma_pago = "";
            //string referencia_forma_pago = "";

            string mensaje = string.Format("El servicio se ha iniciado...");
            _logger.LogInformation(mensaje);
            EnviaPush("Mensaje Informativo", mensaje);

            ImprimeMensajeInformativo();

            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {

                    _logger.LogInformation("Rutina iniciada a las: {time}", DateTimeOffset.Now);

                    using (SqlConnection connection = new SqlConnection(_config.Value.cadenaConexionSql))
                    {
                        connection.Open();
                        using (SqlCommand comm = new SqlCommand("wsConfiaShopAutorizacionesPendientes", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                        {
                            /*
                             *   ca.wsvcCanjeAppId
 ,ca.idTicket
 ,ca.id_empresa
 ,ca.NoCdaGenerada
 ,10 formaPago
                             * 
                             * */

                            using (var reader = comm.ExecuteReader())
                            {
                                Autorizaciones.Clear();
                                while (reader.Read())
                                {
                                    try
                                    {
                                        wsvcCanjeAppId = reader.GetInt64(0);
                                        id_ticket1 = reader.GetString(1);
                                        id_empresa = reader.GetInt32(2).ToString();
                                        NoCda = reader.GetInt64(3);
                                        id_forma_pago = reader.GetInt32(4).ToString();
                                        RespuestaAutorizacion = GetCodigoAutorizacionVenta(id_empresa, id_ticket1, id_forma_pago, NoCda.ToString());

                                        foreach(Respuesta r in RespuestaAutorizacion)
                                        {
                                            Autorizaciones.Add(new Autorizacion() { wsvcCanjeAppId = wsvcCanjeAppId, noCda = NoCda, validacion = r.validacion, mensaje = r.msj, regresa=r.regresa});
                                        }
                                        
                                       
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, "Error al insertar corrida: {corrida}", NoCda);
                                    }
                                }
                            }
                        }
                    }
                    ActualizaEnVr(Autorizaciones);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al insertar");
                    EnviaPush("Error al insertar", ex.Message);
                }
                finally
                {
                     await Task.Delay(1000 * _config.Value.segundosActualizacion, stoppingToken);
                }

            }
        }

        private void ImprimeMensajeInformativo()
        {
            using (SqlConnection connection = new SqlConnection(_config.Value.cadenaConexionSql))
            {
                connection.Open();
                using (SqlCommand comm = new SqlCommand("mensajeRobot", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                {
                    //CREATE PROCEDURE cshActualizaAutorizaciones @autorizaciones ttAutorizacionConfiaShop READONLY, @resultCode INT OUTPUT, @result VARCHAR(500) OUTPUT

                    try
                    {
                        string mensajeRobot = comm.ExecuteScalar().ToString();
                        _logger.LogInformation(mensajeRobot);
                    }
                    catch (Exception ex)
                    {

                        _logger.LogError(ex, "Error al obtener mensaje del robot...");
                    }

                }
            }
        }

            private void ActualizaEnVr(List<Autorizacion> autorizaciones)
        {
            if (autorizaciones.Count > 0)
            {

                using (SqlConnection connection = new SqlConnection(_config.Value.cadenaConexionSql))
                {
                    connection.Open();
                    using (SqlCommand comm = new SqlCommand("cshActualizaAutorizaciones", connection) { CommandType = System.Data.CommandType.StoredProcedure })
                    {
                        //CREATE PROCEDURE cshActualizaAutorizaciones @autorizaciones ttAutorizacionConfiaShop READONLY, @resultCode INT OUTPUT, @result VARCHAR(500) OUTPUT

                        try
                        {
                            SqlParameter tvpParam = comm.Parameters.AddWithValue("@autorizaciones", autorizaciones.ToDataTable());
                            tvpParam.SqlDbType = SqlDbType.Structured;
                            tvpParam.TypeName = "dbo.ttAutorizacionConfiaShop001";

                            comm.Parameters.Add(new SqlParameter("@resultcode", SqlDbType.Int) { Direction = ParameterDirection.Output });
                            comm.Parameters.Add(new SqlParameter("@result", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output });

                            comm.ExecuteNonQuery();

                            if (Convert.ToInt32(comm.Parameters[1].Value) > 0)
                            {
                                throw new Exception(comm.Parameters[2].Value.ToString());
                            }

                            _logger.LogInformation(comm.Parameters[2].Value.ToString());

                        }
                        catch (Exception ex)
                        {

                            _logger.LogError(ex, "Error al actualizar en VR...");
                        }

                    }
                }
            }
            else
                _logger.LogInformation("Nada que actualizar...");
        }

        private List<Respuesta> GetCodigoAutorizacionVenta(string id_empresa,string id_ticket1,string id_forma_pago,string referencia_forma_pago)
        {
            var client = new RestClient(Url.Combine(_config.Value.apiURI, _config.Value.servicioObtenerPagos));
            var request = new RestRequest(Method.GET);

            request.AddQueryParameter("id_empresa", id_empresa);
            request.AddQueryParameter("id_ticket1", id_ticket1);
            request.AddQueryParameter("id_forma_pago", id_forma_pago);
            request.AddQueryParameter("referencia_forma_pago", referencia_forma_pago);

            //IRestResponse response = client.Execute(request);
            IRestResponse response = client.Execute(request);
            _logger.LogDebug("Peticion web realizada a: {uri}", response.ResponseUri);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(String.Format("Error al realizar la peticion web a {0}: {1} {2} ...", Url.Combine(_config.Value.apiURI, _config.Value.servicioObtenerPagos), response.StatusCode.ToString(), response.ErrorMessage));
            }

            string json = response.Content;

            List<Respuesta> resultados = JsonSerializer.Deserialize<List<Respuesta>>(json);

            _logger.LogDebug("Peticion web realizada a: {uri}", response.ResponseUri);

            return resultados;
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
