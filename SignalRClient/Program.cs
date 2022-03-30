using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SignalRClient
{
    class Program
    {
        private static string _token =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJBY2NvdW50SWQiOiIxODUzZjljMC0xYTIwLTRhMzAtOTFjMC1hMzZlNzk4ZGQyZTMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJVc2VyIiwibmJmIjoxNjQyOTM0NDA2LCJleHAiOjE2NDM1MzkyMDYsImlzcyI6IlRDRC1TaW5nbGVTaWduT24iLCJhdWQiOiJUQ0QtU2luZ2xlU2lnbk9uIn0.xiyMrze3RoGHvFORHSZqIeLraYo9_zT1pITHC1CqyGA";

        private static readonly Guid _userId = new Guid("C365D877-9B65-4C6E-1FAD-08DA077AEED3");
        private static readonly Guid _chatId = new Guid("6342a06b-faee-46e1-8b3f-402399f19537");

        private static string _url = "https://localhost:44373/ChatHub";

        private static readonly HttpClient _httpclient = new();

        private static async Task Main()
        {
            var connection = await ConnectAndInitialize();

            if (connection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("Successfully connected to the server.");
            }
            else
            {
                Console.WriteLine("Could not connect to the server.");
            }

            await connection.SendAsync("SendMessage", _userId, "Test!");

            Console.ReadKey();
        }

        private static int IntReadline()
        {
            var input = Console.ReadLine();
            int value;
            while (!int.TryParse(input, out value))
            {
                Console.Write("Enter a valid int value: ");
                input = Console.ReadLine();
            }

            return value;
        }

        private static async Task<HubConnection> ConnectAndInitialize()
        {
            var connection = GetHubConnection();
            await connection.StartAsync();

            await connection.SendAsync("Connect", _userId);
            _httpclient.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);

            return connection;
        }

        private static HubConnection GetHubConnection()
        {
            var connection = new HubConnectionBuilder()
                .WithUrl(_url,
                    c =>
                    {
                        //c.AccessTokenProvider = () => Task.FromResult(_token);
                        c.HttpMessageHandlerFactory = _ => new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };
                    }).Build();

            connection.Closed += (e) =>
            {
                Console.WriteLine("Connection closed.");
                return Task.CompletedTask;
            };

            connection.On<string, string>("ReceiveMessage", PrintReceivedMessage);
            connection.On<string>("PlayerConnected", PlayerConnected);
            connection.On<string>("PlayerDisconnected", PlayerDisconnected);

            return connection;
        }

        private static void PrintReceivedMessage(string user, string message)
        {
            Console.WriteLine($"Player {user} has sent: {message}");
        }

        private static void PlayerConnected(string nickname)
        {
            Console.WriteLine($"{nickname} has connected.");
        }

        private static void PlayerDisconnected(string nickname)
        {
            Console.WriteLine($"{nickname} has disconnected.");
        }
    }

    public class ConnectResponse
    {
        public Guid PlayerId { get; init; }
        public string Nickname { get; init; }
    }
}