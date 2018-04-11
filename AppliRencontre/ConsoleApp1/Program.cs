using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Connection = MySql.Data.MySqlClient.MySqlConnection;
using System.Data;
using System.Drawing;
using System.IO;
using Bogus;
using AppliRencontre;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "server=127.0.0.1;" +
                                        "uid=root;" +
                                        "pwd=;" +
                                        "database=act_vva;";


            MySqlConnection conn = new Connection(connectionString);
            MySqlCommand comm;

            Random p = new Random();


            for (int i  = 0; i<138; i++ )
            {
                conn.Open();

                

                string query2 = "UPDATE LOISANT " +
                    "SET TAILLE = '" + p.Next(150,210).ToString()+"' " +
                    "WHERE NOLOISANT = '"+i.ToString()+"'";

                comm = new MySqlCommand()
                {
                    Connection = conn,
                    CommandText = query2,
                };
                comm.ExecuteNonQuery();
                conn.Close();
                Console.WriteLine(query2);
            }
            Console.ReadKey();
        }


    }


}
