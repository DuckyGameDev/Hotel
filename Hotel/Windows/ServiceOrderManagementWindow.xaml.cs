using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Windows
{
    public partial class ServiceOrderManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<Serviceorder> _serviceOrders;
        private ObservableCollection<Booking> _bookings;
        private ObservableCollection<Service> _services;
        private Serviceorder _currentServiceOrder;

        public ServiceOrderManagementWindow()
        {
            InitializeComponent();
            _serviceOrders = new ObservableCollection<Serviceorder>();
            _bookings = new ObservableCollection<Booking>();
            _services = new ObservableCollection<Service>();

            ServiceOrdersDataGrid.ItemsSource = _serviceOrders;
            LoadData();
            SetFormState(false);
            ServiceDatePicker.SelectedDate = DateTime.Today;
        }

        private async void LoadData()
        {
            await _context.Serviceorders
                .Include(so => so.Booking)
                .ThenInclude(b => b.Guest)
                .Include(so => so.Service)
                .LoadAsync();

            await _context.Bookings.Include(b => b.Guest).LoadAsync();
            await _context.Services.LoadAsync();

            _serviceOrders.Clear();
            _bookings.Clear();
            _services.Clear();

            foreach (var order in _context.Serviceorders.Local)
                _serviceOrders.Add(order);

            foreach (var booking in _context.Bookings.Local)
                _bookings.Add(booking);

            foreach (var service in _context.Services.Local)
                _services.Add(service);

            BookingComboBox.ItemsSource = _bookings;
            ServiceComboBox.ItemsSource = _services;
        }

        private void SetFormState(bool isEditing)
        {
            AddButton.IsEnabled = !isEditing;
            SaveButton.IsEnabled = isEditing;
            CancelButton.IsEnabled = isEditing;
            ServiceOrdersDataGrid.IsEnabled = !isEditing;

            if (!isEditing)
            {
                ClearForm();
            }
        }

        private void ClearForm()
        {
            BookingComboBox.SelectedItem = null;
            ServiceComboBox.SelectedItem = null;
            ServiceDatePicker.SelectedDate = DateTime.Today;
            ServiceTimeTextBox.Text = "12:00";
            _currentServiceOrder = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _currentServiceOrder = new Serviceorder
            {
                ServiceDate = DateOnly.FromDateTime(DateTime.Today),
                ServiceTime = TimeOnly.Parse("12:00")
            };
            FillFormFromServiceOrder();
            SetFormState(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookingComboBox.SelectedItem == null ||
                ServiceComboBox.SelectedItem == null ||
                ServiceDatePicker.SelectedDate == null ||
                !TimeOnly.TryParse(ServiceTimeTextBox.Text, out TimeOnly time))
            {
                MessageBox.Show("Заполните все обязательные поля (бронирование, услуга, дата и корректное время)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentServiceOrder == null) return;

            _currentServiceOrder.Booking = (Booking)BookingComboBox.SelectedItem;
            _currentServiceOrder.Service = (Service)ServiceComboBox.SelectedItem;
            _currentServiceOrder.ServiceDate = DateOnly.FromDateTime(ServiceDatePicker.SelectedDate.Value);
            _currentServiceOrder.ServiceTime = time;

            try
            {
                if (_currentServiceOrder.OrderId == 0)
                {
                    _context.Serviceorders.Add(_currentServiceOrder);
                }

                _context.SaveChanges();
                LoadData();
                SetFormState(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SetFormState(false);
        }

        private void ServiceOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceOrdersDataGrid.SelectedItem is Serviceorder selectedOrder)
            {
                _currentServiceOrder = selectedOrder;
                FillFormFromServiceOrder();
                SetFormState(true);
            }
        }

        private void FillFormFromServiceOrder()
        {
            if (_currentServiceOrder == null) return;

            BookingComboBox.SelectedItem = _currentServiceOrder.Booking;
            ServiceComboBox.SelectedItem = _currentServiceOrder.Service;
            ServiceDatePicker.SelectedDate = new DateTime(
                _currentServiceOrder.ServiceDate.Year,
                _currentServiceOrder.ServiceDate.Month,
                _currentServiceOrder.ServiceDate.Day);
            ServiceTimeTextBox.Text = _currentServiceOrder.ServiceTime.ToString("HH:mm");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            ServiceOrdersDataGrid.ItemsSource = _context.Serviceorders.Local
                .Where(so => so.Booking.Guest.FullName.ToLower().Contains(searchText) ||
                             so.Service.ServiceName.ToLower().Contains(searchText) ||
                             so.ServiceDate.ToString().Contains(searchText) ||
                             so.ServiceTime.ToString().Contains(searchText) ||
                             so.BookingId.ToString().Contains(searchText))
                .ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServiceOrdersDataGrid.SelectedItem is Serviceorder selectedOrder)
            {
                if (MessageBox.Show($"Удалить заказ услуги {selectedOrder.Service.ServiceName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Serviceorders.Remove(selectedOrder);
                        _context.SaveChanges();
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}