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

namespace Hotel.Windows
{
    /// <summary>
    /// Логика взаимодействия для BookingViewWindow.xaml
    /// </summary>
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
                // var detailsWindow = new BookingDetailsWindow(selectedBooking);
                //detailsWindow.ShowDialog();
            }
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsListView.SelectedItem is Booking selectedBooking)
            {
                //var reviewWindow = new AddReviewWindow(selectedBooking);
                //if (reviewWindow.ShowDialog() == true)
                //{
                //    LoadBookings();
                //}
            }
        }
    }
}
