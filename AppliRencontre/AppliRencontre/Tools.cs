using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using Connection = MySql.Data.MySqlClient.MySqlConnection;
using System.Data;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace AppliRencontre
{
    static class Tools
    {
        public static string GetConnectionString()
        {
            return "server=127.0.0.1;" +
                    "uid=root;" +
                    "pwd=;" +
                    "database=act_vva;";
        }

        public static DataTable ExecuteQuery(string query, List<MySqlParameter> parameterCollection = null)
        {
            string action = query.Split(' ')[0];

            DataTable dataTable = new DataTable();
            Connection conn = new Connection(GetConnectionString());

            try
            {
                conn.Open();
                MySqlCommand command = new MySqlCommand()
                {
                    CommandText = query,
                    Connection = conn,
                };

                if (parameterCollection != null)
                {
                    foreach (MySqlParameter parameter in parameterCollection)
                    {
                        command.Parameters.Add(parameter);
                    }
                }

                switch (action)
                {
                    case "SELECT":
                        MySqlDataReader reader = command.ExecuteReader();
                        dataTable.Load(reader);
                        break;

                    case "UPDATE":
                        command.ExecuteNonQuery();
                        break;

                    case "INSERT":
                        command.ExecuteNonQuery();
                        break;

                    case "DELETE":
                        break;
                }
                
            }
            catch(MySqlException x)
            {
                MessageBox.Show("Erreur: " + x.ToString());
            }
            finally
            {
                conn.Close();
            }
                    
            return dataTable;
        }

    }
}
