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
    /// Логика взаимодействия для MenegeWindow.xaml
    /// </summary>
    public partial class MenegeWindow : Window
    {
        public MenegeWindow()
        {
            InitializeComponent();
        }

        private void RoomCategoryManagementWindow_Click(object sender, RoutedEventArgs e)
        {
            RoomCategoryManagementWindow categoryManagementWindow = new RoomCategoryManagementWindow();
            categoryManagementWindow.Show();
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

        private void EmployeeView_Click(object sender, RoutedEventArgs e)
        {
            var employeesWindow = new EmployeeManagementWindow();
            employeesWindow.Show();
            this.Close();
        }

        private void BookingMenegement_Click(object sender, RoutedEventArgs e)
        {
            BookingManagementWindow bookingManagementWindow = new BookingManagementWindow();
            bookingManagementWindow.Show();
            this.Close();
        }

        private void GuestManagementWindow_Click(object sender, RoutedEventArgs e)
        {
            GuestManagementWindow guestManagementWindow = new GuestManagementWindow();
            guestManagementWindow.Show();
            this.Close();
        }

        private void ServiceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            ServiceOrderManagementWindow serviceOrderManagementWindow = new ServiceOrderManagementWindow();
            serviceOrderManagementWindow.Show();
            this.Close();
        }

        private void SpaServiceOrderManagementWindow_Click(object sender, RoutedEventArgs e)
        {
            SpaServiceOrderManagementWindow service = new SpaServiceOrderManagementWindow();
            service.Show();
            this.Close();
        }
    }
}
