// using Azure.Identity;
// using Azure.Core;
// using System.Security.Cryptography.X509Certificates;
// using Azure.Security.KeyVault.Certificates;
// using Azure.ResourceManager;
// using Azure.ResourceManager.OperationalInsights;
// using Azure.ResourceManager.OperationalInsights.Models;
// using Newtonsoft.Json.Linq;
// using Azure.Monitor.Query;
// using Serilog;
// //namespace test
// //{
// class Program
// {
//     static async Task Main(string[] args)
//     {
       
//         string keyVaultUrl = "https://my-keyvault1.vault.azure.net/"; // url of keyvault with certificate

//        ////////-----------------------------------fetching certificate--------------------------------//////
//          var certificateClient = new CertificateClient(new Uri(keyVaultUrl), new ManagedIdentityCredential());
//          string certificateName = "domain"; //certificate name
//          var client = new CertificateClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
//          X509Certificate2 certificate=null ; 
//         try
//         {
//            certificate= client.DownloadCertificate(certificateName);
//            Console.WriteLine($"secret: {certificate.Thumbprint}");
                
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error fetching secret: {ex.Message}");
//         }

//         ///-----------------------------retreving workspace key and id of the azure we want to use LAW--------------------------\\\\

//          var program = new Program();
//              string subscriptionId = "a484fd2e-2fcb-4b68-ac9d-264953fded67"; // id of azure in which we are using LAW / AAD app 
//              string resourceGroupName = "Vinita";
//              string workspaceName = "vinitaLog";
//              var workspaceId = await program.GetWorkspaceIDAsync(subscriptionId, resourceGroupName, workspaceName,certificate);
//              var workspacekey=await program.GetWorkspaceKeyAsync(subscriptionId, resourceGroupName, workspaceName,certificate);
//             // Display the workspace ID nad key\\

//             Console.WriteLine($"Workspace ID: {workspaceId}"); 
//             Console.WriteLine($"Workspace Key: {workspacekey}");

//             ////-------------------------------------------------fetching log---------------------------------------------------\\\\
//              try { 
//                 await FetchLogs(certificate,workspaceId); /// calling function
//                  }
//              catch (Exception ex){
//             Console.WriteLine($"Error fetching secret: {ex.Message}");
//                 }
           

//             ////--------------------------------------------------- writing logs-------------------------------------------------\\\\\
//            string logname="demotable"; // table name in LAW yout want to send logs
//              try
//         {
//             WriteLogsToAzureLogAnalytics(workspaceId,workspacekey,logname); /// calling function
                
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error fetching secret: {ex.Message}");
//         }

//         Console.WriteLine($"--------- done-------");
           
//     }
//         ///----------------------- getting workspace id -----------------\\\
//     async Task<string> GetWorkspaceIDAsync(string workspaceSubscriptionId, string resourceGroupName, string workspaceName,X509Certificate2 certificate)
//         {
//             string accessToken = await this.GetAccessTokenAsync(certificate);
//             return await this.GetWorkspaceId(accessToken, workspaceSubscriptionId, resourceGroupName, workspaceName);
//         }

//         private async Task<string> GetAccessTokenAsync(X509Certificate2 certificate)
//         {
//             //                                                  tenant-id                                     client-id
//             var credential = new ClientCertificateCredential("4f0efc17-71e6-4bfe-ae2a-26e0a9eb0087", "ae4bf4ed-8b3b-4459-a4d3-5236efbafe19", certificate);
//             var accessToken = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://management.azure.com/.default" }));
//             var tokenValue = accessToken.Token;
//             return tokenValue;
//         }



//         //------------------------fetching workspace Key-------------------------------- \\\
//         private async Task<string> GetWorkspaceKeyAsync(string workspaceSubscriptionId, string resourceGroupName, string workspaceName,X509Certificate2 certificate)
//         {
//             //                                            tenant-id                               client-id
//             var cred = new ClientCertificateCredential("4f0efc17-71e6-4bfe-ae2a-26e0a9eb0087", "ae4bf4ed-8b3b-4459-a4d3-5236efbafe19", certificate);

//             var client = new ArmClient(cred);
//             ResourceIdentifier operationalInsightsWorkspaceResourceId = OperationalInsightsWorkspaceResource.CreateResourceIdentifier(workspaceSubscriptionId, resourceGroupName, workspaceName);
//             OperationalInsightsWorkspaceResource operationalInsightsWorkspace = client.GetOperationalInsightsWorkspaceResource(operationalInsightsWorkspaceResourceId);

//             // invoke the operation
//             OperationalInsightsWorkspaceSharedKeys result = await operationalInsightsWorkspace.GetSharedKeysAsync();
//             return result.PrimarySharedKey;
//         }
          
//           /// ------------------ fetching workspace ID ----------------\\\\\
//         private async Task<string> GetWorkspaceId(string accessToken, string subscriptionId, string resourceGroupName, string workspaceName)
//         {
//             using (var httpClient = new HttpClient())
//             {
//                  httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
//                  var requestUrl = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.OperationalInsights/workspaces/{workspaceName}?api-version=2021-12-01-preview";
//                  var response = await httpClient.GetAsync(requestUrl);
//                  var responseContent = await response.Content.ReadAsStringAsync();
//                  // Parse the JSON response
//                  JObject responseObject = JObject.Parse(responseContent);
//                  // Extract the workspace ID from the parsed JSON
//                  var workspaceId = responseObject["properties"]["customerId"].ToString();
//                  return workspaceId;
//             }
//         }
//         ////------------------------------ fetching - LOGS - from LAW --------------------------------\\
//          static async Task FetchLogs(X509Certificate2 certificate,string workspaceid)
//         {
//             //                                             tenant-id                                 client-id
//              var cred = new ClientCertificateCredential("4f0efc17-71e6-4bfe-ae2a-26e0a9eb0087", "ae4bf4ed-8b3b-4459-a4d3-5236efbafe19", certificate);
//              var logsClient = new LogsQueryClient(cred);
//                             // table name read logs from 
//              string query = @"demotable_CL 
//                          | sort by TimeGenerated desc
//                          | limit 1"; // set limit for the no of entries

//              var response = await logsClient.QueryWorkspaceAsync(
//              workspaceid, 
//              query, 
//              new QueryTimeRange(TimeSpan.FromDays(50)) // set no of days from today
//              );
//              foreach (var table in response.Value.AllTables)
//             {
//               foreach (var row in table.Rows)
//               {
//                 foreach (var column in row)
//                 {
//                     Console.Write($"{column} ");
//                 }
//                 Console.WriteLine();
//             }
//         }
//         }
//         //------------------------- Sending Logs to LAW ------------------------------------------\\
//         static void WriteLogsToAzureLogAnalytics(string workspaceId, string workspaceKey, string logType)
//     {
//          Log.Logger = new LoggerConfiguration()
//              .WriteTo.Console() // remove this if you don't want to conlose log logger value to terminal 
//                .WriteTo.AzureAnalytics(workspaceId, workspaceKey, logName: logType)
//                  .CreateLogger();
//                  // logger created
//                  Log.Information("Log message with multiple properties: {@Properties}", new{
//           TimeGenerated = DateTime.UtcNow.ToString("o"),
//             Level = "Information", //column names with values;
//             Message = "This is a sample log message with multiple columns",//column names with values
//             vinita="i am Vinita -----"//column names with values
//         });

//         Log.CloseAndFlush();// flush logger
//     }

// } 
// //}
