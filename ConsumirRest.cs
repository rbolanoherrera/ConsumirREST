public class ConsumirRest
    {
        private string TokenEndpoint;
        private string Header1 { get { return System.Configuration.ConfigurationManager.AppSettings["TokenHeader1"].ToString(); } }
        private string Header2 { get { return ConfigurationManager.AppSettings["TokenHeader2"].ToString(); } }

        public TokenService()
        {
            TokenEndpoint = string.Format(ConfigurationManager.AppSettings["Token_Url_Servicio"].ToString(), TokenTenant);
        }

        public TokenResponse Get(Persona person)
        {
            string res = "";
            string url = "https://url-del-servicio/persona/add";

            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(person));

                request.Headers = new WebHeaderCollection();
                request.Headers.Add("Accept-Encoding", "gzip,deflate,br");
                
                //------- Si el servicio necesita headers colocarlos aquí
                //request.Headers.Add("Ocp-Apim-Subscription-Key", SubcriptionKey);
                //request.Headers.Add(HttpRequestHeader.Authorization, token);
                
                //------- Fin headers del servicio

                request.ServicePoint.Expect100Continue = false;
                request.Method = "POST";
                request.ContentLength = data.Length;
                request.ContentType = "application/json";

                using (var postStream = request.GetRequestStream())
                {
                    postStream.Write(data, 0, data.Length);

                    using (var resp = request.GetResponse() as HttpWebResponse)
                    {
                        Stream responseStream = resp.GetResponseStream();
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

                        Stream outStream = new MemoryStream();

                        Byte[] buffer = new Byte[256];
                        int bytesRead;

                        Debug.WriteLine($"Obteniendo respusta de servicio :: {url}");

                        while ((bytesRead = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            outStream.Write(buffer, 0, bytesRead);

                            Debug.WriteLine(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                            res = res + Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        }

                        Debug.WriteLine($"Respuesta servicio: {res}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Se presento una excepción al consultar el servicio: error: {ex.Message}");
                res = ex.Message;
            }

            return res;
        }

        //Consumir un servicio de manera asincrona
        public async Task<TokenResponse> GetAsync()
        {
            string TokenEndpoint = "https://url-del-servicio/get/token"; 
            TokenResponse tokenResponse = null;
            TokenRequest request = new TokenRequest();

            request.GrantType = TokenGrantType;
            request.ClientId = TokenClientId;
            request.ClientSecret = TokenClientSecret;
            request.Resource = TokenResource;

            LoggingManager.LoggingManager.Publish($"Get: TokenEndpoint: " + TokenEndpoint, true);

            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate)
            };

            try
            {

                using (HttpClient clientWebApi = new HttpClient(handler))
                {
                    Dictionary<string, string> nameValueCollection = request;

                    HttpResponseMessage responseMessage = await clientWebApi.PostAsync(TokenEndpoint, new FormUrlEncodedContent(nameValueCollection));
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        string result = await responseMessage.Content.ReadAsStringAsync();
                        tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error en 'TokenService' GetToken: " + ex.Message);
            }

            return tokenResponse;
        }
        
    }
    
    
    public class TokenResponse
    {
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "expires_in")]
        public string ExpiresIn
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "ext_expires_in")]
        public string ExtExpiresIn
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "expires_on")]
        public string ExpiresOn
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "not_before")]
        public string NotBefore
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "resource")]
        public string Resource
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken
        {
            get;
            set;
        }
    }
    
