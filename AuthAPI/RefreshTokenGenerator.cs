using AuthAPI.Models;
using System.Security.Cryptography;
using System;
using System.Linq;

namespace AuthAPI
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private readonly AuthDBContext context;

        public RefreshTokenGenerator(AuthDBContext _context)
        {
            context = _context;
        }
        public string GenerateToken(string username)
        {
            var randomnumber = new byte[32];
            using (var randomnumbergenerator = RandomNumberGenerator.Create())
            {
                randomnumbergenerator.GetBytes(randomnumber);
                string RefreshToken = Convert.ToBase64String(randomnumber);

                var _user = context.TblRefreshtokens.FirstOrDefault(o => o.UserId == username);
                if (_user != null)
                {
                    _user.RefreshToken = RefreshToken;
                    context.SaveChanges();
                }
                else
                {
                    var entity = new TblRefreshtoken()
                    {
                        UserId = username,
                        TokenId = new Random().Next().ToString(),
                        RefreshToken = RefreshToken,
                        IsActive = true
                    };  
                    this.context.TblRefreshtokens.Add(entity);
                    this.context.SaveChanges();
                } 
                return RefreshToken;
            }
        }
    }
}

