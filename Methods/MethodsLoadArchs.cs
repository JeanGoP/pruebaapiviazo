using APISietemasdereservas.Controllers;
using Microsoft.AspNetCore.Http; 
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic; 
using System.Net.Http; 
using System.Threading.Tasks;

namespace APISietemasdereservas.MethodsLoadArchs
{
    public static class MethodsLoadArchs
    {
        public static async Task<Dictionary<string, object>> upload_files_to_multiplatam(IFormFile[] files, string folder)
        {
            Dictionary<string, object> respuesta = new Dictionary<string, object>();
            List<string> urls = new List<string>();

            try
            {
                if (files == null || files.Length == 0)
                {
                    respuesta.Add("Error", true);
                    respuesta.Add("Mensaje", "No se recibieron archivos.");
                    return respuesta;
                }

                if (string.IsNullOrEmpty(folder))
                {
                    respuesta.Add("Error", true);
                    respuesta.Add("Mensaje", "El nombre de la carpeta es obligatorio.");
                    return respuesta;
                }

                string url = "https://www.sintesiserp.com/ApisS3Amazon/api/s3/upload";

                using (var client = new HttpClient())
                {
                    foreach (var file in files)
                    {
                        using (var form = new MultipartFormDataContent())
                        {
                            var stream = file.OpenReadStream();
                            var streamContent = new StreamContent(stream);
                            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                            //string uniqueFileName = MethodsCompile.EncryptFileName(file.FileName);
                            form.Add(streamContent, "archivo", file.FileName);
                            form.Add(new StringContent(folder), "Folder");

                            var response = client.PostAsync(url, form).Result;

                            if (response.IsSuccessStatusCode)
                            {
                                string content = response.Content.ReadAsStringAsync().Result;
                                var json = JObject.Parse(content);

                                if (json["Url"] != null)
                                {
                                    urls.Add(json["Url"].ToString());
                                }
                                else
                                {
                                    respuesta.Add("Error", true);
                                    respuesta.Add("Mensaje", "No se encontró la URL en la respuesta para el archivo: " + file.FileName);
                                    return respuesta;
                                }
                            }
                            else
                            {
                                respuesta.Add("Error", true);
                                respuesta.Add("Mensaje", "Error al subir el archivo: " + file.FileName);
                                return respuesta;
                            }
                        }
                    }
                }

                // Todo fue exitoso
                respuesta.Add("Error", false);
                respuesta.Add("Urls", urls);
            }
            catch (Exception ex)
            {
                respuesta.Add("Error", true);
                respuesta.Add("Mensaje", ex.Message);
            }

            return respuesta;
        }


        public static object EliminarArchivoEnS3(string key)
        {
            var respuesta = new Dictionary<string, object>();

            try
            {
                if (string.IsNullOrEmpty(key))
                    throw new Exception("El parámetro 'key' es obligatorio.");

                string url = $"https://www.sintesiserp.com/ApisS3Amazon/api/s3/delete?key={Uri.EscapeDataString(key)}";

                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);

                    HttpResponseMessage response = client.SendAsync(request).Result;

                    string content = response.Content.ReadAsStringAsync().Result;

                    if (response.IsSuccessStatusCode)
                    {
                        respuesta.Add("Error", false);
                        respuesta.Add("Mensaje", "Archivo eliminado correctamente");
                        respuesta.Add("RespuestaAPI", content);
                    }
                    else
                    {
                        respuesta.Add("Error", true);
                        respuesta.Add("Mensaje", "Error al eliminar el archivo");
                        respuesta.Add("Detalle", content);
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta.Add("Error", true);
                respuesta.Add("Mensaje", ex.Message);
            }

            return respuesta;
        }


    }
}
