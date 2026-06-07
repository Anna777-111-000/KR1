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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;

namespace KR07_06
{
    public partial class MainWindow : Window
    {
        string connectionString = @"Data Source=DESKTOP-F6R3UN4\SQLEXPRESS;Initial Catalog=06.06.2026_2;Integrated Security=True";

        ObservableCollection<PartnerCard> partners = new ObservableCollection<PartnerCard>();

        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadPartners();
        }

        private void LoadPartners()
        {
            try
            {
                string query = @"
            SELECT 
                p.PartnerID,
                p.PartnerName,
                p.Director,
                p.Phone,
                p.Reiting,
                pt.PartnerType AS PartnerTypeName,
                ISNULL(SUM(pp.CountPhoduct), 0) AS TotalSales
            FROM Partners p
            LEFT JOIN PartnerTypes pt ON p.PartnerTypeID = pt.PartnerTypeID
            LEFT JOIN PartnerProducts pp ON p.PartnerID = pp.PartnerID
            GROUP BY p.PartnerID, p.PartnerName, p.Director, p.Phone, p.Reiting, pt.PartnerType";

                DataTable dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    conn.Open();
                    adapter.Fill(dt);
                }

                partners.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    double sales = Convert.ToDouble(row["TotalSales"]);
                    int discount = 0;
                    if (sales > 300000) discount = 15;
                    else if (sales > 50000) discount = 10;
                    else if (sales > 10000) discount = 5;

                    partners.Add(new PartnerCard
                    {
                        PartnerID = Convert.ToInt32(row["PartnerID"]),
                        PartnerName = row["PartnerName"].ToString(),
                        Director = row["Director"].ToString(),
                        Phone = row["Phone"].ToString(),
                        Reiting = row["Reiting"].ToString(),
                        PartnerTypeName = row["PartnerTypeName"].ToString(),
                        Discount = $"{discount}%"
                    });
                }

                cardsControl.ItemsSource = partners;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow(connectionString, null);
            editWindow.ShowDialog();
            LoadPartners();
        }

        // КЛИК ПО КАРТОЧКЕ - РЕДАКТИРОВАНИЕ
        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            PartnerCard card = border.DataContext as PartnerCard;

            if (card != null)
            {
                var editWindow = new EditWindow(connectionString, card.PartnerID);
                editWindow.ShowDialog();
                LoadPartners();
            }
        }
    }

    public class PartnerCard
    {
        public int PartnerID { get; set; }
        public string PartnerName { get; set; }
        public string Director { get; set; }
        public string Phone { get; set; }
        public string Reiting { get; set; }
        public string PartnerTypeName { get; set; }
        public string Discount { get; set; }
    }
}