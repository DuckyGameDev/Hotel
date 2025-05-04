using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Application = Hotel.Data.Application;

namespace Hotel.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditProfileWindow.xaml
    /// </summary>
    public partial class EditProfileWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private Guest _currentGuest;

        public EditProfileWindow()
        {
            InitializeComponent();
            LoadGuestData();
        }

        private void LoadGuestData()
        {
            if (Application.CurrentGuest == null)
            {
                MessageBox.Show("Необходимо авторизоваться");
                Close();
                return;
            }

            try
            {
                _currentGuest = _context.Guests
                    .FirstOrDefault(g => g.GuestId == Application.CurrentGuest.GuestId);

                if (_currentGuest == null)
                {
                    MessageBox.Show("Профиль не найден");
                    Close();
                    return;
                }

                DataContext = _currentGuest;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}");
                Close();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentGuest.FullName) ||
                string.IsNullOrWhiteSpace(_currentGuest.PassportData) ||
                string.IsNullOrWhiteSpace(_currentGuest.ContactPhone))
            {
                MessageBox.Show("Заполните обязательные поля (ФИО, паспорт, телефон)");
                return;
            }

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Изменения сохранены успешно!");

                // Обновляем данные в Application
                Application.CurrentGuest = _currentGuest;

                DialogResult = true;
                Close();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var changePasswordWindow = new ChangePasswordWindow();
            if (changePasswordWindow.ShowDialog() == true)
            {
                MessageBox.Show("Пароль успешно изменен");
            }
        }
    }
}
