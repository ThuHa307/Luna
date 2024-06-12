﻿using Luna.Utility;
using Microsoft.AspNetCore.Identity;

namespace Luna.Services
{
    public class GlobalService
    {
        private string _consultantId;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public GlobalService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            InitializeConsultantId().GetAwaiter().GetResult(); // Đồng bộ hóa lấy consultantId tại constructor
        }

        private async Task InitializeConsultantId()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var usersInRole = await userManager.GetUsersInRoleAsync("Consultant");

                if (usersInRole.Count == 1)
                {
                    _consultantId = usersInRole.First().Id;
                }
                else
                {
                    throw new InvalidOperationException("There should be exactly one consultant.");
                }
            }
        }

        public string GetConsultantId()
        {
            return _consultantId;
        }
    }

}