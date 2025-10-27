using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tabibi.API.Common;
using Tabibi.API.Configurations;
using Tabibi.API.Database;
using Tabibi.API.DTOs.Auth;
using Tabibi.API.DTOs.Users;
using Tabibi.API.Entities;
using Tabibi.API.Services;
using Tabibi.EmailService;

namespace Tabibi.API.Controllers
{
    [Route("auth")]
    [ApiController]
    [AllowAnonymous]
    public sealed class AuthController(
        UserManager<IdentityUser> userManager,
        AppDbContext appDbContext,
        AppIdentityDbContext identityDbContext,
        TokenProvider tokenProvider,
        IOptions<JwtAuthOptions> options) : ControllerBase
    {
        private readonly JwtAuthOptions jwtAuthOptions = options.Value;


        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(
            RegisterUserDto registerUserDto,
            IValidator<RegisterUserDto> validator,
            IEmailSender emailSender)
        {
            await validator.ValidateAndThrowAsync(registerUserDto);

            bool emailIsTaken = await userManager.FindByEmailAsync(registerUserDto.Email) != null;

            if (emailIsTaken)
            {
                return Problem(
                    detail: $"Email '{registerUserDto.Email}' is already taken",
                    statusCode: StatusCodes.Status409Conflict);
            }

            IdentityUser identityUser = new()
            {
                UserName = registerUserDto.Email,
                Email = registerUserDto.Email
            };

            IdentityResult createUserResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);

            if (!createUserResult.Succeeded)
            {
                Dictionary<string, object?> extensions = new()
                {
                    {
                        "errors",
                        createUserResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                    }
                };

                return Problem(
                    detail: "Unable to register the user, please try again",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: extensions);
            }

            string? role = registerUserDto.Role switch
            {
                RoleDto.Patient => Roles.Patient,
                RoleDto.Doctor => Roles.Doctor,
                _ => null
            };

            if (role == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "Invalid Role");
            }

            IdentityResult addToRoleResult = await userManager.AddToRoleAsync(identityUser, role);

            if (!addToRoleResult.Succeeded)
            {
                Dictionary<string, object?> extensions = new()
                {
                    {
                        "errors",
                        addToRoleResult.Errors.ToDictionary(e => e.Code, e => e.Description)
                    }
                };

                return Problem(
                    detail: "Unable to register the user, please try again",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: extensions);
            }

            //---------
            // Send confirm email

            string code = await userManager.GenerateTwoFactorTokenAsync(identityUser, TokenOptions.DefaultEmailProvider);

            Message message = new(
                [identityUser.Email!],
                "Email confirmation code",
                $"Welcome! Your email confirmation code is: {code}");

            await emailSender.SendEmailAsync(message);

            //---------

            User user = registerUserDto.ToEntity(identityUser.Id);

            await appDbContext.Users.AddAsync(user);
            await appDbContext.SaveChangesAsync();

            return Created();
        }


        [HttpPost("login")]
        [ProducesResponseType<AccessTokensDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccessTokensDto>> Login(
            LoginUserDto loginUserDto,
            IValidator<LoginUserDto> validator)
        {
            await validator.ValidateAndThrowAsync(loginUserDto);

            IdentityUser? identityUser = await userManager.FindByEmailAsync(loginUserDto.Email);

            if (identityUser == null || !await userManager.CheckPasswordAsync(identityUser, loginUserDto.Password))
            {
                return Problem(
                    "Invalid email or password",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!identityUser.EmailConfirmed)
            {
                return Problem("Please confirm your email", statusCode: StatusCodes.Status400BadRequest);
            }

            IList<string> roles = await userManager.GetRolesAsync(identityUser);

            TokenRequestDto tokenRequest = new()
            {
                UserId = identityUser.Id,
                Email = identityUser.Email!,
                Roles = [.. roles]
            };

            AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

            RefreshToken refreshToken = new()
            {
                Id = Guid.CreateVersion7(),
                Token = accessTokens.RefreshToken,
                UserId = identityUser.Id,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtAuthOptions.RefreshTokenExpirationInDays)
            };

            await identityDbContext.RefreshTokens.AddAsync(refreshToken);
            await identityDbContext.SaveChangesAsync();

            return Ok(accessTokens);
        }


        [HttpPost("refresh")]
        [ProducesResponseType<AccessTokensDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AccessTokensDto>> Refresh(
            RefreshTokenDto refreshTokenDto,
            IValidator<RefreshTokenDto> validator)
        {
            await validator.ValidateAndThrowAsync(refreshTokenDto);

            RefreshToken? refreshToken = await identityDbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);

            if (refreshToken == null || refreshToken.ExpiresAtUtc < DateTime.UtcNow)
            {
                return Problem(
                     "Invalid refresh token",
                     statusCode: StatusCodes.Status400BadRequest);
            }

            IList<string> roles = await userManager.GetRolesAsync(refreshToken.User!);

            TokenRequestDto tokenRequest = new()
            {
                UserId = refreshToken.UserId,
                Email = refreshToken.User!.Email!,
                Roles = [.. roles]
            };

            AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

            refreshToken.Token = accessTokens.RefreshToken;
            refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(jwtAuthOptions.RefreshTokenExpirationInDays);

            await identityDbContext.SaveChangesAsync();

            return Ok(accessTokens);
        }


        [HttpPost("email-confirmation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmailConfirmation(
            EmailConfirmationDto emailConfirmationDto,
            IValidator<EmailConfirmationDto> validator)
        {
            await validator.ValidateAndThrowAsync(emailConfirmationDto);

            IdentityUser? identityUser = await userManager.FindByEmailAsync(emailConfirmationDto.Email);

            if (identityUser == null)
            {
                return Problem(
                    "Invalid email confirmation request",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            bool isCodeValid = await userManager
                .VerifyTwoFactorTokenAsync(identityUser, TokenOptions.DefaultEmailProvider, emailConfirmationDto.Code);

            if (!isCodeValid)
            {
                return Problem(
                    "Invalid confirmation code",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            identityUser.EmailConfirmed = true;
            IdentityResult updateResult = await userManager.UpdateAsync(identityUser);

            if (!updateResult.Succeeded)
            {
                return Problem(
                    "An error occurred while confirming the email",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }


        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordDto forgotPasswordDto,
            IValidator<ForgotPasswordDto> validator,
            IEmailSender emailSender)
        {
            await validator.ValidateAndThrowAsync(forgotPasswordDto);

            IdentityUser? identityUser = await userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (identityUser == null)
            {
                return Problem(
                    "Invalid email forgot password request",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            string code = await userManager.GenerateTwoFactorTokenAsync(identityUser, TokenOptions.DefaultEmailProvider);

            string emailBody = $"Your password reset code is: {code}";
            Message message = new([identityUser.Email!], "Reset password code", emailBody);

            await emailSender.SendEmailAsync(message);

            return Ok();
        }


        [HttpPost("verify-password-reset-code")]
        [ProducesResponseType<ResetTokenResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResetTokenResponseDto>> VerifyPasswordResetCode(
            VerifyCodeDto verifyCodeDto,
            IValidator<VerifyCodeDto> validator)
        {
            await validator.ValidateAndThrowAsync(verifyCodeDto);

            IdentityUser? identityUser = await userManager.FindByEmailAsync(verifyCodeDto.Email);

            if (identityUser == null)
            {
                return Problem(
                    "Invalid email verify-password-reset-code request",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            bool isCodeValid = await userManager
                .VerifyTwoFactorTokenAsync(identityUser, TokenOptions.DefaultEmailProvider, verifyCodeDto.Code);

            if (!isCodeValid)
            {
                return Problem(
                    "Invalid code",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            // If code is valid, generate the *real* password reset token
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(identityUser);

            return Ok(new ResetTokenResponseDto(resetToken));
        }


        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordDto resetPasswordDto,
            IValidator<ResetPasswordDto> validator)
        {
            await validator.ValidateAndThrowAsync(resetPasswordDto);

            IdentityUser? identityUser = await userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (identityUser == null)
            {
                return Problem(
                    "Invalid email reset-password request",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            IdentityResult result = await userManager
                .ResetPasswordAsync(identityUser, resetPasswordDto.Token, resetPasswordDto.Password);

            if (!result.Succeeded)
            {
                IDictionary<string, string[]> errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key.ToLower(),
                        g => g.Select(e => e.Description).ToArray());

                return Problem(
                    "Invalid reset-password request",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?>() { { "errors", errors } });
            }

            return Ok();
        }
    }
}
