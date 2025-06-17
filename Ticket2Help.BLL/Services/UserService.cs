// BLL/Services/UserService.cs
using Ticket2Help.DAL.Repositories;
using Ticket2Help.DAL.Models;

namespace Ticket2Help.BLL.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User Login(string email, string password)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user != null && user.Password == password)  // In production, use password hashing techniques
            {
                return user;
            }
            return null;
        }
    }
}
