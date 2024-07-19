using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace My_Project_Step2
{
    public partial class MainWindow : Window
    {
        private TcpClient _currentClient;
        private TcpListener[] _servers;
        private List<TcpClient> _clients = new List<TcpClient>();
        private const int MaxClients = 4;
        private const int BasePort = 5000;

        public MainWindow()
        {
            InitializeComponent();
            _servers = new TcpListener[MaxClients]; // Инициализация массива
            StartServers();
        }

        private async void StartServers()
        {
            for (int i = 0; i < MaxClients; i++)
            {
                int port = BasePort + i;
                _servers[i] = new TcpListener(IPAddress.Any, port);
                _servers[i].Start();
                Console.WriteLine($"Сервер запущен на порту {port}...");

                _ = AcceptClientsAsync(_servers[i]);
            }
        }

        private async Task AcceptClientsAsync(TcpListener server)
        {
            while (true)
            {
                var client = await server.AcceptTcpClientAsync();
                _clients.Add(client);
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            Console.WriteLine("Клиент подключен.");
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Получено сообщение: {message}");

                    // Отправляем сообщение всем клиентам, кроме отправителя
                    await BroadcastMessage(message, client);
                }
            }

            Console.WriteLine("Клиент отключен.");
            _clients.Remove(client);
            client.Close();
        }

        private async Task BroadcastMessage(string message, TcpClient senderClient)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (var client in _clients)
            {
                if (client != senderClient) // Проверяем, что это не отправитель
                {
                    var stream = client.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            string message = MessageTextBox.Text;

            if (!string.IsNullOrWhiteSpace(message))
            {
                // Добавляем сообщение с временной меткой
                string timeStamp = DateTime.Now.ToString("HH:mm:ss");
                MessagesListBox.Items.Add($"[{timeStamp}] {message}");

                // Получаем текущий клиент (укажите ваш метод получения текущего клиента)
                TcpClient currentClient = _currentClient;

                // Отправляем сообщение всем клиентам
                await BroadcastMessage(message, currentClient);

                MessageTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите сообщение.");
            }
        }


        private void UserPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("User photo clicked!");
        }
    }
}
