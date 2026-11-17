using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EmailStudio.Preview;

/// <summary>
/// Lightweight SMTP client for sending test emails to local catchers (e.g. Mailpit on localhost:1025).
/// </summary>
public static class SmtpSender
{
    public static async Task<(bool Success, string Message)> SendAsync(
        string host,
        int port,
        string toAddress,
        EmailDraft draft)
    {
        ArgumentNullException.ThrowIfNull(toAddress);
        ArgumentNullException.ThrowIfNull(draft);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port);

            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            // Read greeting
            string? response = await reader.ReadLineAsync();
            if (response == null || !response.StartsWith("220"))
            {
                return (false, $"SMTP connection failed: {response}");
            }

            // HELO
            await writer.WriteLineAsync("HELO localhost");
            response = await reader.ReadLineAsync();

            // MAIL FROM
            await writer.WriteLineAsync("MAIL FROM:<preview@emailstudio.dev>");
            response = await reader.ReadLineAsync();

            // RCPT TO
            await writer.WriteLineAsync($"RCPT TO:<{toAddress}>");
            response = await reader.ReadLineAsync();

            // DATA
            await writer.WriteLineAsync("DATA");
            response = await reader.ReadLineAsync();

            // Headers & Body (multipart/alternative)
            string boundary = "----=_Part_" + Guid.NewGuid().ToString("N");
            await writer.WriteLineAsync($"From: EmailStudio Preview <preview@emailstudio.dev>");
            await writer.WriteLineAsync($"To: <{toAddress}>");
            await writer.WriteLineAsync($"Subject: {draft.Subject}");
            await writer.WriteLineAsync("MIME-Version: 1.0");
            await writer.WriteLineAsync($"Content-Type: multipart/alternative; boundary=\"{boundary}\"");
            await writer.WriteLineAsync();

            // Plain text part
            await writer.WriteLineAsync($"--{boundary}");
            await writer.WriteLineAsync("Content-Type: text/plain; charset=UTF-8");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync(draft.TextBody);
            await writer.WriteLineAsync();

            // HTML part
            await writer.WriteLineAsync($"--{boundary}");
            await writer.WriteLineAsync("Content-Type: text/html; charset=UTF-8");
            await writer.WriteLineAsync();
            await writer.WriteLineAsync(draft.HtmlBody);
            await writer.WriteLineAsync();

            // End boundary
            await writer.WriteLineAsync($"--{boundary}--");
            await writer.WriteLineAsync(".");
            response = await reader.ReadLineAsync();

            // QUIT
            await writer.WriteLineAsync("QUIT");

            return (true, "Email delivered to local SMTP catcher successfully.");
        }
        catch (Exception ex)
        {
            return (false, $"Failed to send via SMTP ({host}:{port}): {ex.Message}");
        }
    }
}

