
using API.CustomExceptions;
using API.Data;
using API.Interfaces;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class LoanService(DataContext _repository, IFineCalculatorFactory _fineCalculatorFactory) : ILoanService
    {
        // Säännöt vakioina, ei koodiin hajautettuina "taikanumeroina"
        private const int MaxConcurrentLoans = 2;
        private const int LoanDays = 14;

        public async Task<Loan> Borrow(int itemCopyId, int userId)
        {
            // Sääntö 4: onko kappaletta olemassa?
            var copy = await _repository.ItemCopies
                .FirstOrDefaultAsync(c => c.Id == itemCopyId);

            if (copy == null)
            {
                throw new NotFoundException($"item copy {itemCopyId} not found");
            }

            // Sääntö 1: onko kappale jo lainassa?
            // ReturnedAt == null tarkoittaa voimassa olevaa lainaa
            var alreadyOnLoan = await _repository.Loans
                .AnyAsync(l => l.ItemCopyId == itemCopyId && l.ReturnedAt == null);

            if (alreadyOnLoan)
            {
                throw new BusinessRuleException("item copy is already on loan");
            }

            // Sääntö 2: onko käyttäjällä liikaa lainoja?
            var activeLoanCount = await _repository.Loans
                .CountAsync(l => l.AppUserId == userId && l.ReturnedAt == null);

            if (activeLoanCount >= MaxConcurrentLoans)
            {
                throw new BusinessRuleException(
                    $"user already has the maximum of {MaxConcurrentLoans} loans");
            }

            // Sääntö 3: onko maksamattomia maksuja?
            // Maksun lainaaja haetaan lainan kautta
            var hasUnpaidFines = await _repository.Fines
                .AnyAsync(f => f.Loan!.AppUserId == userId && f.PaidAt == null);

            if (hasUnpaidFines)
            {
                throw new BusinessRuleException("user has unpaid fines");
            }

            var now = DateTime.UtcNow;

            var loan = new Loan
            {
                ItemCopyId = itemCopyId,
                AppUserId = userId,
                BorrowedAt = now,
                DueAt = now.AddDays(LoanDays)
            };

            await _repository.Loans.AddAsync(loan);
            await _repository.SaveChangesAsync();

            return loan;
        }

        public async Task<IEnumerable<Loan>> GetMyLoans(int userId)
        {
            return await _repository.Loans
                .Include(l => l.ItemCopy)
                    .ThenInclude(c => c!.Item)
                .Where(l => l.AppUserId == userId && l.ReturnedAt == null)
                .ToListAsync();
        }

        // userId ja role ovat palautuksen tekijän (sisäänkirjautuneen käyttäjän) tiedot,
        // eivät lainaajan. Controller lukee ne tokenin claimeista.
        public async Task<Loan> Return(int loanId, int userId, string role)
        {
            var loan = await _repository.Loans
                .Include(l => l.Borrower)
                .FirstOrDefaultAsync(l => l.Id == loanId);

            if (loan == null)
            {
                throw new NotFoundException($"loan {loanId} not found");
            }

            // Admin ja henkilökunta (staff) voivat palauttaa kenen tahansa lainan.
            // Tavallinen lainaaja (rooli "user") voi palauttaa vain omat lainansa.
            if (role != "admin" && role != "staff" && loan.AppUserId != userId)
            {
                throw new ForbiddenException("only the borrower, staff or admin can return this loan");
            }

            if (loan.ReturnedAt != null)
            {
                throw new BusinessRuleException("loan has already been returned");
            }

            var now = DateTime.UtcNow;

            // Valitaan laskuri lainaajan roolin mukaan (ei palautuksen tekijän)
            var calculator = _fineCalculatorFactory.ForRole(loan.Borrower!.Role);

            loan.ReturnedAt = now;

            var fineCents = calculator.CalculateCents(loan.DueAt, now);

            // Maksu luodaan vain, jos maksettavaa tulee
            if (fineCents > 0)
            {
                await _repository.Fines.AddAsync(new Fine
                {
                    // Maksu kuuluu lainaajalle lainan kautta, ei palautuksen tekijälle
                    LoanId = loan.Id,
                    AmountCents = fineCents,
                    CreatedAt = now
                });
            }

            // Yksi SaveChangesAsync tallentaa palautuksen ja maksun samassa transaktiossa
            await _repository.SaveChangesAsync();
            //await _logWriter.Write(userId, $"returned loan {loanId}");

            return loan;
        }
    }
}