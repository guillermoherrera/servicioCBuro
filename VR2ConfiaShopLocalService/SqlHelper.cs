using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace estadosCBService
{
    class SqlHelper
    {
        public static DataTable ExecuteDataTable(SqlConnection conn, CommandType cmdType, string cmdText, SqlParameter[] cmdParams, int config_dt)
        {
            DataTable dt = new DataTable();
            switch (config_dt)
            {
                //, , , , , , , , , , , , , , , , , 
                case 1:
                    dt.Columns.Add("XmlResultBuroID");
                    dt.Columns.Add("Cvecli");
                    dt.Columns.Add("FHRegistro");
                    dt.Columns.Add("ResultCode");
                    dt.Columns.Add("ResultDesc");
                    dt.Columns.Add("XMLRespuesta");
                    dt.Columns.Add("XMLPeticion");
                    dt.Columns.Add("Estatus");
                    dt.Columns.Add("EstatusID");
                    dt.Columns.Add("Version");
                    dt.Columns.Add("UsuRegistra");
                    dt.Columns.Add("UsuarioConsulta");
                    dt.Columns.Add("ExpedienteID");
                    dt.Columns.Add("SistemaID");
                    dt.Columns.Add("FhConsulta");
                    dt.Columns.Add("solicitudId");
                    dt.Columns.Add("PDF");
                    dt.Columns.Add("XML");
                    dt.Columns.Add("validacionExterna");
                    break;
                case 2:
                    dt.Columns.Add("ID");
                    dt.Columns.Add("Cvecli");
                    dt.Columns.Add("NombreCom");
                    dt.Columns.Add("FHRegistro");
                    dt.Columns.Add("ResultDesc");
                    dt.Columns.Add("Registro");
                    dt.Columns.Add("Estatus");
                    dt.Columns.Add("PDf");
                    dt.Columns.Add("URL");
                    dt.Columns.Add("ExpedienteID");
                    dt.Columns.Add("CveEncar");
                    dt.Columns.Add("Encargado");
                    break;
                default:
                    break;
            }

            SqlDataReader dr = ExecuteReader(conn, cmdType, cmdText, cmdParams);

            switch (config_dt)
            {
                case 1:
                    while (dr.Read())
                    {
                        dt.Rows.Add(dr[0], dr[1], dr[2], dr[3], dr[4], dr[5], dr[6], dr[7], dr[8], dr[9], dr[10], dr[11], dr[12], dr[13], dr[14], dr[15], dr[16], dr[17], dr[18]);
                    }
                    break;
                case 2:
                    while (dr.Read())
                    {
                        dt.Rows.Add(dr[0], dr[1], dr[2], dr[3], dr[4], dr[5], dr[6], dr[7], dr[8], dr[9], dr[10], dr[11]);
                    }
                    break;
                default:
                    break;
            }

            return dt;
        }

        public static SqlDataReader ExecuteReader(SqlConnection conn, CommandType cmdType, string cmdText, SqlParameter[] cmdParams)
        {
            SqlCommand cmd = conn.CreateCommand();
            PrepareCommand(cmd, conn, null, cmdType, cmdText, cmdParams);
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            return rdr;
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, CommandType cmdType, string cmdText, SqlParameter[] cmdParams)
        {
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
            {
                cmd.Transaction = trans;
            }
            cmd.CommandType = cmdType;
            if (cmdParams != null)
            {
                AttachParameters(cmd, cmdParams);
            }
        }

        private static void AttachParameters(SqlCommand cmd, SqlParameter[] cmdParams)
        {
            foreach (SqlParameter p in cmdParams)
            {
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }
                cmd.Parameters.Add(p);
            }
        }
    }
}
