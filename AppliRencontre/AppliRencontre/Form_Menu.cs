using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace AppliRencontre
{
    public partial class Form_Menu : Form
    {
        Profile user;
        Search research;
        public Profile User { get => user; set => user = value; }

        public Form_Menu()
        {
            InitializeComponent();
            P_Profile.Hide();
            P_Search.Hide();
            P_Chat.Hide();
        }

        #region MENU BUTTONS

        private void BT_Profile_Click(object sender, EventArgs e)
        {
            if(P_Profile.Visible == false)
            {
                P_Search.Hide();
                P_Chat.Hide();
                P_Profile.Show();

                //INIT LABELS
                L_Prenom.Text       = user.FirstName;
                L_Nom.Text          = user.LastName;
                L_Age.Text          = user.GetAge().ToString() + " ans";
                L_Sex.Text          = user.Sex;
                L_Dt_Validite.Text  = user.Expiration.ToLongDateString();
                PB_Profile.Image    = user.GetPicture();
                TB_Description.Text = user.Bio;

                //INIT TRACKBAR + LABEL -> height
                if (user.Height > 150 && user.Height < 210)
                {
                    TB_Height.Value = user.Height;
                    L_Taille.Text   = "Taille :  " + user.Height.ToString();
                }

                //INIT RADIO BUTTONS -> sex
                if      (user.Preference == "Homme") RB_Man.Checked   = true;
                else if (user.Preference == "Femme") RB_Woman.Checked = true;
                else                                   RB_Any.Checked   = true;

                //INIT LISTBOX -> animations
                LB_Animations.Items.Clear();
                foreach (string s in user.Animations)
                {
                    LB_Animations.Items.Add(s);
                }
            } 
        }

        private void BT_Search_Click(object sender, EventArgs e)
        {
            if (P_Search.Visible == false)
            {
                P_Profile.Hide();
                P_Chat.Hide();
                research = new Search(user);
                research.LoadSearchResults();

                P_Search.Show();

                BT_Match.Visible = false;
                BT_Skip.Text = "Commencer";
            }
        }

        private void BT_Chat_Click(object sender, EventArgs e)
        {
            if (P_Chat.Visible == false)
            {
                P_Profile.Hide();
                P_Search.Hide();
                P_Chat.Show();

                Chat chat = new Chat(user);

                LB_People.Items.Clear();
                foreach (Profile p in chat.Friends)
                {
                    LB_People.Items.Add(p);
                }
            }
        }

        private void BT_Disconnect_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confimer?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                user = null;
                Form login = new ConnectionLayout();
                Hide();
                login.ShowDialog();
                Close();
            }
            else
            {

            }
        }

        #endregion

        #region PROFILE

        private void TB_Height_ValueChanged(object sender, EventArgs e)
        {
            user.Height = TB_Height.Value;
            L_Taille.Text = "Taille :  ";
            L_Taille.Text = L_Taille.Text + user.Height;
        }

        private void OpenFD_FileOk(object sender, CancelEventArgs e)
        {
            user.PicturePath = openFD.FileName;
            user.Picture     = (Bitmap)Image.FromFile(user.PicturePath);
            PB_Profile.Image = user.GetPicture();
        }

        private void TB_Description_TextChanged(object sender, EventArgs e)
        {
            user.Bio = TB_Description.Text;
        }

        private void PB_Profile_Click(object sender, EventArgs e)
        {
            openFD.ShowDialog();
        }

        private void BT_Save_Click(object sender, EventArgs e)
        {
            user.Preference = 
                (RB_Woman.Checked || RB_Man.Checked) ?
                    GB_Interet.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked).Text :
                    "LesDeux";

            user.UpdateProfile();

            MessageBox.Show("Votre profil a été mis à jour",
                            "Sauvegarde Profil",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
        }

        #endregion

        #region SEARCH


        private void BT_Skip_Click(object sender, EventArgs e)
        {
            if (BT_Match.Visible) research.Iterate();
            BT_Match.Visible = true;
            BT_Skip.Text = "Suivant";


            L_PrenomSearch.Text         = research.GetProfile().FirstName;
            L_NomSearch.Text            = research.GetProfile().LastName;
            L_AgeSearch.Text            = research.GetProfile().GetAge().ToString() + " ans";
            L_TailleSearch.Text         = research.GetProfile().Height.ToString() + " cm";
            L_ValiditeSearch.Text       = research.GetProfile().Expiration.ToLongDateString();
            L_SexSearch.Text            = research.GetProfile().Sex;
            TB_DescriptionSearch.Text   = research.GetProfile().Bio;
            PB_PhotoSearch.Image        = research.GetProfile().GetPicture();

            LB_AnimationsSearch.Items.Clear();
            foreach (string anim in research.AnimationsInCommon())
            {
                LB_AnimationsSearch.Items.Add(anim);
            }
        }

        private void BT_Match_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vous avez aimé le profil de " + research.GetProfile().FirstName + " " + research.GetProfile().LastName,
                            "Information",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

            research.Like();
            if (research.Match())
            {
                DialogResult result = MessageBox.Show("Vous êtes tous les deux sur un coup! \nSouhaitez-vous lui parler?",
                                                     "Félicitation",
                                                     MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Information);
                if (result == DialogResult.No)
                { }
                else if (result == DialogResult.Yes)
                {
                    //OPEN CHAT

                }
            }
        }

        #endregion

        #region CHAT

        private void BT_Talk_Click(object sender, EventArgs e)
        {
            Profile p = (Profile)LB_People.SelectedItem;

            Chat chat = new Chat(user, p);

            if (!chat.Exists()) chat.CreateConversation();
            else
            {
                chat.SetNoConv();

                LB_Messages.Items.Clear();
                foreach (Message msg in chat.GetMessages())
                {
                    LB_Messages.Items.Add(msg);
                }
            }
        }

        private void BT_Send_Click(object sender, EventArgs e)
        {
            Profile receiver = (Profile)LB_People.SelectedItem;
            Chat chat = new Chat(user, receiver);

            chat.SetNoConv();

            Message msg = new Message(DateTime.Now, user, TB_Message.Text);

            chat.SendMessage(msg);

            LB_Messages.Items.Add(msg);

            TB_Message.Clear();
        }

        #endregion



        private void Form_Menu_Load(object sender, EventArgs e)
        {
            P_Profile.Location = P_Chat.Location;
            P_Search.Location = P_Chat.Location;
        }

        private void Form_Menu_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Confirmer?",
                                                    "Confirmation",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);
            e.Cancel = (result == DialogResult.No);
        }

    }
}
