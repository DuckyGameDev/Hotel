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
        private ObservableCollection<Room> _roomsCollection;
        public MainWindow()
        {
            InitializeComponent();
            _roomsCollection = new ObservableCollection<Room>();
            RoomsListView.ItemsSource = _roomsCollection;
            LoadRooms();
            if(Application.IsGuest)
            {
                ServiceBtn.Visibility = Visibility.Hidden;
                SpaServiseBtn.Visibility = Visibility.Hidden;
                AddRoomBtn.Visibility = Visibility.Hidden;
                EditRoomBtn.Visibility = Visibility.Hidden;
                DeleteRoomBtn.Visibility = Visibility.Hidden;
            }
        }

        private void LoadRooms()
        {
            _context.Rooms
           .Include(r => r.Category)
           .Load();

            // Очищаем и обновляем существующую коллекцию вместо создания новой
            _roomsCollection.Clear();
            foreach (var room in _context.Rooms.Local)
            {
                _roomsCollection.Add(room);
            }
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
                var bookingForm = new BookingWindow(selectedRoom);
                if (bookingForm.ShowDialog() == true)
                {
                    // Обновляем список номеров после бронирования
                    LoadRooms();
                }
            }
            else
            {
                MessageBox.Show("Выберите номер для бронирования", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ProfileViewWindow profileViewWindow = new ProfileViewWindow();
            profileViewWindow.Show();
            this.Close();
        }

        private void ServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var servicesWindow = new ServicesManagementWindow();
            servicesWindow.Show();
            this.Close();
        }

        private void SpaServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var spaserviceWindow = new SpaServicesManagementWindow();
            spaserviceWindow.Show();
            this.Close();
        }

        private void SpaServiceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            SpaOrderWindow orderWindow = new SpaOrderWindow();
            orderWindow.Show();
            this.Close();
        }
    }
}