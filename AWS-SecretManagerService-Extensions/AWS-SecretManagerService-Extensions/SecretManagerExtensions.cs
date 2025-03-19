namespace SecretManagerExtensions
{
    public static class SecretManagerExtensions
    {
        public static void AddAWSSecretsManager(this IConfigurationBuilder configurationBuilder,
                       string region,
                       string secretName)
        {
            var configurationSource = new AwsSecretsManagerConfigurationSource(region, secretName);
            configurationBuilder.Add(configurationSource);
        }
    }

    public class AwsSecretsManagerConfigurationSource : IConfigurationSource
    {
        private readonly string _region;
        private readonly string _secretName;

        public AwsSecretsManagerConfigurationSource(string region, string secretName)
        {
            _region = region;
            _secretName = secretName;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new AwsSecretsManagerConfigurationProvider(_region, _secretName);
        }
    }

    public class AwsSecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly string _region;
        private readonly string _secretName;

        public AwsSecretsManagerConfigurationProvider(string region, string secretName)
        {
            _region = region;
            _secretName = secretName;
        }


        public override void Load()
        {
            try
            {
                var secret = GetSecret();
                if (string.IsNullOrWhiteSpace(secret))
                {
                    throw new Exception("Secret retrieved is null or empty.");
                }

                var settings = JsonSerializer.Deserialize<SettingsRoot>(secret);

                if (settings == null)
                {
                    throw new Exception("Deserialized settings object is null.");
                }

                Data = settings.ToDictionary();
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Deserialization error: {jsonEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading AWS secret: {ex.Message}");
                throw;
            }
        }

        private string GetSecret()
        {
            var request = new GetSecretValueRequest
            {
                SecretId = _secretName,
                VersionStage = "AWSCURRENT" 
            };
            using (var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region)))
            {
                var response = client.GetSecretValueAsync(request).Result;

                if (response.SecretString != null)
                {
                    return response.SecretString;
                }
                else
                {
                    using var memoryStream = response.SecretBinary;
                    using var reader = new StreamReader(memoryStream);
                    return Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
                }
            }
        }
    }

    public class SettingsRoot
    {
        public SecretAppSettings SecretAppSettings { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var secretDictionary = new Dictionary<string, string>();

            if (SecretAppSettings?.ConnectionStrings != null)
            {
                foreach (var conn in SecretAppSettings.ConnectionStrings)
                {
                    secretDictionary[$"ConnectionStrings:{conn.Key}"] = conn.Value;
                }
            }

            if (SecretAppSettings?.Endpoints != null)
            {
                foreach (var endpoint in SecretAppSettings.Endpoints)
                {
                    secretDictionary[$"Endpoints:{endpoint.Key}"] = endpoint.Value;
                }
            }           

            if (SecretAppSettings?.App != null)
            {
                foreach (var app in SecretAppSettings.App)
                {
                    secretDictionary[$"App:{app.Key}"] = app.Value;
                }
            }

            if (SecretAppSettings?.AWS != null)
            {
                foreach (var timeout in SecretAppSettings.AWS)
                {
                    secretDictionary[$"AWS:{timeout.Key}"] = timeout.Value;
                }
            }

            if (SecretAppSettings?.EmailConfig != null)
            {
                foreach (var timeout in SecretAppSettings.EmailConfig)
                {
                    secretDictionary[$"EmailConfig:{timeout.Key}"] = timeout.Value;
                }
            }

            return secretDictionary;
        }
    }
    public class SecretAppSettings
    {
        public Dictionary<string, string> ConnectionStrings { get; set; }
        public Dictionary<string, string> Endpoints { get; set; } 
        public Dictionary<string, string> App { get; set; }
        public Dictionary<string, string> AWS { get; set; }
        public Dictionary<string, string> EmailConfig { get; set; }
    }

}
