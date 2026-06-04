using System;
using System.Net.Sockets;
using System.Text;
using Avalonia.Controls;
using System.Threading.Tasks;

namespace chatvor;

public partial class MainWindow : Window
{
    private TcpClient client;
    private NetworkStream stream;
    private string userName;
    private string ipAddress = "192.168.0.65";
    private int port = 13000;
    public MainWindow()
    {
        InitializeComponent();
    }
    
    public async Task StartAsync(string serverIp, int port)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(serverIp, port);
                stream = client.GetStream();
                
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                Console.Write(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                
                userName = Console.ReadLine();
                byte[] nameData = Encoding.UTF8.GetBytes(userName);
                await stream.WriteAsync(nameData, 0, nameData.Length);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    
}