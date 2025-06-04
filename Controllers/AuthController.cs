using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GestionFinanciere.API.Models.Entities;
using GestionFinanciere.API.Models.DTOs;
using GestionFinanciere.API.Services;
using System.Security.Claims;

namespace GestionFinanciere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;
        
        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            JwtService jwtService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtService = jwtService;
            _logger = logger;
        }
        
        /// <summary>
        /// Inscription d'un nouvel utilisateur
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // Vérifier si l'utilisateur existe déjà
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Un utilisateur avec cet email existe déjà." });
                }
                
                // Créer le nouvel utilisateur
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Créer l'utilisateur avec le mot de passe
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (!result.Succeeded)
                {
                    return BadRequest(new { 
                        message = "Erreur lors de la création de l'utilisateur.",
                        errors = result.Errors.Select(e => e.Description).ToList()
                    });
                }
                
                // Assigner le rôle par défaut "Utilisateur"
                await EnsureRoleExists("Utilisateur");
                await _userManager.AddToRoleAsync(user, "Utilisateur");
                
                _logger.LogInformation("Nouvel utilisateur créé: {Email}", user.Email);
                
                // Générer le token JWT
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user, roles);
                
                var response = new AuthResponseDto
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(60),
                    User = new UserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Roles = roles.ToList(),
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription de l'utilisateur");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
        
        /// <summary>
        /// Connexion d'un utilisateur
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                // Rechercher l'utilisateur par email
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect." });
                }
                
                // Vérifier le mot de passe
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect." });
                }
                
                // Mettre à jour la date de dernière connexion
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                
                // Générer le token JWT
                var roles = await _userManager.GetRolesAsync(user);
                var token = _jwtService.GenerateToken(user, roles);
                
                _logger.LogInformation("Utilisateur connecté: {Email}", user.Email);
                
                var response = new AuthResponseDto
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(60),
                    User = new UserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Roles = roles.ToList(),
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                };
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion de l'utilisateur");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
        
        /// <summary>
        /// Obtenir les informations de l'utilisateur connecté
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }
                
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Utilisateur non trouvé." });
                }
                
                var roles = await _userManager.GetRolesAsync(user);
                
                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };
                
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des informations utilisateur");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
        
        /// <summary>
        /// Changer le mot de passe de l'utilisateur connecté
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _userManager.FindByIdAsync(userId!);
                
                if (user == null)
                {
                    return NotFound(new { message = "Utilisateur non trouvé." });
                }
                
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                
                if (!result.Succeeded)
                {
                    return BadRequest(new { 
                        message = "Erreur lors du changement de mot de passe.",
                        errors = result.Errors.Select(e => e.Description).ToList()
                    });
                }
                
                _logger.LogInformation("Mot de passe changé pour l'utilisateur: {Email}", user.Email);
                
                return Ok(new { message = "Mot de passe changé avec succès." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du changement de mot de passe");
                return StatusCode(500, new { message = "Une erreur interne s'est produite." });
            }
        }
        
        /// <summary>
        /// Déconnexion (côté client, le token sera simplement supprimé)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // Avec JWT, la déconnexion se fait côté client en supprimant le token
            // Ici on peut logger l'événement ou faire du nettoyage si nécessaire
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            _logger.LogInformation("Utilisateur déconnecté: {Email}", userEmail);
            
            return Ok(new { message = "Déconnexion réussie." });
        }
        
        /// <summary>
        /// Méthode helper pour s'assurer qu'un rôle existe
        /// </summary>
        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}