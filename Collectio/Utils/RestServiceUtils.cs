using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Collectio.Models;
using Newtonsoft.Json;

namespace Collectio.Utils
{
    public class RestServiceUtils
    {
        /// <summary>
        /// Propiedad privada con la URL
        /// </summary>
#if DEBUG
        private const string BaseUrl = "https://beta.collectioapp.com";
#else
        private const string BaseUrl = "HOSTURL";
#endif

        public static string RestUrl => $"{BaseUrl}/api/v1{{0}}";

        private readonly HttpClient _client;

        /// <summary>
        /// Constructor de la Clase
        /// </summary>
        public RestServiceUtils()
        {
            _client = new HttpClient
            {
                Timeout = new TimeSpan(0, 0, 30)
            };

            _client.DefaultRequestHeaders.Authorization = //new AuthenticationHeaderValue("Basic", "APP_AUTH_HEADER");
                new AuthenticationHeaderValue("Basic", "Q29sbGVjdGlvOnNTcUJyQHIzVndeSG5UbkRDdXQhTQ==");

            _client.DefaultRequestHeaders.Add("Platform", Xamarin.Essentials.DeviceInfo.Platform.ToString());
            _client.DefaultRequestHeaders.Add("Version", Xamarin.Essentials.AppInfo.VersionString);

            if (Xamarin.Essentials.Preferences.Get("LoggedIn", false))
            {
#pragma warning disable 4014
                InsertToken();
#pragma warning restore 4014
            }
        }

        /// <summary>
        /// Función para añadir el token de sesión
        /// </summary>
        public async Task InsertToken()
        {
            if (_client.DefaultRequestHeaders.Contains("Apikey"))
            {
                _client.DefaultRequestHeaders.Remove("Apikey");
            }

            _client.DefaultRequestHeaders.Add("Apikey", await Xamarin.Essentials.SecureStorage.GetAsync("Token"));
        }

        /// <summary>
        /// Función para añadir el idioma de la app
        /// </summary>
        private void SetLanguageHeader()
        {
            if (_client.DefaultRequestHeaders.Contains("lang"))
            {
                _client.DefaultRequestHeaders.Remove("lang");
            }

            _client.DefaultRequestHeaders.Add("lang", Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
        }

        /// <summary>
        /// Tarea asíncrona para peticiones GET
        /// </summary>
        /// <param name="urlArgs"></param>
        /// <returns></returns>
        public async Task<string> GetRequest(string urlArgs)
        {
            SetLanguageHeader();
            try
            {
                var uri = new Uri(string.Format(RestUrl, urlArgs));

                var response = await _client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = "ConnectionError"});
            }
            catch (HttpRequestException ex)
            {
                AppCenterUtils.ReportException(ex, "HttpRequestExceptionGet");
                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = ex.Message});
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "GeneralExceptionGet");
                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = ex.Message});
            }
        }

        /// <summary>
        /// Tarea para peticiones Post
        /// </summary>
        /// <param name="urlArgs"></param>
        /// <param name="form"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task<string> PostRequest(string urlArgs, Dictionary<string, string> form = null,
            Xamarin.Essentials.FileResult[] files = null)
        {
            SetLanguageHeader();
            MultipartFormDataContent content = null;
            try
            {
                var uri = new Uri(string.Format(RestUrl, urlArgs));
                content = await StructureContent(form, files);

                var response = await _client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = "ConnectionError"});
            }
            catch (HttpRequestException ex)
            {
                AppCenterUtils.ReportException(ex, "HttpRequestException");
                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = ex.Message});
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "GeneralException");
                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = ex.Message});
            }
            finally
            {
                content?.Dispose();
            }
        }

        /// <summary>
        /// Tarea para peticiones Put
        /// </summary>
        /// <param name="urlArgs"></param>
        /// <param name="form"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task<string> PutRequest(string urlArgs, Dictionary<string, string> form = null,
            Xamarin.Essentials.FileResult[] files = null)
        {
            SetLanguageHeader();
            MultipartFormDataContent content = null;
            try
            {
                var uri = new Uri(string.Format(RestUrl, urlArgs));
                content = await StructureContent(form, files);

                var response = await _client.PutAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = "ConnectionError"});
            }
            catch (HttpRequestException ex)
            {
                AppCenterUtils.ReportException(ex, "HttpRequestException");
                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = ex.Message});
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "GeneralException");
                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = ex.Message});
            }
            finally
            {
                content?.Dispose();
            }
        }

        /// <summary>
        /// Tarea asíncrona para peticiones Delete
        /// </summary>
        /// <param name="urlArgs"></param>
        /// <returns></returns>
        public async Task<string> DeleteRequest(string urlArgs)
        {
            SetLanguageHeader();
            try
            {
                var uri = new Uri(string.Format(RestUrl, urlArgs));

                var response = await _client.DeleteAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = "ConnectionError"});
            }
            catch (HttpRequestException ex)
            {
                AppCenterUtils.ReportException(ex, "HttpRequestExceptionGet");
                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = ex.Message});
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "GeneralExceptionGet");
                return JsonConvert.SerializeObject(new ResponseWs<int?> {Status = "ko", Message = ex.Message});
            }
        }

        /// <summary>
        /// Tarea para la obtención de imágenes
        /// </summary>
        /// <param name="urlArgs"></param>
        /// <returns></returns>
        public async Task<MemoryStream> GetImageRequest(string urlArgs)
        {
            SetLanguageHeader();
            try
            {
                var uri = new Uri(string.Format(RestUrl, urlArgs));

                var stream = new MemoryStream();
                var response = await _client.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    await response.Content.CopyToAsync(stream);
                    return stream;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "GetFile");
                return null;
            }
            finally
            {
                _client.CancelPendingRequests();
            }
        }

        /// <summary>
        /// Genera el data content de tipo multipart
        /// </summary>
        /// <param name="form"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        private async Task<MultipartFormDataContent> StructureContent(Dictionary<string, string> form,
            Xamarin.Essentials.FileResult[] files)
        {
            var content = new MultipartFormDataContent();

            if (files != null)
            {
                foreach (var file in files.Select((value, pos) => new {pos, value}))
                {
                    using var stream = await file.value.OpenReadAsync();
                    var fileBytes = new byte[stream.Length];
                    await stream.ReadAsync(fileBytes, 0, fileBytes.Length);
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.value.ContentType);
                    content.Add(fileContent, $"file{file.pos}", file.value.FileName);
                }

                content.Add(new StringContent(files.Length.ToString(), Encoding.UTF8), "FilesNumber");
            }

            if (form != null)
            {
                foreach (var element in form)
                {
                    content.Add(new StringContent(element.Value, Encoding.UTF8), element.Key);
                }
            }

            return content;
        }
    }
}