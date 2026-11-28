
namespace API.CustomExceptions
{
    // Käyttäjä on tunnistettu, mutta hänellä ei ole oikeutta tähän operaatioon
    public class ForbiddenException : Exception
    {
        public ForbiddenException() { }

        public ForbiddenException(string message) : base(message) { }
    }
}