using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Infrastructure.Database;

public class PostgreSqlServer(IConfiguration configuration, IHostEnvironment env) 
{
    public NpgsqlConnection OpenConnection()
    {
        string connectionString;

        if (env.IsDevelopment())
        {
            connectionString = configuration["ConnectionStrings:DefaultConnection"] 
                               ?? throw new InvalidOperationException("Connection string is not configured for development environment.");
        }
        else
        {
            throw new ArgumentException("Connection string is not configured for production environment.");
        }
   
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string is not configured.");
        }
        var cnn = new NpgsqlConnection(connectionString);
      
        if(cnn.State == ConnectionState.Closed)
            cnn.Open();

        return cnn;
    }
}
