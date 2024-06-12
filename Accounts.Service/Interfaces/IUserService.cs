using Accounts.Models.Entities;
using Accounts.Models.Dtos;
using System.Threading.Tasks;

namespace Accounts.Service.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(UserRegisterDto userDto);
        Task<User> LoginUserAsync(UserLoginDto userDto);
        Task<User> GetUserByIdAsync(int id);

        Task<User> UpdateUserAsync(int id, UserRegisterDto userDto);
    }
}