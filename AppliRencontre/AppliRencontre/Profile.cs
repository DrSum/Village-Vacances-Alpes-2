using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AppliRencontre
{
    public class Profile
    {
        private string          username     ;
        private string          password     ;
        private int             id           ; 
        private string          lastName     ;
        private string          firstName    ; 
        private string          sex          ;
        private string          bio          ; 
        private int             height       ;
        private DateTime        dob          ;
        private DateTime        expiration   ;
        private string          preference   ;
        private List<string>    animations   ;
        private Bitmap          picture      ;
        private string          picturePath  ;

        public Profile(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
        public Profile(int id, bool complete = true)
        {
            this.id = id;

            if (complete)
            {
                SetProfile();
                SetAnimations();
            }
            else
            {
                SetBasicProfile();
            }
            
        }

        public bool Exists()
        {
            string query = "SELECT p.USER, p.MDP " +
                            "FROM PROFIL p, LOISANT l " +
                            "WHERE p.USER = '" + username + "' " +
                            "AND p.MDP = '" + password + "'" +
                            "AND l.USER = p.USER";

            DataTable dt = Tools.ExecuteQuery(query);
            return (dt.Rows.Count == 1) ? true : false;
        }
        public void SetId()
        {
            string query = "SELECT NOLOISANT " +
                            "FROM LOISANT " +
                            "WHERE USER = '" + username + "'";

            DataTable dt = Tools.ExecuteQuery(query);

            id = Convert.ToInt32(dt.Rows[0]["NOLOISANT"]);
        }
        public void SetProfile()
        {
            string query = "SELECT USER, NOMLOISANT, PRENOMLOISANT, SEXE, DATENAISLOISANT, DATEFINSEJOUR, PHOTOLOISANT, TAILLE, DESCRIPTION, INTERET " +
                            "FROM LOISANT " +
                            "WHERE NOLOISANT = '" + id + "'";

            DataTable dt = Tools.ExecuteQuery(query);

            username    = dt.Rows[0]["USER"]            .ToString();
            firstName   = dt.Rows[0]["PRENOMLOISANT"]   .ToString();
            lastName    = dt.Rows[0]["NOMLOISANT"]      .ToString();
            sex         = dt.Rows[0]["SEXE"]            .ToString();
            dob         = Convert.ToDateTime(dt.Rows[0]["DATENAISLOISANT"]  );
            expiration  = Convert.ToDateTime(dt.Rows[0]["DATEFINSEJOUR"]    );

            if (dt.Rows[0]["DESCRIPTION"]  != DBNull.Value) bio         = dt.Rows[0]["DESCRIPTION"].ToString();
            if (dt.Rows[0]["INTERET"]      != DBNull.Value) preference  = dt.Rows[0]["INTERET"]    .ToString();
            if (dt.Rows[0]["TAILLE"]       != DBNull.Value) height      = Convert.ToInt32(dt.Rows[0]["TAILLE"]);
            if (dt.Rows[0]["PHOTOLOISANT"] != DBNull.Value)
            {
                MemoryStream ms = new MemoryStream();
                Byte[] binData = (byte[])(dt.Rows[0]["PHOTOLOISANT"]);
                ms.Write(binData, 0, binData.Length);
                picture = new Bitmap(ms);
            }
            else
            {
                picture = (Bitmap)Image.FromFile(@"C:\Users\Elliott\Documents\Visual Studio 2017\Projects\AppliRencontre\AppliRencontre\Pic\default_profile_pic.jpg");
            }
        }
        public void SetBasicProfile()
        {
            string query = "SELECT NOMLOISANT, PRENOMLOISANT, DATEFINSEJOUR " +
                            "FROM LOISANT " +
                            "WHERE NOLOISANT = '" + id + "'";

            DataTable dt = Tools.ExecuteQuery(query);

            firstName = dt.Rows[0]["PRENOMLOISANT"].ToString();
            lastName = dt.Rows[0]["NOMLOISANT"].ToString();
            expiration = Convert.ToDateTime(dt.Rows[0]["DATEFINSEJOUR"]);
        }
        public void SetAnimations()
        {
            string query = "SELECT DISTINCT a.NOMANIM " +
                            "FROM ANIMATION a, INSCRIPTION i, LOISANT l, ACTIVITE ac " +
                            "WHERE i.NOLOISANT = '" + id + "' " +
                            "AND i.CODEANIM = ac.CODEANIM " +
                            "AND ac.CODEANIM = a.CODEANIM;";

            DataTable dt = Tools.ExecuteQuery(query);

            animations = new List<string>();

            foreach (DataRow row in dt.Rows)
            {
                animations.Add(row["NOMANIM"].ToString());
            }
        }
        public int GetAge()
        {
            // Age théorique
            int age;
            age = DateTime.Now.Year - dob.Year;

            // Date de l'anniversaire de cette année
            DateTime birthDay = new DateTime(DateTime.Now.Year, dob.Month, dob.Day);

            // Si pas encore passé, retirer 1 an
            if (birthDay > DateTime.Now) age--;

            return age;
        }
        public Bitmap GetPicture()
        {
            PropertyItem pi = picture.PropertyItems.Select(x => x).FirstOrDefault(x => x.Id == 0x0112);

            if (pi == null)
            {
                return picture;
            }

            byte o = pi.Value[0];

            if (o == 2) picture.RotateFlip(RotateFlipType.RotateNoneFlipX);
            if (o == 3) picture.RotateFlip(RotateFlipType.RotateNoneFlipXY);
            if (o == 4) picture.RotateFlip(RotateFlipType.RotateNoneFlipY);
            if (o == 5) picture.RotateFlip(RotateFlipType.Rotate90FlipX);
            if (o == 6) picture.RotateFlip(RotateFlipType.Rotate90FlipNone);
            if (o == 7) picture.RotateFlip(RotateFlipType.Rotate90FlipY);
            if (o == 8) picture.RotateFlip(RotateFlipType.Rotate90FlipXY);

            return picture;
        }
        public void UpdateProfile()
        {
            string query;
            List<MySqlParameter> paramCollection = new List<MySqlParameter>();

            if (picturePath == null)
            {
                query = "UPDATE LOISANT " +
                            "SET INTERET = @interet, DESCRIPTION = @description , TAILLE = @taille " +
                            "WHERE NOLOISANT = @noLoisant";
            }
            else
            {
                FileStream fs;
                Byte[] binData;

                fs = new FileStream(picturePath, FileMode.Open, FileAccess.Read);
                binData = new byte[Convert.ToInt32(fs.Length) - 1];
                fs.Read(binData, 0, Convert.ToInt32(fs.Length) - 1);
                fs.Close();

                query = "UPDATE LOISANT " +
                            "SET PHOTOLOISANT = @img, INTERET = @interet, DESCRIPTION = @description , TAILLE = @taille " +
                            "WHERE NOLOISANT = @noLoisant";

                paramCollection.Add(new MySqlParameter("@img", binData));
            }

            paramCollection.Add(new MySqlParameter("@interet", preference));
            paramCollection.Add(new MySqlParameter("@description", bio));
            paramCollection.Add(new MySqlParameter("@noLoisant", id));
            paramCollection.Add(new MySqlParameter("@taille", height));

            Tools.ExecuteQuery(query, paramCollection);
        }

        public Bitmap           Picture         { get => GetPicture()   ; set => picture        = value; }
        public string           LastName        { get => lastName       ; set => lastName       = value; }
        public string           FirstName       { get => firstName      ; set => firstName      = value; }
        public string           Bio             { get => bio            ; set => bio            = value; }
        public int              Height          { get => height         ; set => height         = value; }
        public DateTime         Expiration      { get => expiration     ; set => expiration     = value; }
        public string           Preference      { get => preference     ; set => preference     = value; }
        public List<string>     Animations      { get => animations     ; set => animations     = value; }
        public string           Username        { get => username       ; set => username       = value; }
        public int              Id              { get => id             ; set => id             = value; }
        public string           PicturePath     { get => picturePath    ; set => picturePath    = value; }
        public string           Sex             { get => sex            ; set => sex            = value; }
        public DateTime         DoB             { get => dob            ; set => dob            = value; }


        public override string ToString()
        {
            return firstName + " " + lastName;
        }

    }
}
