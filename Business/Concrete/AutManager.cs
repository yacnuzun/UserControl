using Business.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.Results;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.Jwt;
using Entity.Dto_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class AutManager : IAuthService
    {
        IUserService _userService;
        ITokenHelper _tokenHelper;

        public AutManager(IUserService userService, ITokenHelper tokenHelper)
        {
            _userService = userService;
            _tokenHelper = tokenHelper;
        }

        public IDataResult<AccessToken> CreateAccessToken(User user)
        {
            var claims = _userService.GetClaims(user);
            var accessToken = _tokenHelper.CreateToken(user, claims);
            return new SuccessDataResult<AccessToken>(accessToken,"Token Oluşturuldu.");
        }

        public IDataResult<User> Login(UserForLoginDto userForLoginDto)
        {
            var userToCheck = _userService.GetByEmail(userForLoginDto.Email);
            if (userToCheck==null)
            {
                return new ErrorDataResult<User>("Böyle bir kullanıcı yok.");
            }
            if (!HashingHelper.VerifyPasswordHash(userForLoginDto.Password,userToCheck.PasswordHash
                ,userToCheck.PasswordSalt))
            {
                return new ErrorDataResult<User>("Kullanıcı adı veya sifre hatalı");
            }
            return new SuccessDataResult<User>(userToCheck,"Giriş başarılı!");
        }

        public IDataResult<User> Register(UserForRegisterDto userForRegisterDto, string password)
        {
            byte[] passwordHash, passwordSalt;
            HashingHelper.CreatePasswordHash(userForRegisterDto.Password, out passwordHash, out passwordSalt);
            var user = new User
            {
                Email = userForRegisterDto.Email,
                FirstName = userForRegisterDto.FirstName,
                LastName = userForRegisterDto.LastName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true
            };
            _userService.Add(user);
            return new SuccessDataResult<User>(user, "Kullanıcı oluşturuldu.");
        }

        public IResult UserExists(string email)
        {
            if (_userService.GetByEmail(email)!=null)
            {
                return new ErrorResult("Bu kullanıcı mevcut!");
            }
            return new SuccessResult();
        }
    }
}
