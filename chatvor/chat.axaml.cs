using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;


namespace chatvor;
public partial class chat : Window
{
    public TcpClient client;
    public string name;
    public NetworkStream stream;
    ObservableCollection<object> chates;

    public chat(TcpClient client, string name, NetworkStream stream)
    {
        InitializeComponent();
        chates = new ObservableCollection<object>();
        MessagesLB.ItemsSource = chates;
        this.client = client;
        this.name = name;
        this.stream = stream;
        _ = ReceiveMessagesAsync();
    }
    public async void SendText_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextMessageTB.Text))
        {
            return;
        }
        string msg = "0" + TextMessageTB.Text;
        byte[] data = Encoding.UTF8.GetBytes(msg);
        await stream.WriteAsync(data, 0, data.Length);
        TextMessageTB.Text = "";
    }
    
    public async void SendPhoto_Click(object sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(
            new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Выберите фото",
                FileTypeFilter = new[] { Avalonia.Platform.Storage.FilePickerFileTypes.ImageAll }
            });

        if (files.Count > 0)
        {
            await using var fs = await files[0].OpenReadAsync();
            using var ms = new MemoryStream();
            await fs.CopyToAsync(ms);
            
            string msg = "1" + Convert.ToBase64String(ms.ToArray());
            byte[] data = Encoding.UTF8.GetBytes(msg);
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
    
    private async Task ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[100000];
        
        try
        {
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Dispatcher.UIThread.Post(() => 
                {
                    if (message.Length > 0 && message[0] == '1')
                    {
                        string base64 = message.Substring(1); 
                        byte[] imgBytes = Convert.FromBase64String(base64);
                        using var ms = new MemoryStream(imgBytes);
                        var bitmap = new Bitmap(ms);
                        chates.Add(bitmap);
                    }
                    else if (message.Length > 0 && message[0] == '0')
                    {
                        string text = message.Substring(1); 
                        chates.Add(text);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
    
}