using Microsoft.EntityFrameworkCore;
using ShoesProject.Models;
using ShoesProject.Properties;

namespace ShoesProject
{
    public partial class FormProducts : Form
    {

        public User CurrentUser { get; private set; }
        public bool isGuest { get; private set; }

        public FormProducts(User user, bool guest)
        {
            InitializeComponent();

            var colPhoto = new DataGridViewImageColumn();
            colPhoto.Name = "colPhoto";
            colPhoto.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colPhoto.Width = 200;
            colPhoto.FillWeight = 30;

            var colInfo = new DataGridViewTextBoxColumn();
            colInfo.Name = "colInfo";
            colInfo.FillWeight = 60;
            colInfo.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            var colDiscount = new DataGridViewTextBoxColumn();
            colDiscount.Name = "colDiscount";
            colDiscount.FillWeight = 10;
            colDiscount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvProducts.Columns.AddRange([colPhoto, colInfo, colDiscount]);

            CurrentUser = user;
            isGuest = guest;
            lbUserName.Text = isGuest ? "Гость" : CurrentUser.FullName;


            LoadProducts(); //метод подгрузки информации товаров
        }

        // Загрузка списка товаров из базы данных и заполнение таблицы с применением стилей
        private void LoadProducts()
        {
            try
            {
                using (var db = new ShopDbContext())
                {
                    var products = db.Products
                        .Include(i => i.Category)
                        .Include(i => i.Manufacturer)
                        .Include(i => i.Supplier)
                        .Include(i => i.Measure)
                        .Include(i => i.ProductType)
                        .ToList();

                    dgvProducts.SuspendLayout();
                    dgvProducts.Rows.Clear();

                    foreach (var product in products)
                    {
                        int rowIndex = dgvProducts.Rows.Add();
                        var row = dgvProducts.Rows[rowIndex];

                        row.Cells["colPhoto"].Value = LoadProductImage(product.PhotoUrl);
                        row.Cells["colInfo"].Value = FormatProductInfo(product);
                        row.Cells["colDiscount"].Value = $"{product.Discount}%";
                        row.Cells["colDiscount"].Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                        ApplyRowStyles(row, product);
                    }

                    dgvProducts.ResumeLayout();
                    dgvProducts.AutoResizeRows(DataGridViewAutoSizeRowsMode.AllCells);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Стилизация строк таблицы в зависимости от скидки и остатка товара
        private void ApplyRowStyles(DataGridViewRow row, Product products)
        {
            if (products.Discount > 15)
            {
                row.DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#2E8B57");
                row.DefaultCellStyle.ForeColor = Color.White;
            }

            if (products.CointInStock <= 0)
            {
                row.DefaultCellStyle.BackColor = Color.LightBlue;
                if (products.Discount <= 15)
                {
                    row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }

            if (products.Discount > 0)
            {
                row.Cells["colDiscount"].Style.ForeColor = Color.Red;
                row.Cells["colDiscount"].Style.Font = new Font("Times New Roman", 12, FontStyle.Bold);
            }
        }

        // Форматирование информации о товаре для отображения (с расчетом скидки)
        private string FormatProductInfo(Product product)
        {
            string priceText;

            if (product.Discount > 0)
            {
                decimal finalPrice = product.Price * (100 - product.Discount) / 100;
                priceText = $"{product.Price:C} -> {finalPrice:C}";

            }
            else
            {
                priceText = $"{product.Price:C}";
            }
            return $"{product.Category.CategoryName} | {product.ProductType.ProdType} " + Environment.NewLine +
                $"Описание товара: {product.Description}" + Environment.NewLine +
                $"Производитель: {product.Manufacturer.ManufacturerName}" + Environment.NewLine +
                $"Поставщик: {product.Supplier.SupplierName}" + Environment.NewLine +
                $"Цена: {priceText}" + Environment.NewLine +
                $"Единица измерения: {product.Measure.MeasureName}" + Environment.NewLine +
                $"Количество на складе: {product.CointInStock}";
        }

        // Загрузка изображения товара по пути (или заглушки, если файл не найден)
        private Image LoadProductImage(string photoUrl)
        {
            if (!String.IsNullOrEmpty(photoUrl))//&& System.IO.File.Exists(photoUrl)
            {
                string resourceName = Path.GetFileNameWithoutExtension(photoUrl).Trim();

                var rm = Properties.Resources.ResourceManager;
                var img = rm.GetObject(resourceName) as Image;

                return img;
            }

            return Resources.picture;
        }

        // Кнопка выхода
        private void btnLogout_Click(object sender, EventArgs e)
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

        // Кнопка просмотра моих заказов
        private void btnViewOrders_Click(object sender, EventArgs e)
        { 
            if (isGuest || CurrentUser == null)
            {
                MessageBox.Show("Просмотр заказов недоступен для гостей. Пожалуйста, авторизуйтесь.",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (FormOrders formOrders = new FormOrders(CurrentUser, isGuest))
            {
                this.Hide();
                formOrders.ShowDialog();
                this.Show();
            }
        }
    }
}