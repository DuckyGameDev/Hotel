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
    public partial class SpaServiceOrderManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<Spaserviceorder> _spaServiceOrders;
        private ObservableCollection<Spaservice> _spaServices;
        private ObservableCollection<User> _users;
        private Spaserviceorder _currentSpaServiceOrder;

        public SpaServiceOrderManagementWindow()
        {
            InitializeComponent();
            _spaServiceOrders = new ObservableCollection<Spaserviceorder>();
            _spaServices = new ObservableCollection<Spaservice>();
            _users = new ObservableCollection<User>();

            SpaServiceOrdersDataGrid.ItemsSource = _spaServiceOrders;
            LoadData();
            SetFormState(false);
            ServiceDatePicker.SelectedDate = DateTime.Today;
        }

        private async void LoadData()
        {
            await _context.Spaserviceorders
                .Include(so => so.SpaService)
                .Include(so => so.User)
                .ThenInclude(u => u.Guest)
                .LoadAsync();

            await _context.Spaservices.LoadAsync();
            await _context.Users.Include(u => u.Guest).LoadAsync();

            _spaServiceOrders.Clear();
            _spaServices.Clear();
            _users.Clear();

            foreach (var order in _context.Spaserviceorders.Local)
                _spaServiceOrders.Add(order);

            foreach (var service in _context.Spaservices.Local)
                _spaServices.Add(service);

            foreach (var user in _context.Users.Local)
                _users.Add(user);

            SpaServiceComboBox.ItemsSource = _spaServices;
            UserComboBox.ItemsSource = _users;
        }

        private void SetFormState(bool isEditing)
        {
            AddButton.IsEnabled = !isEditing;
            SaveButton.IsEnabled = isEditing;
            CancelButton.IsEnabled = isEditing;
            SpaServiceOrdersDataGrid.IsEnabled = !isEditing;

            if (!isEditing)
            {
                ClearForm();
            }
        }

        private void ClearForm()
        {
            SpaServiceComboBox.SelectedItem = null;
            UserComboBox.SelectedItem = null;
            ServiceDatePicker.SelectedDate = DateTime.Today;
            ServiceTimeTextBox.Text = "12:00";
            _currentSpaServiceOrder = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _currentSpaServiceOrder = new Spaserviceorder
            {
                ServiceDate = DateOnly.FromDateTime(DateTime.Today),
                ServiceTime = TimeOnly.Parse("12:00")
            };
            FillFormFromSpaServiceOrder();
            SetFormState(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpaServiceComboBox.SelectedItem == null ||
                UserComboBox.SelectedItem == null ||
                ServiceDatePicker.SelectedDate == null ||
                !TimeOnly.TryParse(ServiceTimeTextBox.Text, out TimeOnly time))
            {
                MessageBox.Show("Заполните все обязательные поля (услуга, пользователь, дата и корректное время)!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentSpaServiceOrder == null) return;

            _currentSpaServiceOrder.SpaService = (Spaservice)SpaServiceComboBox.SelectedItem;
            _currentSpaServiceOrder.User = (User)UserComboBox.SelectedItem;
            _currentSpaServiceOrder.ServiceDate = DateOnly.FromDateTime(ServiceDatePicker.SelectedDate.Value);
            _currentSpaServiceOrder.ServiceTime = time;

            try
            {
                if (_currentSpaServiceOrder.SpaOrderId == 0)
                {
                    _context.Spaserviceorders.Add(_currentSpaServiceOrder);
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

        private void SpaServiceOrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpaServiceOrdersDataGrid.SelectedItem is Spaserviceorder selectedOrder)
            {
                _currentSpaServiceOrder = selectedOrder;
                FillFormFromSpaServiceOrder();
                SetFormState(true);
            }
        }

        private void FillFormFromSpaServiceOrder()
        {
            if (_currentSpaServiceOrder == null) return;

            SpaServiceComboBox.SelectedItem = _currentSpaServiceOrder.SpaService;
            UserComboBox.SelectedItem = _currentSpaServiceOrder.User;
            ServiceDatePicker.SelectedDate = new DateTime(
                _currentSpaServiceOrder.ServiceDate.Year,
                _currentSpaServiceOrder.ServiceDate.Month,
                _currentSpaServiceOrder.ServiceDate.Day);
            ServiceTimeTextBox.Text = _currentSpaServiceOrder.ServiceTime.ToString("HH:mm");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            SpaServiceOrdersDataGrid.ItemsSource = _context.Spaserviceorders.Local
                .Where(so => so.SpaService.ServiceName.ToLower().Contains(searchText) ||
                             so.User.Login.ToLower().Contains(searchText) ||
                             (so.User.Guest != null && so.User.Guest.FullName.ToLower().Contains(searchText)) ||
                             so.ServiceDate.ToString().Contains(searchText) ||
                             so.ServiceTime.ToString().Contains(searchText))
                .ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpaServiceOrdersDataGrid.SelectedItem is Spaserviceorder selectedOrder)
            {
                if (MessageBox.Show($"Удалить заказ SPA-услуги {selectedOrder.SpaService.ServiceName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Spaserviceorders.Remove(selectedOrder);
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