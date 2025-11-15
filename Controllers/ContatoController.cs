using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace Emaileira.Controllers
{
    [ApiController]
    [Route("contato")]
    public class ContatoController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ContatoController> _logger;

        public ContatoController(IConfiguration config, ILogger<ContatoController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpPost("psa")]
        public IActionResult Enviar([FromBody] ContatoModel request)
        {
            try
            {
                var smtpHost = _config["Email:Host"];
                var smtpPort = int.Parse(_config["Email:Port"]!);
                var smtpUser = _config["Email:User"];
                var smtpPass = _config["Email:Password"];

                var mail = new MailMessage();
                mail.From = new MailAddress(smtpUser!);
                mail.To.Add(smtpUser!); // enviar para mim
                mail.Subject = $"[CONTATO PSA] {request.Nome}";
                mail.Body =
                    $@"Nome: {request.Nome}
                    Email: {request.Email}
                    Telefone: {request.Telefone}

                    Mensagem:
                    {request.Mensagem}";
                mail.IsBodyHtml = false;

                var smtp = new SmtpClient(smtpHost, smtpPort);
                smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                smtp.EnableSsl = true;

                smtp.Send(mail);

                return Ok(new { message = "Contato enviado com sucesso." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar contato");
                return StatusCode(500, new { error = "Falha ao enviar contato", details = ex.Message });
            }
        }
    }
}
