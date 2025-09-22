//using FridgeManagementSystem.Areas.Identity.Data;
//using Microsoft.AspNetCore.Identity;

//namespace FridgeManagementSystem.Managers.Validators
//{
//    public class ActiveUserValidator<TUser> : IUserValidator<TUser> where TUser : ApplicationUser
//    {
//        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
//        {
//            var errors = new List<IdentityError>();

//            // Check if user is active
//            if (!user.IsActive)
//            {
//                errors.Add(new IdentityError
//                {
//                    Code = "UserDeactivated",
//                    Description = "This account has been deactivated. Please contact administrator."
//                });
//            }

//            return Task.FromResult(errors.Count == 0
//                ? IdentityResult.Success
//                : IdentityResult.Failed(errors.ToArray()));
//        }
//    }
//}
