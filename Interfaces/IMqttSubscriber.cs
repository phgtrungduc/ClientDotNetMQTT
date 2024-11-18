namespace OwnMqtt.Interfaces
{
	public interface IMqttSubscriber
	{
		public Task SubscribeAsync(string topic, int qos = 1);
		public Task DisconnectAsync();
		public Task ConnectAsync();
	}
}
