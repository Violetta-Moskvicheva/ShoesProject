using Microsoft.EntityFrameworkCore;
using ShoesProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ShoesProject
{
    public partial class FormOrders : Form
    {
        public User CurrentUser { get; private set; }
        public bool IsGuest { get; private set; }

        public FormOrders(User user, bool guest)
        {
            InitializeComponent();
            CurrentUser = user;
            IsGuest = guest;

            // Настройка колонок DataGridView (используем одну большую текстовую колонку)
            var colOrderInfo = new DataGridViewTextBoxColumn
            {
                Name = "colOrderInfo",
                FillWeight = 85,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            colOrderInfo.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            var colStatus = new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                FillWeight = 15,
                Width = 120
            };
            colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvOrders.Columns.AddRange(new DataGridViewColumn[] { colOrderInfo, colStatus });

            lbUserName.Text = IsGuest ? "Гость" : CurrentUser.FullName;

            if (!IsGuest)
            {
                LoadOrders();
            }
            else
            {
                MessageBox.Show("Гости не могут просматривать заказы.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoadOrders()
        {
            try
            {
                using (var db = new ShopDbContext())
                {
                    // Загружаем заказы текущего пользователя со всеми связями
                    var orders = db.Orders
                        .Include(o => o.DeliveryPoint)
                        .Include(o => o.Status)
                        .Include(o => o.ProductsOrders)
                            .ThenInclude(po => po.Product)
                        .Where(o => o.IdUser == CurrentUser.Id) // Фильтр по текущему юзеру
                        .OrderByDescending(o => o.OrderDate)
                        .ToList();

                    dgvOrders.SuspendLayout();
                    dgvOrders.Rows.Clear();

                    foreach (var order in orders)
                    {
                        int rowIndex = dgvOrders.Rows.Add();
                        var row = dgvOrders.Rows[rowIndex];

                        row.Cells["colOrderInfo"].Value = FormatOrderInfo(order);
                        row.Cells["colStatus"].Value = order.Status.StatusName;

                        ApplyRowStyles(row, order);
                    }

                    dgvOrders.ResumeLayout();
                    dgvOrders.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заказов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string FormatOrderInfo(Order order)
        {
            var productsSummary = string.Join(Environment.NewLine, order.ProductsOrders.Select(po =>
                $"  - {po.Product.Art} (Кол-во: {po.Quantity} шт.)"));

            // Расчет общей стоимости заказа с учетом скидок
            decimal totalOrderPrice = order.ProductsOrders.Sum(po =>
                (po.Product.Price * (100 - po.Product.Discount) / 100) * po.Quantity);

            return $"Заказ № {order.Id} от {order.OrderDate:dd.MM.yyyy}" + Environment.NewLine +
                   $"Дата доставки: {order.DeliveryDate:dd.MM.yyyy}" + Environment.NewLine +
                   $"Пункт выдачи: {order.DeliveryPoint.DeliveryAddress}" + Environment.NewLine +
                   $"Код получения: {order.Code}" + Environment.NewLine +
                   $"Состав заказа:" + Environment.NewLine + productsSummary + Environment.NewLine +
                   $"Итого к оплате: {totalOrderPrice:C}";
        }

        private void ApplyRowStyles(DataGridViewRow row, Order order)
        {
            // Стилизация статуса (например, если статус "Завершен" или "Новый")
            if (order.IdStatuses == 2) // Предположим, id 2 - это завершен/готов
            {
                row.Cells["colStatus"].Style.ForeColor = Color.Green;
                row.Cells["colStatus"].Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            }
            else
            {
                row.Cells["colStatus"].Style.ForeColor = Color.OrangeRed;
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close(); // Возврат на форму товаров
        }
    }
}
