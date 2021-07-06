using Cassandra;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using CassandraDemo.Shared;
using Cassandra.Data.Linq;
using Cassandra.Mapping;

namespace CassandraDemo.Server.Services
{
    public class DbService : IDbService
    {
        private readonly ISession _session;
        public DbService(IAKVService akvService, IConfiguration configuration)
        {
            string secretLocation = configuration.GetSection("Cassandra").GetValue<string>("PasswordLocation");
            string userName = configuration.GetSection("Cassandra").GetValue<string>("UserName");
            string clusterName = configuration.GetSection("Cassandra").GetValue<string>("Cluster");
            string keyspaceName = configuration.GetSection("Cassandra").GetValue<string>("keyspaceName");
            string password = akvService.GetKeyVaultSecretAsync(secretLocation).Result;
            var options = new Cassandra.SSLOptions(SslProtocols.Tls12, true, ValidateServerCertificate);
            options.SetHostNameResolver((ipAddress) => clusterName);
            Cluster cluster = Cluster.Builder()
                .WithCredentials(userName, password)
                .WithPort(10350)
                .AddContactPoint(clusterName)
                .WithSSL(options)
                .Build();
            _session = cluster.Connect(keyspaceName);
            MapTables();
        }

        public List<WeatherForecast> GetForcast()
        {
            List<WeatherForecast> weatherForecasts = 
                _session.GetTable<WeatherForecast>().Where(i =>
                i.Date >= DateTime.Now).AllowFiltering().Execute().ToList();
            return weatherForecasts;
        }

        public async Task DeleteForcastsAsync()
        {
            await _session.GetTable<WeatherForecast>().Where(i =>
                i.Year == DateTime.Now.Year &&
                i.Date >= DateTime.Now)
                .AllowFiltering().Delete().ExecuteAsync();
        }

        public async Task AddValueAsync(WeatherForecast forcast)
        {
            var weatherInsertStmt = _session.Prepare(
                "INSERT INTO weather (year, weather_date, summary, tempc, tempf) VALUES (?, ?, ?, ?, ?)");
            var statement = weatherInsertStmt.Bind(forcast.Year, forcast.Date, forcast.Summary, 
                forcast.TemperatureC, forcast.TemperatureF);
            await _session.ExecuteAsync(statement);
        }

        public async Task AddValuesAsync(IEnumerable<WeatherForecast> forcasts)
        {
            List<Task> batch = new ();
            foreach (var forcast in forcasts)
            {
                batch.Add(AddValueAsync(forcast));
            }
            await Task.WhenAll(batch);
        }


        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        private void MapTables()
        {
            MappingConfiguration.Global.Define(
           new Map<WeatherForecast>()
              .TableName("weather")
              .PartitionKey(u => u.Date)
              .Column(u => u.Date, cm => cm.WithName("weather_date"))
              .Column(u => u.Year, cm => cm.WithName("year"))
              .Column(u => u.Summary, cm => cm.WithName("summary"))
              .Column(u => u.TemperatureC, cm => cm.WithName("tempc"))
              .Column(u => u.TemperatureF, cm => cm.WithName("tempf")));
        }
    }
    public interface IDbService
    {
        Task AddValueAsync(WeatherForecast forcast);
        Task AddValuesAsync(IEnumerable<WeatherForecast> forcast);
        Task DeleteForcastsAsync();
        List<WeatherForecast> GetForcast();
    }
}
