using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using UniversalModeration.Models;

namespace UniversalModeration.DataBase
{
    [PluginServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class MySqlDatabase : IMySqlDatabase
    {
        private MySqlConnection m_Connection = new MySqlConnection();
        private string m_Query(string sql) => sql.Replace("UniversalModeration", m_Configuration.GetSection("plugin_configuration:TableName").Get<string>());

        private readonly IConfiguration m_Configuration;

        public MySqlDatabase(IConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        public async Task<Ban> GetBanAsync(string userId)
        {
            const string sql = "SELECT * FROM UniversalModeration WHERE userId = @userId ORDER BY expireDateTime DESC;";
            return await m_Connection.QueryFirstAsync<Ban>(m_Query(sql), new { userId });
        }

        public async Task AddBanAsync(Ban ban)
        {
            const string sql = "INSERT INTO UniversalModeration (userId, punisherId, banReason, expireDateTime, banDateTime) VALUES (@userId, @punisherId, @banReason, @expireDateTime, @banDateTime);";
            await m_Connection.ExecuteAsync(m_Query(sql), ban);
        }

        public async Task Reload()
        {
            m_Connection = new MySqlConnection(m_Configuration.GetSection("plugin_configuration:ConnectionString").Get<string>());
            const string sql = "CREATE TABLE IF NOT EXISTS UniversalModeration (userId VARCHAR(32) NOT NULL, punisherId VARCHAR(32) NOT NULL, banReason VARCHAR(255) NOT NULL, expireDateTime DATETIME NOT NULL, banDateTime DATETIME NOT NULL);";
            await m_Connection.ExecuteAsync(m_Query(sql));
        }
    }
}
