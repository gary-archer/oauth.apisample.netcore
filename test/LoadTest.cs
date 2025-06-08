namespace FinalApi.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Nodes;
    using System.Threading.Tasks;
    using FinalApi.Test.Utils;
    using Xunit;

    /*
     * A basic load test to ensure that the API behaves correctly when there are concurrent requests
     */
    public class LoadTest : IDisposable
    {
        private readonly MockAuthorizationServer mockAuthorizationServer;
        private readonly ApiClient apiClient;
        private readonly string sessionId;
        private int totalCount;
        private int errorCount;

        /*
         * Setup that runs at the start of the test run
         */
        public LoadTest()
        {
            // Create the mock authorization server, which enables productive API tests
            var useProxy = false;
            this.mockAuthorizationServer = new MockAuthorizationServer(useProxy);
            this.mockAuthorizationServer.Start();

            // Create the API client
            var apiBaseUrl = "https://api.authsamples-dev.com:446";
            this.sessionId = Guid.NewGuid().ToString();
            this.apiClient = new ApiClient(apiBaseUrl, "LoadTest", this.sessionId, useProxy);

            // Initialise other fields
            this.totalCount = 0;
            this.errorCount = 0;
        }

        /*
         * Teardown that runs when the load test has completed
         */
        public void Dispose()
        {
            this.mockAuthorizationServer.Stop();
            this.mockAuthorizationServer.Dispose();
        }

        /*
         * Run the load test
         */
        [Fact]
        [Trait("Category", "Load")]
        public void Run()
        {
            // Show a startup message
            Console.WriteLine();
            var startTime = DateTime.UtcNow;
            this.OutputMessage(ConsoleColor.Blue, $"Load test session {this.sessionId} starting at {startTime.ToString("s")}");

            // Show headings for API requests
            string[] headings =
            [
                "OPERATION".PadRight(25, ' '),
                "CORRELATION-ID".PadRight(38, ' '),
                "START-TIME".PadRight(30, ' '),
                "MILLISECONDS-TAKEN".PadRight(21, ' '),
                "STATUS-CODE".PadRight(14, ' '),
                "ERROR-CODE".PadRight(24, ' '),
                "ERROR-ID".PadRight(12, ' '),
            ];
            var header = string.Join(string.Empty, headings);
            this.OutputMessage(ConsoleColor.DarkYellow, header);

            // Get some access tokens to send to the API and send the API requests
            var accessTokens = this.GetAccessTokens();
            this.SendLoadTestRequests(accessTokens);

            // Show a summary end message to finish
            var endTime = DateTime.UtcNow;
            var timeTaken = (endTime - startTime).TotalMilliseconds;
            this.OutputMessage(
                ConsoleColor.Blue,
                $"Load test session {this.sessionId} completed in {timeTaken} milliseconds: {this.errorCount} errors from {this.totalCount} requests");
            Console.WriteLine();
        }

        /*
         * Do some initial work to get multiple access tokens
         */
        private IList<string> GetAccessTokens()
        {
            var jwtOptions = new MockTokenOptions();
            jwtOptions.UseStandardUser();

            var tokens = new List<string>();
            for (int index = 0; index < 5; index++)
            {
                tokens.Add(this.mockAuthorizationServer.IssueAccessToken(jwtOptions));
            }

            return tokens;
        }

        /*
         * Run the main body of API requests, including some invalid requests that trigger errors
         */
        private void SendLoadTestRequests(IList<string> accessTokens)
        {
            // Next produce some requests that will run in parallel
            var requests = new List<Func<Task<ApiResponse>>>();
            for (var index = 0; index < 100; index++)
            {
                // Get the access token
                var accessToken = accessTokens[index % 5];

                // Create a 401 error on request 10, by making the access token act expired
                if (index == 10)
                {
                    accessToken += 'x';
                }

                // Create some promises for various API endpoints
                if (index % 5 == 0)
                {
                    requests.Add(this.CreateUserInfoRequest(accessToken));
                }
                else if (index % 5 == 1)
                {
                    requests.Add(this.CreateTransactionsRequest(accessToken, 2));
                }
                else if (index % 5 == 2)
                {
                    // On request 71 try to access unauthorized data for company 3, to create a 404 error
                    var companyId = (index == 72) ? 3 : 2;
                    requests.Add(this.CreateTransactionsRequest(accessToken, companyId));
                }
                else
                {
                    requests.Add(this.CreateCompaniesRequest(accessToken));
                }
            }

            // Fire the API requests in batches
            this.ExecuteApiRequests(requests);
        }

        /*
         * Create a user info request callback
         */
        private Func<Task<ApiResponse>> CreateUserInfoRequest(string accessToken)
        {
            var options = new ApiRequestOptions(accessToken);
            this.InitializeApiRequest(options);
            return () => this.apiClient.GetUserInfoClaims(options);
        }

        /*
         * Create a companies request callback
         */
        private Func<Task<ApiResponse>> CreateCompaniesRequest(string accessToken)
        {
            var options = new ApiRequestOptions(accessToken);
            this.InitializeApiRequest(options);
            return () => this.apiClient.GetCompanies(options);
        }

        /*
         * Create a user info request callback
         */
        private Func<Task<ApiResponse>> CreateTransactionsRequest(string accessToken, int companyId)
        {
            var options = new ApiRequestOptions(accessToken);
            this.InitializeApiRequest(options);
            return () => this.apiClient.GetCompanyTransactions(options, companyId);
        }

        /*
         * Set any special logic before sending an API request
         */
        private void InitializeApiRequest(ApiRequestOptions options)
        {
            // On request 85 we'll simulate a 500 error via a custom header
            this.totalCount++;
            if (this.totalCount == 85)
            {
                options.RehearseException = true;
            }
        }

        /*
         * Issue API requests in batches of 5, to avoid excessive queueing on a development computer
         * By default there is a limit of 5 concurrent outgoing requests to a single host
         */
        private void ExecuteApiRequests(List<Func<Task<ApiResponse>>> requests)
        {
            // Set counters
            int total = requests.Count;
            int batchSize = 5;
            int current = 0;

            // Process one batch at a time
            while (current < total)
            {
                // Get a batch of requests
                var requestBatch = requests.GetRange(current, Math.Min(batchSize, total - current));

                // Execute them to create promises
                var batchTasks = requestBatch.Select((r) => this.ExecuteApiRequest(r));

                // Wait for the batch to complete
                Task.WaitAll(batchTasks.ToArray());
                current += batchSize;
            }
        }

        /*
         * Start execution and return a success promise regardless of whether the API call succeeded
         */
        private Task<ApiResponse> ExecuteApiRequest(Func<Task<ApiResponse>> resultCallback)
        {
            Func<Task<ApiResponse>, ApiResponse> callback = task =>
            {
                var response = task.Result;
                var statusCode = (int)response.StatusCode;
                if (statusCode >= 200 && statusCode <= 299)
                {
                    // Report successful requests
                    this.OutputMessage(ConsoleColor.Green, this.FormatMetrics(response));
                }
                else
                {
                    // Report failed requests, some of which are expected
                    this.OutputMessage(ConsoleColor.Red, this.FormatMetrics(response));
                    this.errorCount++;
                }

                return response;
            };

            return resultCallback().ContinueWith<ApiResponse>(callback);
        }

        /*
         * Get metrics as a table row
         */
        private string FormatMetrics(ApiResponse response)
        {
            var errorCode = string.Empty;
            var errorId = string.Empty;

            if ((int)response.StatusCode >= 400)
            {
                var error = JsonSerializer.Deserialize<JsonNode>(response.Body);
                if (error["code"] != null)
                {
                    errorCode = error["code"].GetValue<string>();
                }

                if (error["id"] != null)
                {
                    errorId = error["id"].GetValue<int>().ToString();
                }
            }

            string[] values =
            [
                response.Metrics.Operation.PadRight(25),
                response.Metrics.CorrelationId.PadRight(38),
                response.Metrics.StartTime.ToString("s").PadRight(30),
                response.Metrics.MillisecondsTaken.ToString().PadRight(21),
                ((int)response.StatusCode).ToString().PadRight(14),
                errorCode.PadRight(24),
                errorId.PadRight(12),
            ];

            return string.Join(string.Empty, values);
        }

        /*
         * A utility to output in a desired colour
         */
        private void OutputMessage(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
        }
    }
}
