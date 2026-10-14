using LaboratoryIdentity.Mvc.Models;
using LaboratoryIdentity.Mvc.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LaboratoryIdentity.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ---------------------------------------------------------------------
    // Registro
    // ---------------------------------------------------------------------

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    // Pantalla de "revisa tu correo" que se muestra después de registrarse.
    [HttpGet]
    public IActionResult RegisterConfirmation(string? email)
    {
        return View(new RegisterConfirmationViewModel { Email = email ?? string.Empty });
    }

    // Destino del enlace enviado por correo para confirmar la cuenta.
    // TODO: validar userId + code con _userManager.ConfirmEmailAsync y asignar Succeeded con el resultado.
    [HttpGet]
    public IActionResult ConfirmEmail(string? userId, string? code)
    {
        return View(new ConfirmEmailViewModel { Succeeded = true });
    }

    [HttpGet]
    public IActionResult ResendEmailConfirmation(string? email)
    {
        return View(new ResendEmailConfirmationViewModel { Email = email ?? string.Empty });
    }

    // ---------------------------------------------------------------------
    // Inicio de sesión
    // ---------------------------------------------------------------------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpGet]
    public IActionResult Lockout()
    {
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    // ---------------------------------------------------------------------
    // Recuperación de contraseña
    // ---------------------------------------------------------------------

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    // Destino del enlace enviado por correo para restablecer la contraseña.
    // TODO: devolver BadRequest() si code es null cuando la lógica esté lista.
    [HttpGet]
    public IActionResult ResetPassword(string? code, string? email)
    {
        return View(new ResetPasswordViewModel { Code = code ?? string.Empty, Email = email ?? string.Empty });
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    // ---------------------------------------------------------------------
    // Cambio de contraseña (usuario autenticado)
    // ---------------------------------------------------------------------

    // TODO: agregar [Authorize] cuando el login esté implementado.
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpGet]
    public IActionResult ChangePasswordConfirmation()
    {
        return View();
    }
}
