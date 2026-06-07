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

using System.Data;
using System.Data.SqlClient;

namespace KR07_06
{
    public partial class EditWindow : Window
    {
        string connectionString;
        int? editId;

        public EditWindow(string conn, int? id)
        {
            InitializeComponent();
            connectionString = conn;
            editId = id;

            LoadPartnerTypes();

            if (id.HasValue)
            {
                Title = "Редактирование партнера";
                LoadPartner(id.Value);
            }
            else Title = "Добавление партнера";
        }

        void LoadPartnerTypes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection c = new SqlConnection(connectionString))
            using (SqlDataAdapter da = new SqlDataAdapter("SELECT PartnerTypeID, PartnerType FROM PartnerTypes", c))
                da.Fill(dt);
            cmbType.ItemsSource = dt.DefaultView;
        }

        void LoadPartner(int id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection c = new SqlConnection(connectionString))
            using (SqlDataAdapter da = new SqlDataAdapter($"SELECT * FROM Partners WHERE PartnerID={id}", c))
                da.Fill(dt);

            if (dt.Rows.Count == 0) return;

            DataRow r = dt.Rows[0];
            txtName.Text = r["PartnerName"].ToString();
            txtDirector.Text = r["Director"].ToString();
            txtPhone.Text = r["Phone"].ToString();
            txtEmail.Text = r["Email"].ToString();
            txtAddress.Text = r["Fddres"].ToString();
            txtRating.Text = r["Reiting"].ToString();

            int typeId = Convert.ToInt32(r["PartnerTypeID"]);
            foreach (DataRowView item in cmbType.Items)
            {
                if (Convert.ToInt32(item["PartnerTypeID"]) == typeId)
                {
                    cmbType.SelectedItem = item;
                    break;
                }
            }
        }

        void SaveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(txtRating.Text, out int rating) || rating < 0)
            {
                MessageBox.Show("Рейтинг - целое число от 0", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cmbType.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                int typeId = Convert.ToInt32(cmbType.SelectedValue);
                string name = txtName.Text.Replace("'", "''");
                string director = txtDirector.Text.Replace("'", "''");
                string phone = txtPhone.Text;
                string email = txtEmail.Text;
                string address = txtAddress.Text.Replace("'", "''");

                string sql = editId.HasValue ?
                    $@"UPDATE Partners SET PartnerName='{name}', Director='{director}', Phone='{phone}', Email='{email}', Fddres='{address}', Reiting={rating}, PartnerTypeID={typeId} WHERE PartnerID={editId}" :
                    $@"INSERT INTO Partners (PartnerName, Director, Phone, Email, Fddres, Reiting, PartnerTypeID) VALUES ('{name}', '{director}', '{phone}', '{email}', '{address}', {rating}, {typeId})";

                using (SqlConnection c = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(sql, c))
                {
                    c.Open();
                    cmd.ExecuteNonQuery();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void CancelClick(object sender, RoutedEventArgs e) => Close();
    }
}