namespace CityInfo.API.Services;

public class CloudMailService : IMailService
{
    private readonly string _mailFrom = string.Empty;
    private readonly string _mailTo = string.Empty;

    public CloudMailService(IConfiguration configuration)
    {
        _mailFrom = configuration["mailSettings:mailFrom"] ?? throw new InvalidOperationException();
        _mailTo = configuration["mailSettings:mailTo"] ?? throw new InvalidOperationException();
    }
    
    public void Send(string subject, string message)
    {
        // send mail - output to debug window
        Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with {nameof(CloudMailService)}.");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {message}");
    }
}