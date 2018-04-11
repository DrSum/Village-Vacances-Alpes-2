using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Connection = MySql.Data.MySqlClient.MySqlConnection;
using MySql.Data.MySqlClient;

namespace AppliRencontre
{
    public partial class ConnectionLayout : Form
    {
        public ConnectionLayout()
        {
            InitializeComponent();
        }

        private void BT_Connection_Click(object sender, EventArgs e)
        {
            string username = TB_User_Name.Text;
            string password = TB_Password.Text;

            Profile test = new Profile(username, password);

            bool exists = test.Exists();
            if (exists)
            {
                test.SetId();
                Form mainForm = new Form_Menu()
                {
                    User = new Profile(test.Id)
                };

                Hide();
                mainForm.ShowDialog();
                Close();
            }
            else
            {
                MessageBox.Show("Nom d'utilisateur ou mot de passe est erroné.",
                    "Erreur",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            TB_Password.Text = "";
        }

    }
}
