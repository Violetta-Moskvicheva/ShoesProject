using Microsoft.EntityFrameworkCore;
using ShoesProject.Models;
using System.Data;

namespace ShoesProject
{
    public partial class FormOrders : Form
    {
        public User? CurrentUser { get; private set; }
        public bool IsGuest { get; private set; }

        // Конструктор формы: настройка колонок таблицы заказов,
        // отображение имени пользователя и загрузка данных (только для авторизованных)
        public FormOrders(User? user, bool guest)
        {
            InitializeComponent();
            CurrentUser = user;
            IsGuest = guest;

            
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

            dgvOrders.Columns.AddRange([colOrderInfo, colStatus]);

            lbUserName.Text = IsGuest ? "Гость" : CurrentUser.FullName;

            if (!IsGuest && CurrentUser != null)
            {
                LoadOrders();
            }
            else
            {
                MessageBox.Show("Гости не могут просматривать заказы.", "Информация", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Загрузка истории заказов текущего пользователя из базы данных
        // заполнение таблицы с применением стилей
        private void LoadOrders()
        {
            try
            {
                using (var db = new ShopDbContext())
                {
                    var orders = db.Orders
                        .Include(o => o.DeliveryPoint)
                        .Include(o => o.Status)
                        .Include(o => o.ProductsOrders)
                        .ThenInclude(po => po.Product)
                        .Where(o => o.IdUser == CurrentUser.Id)
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
                MessageBox.Show($"{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Форматирование информации о заказе
        private string FormatOrderInfo(Order order)
        {
            var productsDesc = string.Join(Environment.NewLine, order.ProductsOrders.Select(po =>
                $" - {po.Product.Art} (Кол-во: {po.Quantity} шт.)"));

            // Расчет общей стоимости заказа с учетом скидок
            decimal totalOrderPrice = order.ProductsOrders.Sum(po =>
                (po.Product.Price * (100 - po.Product.Discount) / 100) * po.Quantity);

            return $"Заказ № {order.Id}" + Environment.NewLine +
                   $"Дата доставки: {order.DeliveryDate:dd.MM.yyyy}" + Environment.NewLine +
                   $"Пункт выдачи: {order.DeliveryPoint.DeliveryAddress}" + Environment.NewLine +
                   $"Код получения: {order.Code}" + Environment.NewLine +
                   $"Состав заказа:" + Environment.NewLine + productsDesc + Environment.NewLine +
                   $"Итого к оплате: {totalOrderPrice:C}" + Environment.NewLine +
                   $"Дата заказа: {order.OrderDate:dd.MM.yyyy}" ;
        }

        //Стилизация ячейки статуса заказа в таблице
        private void ApplyRowStyles(DataGridViewRow row, Order order)
        {
            if (order.IdStatuses == 1) // id 1 - "Завершен"
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
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // Обработка закрытия формы
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //завершает работу формы и освобождает ресурсы
            base.OnFormClosing(e);
        }
    }
}
