using J_W.Estructura;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public partial class Dbase : IDbase
{
  
    public Dbase()
    {
        //var configuracion = GetConfiguration();
        //var conxion = configuracion.GetSection("ConnectionStrings").GetSection("dbConnection").Value;
        //base.Connection(configuracion.GetSection("ConnectionStrings").GetSection("dbConnection").Value);
    }

    public Dbase(string con)
    {
        base.Connection(con);
    }

    public override Procedure Procedure(string name, params object[] parameters)
    {
        return new Procedure(this.Conexion, name, parameters);
    }
    public override Procedure Query(string guery, bool dynamic)
    {
        return new Procedure(this.Conexion, dynamic, guery);
    }

}
 