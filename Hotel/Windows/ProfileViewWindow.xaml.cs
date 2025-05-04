using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using Application = Hotel.Data.Application;

namespace Hotel.Windows
{
    public partial class ProfileViewWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private Guest _currentGuest;

        public ProfileViewWindow()
        {
            InitializeComponent();
            LoadProfileData();
        }

        private void LoadProfileData()
        {
            if (Application.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться");
                Close();
                return;
            }

            try
            {
                // Загружаем данные гостя с связанными данными
                _currentGuest = _context.Guests
                    .Include(g => g.User)
                    .Include(g => g.Bookings)
                    .FirstOrDefault(g => g.User != null && g.User.UserId == Application.CurrentUser.UserId);

                if (_currentGuest == null)
                {
                    MessageBox.Show("Профиль не найден");
                    Close();
                    return;
                }

                // Устанавливаем контекст данных
                DataContext = new ProfileViewModel
                {
                    User = _currentGuest,
                    BookingsCount = _currentGuest.Bookings.Count,
                    UpcomingBookings = _currentGuest.Bookings
                        .Count(b => b.CheckOutDate > DateOnly.FromDateTime(DateTime.Now))
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}");
                Close();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void ViewBookings_Click(object sender, RoutedEventArgs e)
        {
            new BookingViewWindow().ShowDialog();
        }

        private void ViewProfile_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditProfileWindow();
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем данные на форме
                LoadProfileData();
            }
        }

        private void ViewSpa_Click(object sender, RoutedEventArgs e)
        {
            // Реализуйте по аналогии с другими окнами
            MessageBox.Show("SPA-процедуры будут реализованы в следующей версии");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Application.Logout();
            new AuthorizationWindow().Show();
            Close();
        }
    }

    public class ProfileViewModel
    {
        public Guest User { get; set; }
        public int BookingsCount { get; set; }
        public int UpcomingBookings { get; set; }
    }
}