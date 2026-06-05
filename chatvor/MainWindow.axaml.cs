using System;
using System.Net.Sockets;
using System.Text;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace chatvor;

public partial class MainWindow : Window
{
    private TcpClient client;
    private NetworkStream stream;
    private string userName;
    private string ipAddress = "172.28.135.73";
    private int port = 443;
    public MainWindow()
    {
        InitializeComponent();
    }
    
    public async Task StartAsync(string serverIp, int port, string name)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverIp, port);
                stream = client.GetStream();
                
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                Console.Write(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                userName = name;
                byte[] nameData = Encoding.UTF8.GetBytes(userName);
                await stream.WriteAsync(nameData, 0, nameData.Length);

                var window = new chat(client, name, stream);
                window.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Box == null)
        {
            return;
        }
        await StartAsync(ipAddress, port, Box.Text);
    }
}