
using API.Dtos;
using API.Models;
using AutoMapper;

namespace API.Profiles
{
    public class LoanProfile : Profile
    {
        public LoanProfile()
        {
            // Propertyt ovat samannimisiä, joten AutoMapper osaa mäpätä ne itse
            CreateMap<Loan, LoanDto>();
        }
    }
}