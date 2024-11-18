using ClientDotNet.MQTT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OwnMqtt.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OwnMqtt.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PingController : ControllerBase
	{
		private readonly MqttPublisher _mqttPublisher;
		public PingController(MqttPublisher mqttPublisher) {
			_mqttPublisher = mqttPublisher;
		}
		// GET: api/<PingController>
		[HttpGet]
		public async Task<string> Get()
		{
			await _mqttPublisher.ConnectAsync();
			await _mqttPublisher.PublishAsync("ptduc", JsonConvert.SerializeObject(new { name = "Duc"}));
			return "pong";
		}

	}
}
