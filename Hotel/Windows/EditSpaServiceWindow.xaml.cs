using Hotel.Models.Entities;
using System.Windows;

namespace Hotel.Windows
{
    public partial class EditSpaServiceWindow : Window
    {
        public Spaservice Service { get; private set; }
        public string Title { get; private set; }

        public EditSpaServiceWindow()
        {
            InitializeComponent();
            Service = new Spaservice();
            Title = "Добавление SPA-услуги";
            DataContext = this;
        }

        public EditSpaServiceWindow(Spaservice service)
        {
            InitializeComponent();
            Service = service;
            Title = "Редактирование SPA-услуги";
            DataContext = this;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Service.ServiceName))
            {
                MessageBox.Show("Введите название услуги", "Ошибка");
                return;
            }

            if (Service.Price <= 0)
            {
                MessageBox.Show("Цена должна быть положительным числом", "Ошибка");
                return;
            }

            if (Service.Duration <= 0)
            {
                MessageBox.Show("Длительность должна быть положительным числом", "Ошибка");
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