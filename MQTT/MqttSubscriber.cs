﻿using MQTTnet;
using MQTTnet.Client;
using Microsoft.Extensions.Options;
using ClientDotNet.Configurations;
using MQTTnet.Packets;
using OwnMqtt.Interfaces;

namespace ClientDotNet.MQTT
{
	/// <summary>
	/// Manages MQTT subscriptions and handles incoming messages from an MQTT broker.
	/// </summary>
	public class MqttSubscriber : IMqttSubscriber
	{
		private readonly IMqttClient _mqttClient;
		private readonly MqttClientSettings _settings;

		/// <summary>
		/// Initializes a new instance of the MqttSubscriber class.
		/// </summary>
		/// <param name="settings">Configuration settings for the MQTT client.</param>
		/// <param name="mqttClient">The MQTT client instance for subscribing to topics.</param>
		public MqttSubscriber(IOptions<MqttClientSettings> settings, IMqttClient mqttClient)
		{
			// Validate and assign the provided settings and MQTT client instance.
			_settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
			_mqttClient = mqttClient ?? throw new ArgumentNullException(nameof(mqttClient));

			// Initialize the MQTT client with the provided settings.
			InitializeMqttClient();
		}

		/// <summary>
		/// Configures and initializes the MQTT client based on the provided settings.
		/// </summary>
		private void InitializeMqttClient()
		{
			// Start building MQTT client options with basic connection details.
			var builder = new MqttClientOptionsBuilder()
				.WithClientId(_settings.ClientId)
				.WithTcpServer(_settings.Host, _settings.Port);

			// Enable TLS if configured in settings, specifying the SSL protocol version.
			if (_settings.UseTls)
			{
				builder.WithTlsOptions(options =>
				{
					// Configure TLS options based on your settings
					options.WithSslProtocols(System.Security.Authentication.SslProtocols.Tls12);
				});
			}

			// Include client credentials if specified in settings.
			if (!string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
			{
				builder.WithCredentials(_settings.Username, _settings.Password);
			}

			// Build the MQTT client options and apply them to the client.
			var mqttOptions = builder.Build();

			// Set up handlers for connection events.
			_mqttClient.ConnectedAsync += (async e =>
			{
				Console.WriteLine("Connected to MQTT Broker as subscriber.");
				// Handle post-connection actions here if necessary
			});

			_mqttClient.DisconnectedAsync += (async e =>
			{
				Console.WriteLine("Disconnected from MQTT Broker.");
				// Reconnect or cleanup logic can go here
			});

			// Register the handler for received application messages.
			_mqttClient.ApplicationMessageReceivedAsync += HandleReceivedMessage;
		}

		/// <summary>
		/// Handles incoming messages for subscribed topics.
		/// </summary>
		/// <param name="eventArgs">Event arguments containing the received message.</param>
		private Task HandleReceivedMessage(MqttApplicationMessageReceivedEventArgs eventArgs)
		{
			// Log or process the received message as needed.
			Console.WriteLine($"Message received on topic {eventArgs.ApplicationMessage.Topic}: {eventArgs.ApplicationMessage.ConvertPayloadToString()}");
			return Task.CompletedTask;
		}

		/// <summary>
		/// Connects to the MQTT broker asynchronously if not already connected.
		/// </summary>
		public async Task ConnectAsync()
		{
			if (!_mqttClient.IsConnected)
			{
				await _mqttClient.ConnectAsync(_mqttClient.Options);
			}
		}

		/// <summary>
		/// Disconnects from the MQTT broker asynchronously if connected.
		/// </summary>
		public async Task DisconnectAsync()
		{
			if (_mqttClient.IsConnected)
			{
				await _mqttClient.DisconnectAsync();
			}
		}

		/// <summary>
		/// Subscribes to a specified MQTT topic with a given QoS asynchronously.
		/// </summary>
		/// <param name="topic">The MQTT topic to subscribe to.</param>
		/// <param name="qos">The desired quality of service level for the subscription.</param>
		public async Task SubscribeAsync(string topic, int qos = 1)
		{
			// Ensure the client is connected before attempting to subscribe.
			if (!_mqttClient.IsConnected)
			{
				await ConnectAsync();
			}

			// Create a subscription option with the topic filter.
			var subscribeOptions = new MqttClientSubscribeOptions
			{
				TopicFilters = new List<MqttTopicFilter>
		{
			new MqttTopicFilter
			{
				Topic = topic,
				QualityOfServiceLevel = (MQTTnet.Protocol.MqttQualityOfServiceLevel)qos
			}
		}
			};

			// Subscribe to the topic using the subscription options.
			await _mqttClient.SubscribeAsync(subscribeOptions);
			Console.WriteLine($"Subscribed to {topic}");
		}
	}
}
