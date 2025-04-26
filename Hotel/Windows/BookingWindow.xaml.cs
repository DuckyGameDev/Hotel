using Hotel.Data;
using Hotel.Models.Entities;
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

namespace Hotel.Windows
{
    /// <summary>
    /// Логика взаимодействия для BookingWindow.xaml
    /// </summary>
    public partial class BookingWindow : Window
    {

        public Room SelectedRoom { get; set; }
        public string GuestFullName { get; set; }
        public string GuestPassport { get; set; }
        public string GuestPhone { get; set; }
        public string GuestEmail { get; set; }
        public DateTime CheckInDate { get; set; } = DateTime.Today;
        public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(1);

        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public BookingWindow(Room room)
        {
            InitializeComponent();
            SelectedRoom = room;
            DataContext = this;
        }

        private void ConfirmBooking_Click(object sender, RoutedEventArgs e)
        {
            // Валидация данных
            if (string.IsNullOrWhiteSpace(GuestFullName) ||
                string.IsNullOrWhiteSpace(GuestPassport) ||
                string.IsNullOrWhiteSpace(GuestPhone))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CheckOutDate <= CheckInDate)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Создаем гостя
                var guest = new Guest
                {
                    FullName = GuestFullName,
                    PassportData = GuestPassport,
                    ContactPhone = GuestPhone,
                    ContactEmail = string.IsNullOrWhiteSpace(GuestEmail) ? null : GuestEmail
                };

                // Создаем бронирование
                var booking = new Booking
                {
                    Guest = guest,
                    RoomId = SelectedRoom.RoomId,
                    CheckInDate = DateOnly.FromDateTime(CheckInDate),
                    CheckOutDate = DateOnly.FromDateTime(CheckOutDate)
                };

                // Обновляем статус номера
                SelectedRoom.Status = "занят";

                // Сохраняем в БД
                _context.Guests.Add(guest);
                _context.Bookings.Add(booking);
                _context.Rooms.Update(SelectedRoom);
                _context.SaveChanges();

                MessageBox.Show("Бронирование успешно оформлено!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при бронировании: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
