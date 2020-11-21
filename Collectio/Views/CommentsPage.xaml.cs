using System;
using Collectio.Resources.Culture;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CommentsPage : ContentPage
    {
        public CommentsPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            
            CommentType.Items.Add("Strings.Addition");
            CommentType.Items.Add("Strings.BugReport");
        }

        private void Send_OnClicked(object sender, EventArgs e)
        {
            //ToDo
        }
    }
}