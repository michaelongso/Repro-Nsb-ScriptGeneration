using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Transport.SqlServer;

class Program
{
    static async Task Main()
    {
        Console.Title = "Samples.SqlOutbox.Sender";
        var random = new Random();

        var endpointConfiguration = new EndpointConfiguration("Samples.SqlOutbox.Sender");
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.SendFailedMessagesTo("error");

        #region SenderConfiguration

        // for SqlExpress use Data Source=.\SqlExpress;Initial Catalog=NsbSamplesSqlOutbox;Integrated Security=True;Max Pool Size=100;Encrypt=false
        var connectionString = "Data Source=localhost;Initial Catalog=Nsb-TransactionScope;User ID=sa;Password=Password01!;Connect Timeout=300;Trust Server Certificate=True;Application Name=\"Octopus 0.0.0-local\";Connect Retry Count=3;Connect Retry Interval=10";

        var transport = new SqlServerTransport(connectionString)
        {
            DefaultSchema = "sender",
            TransportTransactionMode = TransportTransactionMode.ReceiveOnly
        };
        transport.SchemaAndCatalog.UseSchemaForQueue("error", "dbo");
        transport.SchemaAndCatalog.UseSchemaForQueue("audit", "dbo");

        endpointConfiguration.UseTransport(transport);

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(
            connectionBuilder: () =>
            {
                return new SqlConnection(connectionString);
            });
        var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
        dialect.Schema("sender");
        persistence.TablePrefix("");

        transport.Subscriptions.DisableCaching = true;
        transport.Subscriptions.SubscriptionTableName = new SubscriptionTableName(
            table: "Subscriptions",
            schema: "dbo");

        endpointConfiguration.EnableOutbox();

        #endregion

        SqlHelper.CreateSchema(connectionString, "sender");
        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);

        Console.WriteLine("Press enter to send a message");
        Console.WriteLine("Press any key to exit");

        while (true)
        {
            var key = Console.ReadKey();
            Console.WriteLine();

            if (key.Key != ConsoleKey.Enter)
            {
                break;
            }
            var orderSubmitted = new OrderSubmitted
            {
                OrderId = Guid.NewGuid(),
                Value = random.Next(100)
            };
            await endpointInstance.Publish(orderSubmitted)
                .ConfigureAwait(false);
        }
        await endpointInstance.Stop()
            .ConfigureAwait(false);
    }
}