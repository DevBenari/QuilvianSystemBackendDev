using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Controllers;
using QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Models;
using QuilvianSystemBackendDev.Models;
using QuilvianSystemBackendDev.Repositories;

namespace QuilvianSystemBackendDev.Areas.ManajemenKesehatan.MasterData.Services
{
    public class serviceMasterData
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public serviceMasterData(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext
                ?? throw new ArgumentNullException(nameof(applicationDbContext));
        }

        public async Task<UserActive?> GetCurrentUserByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;

            var result = await (from user in _applicationDbContext.UserActives
                                where user.Email == email && !user.IsDelete
                                select new UserActive
                                {
                                    UserActiveId = user.UserActiveId,
                                    UserActiveCode = user.UserActiveCode,
                                    FullName = user.FullName,
                                    IdentityNumber = user.IdentityNumber,
                                    PlaceOfBirth = user.PlaceOfBirth,
                                    DateOfBirth = user.DateOfBirth,
                                    Gender = user.Gender,
                                    Address = user.Address,
                                    Handphone = user.Handphone,
                                    Email = user.Email,
                                    IsActive = user.IsActive
                                }).FirstOrDefaultAsync();

            return result;
        }
    }
}

