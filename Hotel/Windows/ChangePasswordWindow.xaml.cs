using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using Application = Hotel.Data.Application;

namespace Hotel.Windows
{
    public partial class ChangePasswordWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private User _currentUser;

        public ChangePasswordWindow()
        {
            InitializeComponent();
            LoadCurrentUser();
        }

        private void LoadCurrentUser()
        {
            if (Application.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться");
                Close();
                return;
            }

            _currentUser = _context.Users
                .FirstOrDefault(u => u.UserId == Application.CurrentUser.UserId);

            if (_currentUser == null)
            {
                MessageBox.Show("Пользователь не найден");
                Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            if (!Verification.VerifyPassword(CurrentPasswordBox.Password, _currentUser.Password))
            {
                MessageBox.Show("Неверный текущий пароль");
                return;
            }

            if (NewPasswordBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов");
                return;
            }

            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Новые пароли не совпадают");
                return;
            }

            try
            {
                _currentUser.Password = Verification.HashPassword(NewPasswordBox.Password);
                _context.SaveChanges();

                // Обновляем данные в Application
                Application.CurrentUser = _currentUser;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при смене пароля: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}