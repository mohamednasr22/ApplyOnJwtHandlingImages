using LearnDotNet7.Repos.Models;
using LearnDotNet7.Repos;
using LearnDotNet7.Service;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace LearnDotNet7.Container
{
    public class RefreshHandler : IRefreshHandler
    {
        private readonly LearndataContext context;
        public RefreshHandler(LearndataContext context)
        {
            this.context = context;
        }
        public async Task<string> GenerateToken(string username)
        {
            var randomnumber = new byte[32];
            using (var randomnumbergenerator = RandomNumberGenerator.Create())
            {
                randomnumbergenerator.GetBytes(randomnumber);
                string refreshtoken = Convert.ToBase64String(randomnumber);
                var Existtoken = this.context.TblRefreshtokens.FirstOrDefaultAsync(item => item.Userid == username).Result;
                if (Existtoken != null)
                {
                    Existtoken.Refreshtoken = refreshtoken;
                }
                else
                {
                    await this.context.TblRefreshtokens.AddAsync(new TblRefreshtoken
                    {
                        Userid = username,
                        Tokenid = new Random().Next().ToString(),
                        Refreshtoken = refreshtoken
                    });
                }
                await this.context.SaveChangesAsync();

                return refreshtoken;

            }
        }
    }
}
