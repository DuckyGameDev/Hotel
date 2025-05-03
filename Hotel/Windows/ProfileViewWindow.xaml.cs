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
using Hotel.Models.Entities;

namespace Hotel.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProfileViewWindow.xaml
    /// </summary>
    public partial class ProfileViewWindow : Window
    {
        public ProfileViewWindow()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void ViewBookings_Click(object sender, RoutedEventArgs e)
        {
            var bookingWindow = new BookingViewWindow(2);
            bookingWindow.ShowDialog();
        }

        private void ViewProfile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ViewSpa_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            AuthorizationWindow authorizationWindow = new AuthorizationWindow();
            authorizationWindow.Show();
            this.Close();
        }
    }
}
