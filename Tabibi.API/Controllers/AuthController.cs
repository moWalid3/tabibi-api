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
using Tabibi.API.Entities;
using Tabibi.API.Services;
using Tabibi.EmailService;

namespace Tabibi.API.Controllers
{
    [Route("auth")]
    [ApiController]
    [AllowAnonymous]
    public sealed class AuthController(
        UserManager<ApplicationUser> userManager,
        AppDbContext appDbContext,
        TokenProvider tokenProvider,
        IOptions<JwtAuthOptions> options) : ControllerBase
    {
        private readonly JwtAuthOptions _jwtAuthOptions = options.Value;


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

            ApplicationUser appUser = new()
            {
                Name = registerUserDto.Name,
                UserName = registerUserDto.Email,
                Email = registerUserDto.Email,
                CreatedAtUtc = DateTime.UtcNow
            };

            IdentityResult createUserResult = await userManager.CreateAsync(appUser, registerUserDto.Password);

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

            IdentityResult addToRoleResult = await userManager.AddToRoleAsync(appUser, role);

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

            // Send confirm email
            string code = await userManager.GenerateTwoFactorTokenAsync(appUser, TokenOptions.DefaultEmailProvider);

            Message message = new(
                [appUser.Email!],
                "Email confirmation code",
                $"Welcome! Your email confirmation code is: {code}");

            await emailSender.SendEmailAsync(message);
            //---------

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

            ApplicationUser? appUser = await userManager.FindByEmailAsync(loginUserDto.Email);

            if (appUser == null || !await userManager.CheckPasswordAsync(appUser, loginUserDto.Password))
            {
                return Problem(
                    "Invalid email or password",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            if (!appUser.EmailConfirmed)
            {
                return Problem("Please confirm your email", statusCode: StatusCodes.Status400BadRequest);
            }

            IList<string> roles = await userManager.GetRolesAsync(appUser);

            TokenRequestDto tokenRequest = new()
            {
                UserId = appUser.Id,
                Email = appUser.Email!,
                Roles = [.. roles]
            };

            AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

            RefreshToken refreshToken = new()
            {
                Id = Guid.CreateVersion7(),
                Token = accessTokens.RefreshToken,
                UserId = appUser.Id,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationInDays)
            };

            await appDbContext.RefreshTokens.AddAsync(refreshToken);
            await appDbContext.SaveChangesAsync();

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

            RefreshToken? refreshToken = await appDbContext.RefreshTokens
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
            refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationInDays);

            await appDbContext.SaveChangesAsync();

            return Ok(accessTokens);
        }


        [HttpPost("email-confirmation")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> EmailConfirmation(
            EmailConfirmationDto emailConfirmationDto,
            IValidator<EmailConfirmationDto> validator)
        {
            await validator.ValidateAndThrowAsync(emailConfirmationDto);

            ApplicationUser? appUser = await userManager.FindByEmailAsync(emailConfirmationDto.Email);

            if (appUser == null)
            {
                return Problem(
                    "Invalid email confirmation request",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            bool isCodeValid = await userManager
                .VerifyTwoFactorTokenAsync(appUser, TokenOptions.DefaultEmailProvider, emailConfirmationDto.Code);

            if (!isCodeValid)
            {
                return Problem(
                    "Invalid confirmation code",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            appUser.EmailConfirmed = true;
            IdentityResult updateResult = await userManager.UpdateAsync(appUser);

            if (!updateResult.Succeeded)
            {
                return Problem(
                    "An error occurred while confirming the email",
                    statusCode: StatusCodes.Status400BadRequest);
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

            ApplicationUser? appUser = await userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (appUser == null)
            {
                return Problem(
                    "Invalid email forgot password request",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            string code = await userManager.GenerateTwoFactorTokenAsync(appUser, TokenOptions.DefaultEmailProvider);

            string emailBody = $"Your password reset code is: {code}";
            Message message = new([appUser.Email!], "Reset password code", emailBody);

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

            ApplicationUser? appUser = await userManager.FindByEmailAsync(verifyCodeDto.Email);

            if (appUser == null)
            {
                return Problem(
                    "Invalid email verify-password-reset-code request",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            bool isCodeValid = await userManager
                .VerifyTwoFactorTokenAsync(appUser, TokenOptions.DefaultEmailProvider, verifyCodeDto.Code);

            if (!isCodeValid)
            {
                return Problem(
                    "Invalid code",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            // If code is valid, generate the *real* password reset token
            string resetToken = await userManager.GeneratePasswordResetTokenAsync(appUser);

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

            ApplicationUser? appUser = await userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (appUser == null)
            {
                return Problem(
                    "Invalid email reset-password request",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            IdentityResult result = await userManager
                .ResetPasswordAsync(appUser, resetPasswordDto.Token, resetPasswordDto.Password);

            if (!result.Succeeded)
            {
                Dictionary<string, object?> extensions = new()
                {
                    {
                        "errors",
                        result.Errors.ToDictionary(e => e.Code, e => e.Description)
                    }
                };

                return Problem(
                    "Invalid reset-password request",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: extensions);
            }

            return Ok();
        }
    }
}
