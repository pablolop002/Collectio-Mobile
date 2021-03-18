using System.Text;
using System.Windows.Input;
using Collectio.Resources.Culture;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Xamarin.Essentials;

namespace Collectio.ViewModels
{
    public class CommentsViewModel : BaseViewModel
    {
        private string _body;
        private int _commentTypeIndex;

        public string Body
        {
            get => _body;
            set => SetProperty(ref _body, value);
        }

        public int CommentTypeIndex
        {
            get => _commentTypeIndex;
            set => SetProperty(ref _commentTypeIndex, value);
        }

        public ObservableRangeCollection<string> CommentType { get; }

        public ICommand SendMessageCommand { get; }

        public CommentsViewModel()
        {
            SendMessageCommand = new Command(SendMessage);
            CommentType = new ObservableRangeCollection<string>()
            {
                Strings.CommentAddition,
                Strings.CommentBugReport
            };

            var builder = new StringBuilder();
            builder.AppendLine($"App Version: {AppInfo.VersionString} | Build: {AppInfo.BuildString}");
            builder.AppendLine($"OS: {DeviceInfo.Platform}");
            builder.AppendLine($"OS Version: {DeviceInfo.VersionString}");
            builder.AppendLine($"Manufacturer: {DeviceInfo.Manufacturer}");
            builder.AppendLine($"Device Model: {DeviceInfo.Model}");
            builder.AppendLine(string.Empty);
            builder.AppendLine(Strings.CommentExplain);
            Body = builder.ToString();
        }

        private void SendMessage()
        {
            if (CommentTypeIndex == -1 || string.IsNullOrWhiteSpace(Body))
            {
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, Strings.CommentNotComplete, Strings.Ok);
                return;
            }

            Email.ComposeAsync($"Collectio Support: {CommentType[CommentTypeIndex]}", Body, "support@collectioapp.com");
            Analytics.TrackEvent("SendComment");
        }
    }
}