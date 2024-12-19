using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using RestSharp;


namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            string dayOfWeek = DateTime.Now.ToString("dddd", new CultureInfo("ru-RU"));
            string time = DateTime.Now.ToString("HH:mm");

            DateTimeText.Text = $"{char.ToUpper(dayOfWeek[0]) + dayOfWeek.Substring(1)}, {time}";
            SearchBox.Text = "Введите название города...";
            SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Введите название города...")
            {
                SearchBox.Text = string.Empty;
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Введите название города...";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Button_Click(null, null);
            }
        }
        private void TimeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateWeatherForSelectedTime();
        }
        private void UpdateWeatherForSelectedTime()
        {
            if (TimeSelector.SelectedItem is ComboBoxItem selectedTimeItem)
            {
                string selectedTime = selectedTimeItem.Content.ToString();


                if (weeklyWeatherData != null)
                {
                    var filteredForecasts = weeklyWeatherData
                        .Where(item =>
                        {
                            var dateTime = DateTimeOffset.FromUnixTimeSeconds(item.Dt).DateTime;
                            return dateTime.ToString("HH:mm") == selectedTime;
                        })
                        .ToList();

                    UpdateDailyForecasts(filteredForecasts);
                }
                else
                {
                    MessageBox.Show("Данные о погоде ещё не загружены. Пожалуйста, выполните поиск города.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void UpdateDailyForecasts(List<WeatherItem> forecasts)
        {

            Day1Forecast.Text = string.Empty;
            Day2Forecast.Text = string.Empty;
            Day3Forecast.Text = string.Empty;
            Day4Forecast.Text = string.Empty;
            Day5Forecast.Text = string.Empty;


            var dailyForecasts = forecasts
                .GroupBy(item => DateTimeOffset.FromUnixTimeSeconds(item.Dt).Date)
                .ToList();

            SetDayForecast(Day1Forecast, Day1Icon, dailyForecasts, 1);
            SetDayForecast(Day2Forecast, Day2Icon, dailyForecasts, 2);
            SetDayForecast(Day3Forecast, Day3Icon, dailyForecasts, 3);
            SetDayForecast(Day4Forecast, Day4Icon, dailyForecasts, 4);
            SetDayForecast(Day5Forecast, Day5Icon, dailyForecasts, 5);
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string cityName = SearchBox.Text.Trim();
            if (!string.IsNullOrEmpty(cityName))
            {
                await GetWeatherDataAsync(cityName);
                await GetWeeklyWeatherAsync(cityName);

            }
            else
            {
                MessageBox.Show("Введите название города.");
            }
        }
        private async Task GetWeatherDataAsync(string cityName)
        {
            if (!string.IsNullOrEmpty(cityName))
            {
                string apiKey = "706409d51e03b2c036730896e1171175";
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&lang=ru&appid={apiKey}&units=metric";

                var client = new RestClient();
                var request = new RestRequest(url, Method.Get);

                try
                {
                    RestResponse response = await client.ExecuteAsync(request);
                    if (response.IsSuccessful)
                    {

                        DisplayWeatherData(response.Content);
                    }
                    else
                    {
                        MessageBox.Show("Не удалось получить данные о погоде. Проверьте название города.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при получении данных о погоде: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Введите название города", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task GetWeeklyWeatherAsync(string cityName)
        {
            string apiKey = "706409d51e03b2c036730896e1171175";
            string url = $"https://api.openweathermap.org/data/2.5/forecast?q={cityName}&appid={apiKey}&units=metric&lang=ru";

            var client = new RestClient();
            var request = new RestRequest(url, Method.Get);

            try
            {
                RestResponse response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    DisplayWeeklyWeather(response.Content);
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении данных о прогнозе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private string GetClothingRecommendation(float temperature, string weatherDescription)
        {
            if (temperature < 0)
                return "Одевайтесь теплее: зимняя куртка, шарф, шапка, перчатки.";

            if (temperature >= 0 && temperature < 10)
                return "Одевайте: пальто, свитер, теплые ботинки.";

            if (temperature >= 10 && temperature < 20)
                return "Легкая куртка или кардиган, удобная обувь.";

            if (temperature >= 20 && temperature < 30)
                return "Футболка и легкие брюки или шорты.";

            if (temperature >= 30)
                return "Легкая одежда: шорты, майка, сандалии.";

            if (weatherDescription.Contains("дождь") || weatherDescription.Contains("ливень"))
                return "Не забудьте взять с собой зонт или дождевик.";

            if (weatherDescription.Contains("снег"))
                return "Теплая одежда, сапоги и шапка.";

            return "Одевайтесь по своему усмотрению.";
        }
        private string GetWeatherIconPath(string description)
        {
            if (description.Contains("дождь"))
            {

                return "E:/programa kurs/WeatherApp/Images/rain.png";
            }
            else if (description.Contains("ясно"))
            {
                return "E:/programa kurs/WeatherApp/Images/clearly.png";
            }
            else if (description.Contains("облачно"))
            {
                return "E:/programa kurs/WeatherApp/Images/cloudy.png";
            }
            else if (description.Contains("облачно с прояснениями"))
            {
                return "E:/programa kurs/WeatherApp/Images/cloudywith.png";
            }
            else if (description.Contains("туман"))
            {
                return "E:/programa kurs/WeatherApp/Images/fog.png";
            }
            else if (description.Contains("пасмурно"))
            {
                return "E:/programa kurs/WeatherApp/Images/gray.png";
            }
            else if (description.Contains("шторм"))
            {
                return "E:/programa kurs/WeatherApp/Images/thundershtorm.png";
            }
            else if (description.Contains("снег"))
            {
                return "E:/programa kurs/WeatherApp/Images/snow.png";
            }

            return "E:/programa kurs/WeatherApp/Images/def.png";
        }
        private void DisplayWeatherData(string weatherJson)
        {
            var weatherData = JsonConvert.DeserializeObject<WeatherResponse>(weatherJson);
            if (weatherData != null)
            {
                TemperatureLabel.Content = $"Температура: {Math.Round(weatherData.Main.Temp)} °C";
                string description = weatherData.Weather[0].Description;
                DescriptionLabel.Content = $"Погода: {description}";


                string recommendation = GetClothingRecommendation(weatherData.Main.Temp, description);
                ClothingRecommendationTextBlock.Text = $"Рекомендация по одежде: {recommendation}";

                string iconPath = GetWeatherIconPath(weatherData.Weather[0].Description);
                WeatherIconToday.Source = new BitmapImage(new Uri(iconPath, UriKind.RelativeOrAbsolute));
            }
        }
        private void DisplayWeeklyWeather(string weatherJson)
        {
            if (string.IsNullOrEmpty(weatherJson))
            {
                MessageBox.Show("Ошибка: Ответ от сервера пустой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var weatherData = JsonConvert.DeserializeObject<WeeklyWeatherResponse>(weatherJson);

                if (weatherData != null && weatherData.List != null)
                {

                    weeklyWeatherData = weatherData.List;


                    var dailyForecasts = weatherData.List
                        .Where(item => DateTimeOffset.FromUnixTimeSeconds(item.Dt).ToString("HH:mm") == "12:00")
                        .GroupBy(item => DateTimeOffset.FromUnixTimeSeconds(item.Dt).Date)
                        .ToList();


                    SetDayForecast(Day1Forecast, Day1Icon, dailyForecasts, 1);
                    SetDayForecast(Day2Forecast, Day2Icon, dailyForecasts, 2);
                    SetDayForecast(Day3Forecast, Day3Icon, dailyForecasts, 3);
                    SetDayForecast(Day4Forecast, Day4Icon, dailyForecasts, 4);
                    SetDayForecast(Day5Forecast, Day5Icon, dailyForecasts, 5);
                }
                else
                {
                    MessageBox.Show("Ошибка: Невозможно разобрать данные о погоде на неделю.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при разборе данных о прогнозе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void SetDayForecast(TextBlock forecastTextBlock, Image weatherIcon, List<IGrouping<DateTime, WeatherItem>> dailyForecasts, int dayOffset)
        {
            var targetDay = DateTime.Now.AddDays(dayOffset).Date;

            var forecast = dailyForecasts.FirstOrDefault(group => group.Key == targetDay);

            if (forecast != null)
            {
                var forecastItem = forecast.First();

                string date = DateTimeOffset.FromUnixTimeSeconds(forecastItem.Dt).ToString("dddd, dd MMMM", new CultureInfo("ru-RU"));
                string temp = $"{Math.Round(forecastItem.Main.Temp)} °C";
                string description = forecastItem.Weather.FirstOrDefault()?.Description ?? string.Empty;
                string recommendation = GetClothingRecommendation(forecastItem.Main.Temp, description);
                string iconPath = GetWeatherIconPath(description);
                weatherIcon.Source = new BitmapImage(new Uri(iconPath, UriKind.RelativeOrAbsolute));


                if (!string.IsNullOrEmpty(description))
                {
                    forecastTextBlock.Text = $"{date}: {temp}, {description}. Рекомендация: {recommendation}";
                }
                else
                {
                    forecastTextBlock.Text = string.Empty;

                }
            }
            else
            {
                forecastTextBlock.Text = string.Empty;
                weatherIcon.Source = null;
            }
        }
        private List<WeatherItem> weeklyWeatherData;
        public class WeatherResponse
        {
            public MainInfo Main { get; set; }
            public List<Weather> Weather { get; set; }
        }
        public class MainInfo
        {
            public float Temp { get; set; }
        }
        public class Weather
        {
            public string Description { get; set; }
        }
        public class WeeklyWeatherResponse
        {
            [JsonProperty("list")]
            public List<WeatherItem> List { get; set; }
        }
        public class WeatherItem
        {
            [JsonProperty("dt")]
            public long Dt { get; set; }

            [JsonProperty("main")]
            public MainInfo Main { get; set; }

            [JsonProperty("weather")]
            public List<Weather> Weather { get; set; }
        }
    }
}