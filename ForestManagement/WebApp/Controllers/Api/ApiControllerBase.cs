using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers.Api;

/// <summary>
/// Base class for all REST API controllers.
/// Enforces JWT Bearer authentication on every action unless the action
/// is explicitly decorated with <see cref="AllowAnonymousAttribute"/>.
/// Using an explicit scheme here means the ASP.NET Identity cookie scheme
/// (used by the MVC Admin area) is not overridden by a global default.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public abstract class ApiControllerBase : ControllerBase;
