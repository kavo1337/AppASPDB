using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace appClient;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly HttpClient _http =
            new HttpClient(new HttpClientHandler
            {
                UseProxy = false,
                AutomaticDecompression = DecompressionMethods.All
            })
            {
                BaseAddress = new Uri("http://localhost:5201"),
                Timeout = TimeSpan.FromSeconds(10)
            };
    public MainWindow()
    {
        InitializeComponent();
    }

    public sealed class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Hobby { get; set; }
        public int Age { get; set; }
        public int Balance { get; set; } = 0;
    }

    private async void PingButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Output.Text = "Запрос к /ping...\r\n";
            using var resp = await _http.GetAsync("/ping");
            var body = await resp.Content.ReadAsStringAsync();
            Output.AppendText($"Статус: {(int)resp.StatusCode} {resp.ReasonPhrase}\r\n");
            Output.AppendText($"Тело: {body}\r\n");
        }
        catch (Exception ex)
        {
            Output.AppendText($"Ошибка /ping: {ex.Message}\r\n");
        }
    }

    private async void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            AddButton.IsEnabled = false;

            var name = (NameBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                Output.AppendText("Введите название.\r\n");
                return;
            }

            var email = (EmailBox.Text ?? "").Trim();
            var username = (UsernameBox.Text ?? "").Trim();
            var password = (PasswordBox.Text ?? "").Trim();
            var hobby = (HobbyBox.Text ?? "").Trim();
            
            int age = 0;
            if (!int.TryParse(AgeBox.Text, out age))
            {
                age = 0;
            }
            
            int balance = 0;
            if (!int.TryParse(BalanceBox.Text, out balance))
            {
                balance = 0;
            }


            var payload = new Users
            {
                Name = name,
                Email = email,
                Username = username,
                Password = password,
                Hobby = hobby,
                Age = age,
                Balance = balance
            };

            var resp = await _http.PostAsJsonAsync("/products", payload);

            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                Output.AppendText($"POST /products → {(int)resp.StatusCode} {resp.ReasonPhrase}: {err}\r\n");
                return;
            }

            Output.AppendText($"Добавлено: {name} (Email: {email}, Username: {username})\r\n");

            NameBox.Clear();
            EmailBox.Clear();
            UsernameBox.Clear();
            PasswordBox.Clear();
            HobbyBox.Clear();
            AgeBox.Clear();
            BalanceBox.Clear();
            
            await ReloadProducts();
        }
        catch (Exception ex)
        {
            Output.AppendText($"Ошибка /products (POST): {ex.Message}\r\n");
        }
        finally
        {
            AddButton.IsEnabled = true;
        }
    }

    private async void LoadButton_Click(object sender, RoutedEventArgs e)
    {
        await ReloadProducts();
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ProductsGrid.SelectedItem is not Users u)
            {
                Output.AppendText("Не выбран элемент для удаления\r\n");
                return;
            }

            var confirm = MessageBox.Show(
                $"Удалить пользователя #{u.Id}: {u.Name}?",
                "Подтверждение",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.OK)
                return;

            var resp = await _http.DeleteAsync($"/products/{u.Id}");

            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                Output.AppendText($"DELETE /products/{u.Id} → {(int)resp.StatusCode} {resp.ReasonPhrase}: {err}\r\n");
                return;
            }

            Output.AppendText($"Удалено: #{u.Id} {u.Name}\r\n");
            await ReloadProducts();
        }
        catch (Exception ex)
        {
            Output.AppendText($"Ошибка DELETE: {ex.Message}\r\n");
        }
    }

    private async Task ReloadProducts()
    {
        try
        {
            var resp = await _http.GetAsync("/products");
            if (resp.IsSuccessStatusCode)
            {
                var users = await resp.Content.ReadFromJsonAsync<List<Users>>();
                ProductsGrid.ItemsSource = users;
            }
        }
        catch (Exception ex)
        {
            Output.AppendText($"Ошибка загрузки данных: {ex.Message}\r\n");
        }
    }
}