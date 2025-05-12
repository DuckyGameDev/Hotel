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
    public partial class GuestManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<Guest> _guests;
        private Guest _currentGuest;

        public GuestManagementWindow()
        {
            InitializeComponent();
            _guests = new ObservableCollection<Guest>();
            GuestsDataGrid.ItemsSource = _guests;
            LoadGuests();
            SetFormState(false);
        }

        private async void LoadGuests()
        {
            await _context.Guests
                .Include(g => g.User)
                .LoadAsync();

            _guests.Clear();

            foreach (var guest in _context.Guests.Local)
                _guests.Add(guest);
        }

        private void SetFormState(bool isEditing)
        {
            AddButton.IsEnabled = !isEditing;
            SaveButton.IsEnabled = isEditing;
            CancelButton.IsEnabled = isEditing;
            GuestsDataGrid.IsEnabled = !isEditing;

            if (!isEditing)
            {
                ClearForm();
            }
        }

        private void ClearForm()
        {
            FullNameTextBox.Text = "";
            PassportTextBox.Text = "";
            PhoneTextBox.Text = "";
            EmailTextBox.Text = "";
            LoginTextBox.Text = "";
            PasswordBox.Password = "";
            _currentGuest = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _currentGuest = new Guest();
            FillFormFromGuest();
            SetFormState(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PassportTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentGuest == null) return;

            _currentGuest.FullName = FullNameTextBox.Text.Trim();
            _currentGuest.PassportData = PassportTextBox.Text.Trim();
            _currentGuest.ContactPhone = PhoneTextBox.Text.Trim();
            _currentGuest.ContactEmail = EmailTextBox.Text.Trim();

            try
            {
                if (_currentGuest.GuestId == 0) // Новый гость
                {
                    // Создаем пользователя для гостя
                    var user = new User
                    {
                        Login = LoginTextBox.Text.Trim(),
                        Password = PasswordBox.Password,
                        Guest = _currentGuest
                    };

                    _context.Guests.Add(_currentGuest);
                    _context.Users.Add(user);
                }
                else
                {
                    // Обновляем данные пользователя, если он существует
                    var user = _context.Users.FirstOrDefault(u => u.GuestId == _currentGuest.GuestId);
                    if (user != null)
                    {
                        user.Login = LoginTextBox.Text.Trim();
                        if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                        {
                            user.Password = PasswordBox.Password;
                        }
                    }
                }

                _context.SaveChanges();
                LoadGuests();
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

        private void GuestsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GuestsDataGrid.SelectedItem is Guest selectedGuest)
            {
                _currentGuest = selectedGuest;
                FillFormFromGuest();
                SetFormState(true);
            }
        }

        private void FillFormFromGuest()
        {
            if (_currentGuest == null) return;

            FullNameTextBox.Text = _currentGuest.FullName;
            PassportTextBox.Text = _currentGuest.PassportData;
            PhoneTextBox.Text = _currentGuest.ContactPhone;
            EmailTextBox.Text = _currentGuest.ContactEmail;

            var user = _context.Users.FirstOrDefault(u => u.GuestId == _currentGuest.GuestId);
            if (user != null)
            {
                LoginTextBox.Text = user.Login;
                PasswordBox.Password = ""; // Не показываем текущий пароль из соображений безопасности
            }
            else
            {
                LoginTextBox.Text = "";
                PasswordBox.Password = "";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            GuestsDataGrid.ItemsSource = _context.Guests.Local
                .Where(g => g.FullName.ToLower().Contains(searchText) ||
                            g.PassportData.ToLower().Contains(searchText) ||
                            g.ContactPhone.Contains(searchText) ||
                            (g.ContactEmail != null && g.ContactEmail.ToLower().Contains(searchText)) ||
                            (g.User != null && g.User.Login.ToLower().Contains(searchText)))
                .ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (GuestsDataGrid.SelectedItem is Guest selectedGuest)
            {
                if (MessageBox.Show($"Удалить гостя {selectedGuest.FullName}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Удаляем связанного пользователя, если он есть
                        var user = _context.Users.FirstOrDefault(u => u.GuestId == selectedGuest.GuestId);
                        if (user != null)
                        {
                            _context.Users.Remove(user);
                        }

                        _context.Guests.Remove(selectedGuest);
                        _context.SaveChanges();
                        LoadGuests();
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
                MessageBox.Show("Выберите гостя для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}