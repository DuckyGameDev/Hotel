using Hotel.Data;
using Hotel.Models.Entities;
using Hotel.Windows;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Application = Hotel.Data.Application;

namespace Hotel
{
    public partial class MainWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<Room> _filteredRooms = new ObservableCollection<Room>();
        private DateTime _defaultCheckInDate = DateTime.Today;
        private DateTime _defaultCheckOutDate = DateTime.Today.AddDays(1);

        public MainWindow()
        {
            InitializeComponent();
            InitializeDates();
            LoadCategories();
            LoadRooms();
            ConfigureAccess();
        }

        private void InitializeDates()
        {
            CheckInDatePicker.SelectedDate = _defaultCheckInDate;
            CheckOutDatePicker.SelectedDate = _defaultCheckOutDate;

            CheckInDatePicker.SelectedDateChanged += (sender, e) =>
            {
                if (CheckInDatePicker.SelectedDate.HasValue)
                {
                    var newCheckIn = CheckInDatePicker.SelectedDate.Value;
                    if (CheckOutDatePicker.SelectedDate <= newCheckIn)
                    {
                        CheckOutDatePicker.SelectedDate = newCheckIn.AddDays(1);
                    }
                }
            };
        }

        private void ConfigureAccess()
        {
            if (Application.IsGuest)
            {
                AddRoomBtn.Visibility = Visibility.Collapsed;
                EditRoomBtn.Visibility = Visibility.Collapsed;
                DeleteRoomBtn.Visibility = Visibility.Collapsed;
                ManageBtn.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadCategories()
        {
            _context.Roomcategories.Load();
            CategoryComboBox.ItemsSource = _context.Roomcategories.Local.ToObservableCollection();
            CategoryComboBox.SelectedIndex = -1;
        }

        private void LoadRooms()
        {
            _context.Rooms
                .Include(r => r.Category)
                .Include(r => r.Bookings)
                .Load();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (!ValidateDates()) return;

            var checkIn = CheckInDatePicker.SelectedDate ?? _defaultCheckInDate;
            var checkOut = CheckOutDatePicker.SelectedDate ?? _defaultCheckOutDate;
            var selectedCategory = CategoryComboBox.SelectedItem as Roomcategory;

            decimal? maxPrice = null;
            if (decimal.TryParse(MaxPriceTextBox.Text, out decimal price) && price > 0)
            {
                maxPrice = price;
            }

            _filteredRooms.Clear();

            foreach (var room in _context.Rooms.Local)
            {
                if (!IsRoomMatchFilters(room, selectedCategory, maxPrice, checkIn, checkOut))
                    continue;

                _filteredRooms.Add(room);
            }

            RoomsListView.ItemsSource = _filteredRooms;
            UpdateStatusText();
        }

        private bool ValidateDates()
        {
            if (CheckInDatePicker.SelectedDate == null || CheckOutDatePicker.SelectedDate == null)
                return false;

            if (CheckOutDatePicker.SelectedDate <= CheckInDatePicker.SelectedDate)
            {
                MessageBox.Show("Дата выезда должна быть позже даты заезда", "Ошибка дат",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsRoomMatchFilters(Room room, Roomcategory category, decimal? maxPrice, DateTime checkIn, DateTime checkOut)
        {
            // Фильтрация по категории
            if (category != null && room.CategoryId != category.CategoryId)
                return false;

            // Фильтрация по цене
            if (maxPrice.HasValue && room.Category.PricePerNight > maxPrice.Value)
                return false;

            // Проверка доступности номера
            return IsRoomAvailable(room, checkIn, checkOut);
        }

        private bool IsRoomAvailable(Room room, DateTime checkIn, DateTime checkOut)
        {
            foreach (var booking in room.Bookings)
            {
                var bookingCheckIn = booking.CheckInDate.ToDateTime(TimeOnly.MinValue);
                var bookingCheckOut = booking.CheckOutDate.ToDateTime(TimeOnly.MinValue);

                if (checkIn < bookingCheckOut && checkOut > bookingCheckIn)
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateStatusText()
        {
            int totalRooms = _context.Rooms.Local.Count;
            int availableRooms = _filteredRooms.Count;

            StatusTextBlock.Text = $"Найдено номеров: {availableRooms} из {totalRooms}";
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            var editForm = new RoomEditWindow(new Room { Status = "свободен" });

            if (editForm.ShowDialog() == true)
            {
                _context.SaveChanges();
                LoadRooms();
            }
        }

        private void EditRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is not Room selectedRoom)
            {
                MessageBox.Show("Выберите номер для редактирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editForm = new RoomEditWindow(selectedRoom);
            if (editForm.ShowDialog() == true)
            {
                _context.SaveChanges();
                LoadRooms();
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is not Room selectedRoom)
            {
                MessageBox.Show("Выберите номер для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить номер {selectedRoom.RoomNumber}?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _context.Rooms.Remove(selectedRoom);
                _context.SaveChanges();
                LoadRooms();
            }
        }

        private void BookRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is not Room selectedRoom)
            {
                MessageBox.Show("Выберите номер для бронирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var checkIn = CheckInDatePicker.SelectedDate ?? _defaultCheckInDate;
            var checkOut = CheckOutDatePicker.SelectedDate ?? _defaultCheckOutDate;

            var bookingForm = new BookingWindow(
                selectedRoom,
                DateOnly.FromDateTime(checkIn),
                DateOnly.FromDateTime(checkOut));

            if (bookingForm.ShowDialog() == true)
            {
                ApplyFilters();
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileViewWindow();
            profileWindow.Show();
            this.Close();
        }

        private void ManageBtn_Click(object sender, RoutedEventArgs e)
        {
            var manageWindow = new MenegeWindow();
            manageWindow.Show();
            this.Close();
        }

        private void SPABtn_Click(object sender, RoutedEventArgs e)
        {
            var spaOrderWimdow = new SpaOrderWindow();
            spaOrderWimdow.Show();
            this.Close();
        }




    }
}