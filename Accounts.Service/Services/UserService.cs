using Accounts.Models.Entities;
using Accounts.Models.Dtos;
using Accounts.Repository.Interfaces;
using Accounts.Service.Interfaces;
using Accounts.Service.Security;
using System.Threading.Tasks;
namespace Accounts.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly pwHasher _pwHasher;

        public UserService(IUserRepository userRepository, pwHasher pwHasher)
        {
            _userRepository = userRepository;
            _pwHasher = pwHasher;
        }

        public async Task<User> RegisterUserAsync(UserRegisterDto userDto)
        {
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Fname = userDto.Fname,
                Lname = userDto.Lname,
                SHA256Password = _pwHasher.HashPw(userDto.Password)
            };

            await _userRepository.AddUserAsync(user);
            return user;
        }

        public async Task<User> LoginUserAsync(UserLoginDto userDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(userDto.Username);
            if (user == null || !_pwHasher.VerifyHashedPw(user.SHA256Password, userDto.Password)) // Implement proper password check
            {
                return null;
            }
            
            return user;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User> UpdateUserAsync(int id, UserRegisterDto userDto)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            user.Username = userDto.Username;
            user.Email = userDto.Email;
            user.Fname = userDto.Fname;
            user.Lname = userDto.Lname;
            user.SHA256Password = _pwHasher.HashPw(userDto.Password);

            await _userRepository.UpdateUserAsync(user);
            return user;
        }
    }
}