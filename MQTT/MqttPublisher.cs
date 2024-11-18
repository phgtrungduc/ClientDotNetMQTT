using MQTTnet;
using MQTTnet.Client;
using ClientDotNet.Configurations;
using Microsoft.Extensions.Options;

namespace ClientDotNet.MQTT
{
    /// <summary>
    /// Handles MQTT message publishing to an MQTT broker using MQTTnet.
    /// </summary>
    public class MqttPublisher
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientSettings _settings;
        private MqttClientOptions _mqttOptions;

        /// <summary>
        /// Initializes a new instance of the MqttPublisher class.
        /// </summary>
        /// <param name="settings">Configuration settings for the MQTT client.</param>
        /// <param name="mqttClient">The MQTT client instance for publishing messages.</param>
        public MqttPublisher(IOptions<MqttClientSettings> settings, IMqttClient mqttClient)
        {
            // Ensure settings and mqttClient are provided.
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));

            // Initialize the MQTT client with provided settings.
            InitializeMqttClient();
        }

        /// <summary>
        /// Configures and initializes the MQTT client based on provided settings.
        /// </summary>
        private void InitializeMqttClient()
        {
            // Start building MQTT client options with server and client details.
            var builder = new MqttClientOptionsBuilder()
                .WithClientId(_settings.ClientId)
                .WithTcpServer(_settings.Host, _settings.Port);

            // If TLS is enabled, configure TLS settings.
            if (_settings.UseTls)
            {
                builder.WithTlsOptions(options =>
                {
                    // Configure TLS options here based on _settings
                    // TLS 1.2 is currently a widely accepted version; We must adjust as necessary.
                    options.WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12);
                });
            }

            // If credentials are provided, include them in the connection options.
            if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                builder.WithCredentials(_settings.Username, _settings.Password);
            }

            // Build and apply the MQTT client options.
            var mqttOptions = builder.Build();
            _mqttOptions = mqttOptions;

			// Setup connection handlers for logging and potential reconnection logic.
			_mqttClient.ConnectedAsync += (async e =>
            {
                Console.WriteLine("Connected to MQTT Broker.");
                // Handle post-connection actions here if necessary
            });

            _mqttClient.DisconnectedAsync += (async e =>
            {
                Console.WriteLine("Disconnected from MQTT Broker.");
                // Reconnect or cleanup logic can go here
            });
        }

        /// <summary>
        /// Connects to the MQTT broker asynchronously.
        /// </summary>
        public async Task ConnectAsync()
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(_mqttOptions);
            }
        }

        /// <summary>
        /// Disconnects from the MQTT broker asynchronously.
        /// </summary>
        public async Task DisconnectAsync()
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }

        /// <summary>
        /// Publishes a message to a specified topic asynchronously.
        /// </summary>
        /// <param name="topic">The topic to which the message will be published.</param>
        /// <param name="payload">The message payload.</param>
        /// <param name="retainFlag">Indicates whether the message should be retained.</param>
        /// <param name="qos">The quality of service level for the message.</param>
        public async Task PublishAsync(string topic, string payload, bool retainFlag = false, int qos = 1)
        {
            // Create and configure the message to publish.
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos)
                .WithRetainFlag(retainFlag)
                .Build();

            // Publish the message asynchronously.
            await _mqttClient.PublishAsync(message);
        }
    }
}

