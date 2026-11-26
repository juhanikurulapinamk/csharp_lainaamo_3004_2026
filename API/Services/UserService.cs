
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.CustomExceptions;
using API.Data;
using API.Interfaces;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace API.Services
{

    // kun nyt käytämme EF Corea, pystymme injektoimaan serviceen
    // suoraan DataContextin ja käyttää sitä repositoryna
    // DataContext on jo rekisteröity aiemmin käyttäen AddDbContext-metodia Program.cs-filussa
    public class UserService(DataContext _repository, IPasswordHasher<AppUser> _passwordHasher, IConfiguration _config) : IUserService
    {




        public async Task<AppUser?> GetByUserName(string username)
        {
            // FirstOrDefaultAsync palauttaa null, jos hakuehdolla ei löydy yhtään riviä tietokannasta
            // muuten sen rivin tiedot, joka niillä löytyy

            // FirstAsync puolestaan heittää poikkeuksen, jos hakuehdolla ei löydy osumia tietokannasta
            

            // EF Core luo tästä sql-kyselyn SELECT * FROM Users WHERE LOWER(UserName) = ? LIMIT 1;
            // jossa kysymysmerkin paikalle tulee username-muuttujan arvo

            // collation

            // äiti <=> Äiti
            // aiti <=> äiti

            // Juhani => juhani



            var user = await _repository.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower());
            return user;
        }

        public async Task<AppUser> Register(string username, string password)
        {


            var existingUser = await GetByUserName(username);
            if (existingUser != null)
            {
                throw new UserRegistrationException("username must be unique");
            }

            var user = new AppUser
            {
                UserName = username,
                Role = "user",
                // Asetetaan väliaikainen arvo, koska property on required.
                // Oikea hash lasketaan heti seuraavalla rivillä.
                HashedPassword = ""
            };

            // HashPassword ottaa parametrikseen käyttäjän ja selkokielisen salasanan.
            // Se arpoo saltin itse, laskee hashin ja palauttaa merkkijonon,
            // johon on pakattu sekä algoritmin tunniste, salt että itse hash.
            user.HashedPassword = _passwordHasher.HashPassword(user, password);
            // tämä tekee käytännössä insertin Users-tietokantatauluun
            // koska olemme määrittäneet DbSetiksi Usersin, pystymme käyttämään sitä näin
            await _repository.Users.AddAsync(user);
            // insert pitää vahvistaa, jotta rivi oikeasti tallennetaan
            await _repository.SaveChangesAsync();

            return user;
        }

        
        // lisää UserService.cs:n alkuun:
        // using System.IdentityModel.Tokens.Jwt;
        // using System.Security.Claims;
        // using System.Text;
        // using Microsoft.IdentityModel.Tokens;


        
        public async Task<IEnumerable<AppUser>> GetAll()
        {
            // tämä tekee sql-kyselyn SELECT * FROM Users;
            var users = await _repository.Users.AsNoTracking().ToListAsync();
            return users;
        }

        public async Task<string> Login(string username, string password)
        {
            
            var user = await GetByUserName(username);
            if (user == null)
            {
                throw new NotFoundException("user not found");
            }

            // VerifyHashedPassword lukee tallennetusta hashista saltin ja asetukset,
            // laskee samoilla asetuksilla hashin annetusta salasanasta ja vertaa niitä.
            // Vertailu on sisäisesti vakioaikainen, joten ajastushyökkäys ei onnistu.
            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password);

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                throw new NotFoundException("user not found");
            }

            // Jos käyttäjä löytyy ja salasana on oikein,
            // luodaan JWT käyttäjän tunnistamista varten 

            var tokenKey = _config["TokenKey"];
            if (tokenKey == null)
            {
                throw new Exception("token key not set");
            }

            // varmista myös, että TokenKey on tarpeeksi pitkä
            if (tokenKey.Length < 64)
            {
                throw new Exception("invalid token key, must be at least 64 characters in length");
            }

            
            
            // SymmetricSecurityKey on tyyppi, jossa jwt:n luonnissa
            // ja jwtn tarkistuksessa käytetään samaa avainta (TokenKey)
            // string pitää muuttaa byte[] arrayksi
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            // jwt:n allekirjoitus tehdään avainta käyttäen
            // HMACSha512-algoritimillä
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            // tässä jwt:n sisään laitettavat käyttäjää koskevat asiat
            // Sub = käyttäjän id
            // Name = käyttäjänimi
            // Jti = random merkkijono
            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role)

            };
            // luodaan token claimit on yo. tiedot, jotka menevät sisään
            // expires on voimassaoloaika (tässä viikko)
            // SigningCredentials on allekirjoitus joka luotiin ylempänä
            var _token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddDays(7), signingCredentials: credentials);
            // tässä tehdään tokenista base64 enkoodattu merkkijono
            // tämä voidaan antaa käyttäjälle kirjautumisen jälkeen
            return new JwtSecurityTokenHandler().WriteToken(_token);
                

        }
    }
}