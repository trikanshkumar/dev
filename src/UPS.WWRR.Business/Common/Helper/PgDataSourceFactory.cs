using Google.Apis.Auth.OAuth2;
using Npgsql;

namespace UPS.WWRR.Business.Common.Helper
{
    public static class PgDataSourceFactory
    {
        // Cloud SQL IAM DB auth scope
        private static readonly string[] AlloyDbScope =
            { "https://www.googleapis.com/auth/cloud-platform" }; // Cloud SQL IAM scope [1](https://eplus.dev/securing-a-cloud-sql-for-postgresql-instance-gsp920)

        public static async Task<NpgsqlDataSource> CreateAsync(
            string host,
            string database,
            string iamDbUser,
            bool requireSsl = true,
            CancellationToken cancellationToken = default)
        {
            var adc = await GoogleCredential.GetApplicationDefaultAsync(cancellationToken);
            var scoped = adc.CreateScoped(AlloyDbScope); // GoogleCredential handles caching/refresh [4](https://docs.cloud.google.com/alloydb/docs/database-users/iam-authentication)

            var builder = new NpgsqlDataSourceBuilder
            {
                ConnectionStringBuilder =
                {
                    Host = host,
                    Database = database,
                    Username = iamDbUser,
                    SslMode = requireSsl ? SslMode.Require : SslMode.Disable
                }
            };

            builder.UsePeriodicPasswordProvider(
                async (settings, ct) =>
                {
                    var token = await scoped.UnderlyingCredential
                        .GetAccessTokenForRequestAsync(cancellationToken: ct);
                    return token ?? string.Empty;
                },
                successRefreshInterval: TimeSpan.FromMinutes(1),
                failureRefreshInterval: TimeSpan.FromSeconds(15)); // Npgsql caches token; short intervals are fine [3](https://docs.aws.amazon.com/AmazonRDS/latest/UserGuide/UsingWithRDS.IAMDBAuth.html)

            return builder.Build();
        }


        public static (string db_host, string database, string iamDbUser) Parse(string connectionString)
        {
            var csb = new NpgsqlConnectionStringBuilder(connectionString);

            // These map directly to the properties exposed by the builder
            string db_host = csb.Host;
            string database = csb.Database;
            string iamDbUser = csb.Username;

            return (db_host, database, iamDbUser);
        }

    }
}

