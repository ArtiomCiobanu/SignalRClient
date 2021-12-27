using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalRClient
{
    class Program
    {
        private static string _token =
            "eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNobWFjLXNoYTI1NiIsInR5cCI6IkpXVCJ9.eyJBY2NvdW50SWQiOiIzMjFkMzA0ZS1mYjgzLTQ3MzgtODUwNy0zMDgwN2FkNDAwYTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJVc2VyIiwibmJmIjoxNjM2ODE2MjcxLCJleHAiOjE2Mzc0MjEwNzEsImlzcyI6IlRDRC1TaW5nbGVTaWduT24iLCJhdWQiOiJUQ0QtU2luZ2xlU2lnbk9uIn0.8Xpn6NQLALrTCp4jXwzv5oJ1i_Qhn00Wp1Dmcm76x00";

        private static Guid _playerId;
        private static string _nickname;

        static async Task Main(string[] args)
        {
            var connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:44373/TileGame",
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
            await connection.StartAsync();

            Console.WriteLine("Connection established");

            connection.On<string>("PlayerConnected", PlayerConnected);
            connection.On<string>("PlayerDisconnected", PlayerDisconnected);

            connection.On<Guid, string>("GetPlayerData", GetPlayerData);

            var user = "aaq"; //Console.ReadLine();
            var message = "bbb"; //Console.ReadLine();
            //await connection.SendAsync("SendOthers", user, message);

            await connection.SendAsync("Connect", _token);

            Console.ReadKey();
            await connection.SendAsync("Disconnect", _playerId);
            await connection.StopAsync();

            Console.ReadKey();
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

        private static void GetPlayerData(Guid playerId, string nickname)
        {
            _playerId = playerId;
            _nickname = nickname;
        }
    }
}