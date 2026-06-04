//using Microsoft.VisualBasic.ApplicationServices;
using ShoesProject.Models;
using System.Data;

namespace ShoesProject
{
    public partial class FormLogin : Form
    {
        public User CurrentUser { get; private set; }
        public bool isGuest {  get; private set; }
        public FormLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(txtLogin.Text) || String.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using(var db = new ShopDbContext())
            {
                var user = db.Users
                    .Where(w => w.Login == txtLogin.Text && w.Pass == txtPassword.Text)
                    .FirstOrDefault();

                if (user != null)
                {
                    CurrentUser = user;
                    isGuest = false;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                } else {
                    MessageBox.Show("Неверный логин и пароль", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnGuest_Click(object sender, EventArgs e)
        {
            CurrentUser = null;
            isGuest = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
