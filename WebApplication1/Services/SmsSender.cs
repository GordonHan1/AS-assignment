using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }

    public class SmsSender : ISmsSender
    {
        // Implement using an SMS provider like Twilio.
        public Task SendSmsAsync(string number, string message)
        {
            // TODO: Replace with real SMS sending code.
            Console.WriteLine($"Sending SMS to {number}: {message}");
            return Task.CompletedTask;
        }
    }
}
