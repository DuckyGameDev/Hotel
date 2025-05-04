using Hotel.Data;
using Hotel.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using Application = Hotel.Data.Application;

namespace Hotel.Windows
{
    public partial class BookingViewWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public BookingViewWindow()
        {
            InitializeComponent();
            LoadBookings();
        }

        private void LoadBookings()
        {
            if (Application.CurrentUser == null)
            {
                MessageBox.Show("Необходимо авторизоваться для просмотра бронирований");
                Close();
                return;
            }

            try
            {
                _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.Room.Category)
                    .Include(b => b.Guest)
                    .Include(b => b.Serviceorders)
                        .ThenInclude(so => so.Service)
                    .Where(b => b.Guest.User != null && b.Guest.User.UserId == Application.CurrentUser.UserId)
                    .Load();

                BookingsListView.ItemsSource = _context.Bookings.Local.ToObservableCollection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке бронирований: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelBooking_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsListView.SelectedItem is Booking selectedBooking)
            {
                if (selectedBooking.CheckInDate <= DateOnly.FromDateTime(DateTime.Now))
                {
                    MessageBox.Show("Невозможно отменить уже начавшееся бронирование");
                    return;
                }

                if (MessageBox.Show("Вы уверены, что хотите отменить бронирование?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    try
                    {
                        selectedBooking.Room.Status = "свободен";
                        _context.Bookings.Remove(selectedBooking);
                        _context.SaveChanges();
                        LoadBookings();
                        MessageBox.Show("Бронирование успешно отменено");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при отмене бронирования: {ex.Message}");
                    }
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
                try
                {
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
                        bookingWindow.ShowDialog();
                        LoadBookings();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке деталей бронирования: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите бронирование для просмотра деталей");
            }
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsListView.SelectedItem is Booking selectedBooking)
            {
                if (selectedBooking.CheckOutDate > DateOnly.FromDateTime(DateTime.Now))
                {
                    MessageBox.Show("Отзыв можно оставить только после выезда");
                    return;
                }

                // Проверяем, не оставлял ли уже пользователь отзыв
                if (_context.Reviews.Any(r => r.BookingId == selectedBooking.BookingId))
                {
                    MessageBox.Show("Вы уже оставляли отзыв на это бронирование");
                    return;
                }

                //var reviewWindow = new AddReviewWindow(selectedBooking);
                //if (reviewWindow.ShowDialog() == true)
                //{
                //    MessageBox.Show("Спасибо за ваш отзыв!");
                //}
            }
            else
            {
                MessageBox.Show("Выберите бронирование для оставления отзыва");
            }
        }
    }
}