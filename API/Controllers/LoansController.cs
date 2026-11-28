
using System.Security.Claims;
using API.CustomExceptions;
using API.Dtos;
using API.Interfaces;
using API.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Luokan yläpuolella oleva [Authorize] koskee kaikkia actioneita:
    // lainata voi vain kirjautunut käyttäjä
    [Authorize]
    public class LoansController(ILoanService _loanService, IMapper _mapper) : ControllerBase
    {
        // POST api/loans
        [HttpPost]
        public async Task<ActionResult<LoanDto>> Borrow(BorrowReq req)
        {
            try
            {
                var loan = await _loanService.Borrow(req.ItemCopyId, GetUserId());

                return Ok(_mapper.Map<LoanDto>(loan));
            }
            catch (NotFoundException e)
            {
                return NotFound(new { title = "item copy not found", detail = e.Message });
            }
            catch (BusinessRuleException e)
            {
                // 409 Conflict: jokin lainauksen säännöistä esti lainauksen
                return Conflict(new { title = "cannot borrow", detail = e.Message });
            }
        }

        // GET api/loans/mine
        [HttpGet("mine")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetMyLoans()
        {
            var loans = await _loanService.GetMyLoans(GetUserId());

            return Ok(_mapper.Map<IEnumerable<LoanDto>>(loans));
        }

        // Sisäänkirjautuneen käyttäjän id on tokenin sub-claimissa.
        // Koska Program.cs:ssä on MapInboundClaims = false, claimin nimi on sama kuin tokenissa.
        // [Authorize] takaa, että token on olemassa, joten claim löytyy aina.


        // POST api/loans/5/return
        [HttpPost("{id}/return")]
        public async Task<ActionResult<LoanDto>> Return(int id)
        {
            // Token luotiin ClaimTypes.Role-claimilla. Koska MapInboundClaims = false,
            // rooli löytyy samalla nimellä.
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            try
            {
                // Controller ei päätä, saako käyttäjä palauttaa lainan.
                // Se välittää palauttajan id:n ja roolin servicelle, joka tekee päätöksen.
                var loan = await _loanService.Return(id, GetUserId(), role);

                return Ok(_mapper.Map<LoanDto>(loan));
            }
            catch (NotFoundException e)
            {
                return NotFound(new { title = "loan not found", detail = e.Message });
            }
            catch (ForbiddenException e)
            {
                // Forbid() ei käy tähän: se käynnistää autentikaation "challenge"-käsittelyn
                // eikä palauta viestiä. StatusCode antaa 403:n ja rungon.
                return StatusCode(StatusCodes.Status403Forbidden, new { title = "forbidden", detail = e.Message });
            }
            catch (BusinessRuleException e)
            {
                return Conflict(new { title = "cannot return loan", detail = e.Message });
            }
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst("sub")!.Value);
        }
    }
}