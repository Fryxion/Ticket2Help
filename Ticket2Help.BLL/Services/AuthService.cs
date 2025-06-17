using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticket2Help.DAL.Repositories;
using Ticket2Help.DAL.Models;

namespace Ticket2Help.BLL.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User Login(string email, string password)
        {
            var user = _userRepository.GetUserByEmail(email);
            if (user != null && user.Password == password)
            {
                return user;
            }
            return null;
        }
    }
}
