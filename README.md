# LaboratoryIdentity

Ejercicio práctico para implementar **ASP.NET Core Identity** en una aplicación **MVC** (.NET 10 + PostgreSQL), sin usar el scaffolding de Razor Pages que trae Identity por defecto.

## 🎯 Objetivo

El cascarón ya está listo: el modelo de usuario, la base de datos, los ViewModels, las vistas y los métodos `GET` del controlador que las muestran.

**Lo que falta es darle funcionalidad.** Hay que escribir la lógica para que todo el flujo de autenticación funcione de punta a punta y el módulo de Auth se pueda considerar consolidado:

- El registro crea usuarios de verdad.
- El usuario debe **confirmar su correo** antes de poder iniciar sesión.
- El login y el logout funcionan.
- La recuperación y el restablecimiento de la contraseña funcionan por correo.
- Un usuario autenticado puede cambiar su contraseña.
- Las rutas protegidas exigen sesión.

---

## 📦 Qué ya está hecho

| Pieza | Ubicación | Estado |
|---|---|---|
| Usuario personalizado (`FirstName`, `LastName`, `Adress`) | `Models/ApplicationUser.cs` | ✅ |
| `DbContext` con Identity + migración inicial | `Data/` | ✅ |
| Registro de Identity en el contenedor | `Program.cs` | ✅ (configuración básica) |
| ViewModels con validaciones en español | `ViewModels/Account/` | ✅ |
| Vistas de Account (Bootstrap 5 + Bootstrap Icons) | `Views/Account/` | ✅ |
| Menú de usuario en el navbar | `Views/Shared/_LoginPartial.cshtml` | ✅ |
| Métodos `GET` que muestran las vistas | `Controllers/AccountController.cs` | ✅ |
| Métodos `POST` y lógica de negocio | `Controllers/AccountController.cs` | ❌ **por hacer** |
| Servicio de envío de correos | `Services/` | ❌ **por hacer** |

### Vistas disponibles

| Ruta | Vista | Para qué sirve |
|---|---|---|
| `/Account/Register` | Registro | Formulario para crear una cuenta |
| `/Account/RegisterConfirmation` | "Revisa tu correo" | Aviso después de registrarse |
| `/Account/ConfirmEmail` | Resultado de la verificación | Destino del enlace que llega por correo (éxito o error) |
| `/Account/ResendEmailConfirmation` | Reenviar confirmación | Pedir un nuevo enlace de verificación |
| `/Account/Login` | Inicio de sesión | Incluye un aviso de "cuenta sin verificar" |
| `/Account/Lockout` | Cuenta bloqueada | Se muestra tras demasiados intentos fallidos |
| `/Account/AccessDenied` | Acceso denegado | Usuario autenticado pero sin permisos |
| `/Account/ForgotPassword` | Olvidé mi contraseña | Pedir el enlace de recuperación |
| `/Account/ForgotPasswordConfirmation` | "Revisa tu correo" | Aviso después de pedir la recuperación |
| `/Account/ResetPassword` | Nueva contraseña | Destino del enlace de recuperación |
| `/Account/ResetPasswordConfirmation` | Contraseña restablecida | Aviso de éxito |
| `/Account/ChangePassword` | Cambiar contraseña | Para el usuario autenticado |
| `/Account/ChangePasswordConfirmation` | Contraseña actualizada | Aviso de éxito |

### Utilidades que ya traen las vistas

- **Mensajes de estado:** cualquier vista de Account muestra una alerta si el controlador asigna:
  ```csharp
  TempData["StatusMessage"] = "Texto del mensaje";
  TempData["StatusType"] = "success"; // success | danger | warning | info
  ```
- **Errores de negocio:** `ModelState.AddModelError(string.Empty, "...")` se muestra en el resumen de validación que hay arriba de cada formulario.
- **Cuenta sin verificar:** si en el `POST` de Login se asigna `model.EmailNotConfirmed = true` y se devuelve `View(model)`, aparece un aviso con un enlace para reenviar la confirmación.

---

## ✅ Tareas del ejercicio

Cada tarea tiene **criterios de aceptación**. Se considera terminada cuando todos se cumplen.

### 1. Configuración de Identity (`Program.cs`)

- [ ] Configurar `IdentityOptions` dentro de `AddIdentity(options => ...)`:
  - `SignIn.RequireConfirmedAccount = true` (o `RequireConfirmedEmail`).
  - `User.RequireUniqueEmail = true`.
  - Reglas de contraseña (`Password.*`) **coherentes con los ViewModels**. Hoy piden un mínimo de 6 caracteres. Si cambias las reglas, actualiza también el `[StringLength]` y el texto de ayuda de la vista de registro.
  - Bloqueo de cuenta (`Lockout.MaxFailedAccessAttempts`, `Lockout.DefaultLockoutTimeSpan`).
- [ ] Configurar la cookie con `ConfigureApplicationCookie`: `LoginPath`, `LogoutPath`, `AccessDeniedPath`, expiración y `SlidingExpiration`.
- [ ] (Opcional) Ajustar la duración de los tokens con `DataProtectionTokenProviderOptions.TokenLifespan`.

### 2. Servicio de correo (`Services/`)

- [ ] Crear una abstracción (por ejemplo `IEmailSender`) y una implementación.
  - Para desarrollo basta con una implementación que **escriba el enlace en el log o la consola**, o que use un SMTP local como [Mailpit](https://github.com/axllent/mailpit) o [smtp4dev](https://github.com/rnwood/smtp4dev).
  - Las credenciales de SMTP van en **User Secrets** o en variables de entorno, nunca en el repositorio.
- [ ] Registrar el servicio en el contenedor.

**Criterios de aceptación:** el sistema puede "enviar" un correo y el desarrollador puede ver el enlace generado.

### 3. Registro + confirmación de correo

- [ ] `POST Register`:
  - Validar `ModelState`.
  - Mapear `RegisterViewModel` → `ApplicationUser` (`UserName = Email`).
  - `_userManager.CreateAsync(user, password)`. Si falla, pasar cada `IdentityError` a `ModelState`.
  - Generar el token con `GenerateEmailConfirmationTokenAsync`, codificarlo para URL (`WebEncoders.Base64UrlEncode`) y construir el enlace con `Url.Action(nameof(ConfirmEmail), ..., Request.Scheme)`.
  - Enviar el correo y redirigir a `RegisterConfirmation` con el email.
  - **No** iniciar sesión automáticamente.
- [ ] `GET ConfirmEmail`: decodificar el token, llamar a `ConfirmEmailAsync` y asignar `Succeeded` según el resultado. Si faltan `userId` o `code`, redirigir o devolver `BadRequest`.
- [ ] `POST ResendEmailConfirmation`: regenerar y reenviar el enlace. Responder lo mismo exista o no el correo (para no revelar qué cuentas existen).

**Criterios de aceptación**
- Un usuario nuevo queda en la base de datos con `EmailConfirmed = false`.
- El enlace del correo lo marca como confirmado.
- Un enlace alterado o vencido muestra la pantalla de error.
- Registrar dos veces el mismo correo muestra un error en el formulario.

### 4. Login + logout

- [ ] `POST Login`:
  - `PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true)`.
  - `Succeeded` → `LocalRedirect(returnUrl)` si es una URL local (`Url.IsLocalUrl`); si no, al `Home`.
  - `IsNotAllowed` → la cuenta no está confirmada: `model.EmailNotConfirmed = true` y volver a la vista.
  - `IsLockedOut` → redirigir a `Lockout`.
  - Otro caso → error genérico ("Correo o contraseña incorrectos"), sin indicar cuál de los dos falló.
- [ ] `POST Logout` (ya lo llama el navbar): `SignOutAsync()` y redirigir al inicio.
- [ ] Si un usuario ya autenticado entra a `Login` o `Register`, redirigirlo al inicio (opcional, pero recomendado).

**Criterios de aceptación**
- Un usuario sin confirmar **no puede** iniciar sesión y ve el aviso.
- Uno confirmado entra y su nombre aparece en el navbar.
- `returnUrl` funciona y no permite redirecciones abiertas a dominios externos.
- Tras N intentos fallidos la cuenta se bloquea.

### 5. Recuperación de contraseña

- [ ] `POST ForgotPassword`: si el usuario existe **y** tiene el correo confirmado, generar el token con `GeneratePasswordResetTokenAsync`, codificarlo y enviar el enlace a `ResetPassword`. **Siempre** redirigir a `ForgotPasswordConfirmation`, exista o no el usuario.
- [ ] `GET ResetPassword`: devolver `BadRequest` si no llega `code` (hoy hay un `TODO`).
- [ ] `POST ResetPassword`: decodificar el token y llamar a `ResetPasswordAsync`. Si tiene éxito, ir a `ResetPasswordConfirmation`; si falla, mostrar los errores en el formulario.

**Criterios de aceptación**
- El enlace permite poner una contraseña nueva y con ella se puede iniciar sesión.
- El mismo enlace no sirve dos veces (el security stamp cambia).
- La respuesta no revela si un correo está registrado.

### 6. Cambio de contraseña (usuario autenticado)

- [ ] Poner `[Authorize]` en `ChangePassword` y `ChangePasswordConfirmation` (hoy hay un `TODO`).
- [ ] `POST ChangePassword`: obtener el usuario con `GetUserAsync(User)`, llamar a `ChangePasswordAsync` y después a `RefreshSignInAsync` para que la sesión siga activa.

**Criterios de aceptación**
- Un anónimo que entra a `/Account/ChangePassword` es enviado al Login, y tras autenticarse vuelve a esa ruta.
- Una contraseña actual incorrecta muestra un error.
- Tras el cambio, la contraseña vieja deja de funcionar.

### 7. Seguridad y buenas prácticas (transversal)

- [ ] `[HttpPost]` + `[ValidateAntiForgeryToken]` en todos los POST, o un filtro global `AutoValidateAntiforgeryToken`.
- [ ] Nunca revelar si un correo existe (en el registro, la recuperación y el reenvío de confirmación).
- [ ] Tokens codificados en Base64Url en las URLs.
- [ ] Ningún secreto (cadena de conexión, SMTP) dentro del repositorio: usar User Secrets.
- [ ] Logging de los eventos importantes (registro, login fallido, bloqueo, cambio de contraseña).

---

## 🚀 Retos extra (opcionales)

- Roles (`Admin`, `User`) con un seeder inicial y una vista protegida con `[Authorize(Roles = "Admin")]`.
- Página de perfil para editar `FirstName`, `LastName`, `Adress` y `PhoneNumber`.
- Cambio de correo con reconfirmación (`GenerateChangeEmailTokenAsync`).
- Autenticación de dos factores (TOTP) con una app autenticadora.
- Login externo (Google o Microsoft).
- Pruebas de integración del flujo completo con `WebApplicationFactory`.

---

## 🛠️ Cómo ejecutar

**Requisitos:** .NET 10 SDK y PostgreSQL.

```bash
# 1. Restaurar la herramienta de EF Core (está definida en dotnet-tools.json)
dotnet tool restore

# 2. Configurar la cadena de conexión (recomendado: User Secrets)
cd src/LaboratoryIdentity.Mvc
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=LaboratoryIdentity;Username=postgres;Password=<tu-password>"

# 3. Aplicar las migraciones
dotnet ef database update

# 4. Ejecutar
dotnet run
```

## 📁 Estructura relevante

```
src/LaboratoryIdentity.Mvc/
├── Controllers/AccountController.cs   ← aquí va la lógica
├── Data/                              ← DbContext + migraciones
├── Models/ApplicationUser.cs
├── Services/                          ← aquí va el servicio de correo
├── ViewModels/Account/                ← un ViewModel por formulario
├── Views/Account/                     ← vistas + _AccountLayout + _StatusMessage
├── Views/Shared/_LoginPartial.cshtml
└── wwwroot/css/account.css, wwwroot/js/account.js
```
