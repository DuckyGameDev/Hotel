using Hotel.Models.Entities;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Hotel.Windows
{
    public partial class RescheduleSpaOrderWindow : Window
    {
        public Spaserviceorder Order { get; }
        public DateOnly NewDate { get; set; }
        public TimeOnly NewTime { get; set; }

        public Dictionary<TimeOnly, string> AvailableTimes { get; } = new Dictionary<TimeOnly, string>
        {
            { new TimeOnly(9, 0), "09:00" },
            { new TimeOnly(11, 0), "11:00" },
            { new TimeOnly(13, 0), "13:00" },
            { new TimeOnly(15, 0), "15:00" },
            { new TimeOnly(17, 0), "17:00" }
        };

        public RescheduleSpaOrderWindow(Spaserviceorder order)
        {
            InitializeComponent();
            Order = order;
            NewDate = order.ServiceDate;
            NewTime = order.ServiceTime;
            DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewDate < DateOnly.FromDateTime(DateTime.Today))
            {
                MessageBox.Show("Нельзя выбрать прошедшую дату");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}