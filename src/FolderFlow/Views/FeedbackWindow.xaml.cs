using System.Net;
using System.Net.Mail;
using System.Windows;

namespace FolderFlow.Views;

public partial class FeedbackWindow : Window
{
    private const string ToAddress = "adeepanethwedage10171@gmail.com";
    private const string SmtpUser = "adeepanethwedage10171@gmail.com";
    private const string SmtpAppPassword = "dhxv mmkf opml gkwv"; // Replace with Gmail App Password

    public FeedbackWindow()
    {
        InitializeComponent();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void OnSendClicked(object sender, RoutedEventArgs e)
    {
        var from = FromTextBox.Text.Trim();
        var subject = SubjectTextBox.Text.Trim();
        var body = BodyTextBox.Text.Trim();

        if (string.IsNullOrEmpty(from) || !from.Contains('@'))
        {
            MessageBox.Show("Please enter a valid sender email address.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(subject))
        {
            MessageBox.Show("Please enter a subject.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrEmpty(body))
        {
            MessageBox.Show("Please enter a message.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SendButton.IsEnabled = false;
        SendButton.Content = "Sending…";

        try
        {
            await Task.Run(() =>
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(SmtpUser, $"OrganizeME Feedback"),
                    Subject = subject,
                    Body = $"From: {from}\n\n{body}",
                    IsBodyHtml = false
                };
                mail.To.Add(ToAddress);
                mail.ReplyToList.Add(new MailAddress(from));

                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(SmtpUser, SmtpAppPassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                smtp.Send(mail);
            });

            MessageBox.Show("Feedback sent successfully! Thank you.", "Sent", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to send feedback:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            SendButton.IsEnabled = true;
            SendButton.Content = "Send Feedback";
        }
    }
}
