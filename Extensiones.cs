using APISietemasdereservas.Controllers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using J_W.Estructura;
using APISietemasdereservas.Models;
using Newtonsoft.Json;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using APISietemasdereservas.Models.Request;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks; 
using System.Net.Mime;
using static System.Formats.Asn1.AsnWriter;

/// <summary>
/// Descripción breve de Buscadores
/// </summary>
static class Extensiones
{
    public static string GetString(this Dictionary<string, object> Dict, string Key)
    {
        try
        {
            if (Dict.ContainsKey(Key))
                return Dict[Key].ToString();
            return "";
        }
        catch (Exception)
        {
            return "";
        }
    }

}

public class Generaldb
{

    Dbase dbase = new Dbase(); 
    public Generaldb()
    { }

    public Dictionary<string, object> ModificarConexion(string server, string database, string password, string userName)
    {
        Dictionary<string, object> dic = new Dictionary<string, object>();
        try
        {
            // Construir la nueva cadena de conexión
            var connectionString = $"Server={server};Database={database};User Id={userName};Password={password};";
            dic.Add("Conexion", connectionString);
        }
        catch (Exception error)
        {
            dic.Add("error", true);
            dic.Add("message", error.Message);
        }

        return dic;
    }



    ///AQUI SE OBTIENE LOS DATOS DEL USUARIO Y DE LA EMPRESA DONDE PERTENECE 
    public Result UsuariosGetData(object v_username, string dbase2)
    {
        dbase.Conexion = dbase2;
        return dbase.Query("SELECT U.id IdUser, userid UserId, R.RoleName, U.estado FROM Dbo.Usuarios U INNER JOIN aspnet_Roles R ON R.id = U.id_perfil WHERE  U.UserName = '" + v_username + "';", true).RunRow();
    }


    public Result infoempresa(object token, string dbase2)
    {
        dbase.Conexion = dbase2;
        return dbase.Query("SELECT U.id, U.Token, u.[database], S.servername, username, U.id_empresa, U.id_aplicacion, " +
                                        "password  FROM [dbo].[EmpresasAplicaciones] U INNER JOIN servidores S ON S.id = U.id_servidor  " +
                                        "WHERE   U.estado = 1 and U.Token = '" + token + "';", true).RunRow();
    }


    ///QAQUI SE REALIZA LA CONSULTA DE CONTRASEÑA Y USER
    public Result ValidateUser(object v_username, object password, string dbase2)
    {
        dbase.Conexion = dbase2;
        return dbase.Query("SELECT U.id IdUser, u.userid UserId, R.RoleName, U.estado, m.[Password] [Password]  FROM Dbo.Usuarios U INNER JOIN aspnet_Roles R ON R.id = U.id_perfil INNER JOIN [dbo].[aspnet_Membership] M ON M.UserId = U.userid WHERE  U.UserName = '" + v_username + "';", true).RunRow();
    }


    public Result UsuarioChangeTokenApp(string stoken, string id, string dbase2)
    {
        dbase.Conexion = dbase2;
        return dbase.Query(String.Format("UPDATE Dbo.Usuarios SET apptoken = '{0}' WHERE id = {1};", stoken, id), true).RunVoid();
    }



    public Dictionary<string, object> returnConfiguracionParametrosOperacional(string clase, string metodo, string dbConnection)
    {
        Result res = new Result();
        Dbase dbase = new Dbase();
        Dictionary<string, object> dic = new Dictionary<string, object>();
        try
        {
            dbase.Conexion = dbConnection;

            res = dbase.Query("SELECT P.configparams, P.storeprocedure, U.estado AS stateApi, p.estado AS stateMetodos  FROM [dbo].[Apis] U INNER JOIN [dbo].[ApisMethods] P ON U.id = P.id_api WHERE U.estado = 1 and P.estado = 1 and P.class = '" + clase + "' and P.method = '" + metodo + "';", true).RunRow();

            if (res.Row.Count == 0)
                throw new Exception("Este metodo no tiene permisos de activacion para realizar esta acción");


            dic.Add("Error", false);
            dic.Add("procedimiento", res.Row.GetString("storeprocedure"));
            dic.Add("configparams", res.Row.GetString("configparams"));
        }
        catch (Exception e)
        {
            dic.Add("Error", true);
            dic.Add("Message", e.Message);

        }
        return dic;
    }


    public object[] returnParameterProcedure(object[] vparams, Dictionary<string, object> dic, object id_user = null)
    {
        object[] parameter2 = new object[vparams.Length];

        for (int i = 0; i < vparams.Length; i += 2)
        {
            parameter2[i] = vparams[i];
            parameter2[i + 1] = dic.GetString(vparams[i + 1].ToString());
        }
        return parameter2;
    }


    public object MetodoGuardar(string Clase, string Metodo, string Params, string dbConnection)
    {
        Result res = new Result();
        Result res1 = new Result();
        Dbase dbase = new Dbase();
        try
        {
            dbase.Conexion = dbConnection;
            Dictionary<string, object> respuesta = returnConfiguracionParametrosOperacional(Clase, Metodo, dbConnection);

            bool resll = (bool)respuesta["Error"];
            if (resll == true)
            {
                res.Error = true;
                res.Message = (dynamic)respuesta["Message"];
                return res;
            }
            string dataproce = respuesta.GetString("configparams");
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(Params);

            object[] parameter = dataproce.Split(';');
            object[] Vparams = returnParameterProcedure(parameter, data);

            Dictionary<string, object> ObtenerResul = new Dictionary<string, object>();
            res = dbase.Procedure(respuesta.GetString("procedimiento"), Vparams).RunRow();
        }
        catch (Exception e)
        {
            res.Error = true;
            res.Message = e.Message;
        }
        return res;
    }


    public object MetodoListarGeneral(Operacional model, string conexion, string dbConnection)
    {
        Result result = new Result();
        DataTable Temp = new DataTable();
        Dbase dbase = new Dbase();
        try
        {
            dbase.Conexion = conexion;
            Dictionary<string, object> respuesta = returnConfiguracionParametrosOperacional(model.Clase, model.Metodo, dbConnection);
            bool resll = (bool)respuesta["Error"];
            if (resll == true)
                return respuesta;

            string proce = respuesta.GetString("procedimiento");
            string dataproce = respuesta.GetString("configparams");
            Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(model.Params);

            object[] parameter = dataproce.Split(';');
            object[] Vparams = returnParameterProcedure(parameter, data);

            result = dbase.Procedure(proce, Vparams).RunData();
            if (!result.Error)
            {
                if (result.Data.Tables.Count > 1)
                    Temp = result.Data.Tables[1];

                return new { data = Props.table2List(result.Data.Tables[0]) };
            }
            else
            {
                return new { error = 1, errorMesage = "No hay resultado" };
            }
        }
        catch (Exception e)
        {
            result.Error = true;
            result.Message = e.Message;
        }
        return result;
    }

 
   
    class Props
    {
        /// <summary>
        /// Utility method to present DataTable as JSON 
        /// </summary>
        /// <param name="dt">Datatable to read from</param>
        /// <returns>a List (wich is actually gonna be serialize as a json string, because serializing a Datatable is fucked</returns>
        public static List<Dictionary<string, object>> table2List(DataTable dt)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return rows;
        }

    }
}