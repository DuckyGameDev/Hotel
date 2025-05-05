using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Application = Hotel.Data.Application;

namespace Hotel.Windows
{
    public partial class SpaOrderWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<SpaServiceItem> _services;

        public class SpaServiceItem
        {
            public Spaservice Service { get; set; }
            public bool IsSelected { get; set; }
            public DateTime? ServiceDate { get; set; }
            public string ServiceTime { get; set; }
            public Dictionary<string, string> AvailableTimes { get; } = new Dictionary<string, string>
            {
                {"09:00", "09:00"}, {"10:00", "10:00"}, {"11:00", "11:00"},
                {"12:00", "12:00"}, {"13:00", "13:00"}, {"14:00", "14:00"},
                {"15:00", "15:00"}, {"16:00", "16:00"}, {"17:00", "17:00"},
                {"18:00", "18:00"}
            };
        }

        public SpaOrderWindow()
        {
            InitializeComponent();
            LoadServices();
        }

        private void LoadServices()
        {
            _context.Spaservices.Load();
            _services = new ObservableCollection<SpaServiceItem>(
                _context.Spaservices.Local.Select(s => new SpaServiceItem
                {
                    Service = s,
                    IsSelected = false,
                    ServiceDate = DateTime.Today
                }).ToList());

            ServicesDataGrid.ItemsSource = _services;
        }

        private void CalculateTotal()
        {
            decimal total = _services
                .Where(s => s.IsSelected)
                .Sum(s => s.Service.Price);

            TotalPriceTextBlock.Text = $"{total:C}";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CalculateTotal();
        }

        private void DatePicker_Changed(object sender, RoutedEventArgs e)
        {
            CalculateTotal();
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedServices = _services.Where(s => s.IsSelected).ToList();
            if (selectedServices.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну процедуру", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка даты и времени для выбранных процедур
            foreach (var service in selectedServices)
            {
                if (service.ServiceDate == null)
                {
                    MessageBox.Show($"Укажите дату для процедуры '{service.Service.ServiceName}'", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(service.ServiceTime))
                {
                    MessageBox.Show($"Укажите время для процедуры '{service.Service.ServiceName}'", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                foreach (var service in selectedServices)
                {
                    var order = new Spaserviceorder
                    {
                        SpaServiceId = service.Service.SpaServiceId,
                        UserId = Application.CurrentUser.UserId,
                        ServiceDate = DateOnly.FromDateTime(service.ServiceDate.Value),
                        ServiceTime = TimeOnly.Parse(service.ServiceTime)
                    };
                    _context.Spaserviceorders.Add(order);
                }

                _context.SaveChanges();
                MessageBox.Show("Процедуры успешно заказаны!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при заказе процедур: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}