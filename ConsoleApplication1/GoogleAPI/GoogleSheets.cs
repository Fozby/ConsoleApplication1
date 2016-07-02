using ConsoleApplication1.GoogleAPI.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Script.v1;
using Google.Apis.Script.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1.GoogleAPI
{
    class GoogleSheets
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/script-dotnet-quickstart.json
        string[] Scopes = { "https://www.googleapis.com/auth/drive", "https://www.googleapis.com/auth/spreadsheets" };
        string ApplicationName = "Google Apps Script Execution API .NET Quickstart";
        ScriptService service;
        string scriptId = "MeuP4-pba_GPoGxkx2sttcSfhjS0oyARv";

        public GoogleSheets()
        {
            UserCredential credential;

            string cred = "GoogleAPI/client_secret.json";

            using (var stream =
                new FileStream(cred, FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/script-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Apps Script Execution API service.
            service = new ScriptService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public void AddPlayerStats(PlayerChampionStats stats)
        {
            List<object> _params = new List<object>();
            _params.Add(stats);

            callMethod("addPlayerStats", _params);
        }

        public void AddChampionStats(ChampionStats stats)
        {
            List<object> _params = new List<object>();
            _params.Add(stats);

            callMethod("addFeaturedStats", _params);
        }

        public void AddCompetitiveChampionStats(CompetitiveStats stats)
        {
            List<object> _params = new List<object>();
            _params.Add(stats);

            callMethod("addCompetitiveChampionStats", _params);
        }

        public void AddCompetitiveChampionStats2(List<CompetitiveStats> stats)
        {
            List<object> _params = new List<object>();
            _params.Add(stats);

            callMethod("addCompetitiveChampionStats", _params);
        }

        public void ClearCompetitiveStats()
        {
            callMethod("resetCompetitiveChampionStats", new List<Object>());
        }

        public void ClearPlayerStats()
        {
            callMethod("resetPlayerStats", new List<Object>());
        }

        private void callMethod(string method, List<Object> _params)
        {
            // Create an execution request object.
            ExecutionRequest request = new ExecutionRequest();
            request.Function = method;
            request.Parameters = _params;

            ScriptsResource.RunRequest runReq =
                    service.Scripts.Run(request, scriptId);

            try
            {
                // Make the API request.
                Console.WriteLine($"Starting at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");

                Operation op = runReq.Execute();

                Console.WriteLine($"Finished at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");


                if (op.Error != null)
                {
                    // The API executed, but the script returned an error.

                    // Extract the first (and only) set of error details
                    // as a IDictionary. The values of this dictionary are
                    // the script's 'errorMessage' and 'errorType', and an
                    // array of stack trace elements. Casting the array as
                    // a JSON JArray allows the trace elements to be accessed
                    // directly.
                    IDictionary<string, object> error = op.Error.Details[0];
                    Console.WriteLine(
                            "Script error message: {0}", error["errorMessage"]);
                    Console.ReadLine();
                    if (error["scriptStackTraceElements"] != null)
                    {
                        // There may not be a stacktrace if the script didn't
                        // start executing.
                        Console.WriteLine("Script error stacktrace:");
                        Newtonsoft.Json.Linq.JArray st =
                            (Newtonsoft.Json.Linq.JArray)error["scriptStackTraceElements"];
                        foreach (var trace in st)
                        {
                            Console.WriteLine(
                                    "\t{0}: {1}",
                                    trace["function"],
                                    trace["lineNumber"]);
                        }
                    }
                }
                else
                {
                    // The result provided by the API needs to be cast into
                    // the correct type, based upon what types the Apps
                    // Script function returns. Here, the function returns
                    // an Apps Script Object with String keys and values.
                    // It is most convenient to cast the return value as a JSON
                    // JObject (folderSet).
                    string s = (string)op.Response["result"];

                    Console.WriteLine("Found spreadsheet: [" + s + "]");
                }
            }
            catch (Google.GoogleApiException e)
            {
                // The API encountered a problem before the script
                // started executing.
                Console.WriteLine("Error calling API:\n{0}", e);
            }
            catch (TaskCanceledException e)
            {
                Console.WriteLine($"Error at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            }
        }
    }
}
