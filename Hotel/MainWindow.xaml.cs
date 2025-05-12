using Hotel.Data;
using Hotel.Models.Entities;
using Hotel.Windows;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

namespace Hotel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        private ObservableCollection<Room> _filteredRooms = new ObservableCollection<Room>();
        //private ObservableCollection<Room> _roomsCollection;
        public MainWindow()
        {
            InitializeComponent();
            //_roomsCollection = new ObservableCollection<Room>();
            //RoomsListView.ItemsSource = _roomsCollection;
            LoadCategories();
            LoadRooms();
            CheckInDatePicker.SelectedDateChanged += (sender, e) =>
            {
                if (CheckInDatePicker.SelectedDate.HasValue)
                {
                    var newCheckIn = CheckInDatePicker.SelectedDate.Value;
                    //CheckOutDatePicker.SelectedDate = newCheckIn.AddDays(1);
                }
            };
            if (Application.IsGuest)
            {
                AddRoomBtn.Visibility = Visibility.Hidden;
                EditRoomBtn.Visibility = Visibility.Hidden;
                DeleteRoomBtn.Visibility = Visibility.Hidden;
            }
        }

        private void LoadCategories()
        {
            _context.Roomcategories.Load();
            CategoryComboBox.ItemsSource = _context.Roomcategories.Local.ToObservableCollection();
            //CategoryComboBox.Items.Insert(0, "Все");
            CategoryComboBox.SelectedIndex = 0;
        }

        private void LoadRooms()
        {
            _context.Rooms
             .Include(r => r.Category)
             .Include(r => r.Bookings)
             .Load();



            //// Очищаем и обновляем существующую коллекцию вместо создания новой
            //_roomsCollection.Clear();
            //foreach (var room in _context.Rooms.Local)
            //{
            //    _roomsCollection.Add(room);
            //}

            ApplyFilters();
        }


        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }


        private void ApplyFilters()
        {
            var checkIn = CheckInDatePicker.SelectedDate ?? DateTime.Today;
            var checkOut = CheckOutDatePicker.SelectedDate ?? checkIn.AddDays(1);

            // Проверка, что выезд минимум на 1 день позже заезда
            if (checkOut.Date <= checkIn.Date)
            {
                Debug.WriteLine(checkIn);
                Debug.WriteLine(checkOut);
                MessageBox.Show("Дата выезда должна быть позже даты заезда");
                return;
            }

            decimal? maxPrice = null;
            if (decimal.TryParse(MaxPriceTextBox.Text, out decimal price))
            {
                maxPrice = price;
            }

            var selectedCategory = CategoryComboBox.SelectedItem as Roomcategory;

            _filteredRooms.Clear();

            foreach (var room in _context.Rooms.Local)
            {
                // Фильтрация по категории
                if (selectedCategory != null && room.CategoryId != selectedCategory.CategoryId)
                    continue;

                // Фильтрация по цене
                if (maxPrice.HasValue && room.Category.PricePerNight > maxPrice.Value)
                    continue;

                // Фильтрация по датам
                bool isAvailable = true;
                foreach (var booking in room.Bookings)
                {
                    var bookingCheckIn = booking.CheckInDate.ToDateTime(TimeOnly.MinValue);
                    var bookingCheckOut = booking.CheckOutDate.ToDateTime(TimeOnly.MinValue);

                    if (!(checkIn >= bookingCheckOut || checkOut <= bookingCheckIn))
                    {
                        isAvailable = false;
                        break;
                    }
                }

                if (isAvailable)
                {
                    _filteredRooms.Add(room);
                }
            }

            RoomsListView.ItemsSource = _filteredRooms;
        }


        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            var editForm = new RoomEditWindow(new Room
            {
                Status = "свободен" // Устанавливаем статус по умолчанию
            });

            if (editForm.ShowDialog() == true)
            {
                LoadRooms(); // Обновляем список
            }
        }

        private void EditRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is Room selectedRoom)
            {
                var editForm = new RoomEditWindow(selectedRoom);
                if (editForm.ShowDialog() == true)
                {
                    _context.SaveChanges();
                    LoadRooms();
                }
            }
            else
            {
                MessageBox.Show("Выберите номер для редактирования");
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is Room selectedRoom)
            {
                if (MessageBox.Show("Удалить этот номер?", "Подтверждение",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _context.Rooms.Remove(selectedRoom);
                    _context.SaveChanges();
                    LoadRooms();
                }
            }
            else
            {
                MessageBox.Show("Выберите номер для удаления");
            }
        }

        private void BookRoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (RoomsListView.SelectedItem is Room selectedRoom)
            {
                var checkIn = CheckInDatePicker.SelectedDate ?? DateTime.Today;
                var checkOut = CheckOutDatePicker.SelectedDate ?? DateTime.Today.AddDays(1);

                var bookingForm = new BookingWindow(
                    selectedRoom,
                    DateOnly.FromDateTime(checkIn),
                    DateOnly.FromDateTime(checkOut));

                if (bookingForm.ShowDialog() == true)
                {
                    ApplyFilters();
                }
            }

            //if (RoomsListView.SelectedItem is Room selectedRoom)
            //{
            //    var bookingForm = new BookingWindow(selectedRoom);
            //    if (bookingForm.ShowDialog() == true)
            //    {
            //        // Обновляем список номеров после бронирования
            //        LoadRooms();
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Выберите номер для бронирования", "Ошибка",
            //        MessageBoxButton.OK, MessageBoxImage.Warning);
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProfileViewWindow profileViewWindow = new ProfileViewWindow();
            profileViewWindow.Show();
            this.Close();
        }


        private void MenageBtn_Click(object sender, RoutedEventArgs e)
        {
            MenegeWindow menegeWindow = new MenegeWindow();
            menegeWindow.Show();
            this.Close();
        }
    }
}