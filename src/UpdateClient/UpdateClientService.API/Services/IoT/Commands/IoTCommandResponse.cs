namespace UpdateClientService.API.Services.IoT.Commands;

public class IoTCommandResponse<T>
{
	public T Data { get; set; }

	public string ErrorMessage { get; set; }
}
