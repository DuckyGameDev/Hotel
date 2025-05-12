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
    public partial class BookingManagementWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<Booking> _bookings;
        private ObservableCollection<Guest> _guests;
        private ObservableCollection<Room> _rooms;
        private Booking _currentBooking;

        public BookingManagementWindow()
        {
            InitializeComponent();
            _bookings = new ObservableCollection<Booking>();
            _guests = new ObservableCollection<Guest>();
            _rooms = new ObservableCollection<Room>();

            BookingsDataGrid.ItemsSource = _bookings;
            LoadData();
            SetFormState(false);
            CheckInDatePicker.SelectedDate = DateTime.Today;
            CheckOutDatePicker.SelectedDate = DateTime.Today.AddDays(1);
        }

        private async void LoadData()
        {
            await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .ThenInclude(r => r.Category)
                .LoadAsync();

            await _context.Guests.LoadAsync();
            await _context.Rooms.Include(r => r.Category).LoadAsync();

            _bookings.Clear();
            _guests.Clear();
            _rooms.Clear();

            foreach (var booking in _context.Bookings.Local)
                _bookings.Add(booking);

            foreach (var guest in _context.Guests.Local)
                _guests.Add(guest);

            foreach (var room in _context.Rooms.Local)
                _rooms.Add(room);

            GuestComboBox.ItemsSource = _guests;
            RoomComboBox.ItemsSource = _rooms;
        }

        private void SetFormState(bool isEditing)
        {
            AddButton.IsEnabled = !isEditing;
            SaveButton.IsEnabled = isEditing;
            CancelButton.IsEnabled = isEditing;
            BookingsDataGrid.IsEnabled = !isEditing;

            if (!isEditing)
            {
                ClearForm();
            }
        }

        private void ClearForm()
        {
            GuestComboBox.SelectedItem = null;
            RoomComboBox.SelectedItem = null;
            CheckInDatePicker.SelectedDate = DateTime.Today;
            CheckOutDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            _currentBooking = null;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            _currentBooking = new Booking
            {
                CheckInDate = DateOnly.FromDateTime(DateTime.Today),
                CheckOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };
            FillFormFromBooking();
            SetFormState(true);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (GuestComboBox.SelectedItem == null ||
                RoomComboBox.SelectedItem == null ||
                CheckInDatePicker.SelectedDate == null ||
                CheckOutDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentBooking == null) return;

            _currentBooking.Guest = (Guest)GuestComboBox.SelectedItem;
            _currentBooking.Room = (Room)RoomComboBox.SelectedItem;
            _currentBooking.CheckInDate = DateOnly.FromDateTime(CheckInDatePicker.SelectedDate.Value);
            _currentBooking.CheckOutDate = DateOnly.FromDateTime(CheckOutDatePicker.SelectedDate.Value);

            try
            {
                if (_currentBooking.BookingId == 0)
                {
                    _context.Bookings.Add(_currentBooking);
                }

                _context.SaveChanges();
                LoadData();
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

        private void BookingsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking selectedBooking)
            {
                _currentBooking = selectedBooking;
                FillFormFromBooking();
                SetFormState(true);
            }
        }

        private void FillFormFromBooking()
        {
            if (_currentBooking == null) return;

            GuestComboBox.SelectedItem = _currentBooking.Guest;
            RoomComboBox.SelectedItem = _currentBooking.Room;
            CheckInDatePicker.SelectedDate = new DateTime(
                _currentBooking.CheckInDate.Year,
                _currentBooking.CheckInDate.Month,
                _currentBooking.CheckInDate.Day);
            CheckOutDatePicker.SelectedDate = new DateTime(
                _currentBooking.CheckOutDate.Year,
                _currentBooking.CheckOutDate.Month,
                _currentBooking.CheckOutDate.Day);
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            BookingsDataGrid.ItemsSource = _context.Bookings.Local
                .Where(b => b.Guest.FullName.ToLower().Contains(searchText) ||
                             b.Room.RoomNumber.Contains(searchText) ||
                             b.Room.Category.CategoryName.ToLower().Contains(searchText) ||
                             b.CheckInDate.ToString().Contains(searchText) ||
                             b.CheckOutDate.ToString().Contains(searchText))
                .ToList();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItem is Booking selectedBooking)
            {
                if (MessageBox.Show($"Удалить бронирование {selectedBooking.BookingId}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Bookings.Remove(selectedBooking);
                        _context.SaveChanges();
                        LoadData();
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
                MessageBox.Show("Выберите бронирование для удаления", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}