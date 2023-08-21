namespace CityInfo.API.Services;

public class LocalMailService : IMailService
{
    private readonly string _mailFrom = string.Empty;
    private readonly string _mailTo = string.Empty;

    public LocalMailService(IConfiguration configuration)
    {
        _mailFrom = configuration["mailSettings:mailFrom"] ?? throw new InvalidOperationException();
        _mailTo = configuration["mailSettings:mailTo"] ?? throw new InvalidOperationException();
    }
    
    public void Send(string subject, string message)
    {
        // send mail - output to debug window
        Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with {nameof(LocalMailService)}.");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {message}");
    }
}