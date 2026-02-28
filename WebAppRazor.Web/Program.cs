using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebAppRazor.BLL.Services;
using WebAppRazor.DAL.Data;
using WebAppRazor.DAL.Repositories;
using WebAppRazor.Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Configure SQL Server Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register DAL
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IHealthProfileRepository, HealthProfileRepository>();
builder.Services.AddScoped<IMealPlanRepository, MealPlanRepository>();
builder.Services.AddScoped<IMealReviewRepository, MealReviewRepository>();
builder.Services.AddScoped<IProgressRepository, ProgressRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Register BLL
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHealthProfileService, HealthProfileService>();
builder.Services.AddScoped<IMealPlanService, MealPlanService>();
builder.Services.AddScoped<IMealReviewService, MealReviewService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Auto-migrate database: handle both new and existing databases
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Check if database already has Users table (existing DB)
    bool dbExists = false;
    try
    {
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users'";
        var result = cmd.ExecuteScalar();
        dbExists = result != null && Convert.ToInt32(result) > 0;
        conn.Close();
    }
    catch { /* DB does not exist yet */ }

    if (!dbExists)
    {
        // Fresh database - create all tables
        db.Database.EnsureCreated();
    }
    else
    {
        // Existing database - add missing columns and tables via raw SQL
        var conn2 = db.Database.GetDbConnection();
        conn2.Open();

        void ExecuteSql(string sql)
        {
            try
            {
                using var c = conn2.CreateCommand();
                c.CommandText = sql;
                c.ExecuteNonQuery();
            }
            catch { /* Column/table already exists, skip */ }
        }

        // Add new columns to Users table
        ExecuteSql("ALTER TABLE [Users] ADD [SubscriptionTier] nvarchar(20) NOT NULL DEFAULT 'Free'");
        ExecuteSql("ALTER TABLE [Users] ADD [SubscriptionExpiresAt] datetime2 NULL");
        ExecuteSql("ALTER TABLE [Users] ADD [ReviewPoints] int NOT NULL DEFAULT 0");

        // Create HealthProfiles table
        ExecuteSql(@"CREATE TABLE [HealthProfiles] (
            [Id] int NOT NULL IDENTITY,
            [UserId] int NOT NULL,
            [Age] int NOT NULL,
            [Gender] nvarchar(10) NOT NULL DEFAULT '',
            [Height] float NOT NULL,
            [Weight] float NOT NULL,
            [ActivityLevel] nvarchar(30) NOT NULL DEFAULT '',
            [Goal] nvarchar(20) NOT NULL DEFAULT '',
            [BMI] float NOT NULL,
            [BMR] float NOT NULL,
            [TDEE] float NOT NULL,
            [DailyCalorieTarget] float NOT NULL,
            [CreatedAt] datetime2 NOT NULL,
            CONSTRAINT [PK_HealthProfiles] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_HealthProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
        )");

        // Create MealPlans table
        ExecuteSql(@"CREATE TABLE [MealPlans] (
            [Id] int NOT NULL IDENTITY,
            [UserId] int NOT NULL,
            [Title] nvarchar(200) NOT NULL DEFAULT '',
            [TargetCalories] float NOT NULL,
            [PlanDate] datetime2 NOT NULL,
            [CreatedAt] datetime2 NOT NULL,
            CONSTRAINT [PK_MealPlans] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_MealPlans_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
        )");

        // Create MealItems table
        ExecuteSql(@"CREATE TABLE [MealItems] (
            [Id] int NOT NULL IDENTITY,
            [MealPlanId] int NOT NULL,
            [MealType] nvarchar(20) NOT NULL DEFAULT '',
            [Name] nvarchar(200) NOT NULL DEFAULT '',
            [Description] nvarchar(max) NOT NULL DEFAULT '',
            [Calories] float NOT NULL,
            [Protein] float NOT NULL,
            [Carbs] float NOT NULL,
            [Fat] float NOT NULL,
            [Ingredients] nvarchar(max) NOT NULL DEFAULT '',
            [CookingInstructions] nvarchar(max) NOT NULL DEFAULT '',
            CONSTRAINT [PK_MealItems] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_MealItems_MealPlans_MealPlanId] FOREIGN KEY ([MealPlanId]) REFERENCES [MealPlans] ([Id]) ON DELETE CASCADE
        )");

        // Create MealReviews table
        ExecuteSql(@"CREATE TABLE [MealReviews] (
            [Id] int NOT NULL IDENTITY,
            [UserId] int NOT NULL,
            [MealItemId] int NOT NULL,
            [Rating] int NOT NULL,
            [Comment] nvarchar(max) NOT NULL DEFAULT '',
            [PointsEarned] int NOT NULL,
            [CreatedAt] datetime2 NOT NULL,
            CONSTRAINT [PK_MealReviews] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_MealReviews_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
            CONSTRAINT [FK_MealReviews_MealItems_MealItemId] FOREIGN KEY ([MealItemId]) REFERENCES [MealItems] ([Id]) ON DELETE NO ACTION
        )");

        // Create ProgressEntries table
        ExecuteSql(@"CREATE TABLE [ProgressEntries] (
            [Id] int NOT NULL IDENTITY,
            [UserId] int NOT NULL,
            [Weight] float NOT NULL,
            [BMI] float NOT NULL,
            [BMR] float NOT NULL,
            [TDEE] float NOT NULL,
            [Notes] nvarchar(max) NOT NULL DEFAULT '',
            [RecordedAt] datetime2 NOT NULL,
            CONSTRAINT [PK_ProgressEntries] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_ProgressEntries_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
        )");

        // Create Notifications table
        ExecuteSql(@"CREATE TABLE [Notifications] (
            [Id] int NOT NULL IDENTITY,
            [UserId] int NOT NULL,
            [Title] nvarchar(200) NOT NULL DEFAULT '',
            [Message] nvarchar(max) NOT NULL DEFAULT '',
            [Type] nvarchar(50) NOT NULL DEFAULT '',
            [IsRead] bit NOT NULL,
            [CreatedAt] datetime2 NOT NULL,
            CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
        )");

        conn2.Close();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
