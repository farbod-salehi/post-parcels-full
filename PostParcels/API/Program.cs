using API.Helpers;
using Azure.Core;
using Entities;
using Entities.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Repository;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

IWebHostEnvironment environment = builder.Environment;

string connectionString = environment.IsDevelopment() ?
                Constances.DevelopmentConnectionString :
                Constances.ProductionConnectionString;

string uploadsFolderPath = Path.Combine(environment.ContentRootPath, "Uploads");


#region Services

builder.Services.AddDbContext<RepositoryContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("API").EnableRetryOnFailure()));

//In Minimal APIs, authentication is handled using the IAuthenticationService rather than AuthenticationManager
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = Constances.JWTSettings.Issuer;
        options.Audience = Constances.JWTSettings.Audience;
    });

builder.Services.AddAuthorization();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
});

;

// To validate a username and password using UserManager<IdentityUser> from ASP.NET Core Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<RepositoryContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CORSPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin => true).
                            AllowAnyMethod().
                            AllowAnyHeader().
                            AllowCredentials();
    });
});

// To access HttpContext
builder.Services.AddHttpContextAccessor();

// Add anti-forgery services
builder.Services.AddAntiforgery();

//These will be created once per request and shared within that request lifecycle.
builder.Services.AddScoped<RepositoryManager>();
builder.Services.AddScoped<TokenManager>();
builder.Services.AddScoped<SignInManager<User>>();
builder.Services.AddSingleton<ILookupNormalizer, UpperInvariantLookupNormalizer>();

#endregion Services

// We should call this after adding all services
var app = builder.Build();

#region  Configuration of HTTP request pipeline

app.UseCors("CORSPolicy");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Enable serving static files from the Uploads folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsFolderPath),
    RequestPath = "/Uploads"
});

// Custom exception handling middleware
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new
        {
            Error = "خطایی در سمت سرور اتفاق افتاده است.",
            Details = ex.Message
        });
    }
});

#endregion Configuration of HTTP request pipeline



#region My Account Endpoints


app.MapPost("/api/login", async ([FromServices] UserManager<User> userManager, [FromServices] RepositoryManager repositoryManager, [FromServices] SignInManager<User> signInManager, [FromBody] LoginRequest request) =>
{
    var user = await userManager.FindByNameAsync(request.Username);

    if (user == null)
    {
        return Results.Json(new { Error = "نام کاربری یا کلمه عبور اشتباه است" }, statusCode: 401);
    }

    MyUtility utility = new();

    if (user.PasswordHash != utility.ComputeSha256Hash(request.Password))
    {
        return Results.Json(new { Error = "نام کاربری یا کلمه عبور اشتباه است" }, statusCode: 401);
    }

    if (user.Active == false)
    {
        return Results.UnprocessableEntity(new { Error = "این حساب کاربری غیرفعال است." });
    }

    TokenManager tokenManager = new();

    string token = tokenManager.GenerateJWTToken(user);

    string hashedToken = utility.ComputeSha256Hash(token);

    var userToken = new UserToken
    {
        ExpiredAt = DateTime.UtcNow.AddDays(Constances.JWTSettings.ValidDays),
        HashedToken = hashedToken,
        UserId = user.Id,
    };

    repositoryManager.UserToken.Create(userToken);

    await repositoryManager.SaveAsync();

    return Results.Ok(new { UserTitle = user.Title, UserRole = user.Role, UserToken = token });
});

app.MapPost("/api/signout", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] TokenManager tokenManager, [FromServices] RepositoryManager repositoryManager, [FromServices] SignInManager<User> signInManager) => {
    
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess)
    {
        string token = tokenManager.GetJWTToken(httpContextAccessor.HttpContext!.Request);

        string hashedToken = utility.ComputeSha256Hash(token);

        UserToken? userToken = await repositoryManager.UserToken.GetByToken(true, hashedToken);

        if(userToken != null)
        {
            repositoryManager.UserToken.Delete(userToken);

            await repositoryManager.SaveAsync();
        }
    }

    return Results.NoContent();
});

app.MapPatch("/api/changepassword", async ([FromServices] UserManager<User> userManager, [FromServices] TokenManager tokenManager, [FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromBody] ChangeMyPasswordRequest request) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    if (string.IsNullOrWhiteSpace(request.NewPassword) || 
        string.IsNullOrWhiteSpace(request.ConfirmPassword) || 
        string.IsNullOrWhiteSpace(request.CurrentPassword))
    {
        return Results.BadRequest(new { Error = "همه اطلاعات مورد نیاز را وارد نمایید." });
    }

    if (request.NewPassword != request.ConfirmPassword)
        return Results.BadRequest(new { Error = "کلمه عبور جدید و تکرار آن باید یکسان باشند." });

    if (request.NewPassword.Length < Constances.UserPasswordMinLength)
        return Results.BadRequest(new { Error = "طول کلمه عبور جدید صحیح نمی‌باشد." });

    string? userId = tokenManager.GetUserIdFromTokenClaims(httpContextAccessor.HttpContext!);

    User? user = await repositoryManager.User.GetById(true, userId!);

    if(user == null)
    {
        return Results.NotFound(new { Error = "داده موردنظر یافت نشد." });
    }

    if(user.PasswordHash != utility.ComputeSha256Hash(request.CurrentPassword))
    {
        return Results.BadRequest(new { Error = "کلمه عبور فعلی صحیح نمی‌باشد." });
    }

    user.PasswordChangedAt = DateTime.UtcNow;
    user.PasswordHash = utility.ComputeSha256Hash(request.NewPassword);

    await repositoryManager.SaveAsync();


    return Results.NoContent();

});

#endregion  My Account Endpoints

#region User Endpoints

app.MapPost("/api/users/add", async ([FromServices] UserManager<User> userManager, [FromServices] TokenManager tokenManager, [FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromBody] AddUserRequest request) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, [(int)Constances.UserRole.admin]);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    if (string.IsNullOrWhiteSpace(request.Username) ||
    string.IsNullOrWhiteSpace(request.Password) ||
    string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(new { Error = "همه اطلاعات مورد نیاز را وارد نمایید." });
    }

    if (request.Password.Length < Constances.UserPasswordMinLength)
        return Results.BadRequest(new { Error = "طول کلمه عبور صحیح نمی‌باشد." });

    if (request.Role != (int)Constances.UserRole.manager && request.Role != (int)Constances.UserRole.user)
        return Results.BadRequest(new { Error = "نقش کاربر به درستی مشخص نشده است." });

    string username = utility.CorrectArabicChars(request.Username.Trim())!;

    var user = await userManager.FindByNameAsync(username);

    if (user != null)
        return Results.UnprocessableEntity(new { Error = "این نام کاربری قبلا ثبت شده است" });


    var newUser = new User
    {
        Active = true,
        Title = utility.CorrectArabicChars(request.Title)!,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = tokenManager.GetUserIdFromTokenClaims(httpContextAccessor.HttpContext!),
        PasswordHash = utility.ComputeSha256Hash(request.Password),
        UserName = username,
        Role = request.Role
    };


    await userManager.CreateAsync(newUser);

    return Results.Created($"/api/users/{newUser.Id}", new {});

});

app.MapGet("/api/users", async ([FromServices] UserManager <User> userManager, [FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, string? query = null, int? page = 1, int? count = 10) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, [(int)Constances.UserRole.admin]);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    (List<User> list, int pagesCount) = await repositoryManager.User.GetList(false, (int)Constances.UserRole.admin, query, page ?? 1, count ?? 10);

    return Results.Ok(new
    {
        list = list.Select(x => new
        {
            x.Id,
            x.UserName,
            x.Title,
            x.Active,
            Role = Enum.GetName(typeof(Constances.UserRole), x.Role)
        }).ToList(),
        pagesCount
    });
});

app.MapGet("/api/users/{id}", async ([FromServices] UserManager<User> userManager, [FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromRoute]string id) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, [(int)Constances.UserRole.admin]);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    User? user = await repositoryManager.User.GetById(false, id);

    if(user == null)
    {
        return Results.NotFound(new { Error ="داده موردنظر یافت نشد." });
    }

    return Results.Ok(new {user.Role, user.Title, user.UserName});
});

app.MapPatch("/api/users/{id}/update", async ([FromServices] UserManager<User> userManager, [FromServices] TokenManager tokenManager, [FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager,[FromRoute]string id, [FromBody] UpdateUserRequest request) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, [(int)Constances.UserRole.admin]);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(new { Error = "همه اطلاعات مورد نیاز را وارد نمایید." });
    }

    if (request.Password != null && request.Password.Length < Constances.UserPasswordMinLength)
        return Results.BadRequest(new { Error = "طول کلمه عبور صحیح نمی‌باشد." });

    if (request.Role != (int)Constances.UserRole.manager && request.Role != (int)Constances.UserRole.user)
        return Results.BadRequest(new { Error = "نقش کاربر به درستی مشخص نشده است." });

    User? user = await repositoryManager.User.GetById(true, id!, (int)Constances.UserRole.admin);

    if (user == null)
    {
        return Results.NotFound(new { Error = "داده موردنظر یافت نشد." });
    }

    user.Title = utility.CorrectArabicChars(request.Title)!;
    user.Role = request.Role;

    if (request.Password != null)
    {
        user.PasswordChangedAt = DateTime.UtcNow;
        user.PasswordHash = utility.ComputeSha256Hash(request.Password);
    }

    await repositoryManager.SaveAsync();

    return Results.NoContent();

});

#endregion User Endpoints

#region Parcel Endpoints

app.MapGet("/api/parcels",async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, string? query = null, int? page = 1, int? count = 10) =>
{
    
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    (List<Parcel> list, int pagesCount) = await repositoryManager.Parcel.GetList(false, query, page ?? 1, count ?? 10, x => x.Include(y => y.ParcelItems));

    return Results.Ok(new
    {
        list = list.Select(x=> new
        {
            x.Id,
            x.Code,
            ItemsCount = x.ParcelItems?.Where(x=>x.DeletedAt == null).Count() ?? 0,
            CreatedAt = x.CreatedAt != null ? new PersianDateTime((DateTime)x.CreatedAt!).GetShortDateTime() : default,
        }),
        pagesCount
    });
});

app.MapGet("/api/parcels/{id}", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromRoute] Guid id) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    Parcel? parcel = await repositoryManager.Parcel.GetById(false, id);

    if (parcel == null)
    {
        return Results.NotFound(new { Error = "داده موردنظر یافت نشد." });
    }

    return Results.Ok(new
    {
        parcel.Code,
        list = parcel.ParcelItems?.Where(x => x.DeletedAt == null).Select( x=> new { 
            x.PacketNumber, 
            x.Type,
            TypeName = Constances.GetParcelItemTypeTitle(x.Type),
            x.Code, 
            x.Description, 
            x.SenderId, 
            x.ReceiverId,
            SenderTitle = x.SenderUnit?.Name,
            ReceiverTitle = x.ReceiverUnit?.Name,
            x.UndefinedReceiverTitle, 
            x.UndefinedSenderTitle}).ToList()
    });
});

app.MapPost("/api/parcels/add", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromServices] TokenManager tokenManager, [FromBody] List<ParcelItemRequest> request) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager,null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }


    if (request == null || request.Count == 0)
    {
        return Results.BadRequest(new { Error = "هر پکیج باید حداقل یک مرسوله داشته باشد." });
    }


    string parcelCodeBeginning = new PersianDateTime(DateTime.Now).GetShortDate(false).Replace("/", "");

    int todayParcelsCount = await repositoryManager.Parcel.GetParcelsCountWithCodePattern(parcelCodeBeginning);

    string? userId = tokenManager.GetUserIdFromTokenClaims(httpContextAccessor.HttpContext!);

    string parcelCode = $"{parcelCodeBeginning}/{1000 + (++todayParcelsCount)}";
    
    Parcel newParcel = new()
    {
        Code = parcelCode,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = userId,
    };

    repositoryManager.Parcel.Create(newParcel);



    foreach (var item in request!)
    {

        if (string.IsNullOrWhiteSpace(item.Code))
        {
            return Results.BadRequest(new { Error = "شناسه هر مرسوله باید مشخص باشد." });
        }

        if (item.Type != (int)Constances.ParcelItemType.Letter &&
          item.Type != (int)Constances.ParcelItemType.Case &&
          item.Type != (int)Constances.ParcelItemType.Docs &&
          item.Type != (int)Constances.ParcelItemType.Zonekan &&
          item.Type != (int)Constances.ParcelItemType.Carton &&
          item.Type != (int)Constances.ParcelItemType.Sack)
        {
            return Results.BadRequest(new { Error = "نوع هر مرسوله باید صحیح ‌باشد." });
        }

        if ((item.SenderId == null || item.SenderId == default) && string.IsNullOrWhiteSpace(item.UndefinedSenderTitle))
        {
            return Results.BadRequest(new { Error = "فرستنده هر مرسوله باید مشخص باشد." });
        }

        if ((item.ReceiverId == null || item.ReceiverId == default) && string.IsNullOrWhiteSpace(item.UndefinedReceiverTitle))
        {
            return Results.BadRequest(new { Error = "گیرنده هر مرسوله باید مشخص باشد." });
        }

        ParcelItem newParcelItem = new()
        {
            Code = item.Code,
            Description = utility.CorrectArabicChars(item.Description),
            PacketNumber = item.PacketNumber,
            Parcel = newParcel,
            ReceiverId = item.ReceiverId,
            SenderId = item.SenderId,
            Type = item.Type,
            UndefinedReceiverTitle = item.UndefinedReceiverTitle,
            UndefinedSenderTitle = item.UndefinedSenderTitle,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        repositoryManager.ParcelItem.Create(newParcelItem);

    }


    await repositoryManager.SaveAsync();

    return Results.Created($"/api/parcels/{newParcel.Id}", new { ParcelCode = parcelCode, ItemsCount = request.Count, newParcel.Id });

});

app.MapPatch("/api/parcels/{id}/update", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromServices] TokenManager tokenManager, [FromRoute] Guid id, [FromBody] List<ParcelItemRequest> request) => {

    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    if (request == null || request.Count == 0)
    {
        return Results.BadRequest(new { Error = "هر پکیج باید حداقل یک مرسوله داشته باشد." });
    }

    Parcel? parcel = await repositoryManager.Parcel.GetById(true, id);

    if (parcel == null)
    {
        return Results.NotFound(new { Error = "داده موردنظر یافت نشد." });
    }

    string? userId = tokenManager.GetUserIdFromTokenClaims(httpContextAccessor.HttpContext!);

    foreach (var item in parcel.ParcelItems!)
    {
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = userId;
    }    

    foreach (var item in request!)
    {

        if (string.IsNullOrWhiteSpace(item.Code))
        {
            return Results.BadRequest(new { Error = "شناسه هر مرسوله باید مشخص باشد." });
        }

        if (item.Type != (int)Constances.ParcelItemType.Letter &&
            item.Type != (int)Constances.ParcelItemType.Case &&
            item.Type != (int)Constances.ParcelItemType.Docs &&
            item.Type != (int)Constances.ParcelItemType.Zonekan &&
            item.Type != (int)Constances.ParcelItemType.Carton &&
            item.Type != (int)Constances.ParcelItemType.Sack)
        {
            return Results.BadRequest(new { Error = "نوع هر مرسوله باید صحیح ‌باشد." });
        }

        if ((item.SenderId == null || item.SenderId == default) && string.IsNullOrWhiteSpace(item.UndefinedSenderTitle))
        {
            return Results.BadRequest(new { Error = "فرستنده هر مرسوله باید مشخص باشد." });
        }

        if ((item.ReceiverId == null || item.ReceiverId == default) && string.IsNullOrWhiteSpace(item.UndefinedReceiverTitle))
        {
            return Results.BadRequest(new { Error = "گیرنده هر مرسوله باید مشخص باشد." });
        }

        ParcelItem newParcelItem = new()
        {
            Code = item.Code,
            Description = utility.CorrectArabicChars(item.Description),
            PacketNumber = item.PacketNumber,
            ParcelId = parcel.Id,
            ReceiverId = item.ReceiverId,
            SenderId = item.SenderId,
            Type = item.Type,
            UndefinedReceiverTitle = item.UndefinedReceiverTitle,
            UndefinedSenderTitle = item.UndefinedSenderTitle,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        repositoryManager.ParcelItem.Create(newParcelItem);

    }

   

    await repositoryManager.SaveAsync();

    return Results.Ok(new { ParcelId = id, ItemsCount = request.Count });
});

app.MapGet("/api/parcels/items", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, string? query = null, int page = 1, int count = 10) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    (List<ParcelItem> list, int pagesCount) = await repositoryManager.ParcelItem.GetList(false, query, page , count);

    return Results.Ok(new
    {
        list = list.Select(x => new
        {
            x.Id,
            x.Code,
            Type = Constances.GetParcelItemTypeTitle(x.Type),
            ParcelCode = x.Parcel!.Code,
            Sender = x.SenderUnit != null ? x.SenderUnit.Name : x.UndefinedSenderTitle,
            Receiver = x.ReceiverUnit != null ? x.ReceiverUnit.Name : x.UndefinedReceiverTitle,
            CreatedAt = x.CreatedAt != null ? new PersianDateTime((DateTime)x.CreatedAt!).GetShortDateTime() : default,
            x.ParcelId,
        }),
        pagesCount
    });

});

app.MapGet("/api/parcels/statistic", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, string? from = null, string? to = null) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    from = from?.PadRight(8, '0');
    to = to?.PadRight(8, '9');

    (List<(string date, int count)> list, int pagesCount) = await repositoryManager.Parcel.GetStatistic(false, from, to);

    return Results.Ok(new
    {
        list = list.Select((x, index) => new 
        { 
            Id = index + 1, 
            Date = x.date, 
            Count = x.count 
        }),
        pagesCount
    });

});

#endregion Parcel Endpoints

#region Parcel Documents Endpoints

app.MapPost("/api/parcels/{id}/documents/add", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] TokenManager tokenManager, [FromServices] RepositoryManager repositoryManager, [FromRoute]Guid id, HttpRequest request) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    IFormFile? doc = request.Form.Files["image"];

    if(doc == null)
    {
        return Results.BadRequest(new {Error = "فایلی دریافت نشد."});
    }

    string? extension = Path.GetExtension(doc.FileName)?.ToLower();

    if (extension == null || 
            (Constances.GetImagesValidExtensions().Contains(extension) == false && Constances.GetDocumentValidExtensions().Contains(extension) == false))
    {
        return Results.BadRequest(new { Error = "فرمت فایل ارسالی صحیح نمی‌باشد." });
    }
        

    if (doc.Length > 1024 * 1024) // Max: 1 MB
    {
        return Results.BadRequest(new { Error = "حداکثر حجم مجاز فایل ۱ مگابایت است." });
    }

    Parcel? parcel = await repositoryManager.Parcel.GetById(false, id);

    if (parcel == null)
    {
        return Results.UnprocessableEntity(new { Error = "داده موردنظر یافت نشد." });
    }

    bool isImage = Constances.GetImagesValidExtensions().Contains(extension);

    string docName = $"{Path.GetRandomFileName()}{(isImage ? ".jpg" : extension)}";

    var directoryPath = $"{environment.ContentRootPath}\\Uploads\\Parcels\\{id}";

    if (Directory.Exists(directoryPath) == false)
    {
        Directory.CreateDirectory(directoryPath);
    }

    string docPath = directoryPath + "\\" + docName;

    if(isImage)
    {
        using var stream = doc.OpenReadStream();
        using var imageLoad = Image.Load(stream);
        imageLoad.Save(docPath, new JpegEncoder { Quality = 85 });
    }
    else
    {
        using var fileStream = new FileStream(docPath, FileMode.Create);
        await doc.CopyToAsync(fileStream);
    }

    string userId = tokenManager.GetUserIdFromTokenClaims(httpContextAccessor.HttpContext!)!;


    ParcelDocument parcelDocument = new()
    {
        CreatedAt = DateTime.UtcNow,
        CreatedBy = userId,
        FileName = docName,
        ParcelId = id,
    };

    repositoryManager.ParcelDocument.Create(parcelDocument);

    await repositoryManager.SaveAsync();

    return Results.Ok(new { Path = $"Uploads/Parcels/{id}/{docName}", parcelDocument.Id });

});

app.MapGet("/api/parcels/{id}/documents", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] TokenManager tokenManager, [FromServices] RepositoryManager repositoryManager, [FromRoute] Guid id) => {

    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    Parcel? parcel = await repositoryManager.Parcel.GetById(false, id);

    if (parcel == null)
    {
        return Results.UnprocessableEntity(new { Error = "داده موردنظر یافت نشد." });
    }

    List<ParcelDocument> parcelDocuments = await repositoryManager.ParcelDocument.GetAll(false, id);

    return Results.Ok(new
    {
        List = parcelDocuments.Select(x => new {x.Id, Path = $"Uploads/Parcels/{id}/{x.FileName}" }).ToList(),
        ParcelCode = parcel.Code
    });

});

app.MapDelete("/api/parcels/documents/{id}", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] TokenManager tokenManager, [FromServices] RepositoryManager repositoryManager, [FromRoute] Guid id) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    ParcelDocument? parcelDocument = await repositoryManager.ParcelDocument.GetById(true, id);

    if (parcelDocument == null)
    {
        return Results.UnprocessableEntity(new { Error = "داده موردنظر یافت نشد." });
    }

    string userId = tokenManager.GetUserIdFromTokenClaims(httpContextAccessor.HttpContext!)!;

    parcelDocument.DeletedAt = DateTime.UtcNow;
    parcelDocument.DeletedBy = userId;

    await repositoryManager.SaveAsync();

    return Results.NoContent();
});

#endregion Parcel Documents Endpoints

#region Unit Endpoints

app.MapGet("/api/units", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, string? parentId = null, string? name = null, bool? all = null, bool onlyParents = true, bool? active = null, int? page = 1, int? count = 10) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, null);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    List<Unit> list;
    int pagesCount = 1;

    if (all == true)
    {
        list = await repositoryManager.Unit.GetAll(false, onlyParents, true);
    }
    else
    {
        (list, pagesCount) = await repositoryManager.Unit.GetList(false, utility.CorrectArabicChars(name), parentId == null ? null : Guid.Parse(parentId!), active, page ?? 1, count ?? 10);
    }

    return Results.Ok(new
    {
        list = list.Select(x => new
        {
            x.Id,
            x.Code,
            x.Name,
            x.ParentId,
            x.Active,
            CreatedAt = x.CreatedAt != null ? new PersianDateTime((DateTime)x.CreatedAt!) : default
        }),
        pagesCount
    });
});

app.MapGet("/api/units/{id}", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromRoute]Guid id) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, [(int)Constances.UserRole.admin, (int)Constances.UserRole.manager]);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    Unit? unit = await repositoryManager.Unit.GetById(false, id);

    if(unit == null)
    {
        return Results.NotFound(new { Error = "داده موردنظر یافت نشد." });
    }

    return Results.Ok(new
    {
       unit.Id,
       unit.Code,
       unit.Name,
       unit.ParentId,
       unit.Active
    });
});

app.MapPost("/api/units/add", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromServices] TokenManager tokenManager, [FromBody] UnitItemRequest request) =>
{
    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, [(int)Constances.UserRole.admin, (int)Constances.UserRole.manager]);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { Error = "همه اطلاعات مورد نیاز را وارد نمایید." });
    }

    string code = request.Code.Trim();

    Unit? unit = await repositoryManager.Unit.GetByCode(false, code);

    if (unit != null)
    {
        return Results.UnprocessableEntity(new { Error = "این کد قبلا مورد استفاده قرار گرفته است." });
    }


    Unit newUnit = new()
    {
        Active = request.Active,
        ParentId = request.ParentId,
        Code = code,
        Name = utility.CorrectArabicChars(request.Name)!,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = tokenManager.GetUserIdFromTokenClaims(httpContextAccessor.HttpContext!)
    };


    repositoryManager.Unit.Create(newUnit);

    await repositoryManager.SaveAsync();

    return Results.Created($"/api/units/{newUnit.Id}",new { newUnit.Id, newUnit.Code, newUnit.Name });

});

app.MapPatch("/api/units/{id}/update", async ([FromServices] IHttpContextAccessor httpContextAccessor, [FromServices] RepositoryManager repositoryManager, [FromServices] TokenManager tokenManager,[FromRoute] Guid id, [FromBody] UnitItemRequest request) => {

    MyUtility utility = new();

    UserRequestAccessResult userRequestAccessResult = await utility.CheckUserAccess(httpContextAccessor.HttpContext!, repositoryManager, [(int)Constances.UserRole.admin,(int)Constances.UserRole.manager]);

    if (userRequestAccessResult.HasAccess == false)
    {
        return Results.Json(new { userRequestAccessResult.Error, userRequestAccessResult.Act }, statusCode: userRequestAccessResult.StatusCode);
    }

    if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { Error = "همه اطلاعات مورد نیاز را وارد نمایید." });
    }

    Unit? unit = await repositoryManager.Unit.GetById(true, id);

    if (unit == null)
    {
        return Results.NotFound(new { Error = "داده موردنظر یافت نشد." });
    }

    string code = request.Code.Trim();

    if (unit.Code != code)
    {
        Unit? unitByCode = await repositoryManager.Unit.GetByCode(false, code);

        if (unitByCode != null && unitByCode.Id != unit.Id)
        {
            return Results.UnprocessableEntity(new { Error = "این کد قبلا مورد استفاده قرار گرفته است." });
        }
    }

    if (request.ParentId != null && (unit.SubUnits?.Count ?? 0) > 0)
    {
        return Results.UnprocessableEntity(new { Error ="این مرکز دارای زیرمجموعه است و امکان قرارگیری در زیر مرکز دیگری را ندارد" });
    }

    unit.Code = request.Code;
    unit.Name = request.Name;
    unit.ParentId = request.ParentId;
    unit.Active = request.Active;

    await repositoryManager.SaveAsync();

    return Results.NoContent();
});

#endregion Unit Endpoints

app.MapGet("/api/test", () => { return Results.Ok("Hello Farbod!"); });


app.Run();

public record UserRequestAccessResult(bool HasAccess, string? Error = null, string? Act = null, int? StatusCode = null);
file record LoginRequest(string Username, string Password);
file record AddUserRequest(string Username, string Password, string Title, int Role);
file record UpdateUserRequest(string? Password, string Title, int Role);
file record ChangeMyPasswordRequest(string NewPassword, string ConfirmPassword, string CurrentPassword);
file record UnitItemRequest(string Name, string Code, Guid? ParentId, bool Active);
file record ParcelItemRequest(string Code, int PacketNumber, int Type, Guid? SenderId, string? UndefinedSenderTitle, Guid? ReceiverId, string? UndefinedReceiverTitle, string? Description);
