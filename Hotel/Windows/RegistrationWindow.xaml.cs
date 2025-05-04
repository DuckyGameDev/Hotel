using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using Application = Hotel.Data.Application;

namespace Hotel.Windows
{
    public partial class RegistrationWindow : Window
    {
        private readonly ApplicationDbContext _context;

        public RegistrationWindow()
        {
            InitializeComponent();
            _context = new ApplicationDbContext();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(LoginBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password) ||
                string.IsNullOrWhiteSpace(FullNameBox.Text) ||
                string.IsNullOrWhiteSpace(PassportBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Проверка существования пользователя
                if (await _context.Users.AnyAsync(u => u.Login == LoginBox.Text))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка паспортных данных
                if (await _context.Guests.AnyAsync(g => g.PassportData == PassportBox.Text))
                {
                    MessageBox.Show("Гость с такими паспортными данными уже зарегистрирован", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Создание гостя
                var guest = new Guest
                {
                    FullName = FullNameBox.Text,
                    PassportData = PassportBox.Text,
                    ContactPhone = PhoneBox.Text,
                    ContactEmail = string.IsNullOrWhiteSpace(EmailBox.Text) ? null : EmailBox.Text
                };

                // Создание пользователя
                var user = new User
                {
                    Login = LoginBox.Text,
                    Password = Verification.HashPassword(PasswordBox.Password),
                    Guest = guest,
                    RoleId = await _context.Roles
                        .Where(r => r.RoleName == "guest")
                        .Select(r => r.RoleId)
                        .FirstOrDefaultAsync()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Автоматический вход после регистрации
                Application.SetCurrentUser(user);
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new AuthorizationWindow();
            authWindow.Show();
            this.Close();
        }
    }
}