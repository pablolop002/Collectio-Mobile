using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Resources.Culture;
using Collectio.Utils;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Xamarin.Essentials;

namespace Collectio.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public ICommand CommentsPageCommand { get; }

        public ICommand DeleteCacheCommand { get; }

        public ICommand DeleteDataCommand { get; }

        public ICommand MakeBackupCommand { get; }

        public ICommand RestoreBackupCommand { get; }

        //public ICommand SupportCommand { get; }

        public ICommand SyncCategoriesCommand { get; }

        public bool AppCenter
        {
            get => Preferences.Get("AppCenter", true);
            set
            {
                if (Preferences.Get("AppCenter", true) == value) return;
                Preferences.Set("AppCenter", value);
                OnPropertyChanged();
            }
        }

        public SettingsViewModel()
        {
            CommentsPageCommand = new Command(CommentsPage);
            DeleteCacheCommand = new Command(DeleteCache);
            DeleteDataCommand = new AsyncCommand(DeleteData);
            MakeBackupCommand = new Command(MakeBackup);
            RestoreBackupCommand = new Command(RestoreBackup);
            //SupportCommand = new AsyncCommand(Support);
            SyncCategoriesCommand = new AsyncCommand(SyncCategories);
        }

        private async Task SyncCategories()
        {
            await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.SyncCategories,
                await App.DataRepo.CreateOrUpdateCategories()
                    ? Strings.CategoriesSynced
                    : Strings.CategoriesNotSynced, Strings.Ok);
            AppCenterUtils.TrackAction("SyncCategories");
        }

        private void MakeBackup()
        {
            try
            {
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Backup,
                    App.DataRepo.CreateBackup() ? Strings.BackupCorrect : Strings.BackupError, Strings.Ok);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "CreateBackup");
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Backup, Strings.BackupError, Strings.Ok);
            }
        }

        private void RestoreBackup()
        {
            if (Preferences.Get("LoggedIn", false))
            {
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, Strings.DisabledFunction, Strings.Ok);
                return;
            }

            try
            {
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.RestoreBackup,
                    App.DataRepo.RestoreBackup() ? Strings.RestoreBackupCorrect : Strings.RestoreBackupError,
                    Strings.Ok);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "RestoreBackup");
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.RestoreBackup, Strings.RestoreBackupError, Strings.Ok);
            }
        }

        private async Task DeleteData()
        {
            if (!await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.SureQuestion, Strings.DeleteDataDescription,
                Strings.DeleteData, Strings.Cancel)) return;

            try
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.DeleteData,
                    App.DataRepo.DeleteAllData() ? Strings.DeleteDataCorrect : Strings.DeleteDataError,
                    Strings.Ok);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "DeleteData");
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.DeleteData, Strings.DeleteDataError, Strings.Ok);
            }
        }

        private void DeleteCache()
        {
            FileSystemUtils.ClearTempPath();

            if (!Preferences.Get("LoggedIn", false))
            {
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, Strings.DisabledFunction, Strings.Ok);
                return;
            }

            try
            {
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.DeleteCache,
                    FileSystemUtils.DeleteCache() ? Strings.DeleteCacheCorrect : Strings.DeleteCacheError,
                    Strings.Ok);
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "DeleteCache");
                Xamarin.Forms.Shell.Current.DisplayAlert(Strings.DeleteCache, Strings.DeleteCacheError, Strings.Ok);
            }
        }

        private void CommentsPage()
        {
            Xamarin.Forms.Shell.Current.GoToAsync("comments");
        }

        /*private async Task Support()
        {
            var billing = CrossInAppBilling.Current;
            try
            {
                var connected = await billing.ConnectAsync();
                if (!connected)
                    return;

                var productsInfo = (await billing.GetProductInfoAsync(ItemType.InAppPurchase)).ToList();
                var response = await Xamarin.Forms.Shell.Current.DisplayActionSheet(Strings.Support, Strings.Cancel,
                    null,
                    productsInfo.Select(productInfo => $"{productInfo.Name} - {productInfo.LocalizedPrice}").ToArray());

                if (response == null || response == Strings.Cancel) return;

                var product = productsInfo.First(p => p.LocalizedPrice == response);

                var purchase = await billing.PurchaseAsync(product.ProductId, ItemType.InAppPurchase);

                if (purchase != null && purchase.State == PurchaseState.Purchased)
                {
                    //purchased, we can now consume the item or do it later

                    //If we are on iOS we are done, else try to consume the purchase
                    if (DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.macOS)
                        return;

                    var consumedItem = await CrossInAppBilling.Current.ConsumePurchaseAsync(purchase.ProductId,
                        purchase.PurchaseToken);

                    if (consumedItem)
                    {
                        //Consumed!!
                        AppCenterUtils.TrackAction("Donation");
                    }
                }
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "Donations");
            }
            finally
            {
                await billing.DisconnectAsync();
            }
        }*/
    }
}