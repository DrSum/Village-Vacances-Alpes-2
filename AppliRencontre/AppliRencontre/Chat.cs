using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace AppliRencontre
{
    class Chat
    {
        private int convNo;
        private List<Profile> friends = new List<Profile>();
        private Tuple<Profile, Profile> users;
        private List<Message> messages = new List<Message>();

        public List<Profile> Friends { get => friends; set => friends = value; }

        public Chat(Profile user1)
        {
            users = Tuple.Create(user1, (Profile)null);

            SetFriends();
        }
        public Chat(Profile user1, Profile user2)
        {
            users = Tuple.Create(user1, user2);
        }


        public void SetFriends()
        {
            string query = "SELECT DISTINCT NOLOISANT " +
                                "FROM RELATION r, LOISANT l " +
                                "WHERE L1AIME = '1' " +
                                "AND L2AIME = '1' " +
                                "AND " +
                                    "(LOISANT1 = '" + users.Item1.Id + "' " +
                                    "OR LOISANT2 = '" + users.Item1.Id + "') " +
                                "AND " +
                                    "(LOISANT1 = NOLOISANT " +
                                    "OR LOISANT2 = NOLOISANT) " +
                                "AND l.USER <> '" + users.Item1.Username + "' ";

            DataTable dt = Tools.ExecuteQuery(query);

            friends.Clear();
            List<Profile> canTalk = new List<Profile>();
            foreach (DataRow row in dt.Rows)
            {
                friends.Add(new Profile(Convert.ToInt32(row["NOLOISANT"])));
            }
        }

        public void SetNoConv()
        {
            string query = "SELECT NOCONVERSATION " +
                            "FROM CONVERSATION " +
                            "WHERE " +
                                "(LOISANT1 = '" + users.Item1.Id + "' " +
                                "AND LOISANT2 = '" + users.Item2.Id + "') " +
                            "OR " +
                                "(LOISANT1 = '" + users.Item2.Id + "' " +
                                "AND LOISANT2 = '" + users.Item1.Id + "') ";

            DataTable dt = Tools.ExecuteQuery(query);
            convNo = Convert.ToInt32(dt.Rows[0]["NOCONVERSATION"]);
        }

        public bool Exists()
        {
            string query = "SELECT * " +
                            "FROM CONVERSATION " +
                            "WHERE " +
                                "(LOISANT1 = '" + users.Item1.Id + "' " +
                                "AND LOISANT2 = '" + users.Item2.Id + "') " +
                            "OR " +
                                "(LOISANT1 = '" + users.Item2.Id + "' " +
                                "AND LOISANT2 = '" + users.Item1.Id + "') ";

            DataTable dt = Tools.ExecuteQuery(query);

            return (dt.Rows.Count == 1) ? true : false;
        }

        public void CreateConversation()
        {
            string query = "INSERT INTO CONVERSATION (LOISANT1, LOISANT2) " +
                            "VALUE (@l1, @l2)";
            List<MySqlParameter> parameters = new List<MySqlParameter>
                {
                    new MySqlParameter("@l1", users.Item1.Id),
                    new MySqlParameter("@l2", users.Item2.Id),
                };

            Tools.ExecuteQuery(query, parameters);
        }

        public List<Message> GetMessages()
        {
            string query = "SELECT DATETIME, PRENOMLOISANT, m.NOLOISANT, CONTENU " +
                            "FROM MESSAGE m, LOISANT l " +
                            "WHERE NOCONVERSATION = '" + convNo + "' " +
                            "AND m.NOLOISANT = l.NOLOISANT " +
                            "ORDER BY DATETIME ASC";

            DataTable dt = Tools.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                messages.Add(new Message(Convert.ToDateTime(row["DATETIME"]), 
                                         new Profile(Convert.ToInt32(row["NOLOISANT"]), false),
                                         row["CONTENU"].ToString()));
            }

            return messages;
        }

        public void SendMessage(Message msg)
        {
            string query = "INSERT INTO MESSAGE (NOLOISANT, CONTENU, DATETIME, NOCONVERSATION) " +
                            "VALUES (@sender, @content, @datetime, @noConv)";

            List<MySqlParameter> parameters = new List<MySqlParameter>
            {
                new MySqlParameter("@sender", users.Item1.Id),
                new MySqlParameter("@content", msg.TextSent),
                new MySqlParameter("@datetime", msg.TimeSent),
                new MySqlParameter("@noConv", convNo),
            };

            Tools.ExecuteQuery(query, parameters);

            messages.Add(msg);
        }
    }
}
