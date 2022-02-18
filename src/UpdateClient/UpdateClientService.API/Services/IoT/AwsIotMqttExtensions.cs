namespace UpdateClientService.API.Services.IoT;

public static class AwsIotMqttExtensions
{
	public static byte ToM2MqttQOS(this QualityOfServiceLevel qos)
	{
		if (qos == QualityOfServiceLevel.AtMostOnce)
		{
			return 0;
		}
		_ = 1;
		return 1;
	}
}
