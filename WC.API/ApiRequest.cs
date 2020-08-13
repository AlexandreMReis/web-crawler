using WC.Logger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using WC.API.Enums;
using WC.API.HTTP;
using WC.API.Models;
using System.Web;
using System.IO.Compression;
using System.Text;

namespace WC.API
{
    public class ApiRequest
    {
        /// <summary>
        /// The time out in milliseconds
        /// </summary>
        private const int TimeOutInMS = 3000;

        /// <summary>
        /// Catches the web exception
        /// </summary>
        /// <typeparam name="T">The generic 'T' type</typeparam>
        /// <param name="ex">The exception</param>
        private static APIClientResult<T> CatchWebException<T>(WebException ex)
        {
            var result = new APIClientResult<T> { ExceptionStatus = ex.Status };

            if (ex.Response != null)
            {
                var exResponse = (HttpWebResponse)ex.Response;

                result.StatusCode = exResponse.StatusCode;

                if (exResponse.ContentLength > 0)
                {
                    using (Stream dataStream = exResponse.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        string errorDataString = reader.ReadToEnd();

                        try
                        {
                            result.ErrorData = JsonConvert.DeserializeObject<object>(errorDataString);
                        }
                        catch
                        {
                            result.ErrorData = errorDataString;
                        }
                    }
                }

                return result;
            }

            switch (ex.Status)
            {
                case WebExceptionStatus.Timeout:
                    result.StatusCode = HttpStatusCode.RequestTimeout;
                    break;

                case WebExceptionStatus.NameResolutionFailure:
                case WebExceptionStatus.ConnectFailure:
                case WebExceptionStatus.ReceiveFailure:
                case WebExceptionStatus.SendFailure:
                case WebExceptionStatus.PipelineFailure:
                case WebExceptionStatus.RequestCanceled:
                case WebExceptionStatus.ProtocolError:
                case WebExceptionStatus.ConnectionClosed:
                case WebExceptionStatus.TrustFailure:
                case WebExceptionStatus.SecureChannelFailure:
                case WebExceptionStatus.ServerProtocolViolation:
                case WebExceptionStatus.KeepAliveFailure:
                case WebExceptionStatus.Pending:
                case WebExceptionStatus.ProxyNameResolutionFailure:
                case WebExceptionStatus.UnknownError:
                case WebExceptionStatus.MessageLengthLimitExceeded:
                case WebExceptionStatus.CacheEntryNotFound:
                case WebExceptionStatus.RequestProhibitedByCachePolicy:
                case WebExceptionStatus.RequestProhibitedByProxy:
                    result.StatusCode = HttpStatusCode.InternalServerError;
                    break;

                default:
                    break;
            }

            return result;
        }

        public static string Decompress(Stream originalStream)
        {
            using (MemoryStream decompressedMemoryStream = new MemoryStream())
            {
                using (GZipStream decompressionStream = new GZipStream(originalStream, CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(decompressedMemoryStream);
                }
                StreamReader reader = new StreamReader(decompressedMemoryStream);
                return reader.CurrentEncoding.GetString(decompressedMemoryStream.ToArray()); ;
            }
        }

        /// <summary>
        /// Gets the API GET result synchronously
        /// </summary>
        /// <typeparam name="T">The generic type</typeparam>
        /// <param name="deviceInfo">(internal) The device info</param>
        /// <param name="methodName">(internal) The method name (requester)</param>
        /// <param name="metricDestination">(internal) The metric log destination</param>
        /// <param name="url">The url</param>
        /// <param name="headers">The headers</param>
        /// <param name="timeOutInMS">The time out in milliseconds</param>
        /// <returns>The API result including the HTTP status code</returns>
        public static APIClientResult<string> GetHTML(string url, Dictionary<string, string> headers = null, int timeOutInMS = TimeOutInMS)
        {
            APIClientResult<string> output = new APIClientResult<string> { };

            Stopwatch sw = Stopwatch.StartNew();

            try
            {
                var request = WebRequest.CreateHttp(url);
                request.Timeout = timeOutInMS;
                request.Method = APIRequestVerb.Get;
                request.ContentType = "application/json; utf-8";
                request.Accept = "application/json";

                if (headers != null && headers.Any())
                {
                    foreach (var header in headers)
                    {
                        switch (header.Key.ToLower())
                        {
                            case HttpCustomHeader.UserAgent:
                                request.UserAgent = header.Value;
                                break;

                            default:
                                if (request.Headers.AllKeys.Contains(header.Key))
                                    request.Headers.Remove(header.Key);

                                request.Headers.Set(header.Key, header.Value);
                                break;
                        }
                    }
                }

                var response = (HttpWebResponse)request.GetResponse();
                if (!response.ContentType.Contains("text/html"))
                {
                    return output;
                }

                using (Stream dataStream = response.GetResponseStream())
                {
                    output = new APIClientResult<string>
                    {
                        StatusCode = response.StatusCode,
                        Data = Decompress(dataStream)
                    };
                }

                response.Close();

                return output;
            }
            catch (WebException ex)
            {
                return output = CatchWebException<string>(ex);
            }
            catch (Exception ex)
            {
                LogEngine.CrawlerLogger.Error($"APIClient.Exception: {ex}");

                return output = new APIClientResult<string> { };
            }
            finally
            {
                sw.Stop();
                LogEngine.CrawlerLogger.Info($"Method:{APIRequestVerb.Get};URL:{url};ElapsedInMS:{sw.Elapsed.TotalMilliseconds};Success={output.Success}");
            }
        }
    }
}
