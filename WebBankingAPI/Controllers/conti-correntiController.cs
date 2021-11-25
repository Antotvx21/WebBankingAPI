using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebBankingAPI.Model;
using WebBankingAPI.Models;
namespace WebBankingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class conti_correntiController : ControllerBase
    {
        [Authorize]
        [HttpGet("/conti-correnti")]
        public ActionResult Conti()
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(f => f.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);

                if (soggetto.IsBanker)
                {
                    var ProBankAccount = model.BankAccounts.ToList();
                    if (ProBankAccount == null)
                        return NotFound("Conti bancarii non trovato");
                    else if (ProBankAccount.Count == 0)
                        return NotFound("Non ci sono conti bancari");
                    else return Ok(ProBankAccount);
                }
                else
                {
                    var UserBankAccount = model.BankAccounts.Where(q => q.FkUser == soggetto.Id).Select(a => new { a.Id, a.Iban }).ToList();
                    if (UserBankAccount == null)
                        return NotFound("Conto bancario non trovato ");
                    else
                        return Ok(UserBankAccount);
                }
            }
        }
        [Authorize]
        [HttpGet("/conti-correnti/{id}")]
        public ActionResult Conto(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);

                if (soggetto.IsBanker)
                {
                    var scelto = model.BankAccounts.Where(q => q.Id == id).ToList();
                    if (scelto.Count == 0 || scelto == null)
                        return NotFound("Conto bancario non trovato");
                    else
                        return Ok(scelto);
                }
                else
                {
                    var scelto = model.BankAccounts.Where(q => q.Id == id && q.FkUser == soggetto.Id).Select(a => new { a.Iban, a.Id }).ToList();
                    if (scelto.Count == 0 || scelto == null)
                        return NotFound("Conto bancario non trovato");
                    else
                        return Ok(scelto);
                }
            }
        }
        [Authorize]
        [HttpGet("/{id}/movimenti")]
        public ActionResult Movimenti(int id)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);

                if (soggetto.IsBanker)
                {
                    var selezionato = model.AccountMovements.Where(q => q.FkBankAccount == id).Select(q => new { q.Id, q.Date, q.FkBankAccount, q.In, q.Out, q.Description }).OrderBy(d => d.Date).ToList();
                    if (selezionato.Count == 0 || selezionato == null)
                        return NotFound("Movimento non trovato");
                    else
                        return Ok(selezionato);
                }
                else
                {
                    var selezionato = model.AccountMovements.Where(q => q.FkBankAccount == id && q.FkBankAccountNavigation.FkUser == soggetto.Id).Select(q => new { q.Id, q.Date, q.FkBankAccount, q.In, q.Out, q.Description }).OrderBy(d => d.Date).ToList();
                    if (selezionato.Count == 0 || selezionato == null)
                        return NotFound("Movimento non trovato");
                    else
                        return Ok(selezionato);
                }

            }
        }
        [Authorize]
        [HttpGet("/{id}/movimenti/{idmovimento}")]
        public ActionResult Movimento(int id, int idmovimento)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);

                if (soggetto.IsBanker)
                {
                    var selezionato = model.AccountMovements.Where(q => q.FkBankAccount == id).Select(q => new { q.Id, q.Date, q.FkBankAccount, q.In, q.Out, q.Description }).OrderBy(d => d.Date).ToList();
                    if (selezionato.Count == 0 || selezionato == null)
                        return NotFound("Movimento non trovato");
                    else
                        return Ok(selezionato);
                }
                else
                {
                    var selezionato = model.AccountMovements.Where(o => o.FkBankAccount == id && o.FkBankAccountNavigation.FkUser == soggetto.Id && o.Id == idmovimento).Select(o => new { o.Id, o.Date, o.FkBankAccount, o.In, o.Out, o.Description }).ToList();
                    if (selezionato.Count == 0 || selezionato == null)
                        return NotFound("Movimento non trovato");
                    else
                        return Ok(selezionato);
                }
            }
        }
        public double Saldo(BankAccount utente)
        {
            double saldo = 0;

            using (WebBankingContext model = new WebBankingContext())
            {
                double saldoin = utente.AccountMovements.Sum(i => i.In).Value;
                double saldoout = utente.AccountMovements.Sum(o => o.Out).Value;
                saldo = saldoin + saldoout;
            }

            return saldo;
        }
        [Authorize]
        [HttpPost("/conti-correnti/{id}/bonifico")]
        public ActionResult Bonifico(int id, [FromBody] Bonifico bonifico)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);


                if (soggetto.IsBanker)
                {
                    var mittente = model.BankAccounts.Include(o => o.AccountMovements).Where(O => O.Id == id).FirstOrDefault();
                    var destinatario = bonifico.Iban;

                    if (mittente == null)
                        return NotFound("Conto corrente mittente non trovato");
                    else
                    {

                        if (destinatario == mittente.Iban)
                        {
                            model.AccountMovements.Add(new AccountMovement { Date = DateTime.Now, In = bonifico.Importo, Out = null, FkBankAccount = mittente.Id });
                            model.SaveChanges();
                            return Ok(bonifico);
                        }
                        else
                        {
                            double Saldoa = Saldo(mittente);
                            if (Saldoa < bonifico.Importo)
                                return Problem("Non hai abbasanza soldi per completare il bonifico!");

                            var contoDestinatario = model.BankAccounts.Where(o => o.Iban == destinatario).FirstOrDefault();

                            if (contoDestinatario != null)
                            {
                                model.AccountMovements.Add(new AccountMovement
                                {
                                    Date = DateTime.Now,
                                    In = null,
                                    Out = bonifico.Importo,
                                    FkBankAccount = mittente.Id
                                });

                                model.AccountMovements.Add(new AccountMovement
                                {
                                    Date = DateTime.Now,
                                    In = bonifico.Importo,
                                    Out = null,
                                    FkBankAccount = contoDestinatario.Id
                                });

                                model.SaveChanges();

                                return Ok(bonifico);
                            }
                            else
                            {
                                model.AccountMovements.Add(new AccountMovement
                                {
                                    Date = DateTime.Now,
                                    In = null,
                                    Out = bonifico.Importo,
                                    FkBankAccount = mittente.Id
                                });
                                model.SaveChanges();

                                return Ok(bonifico);
                            }
                        }
                    }
                }
                else if (!soggetto.IsBanker)
                {
                    var UserBank = model.BankAccounts.Include(o => o.AccountMovements).Where(O => O.Id == id && O.FkUser == soggetto.Id).FirstOrDefault();
                    var destinatario = (bonifico.Iban);
                    if (UserBank == null)
                        return NotFound("Conto corrente mittente non trovato");
                    else
                    {
                        if (destinatario == UserBank.Iban)
                        {
                            model.AccountMovements.Add(new AccountMovement { Date = DateTime.Now, In = bonifico.Importo, Out = null, FkBankAccount = UserBank.Id });
                            model.SaveChanges();
                            return Ok(bonifico);
                        }
                        else
                        {
                            double SaldoAttuale = Saldo(UserBank);
                            if (SaldoAttuale < bonifico.Importo) return Problem("Non hai abbasanza soldi per completare il bonifico!");

                            var contoDestinatario = model.BankAccounts.Where(o => o.Iban == destinatario).FirstOrDefault();

                            if (contoDestinatario != null)
                            {
                                model.AccountMovements.Add(new AccountMovement
                                {
                                    Date = DateTime.Now,
                                    In = null,
                                    Out = bonifico.Importo,
                                    FkBankAccount = UserBank.Id
                                });

                                model.AccountMovements.Add(new AccountMovement
                                {
                                    Date = DateTime.Now,
                                    In = bonifico.Importo,
                                    Out = null,
                                    FkBankAccount = contoDestinatario.Id
                                });

                                model.SaveChanges();

                                return Ok(bonifico);
                            }
                            else
                            {
                                model.AccountMovements.Add(new AccountMovement
                                {
                                    Date = DateTime.Now,
                                    In = null,
                                    Out = bonifico.Importo,
                                    FkBankAccount = UserBank.Id
                                });
                                model.SaveChanges();

                                return Ok(bonifico);
                            }
                        }
                    }
                }
                else
                {
                    return Problem("Errore nell'eseguire il bonifico");
                }
            }
        }
        [Authorize]
        [HttpPost("/conti-correnti")]
        public ActionResult Creaconto([FromBody] BankAccount conto)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);
                if (soggetto.IsBanker)
                {
                    model.BankAccounts.Add(new BankAccount
                    {
                        Iban = conto.Iban,
                        FkUser = conto.FkUser,
                    });
                    model.SaveChanges();
                    return Ok(conto);
                }
                else return Unauthorized("Non sei un banchiere!");
            }
        }
        [Authorize]
        [HttpPut("/conti-correnti/{id}")]
        public ActionResult Aggiorna(int id, [FromBody] BankAccount conto)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);
                if (soggetto.IsBanker)
                {
                    var scelto = model.BankAccounts.Where(o => o.Id == id).FirstOrDefault();
                    if (scelto == null) return
                    NotFound("Conto non trovato");
                    var esiste = model.BankAccounts.Where(o => o.Iban == conto.Iban).FirstOrDefault();
                    if (esiste != null)
                        return Problem("questo conto esiste gia!");

                    scelto.Iban = conto.Iban;
                    scelto.FkUser = conto.FkUser;
                    model.SaveChanges();
                    return Ok(conto);
                }
                else return Unauthorized("Non sei un banchiere!");

            }
        }
        [Authorize]
        [HttpDelete("/conti-correnti/{id}")]
        public ActionResult Elimina(int id, [FromBody] BankAccount conto)
        {
            using (WebBankingContext model = new WebBankingContext())
            {
                var ID = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Id").Value;
                var name = HttpContext.User.Claims.FirstOrDefault(i => i.Type == "Username").Value;
                User soggetto = model.Users.FirstOrDefault(q => q.Id.ToString() == ID && q.Username == name);
                if (soggetto.IsBanker)
                {
                    var scelto = model.BankAccounts.Where(o => o.Id == id).FirstOrDefault();
                    if (scelto == null)
                        NotFound("Conto non trovato");
                    else
                        model.BankAccounts.Remove(scelto);
                    model.SaveChanges();
                    return Ok();
                }
                else return Unauthorized("Non sei un banchiere!");
            }
        }
    }
}
