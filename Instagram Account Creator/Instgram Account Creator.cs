using System;
using System.Windows.Forms;

namespace Instagram_Account_Creator
{
    public partial class InstagramAccountCreator : Form
    {
        Instagram Instagram = new Instagram();
        public InstagramAccountCreator()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(await Instagram.CreateAccount(UsernameBox.Text, FirstNameBox.Text, PasswordBox.Text, EmailBox.Text));
        }
    }
}
