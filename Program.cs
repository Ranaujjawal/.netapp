//using System;
//using System.Threading.Tasks;
using Serilog;
//using Serilog.Sinks.AzureAnalytics;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Monitor.Query;
//using Azure.Monitor.Query.Models;

class Program{
     static async Task Main(string[] args){
        string keyVaultName = "vinikeyvault";
            string tenantid="";
            string clientid="";
            string clientsecret="";
            string workspaceid="";
            string workspaceprimarykey="";

               try
            {
                 tenantid = await FetchSecretFromKeyVaultAsync(keyVaultName, "tenantid");
                 clientid = await FetchSecretFromKeyVaultAsync(keyVaultName, "clientid");
                 workspaceid = await FetchSecretFromKeyVaultAsync(keyVaultName, "workspaceid");
                 clientsecret= await FetchSecretFromKeyVaultAsync(keyVaultName, "clientsecret");
                 workspaceprimarykey = await FetchSecretFromKeyVaultAsync(keyVaultName, "lawprimarykey");
              
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            /// creating logger
            
             Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
               .WriteTo.AzureAnalytics(workspaceid, workspaceprimarykey, logName: "demotable")
                 .CreateLogger();
                 Log.Information("Log message with multiple properties: {@Properties}", new
        {
          TimeGenerated = DateTime.UtcNow.ToString("o"),
            Level = "Information", //column names with values;
            Message = "This is a sample log message with multiple columns",//column names with values
            vinita="i am jerry"//column names with values
        });
        Log.CloseAndFlush();

        Console.WriteLine("do you want to print logs then type Y else N");
        string res = Console.ReadLine() ?? "N";
        if(res=="Y"){
         await FetchLogs(tenantid,clientid,clientsecret,workspaceid); // function called to print logs
        }
             Console.WriteLine($"{workspaceid}");
     }

                /// function to fetch secret from vault
     static async Task<string> FetchSecretFromKeyVaultAsync(string keyVaultName, string secretName)
        {
            var Credential = new  DefaultAzureCredential();
            var client = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), Credential);

            KeyVaultSecret secret = await client.GetSecretAsync(secretName);
            return secret.Value;
        }

            ////// function to fetch logs from LAW
        static async Task FetchLogs(string tenantid, string clientid, string clientsecret,string workspaceid)
        {
           var clientSecretCredential = new ClientSecretCredential(tenantid, clientid, clientsecret);
            var logsClient = new LogsQueryClient(clientSecretCredential);

        string query = @"demotable_CL
                         | sort by TimeGenerated desc
                         | limit 1";

        var response = await logsClient.QueryWorkspaceAsync(
            workspaceid, 
            query, 
            new QueryTimeRange(TimeSpan.FromDays(1))
        );
        foreach (var table in response.Value.AllTables)
        {
            foreach (var row in table.Rows)
            {
                foreach (var column in row)
                {
                    Console.Write($"{column} ");
                }
                Console.WriteLine();
            }
        }
        }


}
