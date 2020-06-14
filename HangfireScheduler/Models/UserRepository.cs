using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangfireScheduler.Models
{
    public class UserRepository : IRepository<User>
    {
        private readonly Dictionary<string, User> _nameUserDictionary;

        public UserRepository(IConfiguration configuration)
        {
            var userList = configuration.GetSection("Users").Get<List<User>>();
            _nameUserDictionary = ConvertToDictionary(userList);
        }

        private Dictionary<string, User> ConvertToDictionary(List<User> userList)
        {
            if (userList == null)
                throw new ArgumentNullException($"Argument {nameof(userList)} can not be null");

            if (!userList.Any())
                throw new ArgumentException($"Argument {nameof(userList)} can not be empty");

            var nameProgramDictionary = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
            foreach (var user in userList)
            {
                if (nameProgramDictionary.ContainsKey(user.UserName))
                    throw new ArgumentException($"Record with the key {user.UserName} is already there. {nameof(user.UserName)} must be unique");

                nameProgramDictionary.Add(user.UserName, user);
            }

            return nameProgramDictionary;
        }

        public void AddOrUpdate(User value)
        {
            if (_nameUserDictionary.ContainsKey(value.UserName))
                _nameUserDictionary[value.UserName] = value;
            else
                _nameUserDictionary.Add(value.UserName, value);
        }

        public void Delete(string key)
        {
            if (!_nameUserDictionary.ContainsKey(key))
                throw new KeyNotFoundException($"Could not find key {key} to delete");

            _nameUserDictionary.Remove(key);
        }

        public User Get(string key)
        {
            if (!_nameUserDictionary.TryGetValue(key, out User value))
                throw new KeyNotFoundException($"{nameof(User)} not found by key {key}");

            return value;
        }
    }
}
