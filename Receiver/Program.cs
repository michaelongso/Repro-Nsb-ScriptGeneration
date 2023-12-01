using System;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Transport.SqlServer;

class Program
{
    const string DefaultSchema = "NSB";
    const string OctopusServerEndpointName = "OctopusServer";
    // const string ErrorQueueTable = $"{PersistenceTablePrefix}Error";
    // const string AuditQueueTable = $"{PersistenceTablePrefix}Audit";
    // const string SubscriptionRoutingTable = $"{PersistenceTablePrefix}SubscriptionRouting";
    
    
    static async Task Main()
    {
        Console.Title = "Samples.SQLOutboxEF.Receiver";

        // for SqlExpress use Data Source=.\SqlExpress;Initial Catalog=NsbSamplesSqlOutbox;Integrated Security=True;Max Pool Size=100;Encrypt=false
        var connectionString = "Data Source=localhost;Initial Catalog=Nsb-TransactionScope;User ID=sa;Password=Password01!;Connect Timeout=300;Trust Server Certificate=True;Application Name=\"Octopus 0.0.0-local\";Connect Retry Count=3;Connect Retry Interval=10";

        var endpointConfiguration = new EndpointConfiguration(OctopusServerEndpointName);
        endpointConfiguration.EnableInstallers();
        // endpointConfiguration.SendFailedMessagesTo(ErrorQueueTable);
        // endpointConfiguration.AuditProcessedMessagesTo(AuditQueueTable);

        #region ReceiverConfiguration

        var transport = new SqlServerTransport(connectionString)
        {
            DefaultSchema = DefaultSchema,
            TransportTransactionMode = TransportTransactionMode.ReceiveOnly
        };
        // transport.SchemaAndCatalog.UseSchemaForQueue(ErrorQueueTable, DefaultSchema);
        // transport.SchemaAndCatalog.UseSchemaForQueue(AuditQueueTable, DefaultSchema);

        var routing = endpointConfiguration.UseTransport(transport);
        // routing.UseSchemaForEndpoint("Samples.SqlOutbox.Sender", "sender");

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                return new SqlConnection(connectionString);
            });
        var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
        dialect.Schema(DefaultSchema);
        // persistence.TablePrefix(PersistenceTablePrefix);

        transport.Subscriptions.DisableCaching = true;
        // transport.Subscriptions.SubscriptionTableName = new SubscriptionTableName(
        //     table: SubscriptionRoutingTable,
        //     schema: DefaultSchema);

        endpointConfiguration.EnableOutbox();

        #endregion
        SqlHelper.CreateSchema(connectionString, DefaultSchema);

        // SqlHelper.ExecuteSql(connectionString, File.ReadAllText("Startup.sql"));

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}