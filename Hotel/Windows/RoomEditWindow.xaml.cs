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
using Microsoft.EntityFrameworkCore;

namespace Hotel.Windows
{
    /// <summary>
    /// Логика взаимодействия для RoomEditWindow.xaml
    /// </summary>
    public partial class RoomEditWindow : Window
    {
        public Room Room { get; set; }
        private readonly ApplicationDbContext _context = new ApplicationDbContext();
        public RoomEditWindow(Room room)
        {
            InitializeComponent();
            Room = room;
            DataContext = this;

            // Загружаем категории
            _context.Roomcategories.Load();
            CategoryComboBox.ItemsSource = _context.Roomcategories.Local.ToObservableCollection();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
