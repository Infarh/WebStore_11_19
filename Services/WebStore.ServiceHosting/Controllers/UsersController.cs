﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.Context;
using WebStore.Domain.DTO.Identity;
using WebStore.Domain.Entities;

namespace WebStore.ServiceHosting.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _Logger;
        private readonly UserStore<User> _UserStore;

        public UsersController(WebStoreContext db, ILogger<UsersController> Logger)
        {
            _Logger = Logger;
            _UserStore = new UserStore<User>(db) { AutoSaveChanges = true };
        }

        #region Users

        [HttpGet("AllUsers")]
        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _UserStore.Users.ToArrayAsync();
        }

        [HttpPost("UserId")]
        public async Task<string> GetUserIdAsync([FromBody] User user) => await _UserStore.GetUserIdAsync(user);

        [HttpPost("UserName")]
        public async Task<string> GetUserNameAsync([FromBody] User user) => await _UserStore.GetUserNameAsync(user);

        [HttpPost("UserName/{name}")]
        public async Task SetUserNameAsync([FromBody] User user, string name) => await _UserStore.SetUserNameAsync(user, name);

        [HttpPost("NormalUserName")]
        public async Task<string> GetNormalizedUserNameAsync([FromBody] User user) => await _UserStore.GetNormalizedUserNameAsync(user);

        [HttpPost("NormalUserName/{name}")]
        public Task SetNormalizedUserNameAsync([FromBody] User user, string name) => _UserStore.SetNormalizedUserNameAsync(user, name);

        [HttpPost("User")]
        public async Task<bool> CreateAsync([FromBody] User user)
        {
            if (user is null)
            {
                _Logger.LogError("В процессе создания нового пользователя была передана пустая ссылка на объект User");
                throw new ArgumentNullException(nameof(user));
            }

            using (_Logger.BeginScope("Создание нового пользователя {0}", user.UserName))
            {
                var creation_result = await _UserStore.CreateAsync(user);

                if(creation_result.Succeeded)
                    _Logger.LogInformation("Пользователь {0} успешно создан", user.UserName);
                else
                    _Logger.LogWarning("При создании пользователя {0} возникли ошибки: {1}",
                        user.UserName,
                        string.Join(", ", creation_result.Errors.Select(e => e.Description)));

                return creation_result.Succeeded;
            }
        }

        [HttpPut("User")]
        public async Task<bool> UpdateAsync([FromBody] User user) => (await _UserStore.UpdateAsync(user)).Succeeded;

        [HttpPost("User/Delete")]
        public async Task<bool> DeleteAsync([FromBody] User user) => (await _UserStore.DeleteAsync(user)).Succeeded;

        [HttpGet("User/Find/{id}")]
        public async Task<User> FindByIdAsync(string id) => await _UserStore.FindByIdAsync(id);

        [HttpGet("User/Normal/{name}")]
        public async Task<User> FindByNameAsync(string name) => await _UserStore.FindByNameAsync(name);

        [HttpPost("Role/{role}")]
        public async Task AddToRoleAsync([FromBody] User user, string role) => await _UserStore.AddToRoleAsync(user, role);

        [HttpPost("Role/Delete/{role}")]
        public async Task RemoveFromRoleAsync([FromBody] User user, string role) => await _UserStore.RemoveFromRoleAsync(user, role);

        [HttpPost("Roles")]
        public async Task<IList<string>> GetRolesAsync([FromBody] User user) => await _UserStore.GetRolesAsync(user);

        [HttpPost("InRole/{role}")]
        public async Task<bool> IsInRoleAsync([FromBody] User user, string role) => await _UserStore.IsInRoleAsync(user, role);

        [HttpGet("UsersInRole/{role}")]
        public async Task<IList<User>> GetUsersInRoleAsync(string role) => await _UserStore.GetUsersInRoleAsync(role);

        [HttpPost("GetPasswordHash")]
        public async Task<string> GetPasswordHashAsync([FromBody] User user) => await _UserStore.GetPasswordHashAsync(user);

        [HttpPost("SetPasswordHash")]
        public async Task<string> SetPasswordHashAsync([FromBody] PasswordHashDTO hash)
        {
            await _UserStore.SetPasswordHashAsync(hash.User, hash.Hash);
            return hash.User.PasswordHash;
        }

        [HttpPost("HasPassword")]
        public async Task<bool> HasPasswordAsync([FromBody] User user) => await _UserStore.HasPasswordAsync(user);

        #endregion

        #region Claims

        [HttpPost("GetClaims")]
        public async Task<IList<Claim>> GetClaimsAsync([FromBody] User user) => await _UserStore.GetClaimsAsync(user);

        [HttpPost("AddClaims")]
        public async Task AddClaimsAsync([FromBody] AddClaimDTO ClaimInfo) => await _UserStore.AddClaimsAsync(ClaimInfo.User, ClaimInfo.Claims);

        [HttpPost("ReplaceClaim")]
        public async Task ReplaceClaimAsync([FromBody] ReplaceClaimDTO ClaimInfo) =>
            await _UserStore.ReplaceClaimAsync(ClaimInfo.User, ClaimInfo.Claim, ClaimInfo.NewClaim);

        [HttpPost("RemoveClaim")]
        public async Task RemoveClaimsAsync([FromBody] RemoveClaimDTO ClaimInfo) =>
            await _UserStore.RemoveClaimsAsync(ClaimInfo.User, ClaimInfo.Claims);

        [HttpPost("GetUsersForClaim")]
        public async Task<IList<User>> GetUsersForClaimAsync([FromBody] Claim claim) =>
            await _UserStore.GetUsersForClaimAsync(claim);

        #endregion

        #region TwoFactor

        [HttpPost("GetTwoFactorEnabled")]
        public async Task<bool> GetTwoFactorEnabledAsync([FromBody] User user) =>
            await _UserStore.GetTwoFactorEnabledAsync(user);

        [HttpPost("SetTwoFactor/{enable}")]
        public async Task SetTwoFactorEnabledAsync([FromBody] User user, bool enable) =>
            await _UserStore.SetTwoFactorEnabledAsync(user, enable);

        #endregion

        #region EMail/Phone

        [HttpPost("GetEmail")]
        public async Task<string> GetEmailAsync([FromBody] User user) => await _UserStore.GetEmailAsync(user);

        [HttpPost("SetEmail/{email}")]
        public async Task SetEmailAsync([FromBody] User user, string email) => await _UserStore.SetEmailAsync(user, email);

        [HttpPost("GetEmailConfirmed")]
        public async Task<bool> GetEmailConfirmedAsync([FromBody] User user) => await _UserStore.GetEmailConfirmedAsync(user);

        [HttpPost("SetEmailConfirmed/{enable}")]
        public async Task SetEmailConfirmedAsync([FromBody] User user, bool enable) => await _UserStore.SetEmailConfirmedAsync(user, enable);

        [HttpGet("UserFindByEmail/{email}")]
        public async Task<User> FindByEmailAsync(string email) => await _UserStore.FindByEmailAsync(email);

        [HttpPost("GetNormalizedEmail")]
        public async Task<string> GetNormalizedEmailAsync([FromBody] User user) => await _UserStore.GetNormalizedEmailAsync(user);

        [HttpPost("SetNormalizedEmail/{email?}")]
        public async Task SetNormalizedEmailAsync([FromBody] User user, string email) => await _UserStore.SetNormalizedEmailAsync(user, email);

        [HttpPost("GetPhoneNumber")]
        public async Task<string> GetPhoneNumberAsync([FromBody] User user) => await _UserStore.GetPhoneNumberAsync(user);

        [HttpPost("SetPhoneNumber/{phone}")]
        public async Task SetPhoneNumberAsync([FromBody] User user, string phone) => await _UserStore.SetPhoneNumberAsync(user, phone);

        [HttpPost("GetPhoneNumberConfirmed")]
        public async Task<bool> GetPhoneNumberConfirmedAsync([FromBody] User user) =>
            await _UserStore.GetPhoneNumberConfirmedAsync(user);

        [HttpPost("SetPhoneNumberConfirmed/{confirmed}")]
        public async Task SetPhoneNumberConfirmedAsync([FromBody] User user, bool confirmed) =>
            await _UserStore.SetPhoneNumberConfirmedAsync(user, confirmed);

        #endregion

        #region Login/Lockout

        [HttpPost("AddLogin")]
        public async Task AddLoginAsync([FromBody] AddLoginDTO login) => await _UserStore.AddLoginAsync(login.User, login.UserLoginInfo);

        [HttpPost("RemoveLogin/{LoginProvider}/{ProviderKey}")]
        public async Task RemoveLoginAsync([FromBody] User user, string LoginProvider, string ProviderKey) =>
            await _UserStore.RemoveLoginAsync(user, LoginProvider, ProviderKey);

        [HttpPost("GetLogins")]
        public async Task<IList<UserLoginInfo>> GetLoginsAsync([FromBody] User user) =>
            await _UserStore.GetLoginsAsync(user);

        [HttpGet("User/FindByLogin/{LoginProvider}/{ProviderKey}")]
        public async Task<User> FindByLoginAsync(string LoginProvider, string ProviderKey) =>
            await _UserStore.FindByLoginAsync(LoginProvider, ProviderKey);

        [HttpPost("GetLockoutEndDate")]
        public async Task<DateTimeOffset?> GetLockoutEndDateAsync([FromBody] User user) =>
            await _UserStore.GetLockoutEndDateAsync(user);

        [HttpPost("SetLockoutEndDate")]
        public async Task SetLockoutEndDateAsync([FromBody] SetLockoutDTO LocoutInfo) =>
            await _UserStore.SetLockoutEndDateAsync(LocoutInfo.User, LocoutInfo.LockoutEnd);

        [HttpPost("IncrementAccessFailedCount")]
        public async Task<int> IncrementAccessFailedCountAsync([FromBody] User user) => await _UserStore.IncrementAccessFailedCountAsync(user);

        [HttpPost("ResetAccessFailedCount")]
        public async Task ResetAccessFailedCountAsync([FromBody] User user) => await _UserStore.ResetAccessFailedCountAsync(user);

        [HttpPost("GetAccessFailedCount")]
        public async Task<int> GetAccessFailedCountAsync([FromBody] User user) => await _UserStore.GetAccessFailedCountAsync(user);

        [HttpPost("GetLockoutEnabled")]
        public async Task<bool> GetLockoutEnabledAsync([FromBody] User user) => await _UserStore.GetLockoutEnabledAsync(user);

        [HttpPost("SetLockoutEnabled/{enable}")]
        public async Task SetLockoutEnabledAsync([FromBody] User user, bool enable) => await _UserStore.SetLockoutEnabledAsync(user, enable);

        #endregion
    }
}