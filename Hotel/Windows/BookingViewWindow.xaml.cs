using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace Hotel.Windows
{
    public partial class BookingViewWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private readonly int _currentGuestId;

        public BookingViewWindow(int guestId)
        {
            InitializeComponent();
            _currentGuestId = guestId;
            LoadBookings();
        }

        private void LoadBookings()
        {
            _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Room.Category)
                .Include(b => b.Guest)
                .Include(b => b.Serviceorders)
                    .ThenInclude(so => so.Service)
                .Where(b => b.GuestId == _currentGuestId)
                .Load();

            BookingsListView.ItemsSource = _context.Bookings.Local.ToObservableCollection();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsListView.SelectedItem is Booking selectedBooking)
            {
                if (MessageBox.Show("Вы уверены, что хотите отменить бронирование?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _context.Bookings.Remove(selectedBooking);
                    selectedBooking.Room.Status = "свободен";
                    _context.SaveChanges();
                    LoadBookings();
                }
            }
            else
            {
                MessageBox.Show("Выберите бронирование для отмены");
            }
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsListView.SelectedItem is Booking selectedBooking)
            {
                // Загружаем полные данные бронирования
                var fullBooking = _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.Room.Category)
                    .Include(b => b.Guest)
                    .Include(b => b.Serviceorders)
                        .ThenInclude(so => so.Service)
                    .FirstOrDefault(b => b.BookingId == selectedBooking.BookingId);

                if (fullBooking != null)
                {
                    var bookingWindow = new BookingWindow(fullBooking, true);
                    if (bookingWindow.ShowDialog() == true)
                    {
                        // Обновляем данные после редактирования
                        _context.SaveChanges();
                        LoadBookings();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите бронирование для просмотра деталей");
            }
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            //if (BookingsListView.SelectedItem is Booking selectedBooking)
            //{
            //    var reviewWindow = new AddReviewWindow(selectedBooking);
            //    if (reviewWindow.ShowDialog() == true)
            //    {
            //        LoadBookings();
            //    }
            //}
        }
    }
}