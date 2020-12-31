using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Collectio.Models;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Xamarin.Essentials;

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
        private const string BaseUrl = "https://collectioapp.com";
#endif

        public static string RestUrl => $"{BaseUrl}/api/v1{{0}}";

        private readonly HttpClient _client;

        /// <summary>
        /// Constructor de la Clase
        /// </summary>
        public RestServiceUtils()
        {
            _client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 30)
            };

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", "APPAUTHHEADER");

            _client.DefaultRequestHeaders.Add("platform", DeviceInfo.Platform.ToString());
            _client.DefaultRequestHeaders.Add("version", AppInfo.VersionString);
        }

        /// <summary>
        /// Función para añadir el token de sesión
        /// </summary>
        /// <param name="token"></param>
        public void InsertToken(string token)
        {
            if (_client.DefaultRequestHeaders.Contains("token"))
            {
                _client.DefaultRequestHeaders.Remove("token");
            }

            _client.DefaultRequestHeaders.Add("token", token);
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

            _client.DefaultRequestHeaders.Add("lang",
                Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);
        }

        /// <summary>
        /// Tarea asíncrona para peticiones POST
        /// </summary>
        /// <param name="content"></param>
        /// <param name="urlArgs"></param>
        /// <returns></returns>
        public async Task<string> PostRequest(StringContent content, string urlArgs)
        {
            SetLanguageHeader();
            try
            {
                var uri = new Uri(string.Format(RestUrl, urlArgs));

                var response = await _client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = "ConnectionError"});
            }
            catch (HttpRequestException ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>() {{"Category", "HttpRequestException"}});
                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = ex.Message});
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>() {{"Category", "GeneralException"}});
                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = ex.Message});
            }
            finally
            {
                content.Dispose();
            }
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

                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = "ConnectionError"});
            }
            catch (HttpRequestException ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>() {{"Category", "HttpRequestExceptionGet"}});
                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = ex.Message});
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>() {{"Category", "GeneralExceptionGet"}});
                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = ex.Message});
            }
        }

        /*/// <summary>
        /// Petición para subida de archivos en formato multipart/form-data incluyendo el formulario en la petición.
        /// No es necesario incluir el numero de archivos como parte del formulario ya que se añade de forma automática
        /// como el campo "FilesNumber"
        /// </summary>
        /// <param name="form"></param>
        /// <param name="files"></param>
        /// <param name="urlArgs"></param>
        /// <returns></returns>
        public async Task<string> FileUploader(Dictionary<string, string> form,
            Data.Archive[] files, string urlArgs)
        {
            SetLanguageHeader();
            var content = new MultipartFormDataContent();
            try
            {
                var uri = new Uri(string.Format(RestUrl, urlArgs));

                foreach (var file in files.Select((value, pos) => new {pos, value}))
                {
                    try
                    {
                        ByteArrayContent fileContent;

                        if (file.value.ByteArray == null)
                        {
                            var stream = File.OpenRead($"{file.value.Dir}/{file.value.FileName}");
                            var fileBytes = new byte[stream.Length];
                            stream.Read(fileBytes, 0, fileBytes.Length);
                            stream.Close();
                            fileContent = new ByteArrayContent(fileBytes);
                        }
                        else
                        {
                            fileContent = new ByteArrayContent(file.value.ByteArray);
                        }

                        var mimeType = DependencyService.Get<ISaveDocs>().GetMimeType(file.value.FileName);
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                        content.Add(fileContent, $"file{file.pos}", file.value.FileName);
                    }
                    catch (Exception ex)
                    {
                        Crashes.TrackError(ex,
                            new Dictionary<string, string>() {{"Category", "FileAttachToMultipartError"}});
                    }
                }

                content.Add(new StringContent(files.Length.ToString(), Encoding.UTF8), "FilesNumber");

                foreach (var element in form)
                {
                    content.Add(new StringContent(element.Value, Encoding.UTF8), element.Key);
                }

                var response = await _client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = "ConnectionError"});
            }
            catch (HttpRequestException ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>() {{"Category", "HttpRequestException"}});
                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = ex.Message});
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>() {{"Category", "GeneralException"}});
                return JsonConvert.SerializeObject(new ResponseWs<int?>() {Status = "ko", Message = ex.Message});
            }
            finally
            {
                content.Dispose();
            }
        }*/

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
                    return null;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>()
                {
                    {"Category", "GetFile"}
                });
                return null;
            }
            finally
            {
                _client.CancelPendingRequests();
            }
        }
    }
}