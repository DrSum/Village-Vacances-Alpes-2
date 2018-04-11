using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppliRencontre
{
    class Search
    {
        private List<Profile> searchResults;
        private Profile user;
        private static int i = 0;
           
        public Search(Profile user)
        {
            this.user = user;
        }

        public void LoadSearchResults()
        {
            //sexPreference -> the sex of the people being looked for
            string sexPreference = "";
            if (user.Preference == "Homme") sexPreference = "M";
            else if (user.Preference == "Femme") sexPreference = "F";

            //sexReciprocity -> person who's being looked at sexPreference
            string sexReciprocity = "";
            sexReciprocity = (user.Sex == "M") ? "Homme" : "Femme";


            string query = "SELECT NOLOISANT " +
                            "FROM LOISANT " +
                            "WHERE  DATENAISLOISANT < '" + (user.DoB.Year + 5).ToString() + "' " +
                            "AND    DATENAISLOISANT > '" + (user.DoB.Year - 5).ToString() + "' " +
                            "AND    NOLOISANT <>  '" + user.Id.ToString() + "' " +
                            "AND    (INTERET = 'LesDeux' OR INTERET = '" + sexReciprocity + "')";

            if (sexPreference == "F" || sexPreference == "M") query += "AND SEXE = '" + sexPreference + "'";

            DataTable dt = Tools.ExecuteQuery(query);

            searchResults = new List<Profile>();
            foreach (DataRow row in dt.Rows)
            {
                Profile result = new Profile(Convert.ToInt32(row["NOLOISANT"]));
                searchResults.Add(result);
            }
        }

        public List<string> AnimationsInCommon()
        {
            List<string> animationsInCommon = new List<string>();

            foreach(string anim in user.Animations)
            {
                foreach(string anim2 in searchResults[i].Animations)
                {
                    if (anim2 == anim)
                    {
                        animationsInCommon.Add(anim2);
                    }
                }
            }

            return animationsInCommon;
        }

        public void Like()
        {
            string query = "SELECT * " +
                            "FROM RELATION " +
                            "WHERE " +
                                "(LOISANT1 = '" + user.Id + "' " +
                                "OR LOISANT2 = '" + user.Id + "') " +
                            "AND " +
                                "(LOISANT2 = '" + GetProfile().Id + "' " +
                                "OR LOISANT1 = '" + GetProfile().Id + "') ";

            DataTable dt = Tools.ExecuteQuery(query);

            if (dt.Rows.Count == 0)
            {
                query = "INSERT INTO RELATION (LOISANT1, LOISANT2, L1AIME) " +
                        "VALUES (@l1, @l2, '1')";

                List<MySqlParameter> paramCollection = new List<MySqlParameter>
                {
                    new MySqlParameter("@l1", user.Id),
                    new MySqlParameter("@l2", GetProfile().Id),
                };
                Tools.ExecuteQuery(query, paramCollection);
            }
            else
            {
                string variable = (Convert.ToInt32(dt.Rows[0]["LOISANT1"]) == user.Id) ? "L1AIME" : "L2AIME";

                query = "UPDATE RELATION " +
                            "SET " + variable + " = '1' " +
                            "WHERE NORELATION = '" + Convert.ToInt32(dt.Rows[0]["NORELATION"]) + "' ";

                Tools.ExecuteQuery(query);
            }
        }

        public bool Match()
        {
            string query = "SELECT * " +
                            "FROM RELATION " +
                            "WHERE " +
                                "(LOISANT1 = '" + user.Id + "' " +
                                "OR LOISANT2 = '" + user.Id + "') " +
                            "AND " +
                                "(LOISANT2 = '" + GetProfile().Id + "' " +
                                "OR LOISANT1 = '" + GetProfile().Id + "') " +
                            "AND L1AIME = '1' " +
                            "AND L2AIME = '1' ";

            DataTable dt = Tools.ExecuteQuery(query);

            return (dt.Rows.Count > 0) ? true : false;
        }

        public int Iterate()
        {
            return (i == searchResults.Count - 1) ? i = 0 : ++i;
        }

        public Profile GetProfile()
        {
            return searchResults[i];
        }
    }
}
