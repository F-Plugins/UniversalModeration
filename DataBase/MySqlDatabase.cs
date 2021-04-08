using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using OpenMod.API.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using UniversalModeration.Models;
using System.Linq;
using UniversalModeration.Enums;

namespace UniversalModeration.DataBase
{
    [PluginServiceImplementation(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class MySqlDatabase : IMySqlDatabase
    {
        private MySqlConnection m_Connection = new MySqlConnection();
        private string m_Query(string sql, QueryType type)
        {
            if(type == QueryType.BAN)
            {
                return sql.Replace("UniversalModerationBans", m_Configuration.GetSection("plugin_configuration:BansTableName").Get<string>());
            }
            else
            {
                return sql.Replace("UniversalModerationWarns", m_Configuration.GetSection("plugin_configuration:WarnsTableName").Get<string>());
            }
        }

        private readonly IConfiguration m_Configuration;

        public MySqlDatabase(IConfiguration configuration)
        {
            m_Configuration = configuration;
        }

        public async Task<Ban> GetBanAsync(string userId)
        {
            const string sql = "SELECT * FROM UniversalModerationBans WHERE userId = @userId AND unBanned = TRUE ORDER BY expireDateTime DESC;";
            return await m_Connection.QueryFirstAsync<Ban>(m_Query(sql, QueryType.BAN), new { userId });
        }

        public async Task<List<Ban>> GetBansAsync(string userId)
        {
            const string sql = "SELECT * FROM UniversalModerationBans WHERE userId = @userId ORDER BY expireDateTime DESC;";
            var query = await m_Connection.QueryAsync<Ban>(m_Query(sql, QueryType.BAN), new { userId }).ConfigureAwait(false);
            return query.ToList();
        }

        public async Task UpdateLastBanAsync(string userId, bool unBanned)
        {
            const string sql = "UPDATE UniversalModerationBans SET unBanned = @unBanned WHERE userId = @userId ORDER BY expireDateTime DESC;";
            await m_Connection.ExecuteAsync(m_Query(sql, QueryType.BAN), new { unBanned, userId });
        }

        public async Task AddBanAsync(Ban ban)
        {
            const string sql = "INSERT INTO UniversalModerationBans (userId, punisherId, banReason, unBanned, expireDateTime, banDateTime) VALUES (@userId, @punisherId, @banReason, @unBanned, @expireDateTime, @banDateTime);";
            await m_Connection.ExecuteAsync(m_Query(sql, QueryType.BAN), ban);
        }

        public async Task<List<Warn>> GetWarnsAsync(string userId)
        {
            const string sql = "SELECT * FROM UniversalModerationWarns WHERE userId = @userId ORDER BY warnDateTime DESC;";
            var query = await m_Connection.QueryAsync<Warn>(m_Query(sql, QueryType.WARN), new { userId }).ConfigureAwait(false);
            return query.ToList();
        }

        public async Task AddWarnAsync(Warn warn)
        {
            const string sql = "INSERT INTO UniversalModerationWarns (userId, punisherId, warnReason, warnDateTime) VALUES (@userId, @punisherId, @warnReason, @warnDateTime)";
            await m_Connection.ExecuteAsync(m_Query(sql, QueryType.WARN), warn);
        }

        public async Task Reload()
        {
            m_Connection = new MySqlConnection(m_Configuration.GetSection("plugin_configuration:ConnectionString").Get<string>());
            const string sql = "CREATE TABLE IF NOT EXISTS UniversalModerationBans (userId VARCHAR(32) NOT NULL, punisherId VARCHAR(32) NOT NULL, banReason VARCHAR(255) NOT NULL, unBanned BOOLEAN, expireDateTime DATETIME NOT NULL, banDateTime DATETIME NOT NULL);";
            await m_Connection.ExecuteAsync(m_Query(sql, QueryType.BAN));

            const string sql2 = "CREATE TABLE IF NOT EXISTS UniversalModerationWarns (userId VARCHAR(32) NOT NULL, punisherId VARCHAR(32) NOT NULL, warnReason VARCHAR(255) NOT NULL, warnDateTime DATETIME NOT NULL);";
            await m_Connection.ExecuteAsync(m_Query(sql2, QueryType.WARN));
        }
    }
}
