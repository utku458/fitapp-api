using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using FitApp.Data;
using FitApp.Services;
using FitApp.Helpers;
using FitApp.Models;
using FitApp.Options;
using System.Text.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5270");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Swagger with JWT authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "FitApp API", 
        Version = "v1",
        Description = "FitApp API with JWT Authentication"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Add XML comments support (optional)
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21)) // Use fixed version instead of AutoDetect to avoid connection on startup
    ));

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not found in configuration");
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// Add Configuration Options
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.Configure<EmailFromOptions>(builder.Configuration.GetSection("Email:From"));

// Add Memory Cache for Rate Limiting
builder.Services.AddMemoryCache();

// Add Services
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();

// Add CurrentUserService for Global Query Filter
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Database migration and seeding - wrapped in try-catch to prevent startup failure
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate(); // garanti

    if (!await db.FoodItems.AnyAsync())
    {
        db.FoodItems.AddRange(
            new FoodItem { Name="Tavuk Göğsü", CaloriesPer100g=110, ProteinPer100g=23, CarbsPer100g=0, FatPer100g=1, Locale="tr-TR", Category="Et", PortionName="100 g", PortionGrams=100 },
            new FoodItem { Name="Pirinç Pilavı", CaloriesPer100g=130, ProteinPer100g=2, CarbsPer100g=28, FatPer100g=0, Locale="tr-TR", Category="Tahıl", PortionName="100 g", PortionGrams=100 },
            new FoodItem { Name="Yumurta", CaloriesPer100g=155, ProteinPer100g=13, CarbsPer100g=1, FatPer100g=11, Locale="tr-TR", Category="Süt & Yumurta", PortionName="1 adet (50 g)", PortionGrams=50 },
            new FoodItem { Name="Yoğurt (Sade)", CaloriesPer100g=60, ProteinPer100g=4, CarbsPer100g=5, FatPer100g=3, Locale="tr-TR", Category="Süt Ürünleri", PortionName="100 g", PortionGrams=100 },
            new FoodItem { Name="Zeytinyağı", CaloriesPer100g=884, ProteinPer100g=0, CarbsPer100g=0, FatPer100g=100, Locale="tr-TR", Category="Yağlar", PortionName="1 yemek kaşığı (10 g)", PortionGrams=10 }
        );
        await db.SaveChangesAsync();
    }

    // Seed Exercises if empty
    if (!await db.Exercises.AnyAsync())
    {
        db.Exercises.AddRange(
            new Exercise { 
                Name = "Squat", 
                Description = "Compound lower body exercise targeting quads, hamstrings, and glutes", 
                MuscleGroup = "Legs", 
                Equipment = "Barbell", 
                Sets = 4, 
                Reps = "8-12", 
                RestTime = "2-3 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Stand with feet shoulder-width apart", "Lower your body as if sitting back into a chair", "Keep your chest up and knees behind toes", "Return to starting position" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Keep your core tight", "Don't let knees cave in", "Go as deep as your mobility allows" })
            },
            new Exercise { 
                Name = "Bench Press", 
                Description = "Compound chest exercise for building upper body strength", 
                MuscleGroup = "Chest", 
                Equipment = "Barbell", 
                Sets = 4, 
                Reps = "8-12", 
                RestTime = "2-3 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Lie on bench with feet flat on ground", "Grip bar slightly wider than shoulder width", "Lower bar to chest with control", "Press bar back up to starting position" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Keep your shoulder blades retracted", "Don't bounce the bar off your chest", "Maintain natural arch in lower back" })
            },
            new Exercise { 
                Name = "Deadlift", 
                Description = "Full body compound movement for overall strength", 
                MuscleGroup = "Full Body", 
                Equipment = "Barbell", 
                Sets = 4, 
                Reps = "6-10", 
                RestTime = "3-4 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Stand with feet hip-width apart", "Bend at hips and knees to grasp bar", "Keep bar close to your legs", "Stand up by extending hips and knees" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Keep your back straight", "Don't round your lower back", "Drive through your heels" })
            },
            new Exercise { 
                Name = "Pull-ups", 
                Description = "Compound back exercise for upper body strength", 
                MuscleGroup = "Back", 
                Equipment = "Pull-up Bar", 
                Sets = 4, 
                Reps = "8-12", 
                RestTime = "2-3 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Hang from pull-up bar with hands shoulder-width apart", "Pull your body up until chin is over the bar", "Lower yourself with control", "Repeat for desired reps" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Engage your lats", "Don't swing your body", "Full range of motion" })
            },
            new Exercise { 
                Name = "Overhead Press", 
                Description = "Compound shoulder exercise for upper body strength", 
                MuscleGroup = "Shoulders", 
                Equipment = "Barbell", 
                Sets = 4, 
                Reps = "8-12", 
                RestTime = "2-3 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Stand with feet shoulder-width apart", "Hold bar at shoulder level", "Press bar overhead", "Lower with control" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Keep your core tight", "Don't lean back excessively", "Full lockout at top" })
            },
            new Exercise { 
                Name = "Incline Dumbbell Press", 
                Description = "Upper chest focused pressing movement", 
                MuscleGroup = "Chest", 
                Equipment = "Dumbbells", 
                Sets = 4, 
                Reps = "8-12", 
                RestTime = "2-3 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Set bench to 30-45 degree incline", "Start with dumbbells at chest level", "Press up and slightly together", "Lower with control" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Focus on upper chest", "Keep shoulder blades retracted", "Don't bounce off chest" })
            },
            new Exercise { 
                Name = "Lateral Raises", 
                Description = "Isolation exercise for shoulder width", 
                MuscleGroup = "Shoulders", 
                Equipment = "Dumbbells", 
                Sets = 3, 
                Reps = "12-15", 
                RestTime = "1-2 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Hold dumbbells at sides", "Raise arms to shoulder height", "Keep slight bend in elbows", "Lower with control" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Don't use momentum", "Lead with pinkies", "Control the negative" })
            },
            new Exercise { 
                Name = "Bulgarian Split Squats", 
                Description = "Single leg squat variation for leg strength", 
                MuscleGroup = "Legs", 
                Equipment = "Bodyweight", 
                Sets = 3, 
                Reps = "10-12 each leg", 
                RestTime = "2-3 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Place rear foot on bench", "Lower into lunge position", "Drive through front heel", "Return to starting position" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Keep front knee behind toe", "Maintain upright torso", "Focus on front leg" })
            },
            new Exercise { 
                Name = "Romanian Deadlifts", 
                Description = "Hip hinge movement for hamstrings and glutes", 
                MuscleGroup = "Legs", 
                Equipment = "Dumbbells", 
                Sets = 4, 
                Reps = "8-12", 
                RestTime = "2-3 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Hold dumbbells in front", "Hinge at hips keeping back straight", "Lower until you feel stretch", "Drive hips forward to return" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Keep core tight", "Don't round back", "Feel stretch in hamstrings" })
            },
            new Exercise { 
                Name = "Plank", 
                Description = "Core stability exercise", 
                MuscleGroup = "Core", 
                Equipment = "Bodyweight", 
                Sets = 3, 
                Reps = "30-60 seconds", 
                RestTime = "1-2 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Start in push-up position", "Hold body in straight line", "Engage core muscles", "Breathe normally" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Don't let hips sag", "Keep head neutral", "Focus on core engagement" })
            },
            new Exercise { 
                Name = "Russian Twists", 
                Description = "Rotational core exercise", 
                MuscleGroup = "Core", 
                Equipment = "Bodyweight", 
                Sets = 3, 
                Reps = "20-30 each side", 
                RestTime = "1-2 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Sit with knees bent", "Lean back slightly", "Rotate torso side to side", "Keep feet off ground for extra challenge" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Keep core engaged", "Control the movement", "Don't use momentum" })
            },
            new Exercise { 
                Name = "Burpees", 
                Description = "Full body cardio exercise", 
                MuscleGroup = "Full Body", 
                Equipment = "Bodyweight", 
                Sets = 4, 
                Reps = "8-12", 
                RestTime = "1-2 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Start standing", "Drop to push-up position", "Do push-up", "Jump feet to hands", "Jump up with arms overhead" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Maintain good form", "Keep core tight", "Land softly" })
            },
            new Exercise { 
                Name = "Mountain Climbers", 
                Description = "High intensity cardio exercise", 
                MuscleGroup = "Full Body", 
                Equipment = "Bodyweight", 
                Sets = 4, 
                Reps = "20-30 seconds", 
                RestTime = "1-2 min",
                ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400",
                Instructions = JsonSerializer.Serialize(new List<string> { "Start in plank position", "Bring knees to chest alternately", "Keep core tight", "Maintain plank position" }),
                Tips = JsonSerializer.Serialize(new List<string> { "Keep hips level", "Don't let form break", "Focus on speed" })
            }
        );
        await db.SaveChangesAsync();
    }

    // Update existing workout programs focusArea
    var upperLowerProgram = await db.WorkoutPrograms.FirstOrDefaultAsync(wp => wp.Title == "Upper Lower Split");
    if (upperLowerProgram != null && upperLowerProgram.FocusArea == "Full Body")
    {
        upperLowerProgram.FocusArea = "Upper Body";
    }

    // Add new workout programs if they don't exist
    var exercises = await db.Exercises.ToListAsync();
    
    // Check if new programs exist, if not add them
    var existingPrograms = await db.WorkoutPrograms.ToListAsync();
    var programTitles = existingPrograms.Select(p => p.Title).ToList();
    
    var newPrograms = new List<WorkoutProgram>();
    
    if (!programTitles.Contains("Chest & Shoulders"))
    {
        newPrograms.Add(new WorkoutProgram { 
            Title = "Chest & Shoulders", 
            ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400", 
            Duration = "6 weeks", 
            Difficulty = "Intermediate", 
            FocusArea = "Upper Body",
            ProgramType = "Upper Body",
            WeeklyFrequency = 3,
            EstimatedDuration = "6 weeks",
            Equipment = JsonSerializer.Serialize(new List<string> { "Dumbbells", "Barbell", "Bench", "Cable Machine" }),
            TargetMuscles = JsonSerializer.Serialize(new List<string> { "Chest", "Shoulders", "Triceps" }),
            ProgramOverview = "Focused upper body program targeting chest and shoulders for maximum development. Perfect for building a strong, defined upper body.",
            Benefits = JsonSerializer.Serialize(new List<string> { "Builds chest and shoulder strength", "Improves upper body aesthetics", "Increases pressing power", "Great for intermediate lifters" }),
            Tips = JsonSerializer.Serialize(new List<string> { "Focus on proper form", "Progressive overload", "Adequate rest between sessions", "Warm up properly" })
        });
    }
    
    if (!programTitles.Contains("Leg Day Special"))
    {
        newPrograms.Add(new WorkoutProgram { 
            Title = "Leg Day Special", 
            ImageUrl = "https://images.unsplash.com/photo-1518611012118-696072aa579a?w=400", 
            Duration = "8 weeks", 
            Difficulty = "Intermediate", 
            FocusArea = "Lower Body",
            ProgramType = "Lower Body",
            WeeklyFrequency = 2,
            EstimatedDuration = "8 weeks",
            Equipment = JsonSerializer.Serialize(new List<string> { "Dumbbells", "Barbell", "Bench", "Bodyweight" }),
            TargetMuscles = JsonSerializer.Serialize(new List<string> { "Quads", "Hamstrings", "Glutes", "Calves" }),
            ProgramOverview = "Intensive lower body program designed to build strong, powerful legs. Focus on compound movements and single-leg variations.",
            Benefits = JsonSerializer.Serialize(new List<string> { "Builds leg strength and power", "Improves athletic performance", "Increases muscle mass", "Better balance and stability" }),
            Tips = JsonSerializer.Serialize(new List<string> { "Start with compound movements", "Include single-leg exercises", "Don't skip leg day", "Focus on full range of motion" })
        });
    }
    
    if (!programTitles.Contains("Core Blaster"))
    {
        newPrograms.Add(new WorkoutProgram { 
            Title = "Core Blaster", 
            ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400", 
            Duration = "4 weeks", 
            Difficulty = "Beginner", 
            FocusArea = "Core",
            ProgramType = "Core",
            WeeklyFrequency = 4,
            EstimatedDuration = "4 weeks",
            Equipment = JsonSerializer.Serialize(new List<string> { "Bodyweight", "Mat" }),
            TargetMuscles = JsonSerializer.Serialize(new List<string> { "Abs", "Obliques", "Lower Back", "Hip Flexors" }),
            ProgramOverview = "Intensive core strengthening program using bodyweight exercises. Perfect for building a strong, stable core foundation.",
            Benefits = JsonSerializer.Serialize(new List<string> { "Builds core strength", "Improves posture", "Reduces back pain", "Better athletic performance" }),
            Tips = JsonSerializer.Serialize(new List<string> { "Focus on quality over quantity", "Engage core throughout", "Breathe properly", "Progress gradually" })
        });
    }
    
    if (!programTitles.Contains("HIIT Cardio"))
    {
        newPrograms.Add(new WorkoutProgram { 
            Title = "HIIT Cardio", 
            ImageUrl = "https://images.unsplash.com/photo-1518611012118-696072aa579a?w=400", 
            Duration = "6 weeks", 
            Difficulty = "Advanced", 
            FocusArea = "Cardio",
            ProgramType = "HIIT",
            WeeklyFrequency = 3,
            EstimatedDuration = "6 weeks",
            Equipment = JsonSerializer.Serialize(new List<string> { "Bodyweight", "Timer" }),
            TargetMuscles = JsonSerializer.Serialize(new List<string> { "Full Body", "Cardiovascular System" }),
            ProgramOverview = "High-intensity interval training program for maximum fat burning and cardiovascular fitness. Short, intense workouts with maximum results.",
            Benefits = JsonSerializer.Serialize(new List<string> { "Burns maximum calories", "Improves cardiovascular fitness", "Time efficient", "Boosts metabolism" }),
            Tips = JsonSerializer.Serialize(new List<string> { "Warm up properly", "Give maximum effort", "Rest between intervals", "Stay hydrated" })
        });
    }
    
    if (newPrograms.Any())
    {
        db.WorkoutPrograms.AddRange(newPrograms);
        await db.SaveChangesAsync();
        
        // Add exercises to new programs
        if (exercises.Count >= 10)
        {
            var newProgramExercises = new List<WorkoutProgramExercise>();
            
            foreach (var program in newPrograms)
            {
                if (program.Title == "Chest & Shoulders")
                {
                    newProgramExercises.AddRange(new[]
                    {
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[1].Id, OrderIndex = 1 }, // Bench Press
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[5].Id, OrderIndex = 2 }, // Incline Dumbbell Press
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[4].Id, OrderIndex = 3 }, // Overhead Press
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[6].Id, OrderIndex = 4 }  // Lateral Raises
                    });
                }
                else if (program.Title == "Leg Day Special")
                {
                    newProgramExercises.AddRange(new[]
                    {
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[0].Id, OrderIndex = 1 }, // Squat
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[7].Id, OrderIndex = 2 }, // Bulgarian Split Squats
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[8].Id, OrderIndex = 3 }  // Romanian Deadlifts
                    });
                }
                else if (program.Title == "Core Blaster")
                {
                    newProgramExercises.AddRange(new[]
                    {
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[9].Id, OrderIndex = 1 }, // Plank
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[10].Id, OrderIndex = 2 } // Russian Twists
                    });
                }
                else if (program.Title == "HIIT Cardio")
                {
                    newProgramExercises.AddRange(new[]
                    {
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[11].Id, OrderIndex = 1 }, // Burpees
                        new WorkoutProgramExercise { WorkoutProgramId = program.Id, ExerciseId = exercises[12].Id, OrderIndex = 2 } // Mountain Climbers
                    });
                }
            }
            
            if (newProgramExercises.Any())
            {
                db.WorkoutProgramExercises.AddRange(newProgramExercises);
                await db.SaveChangesAsync();
            }
        }
    }

    // Seed WorkoutPrograms if empty
    if (!await db.WorkoutPrograms.AnyAsync())
    {
        var fullBodyProgram = new WorkoutProgram { 
            Title = "Full Body Strength", 
            ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400", 
            Duration = "8 weeks", 
            Difficulty = "Intermediate", 
            FocusArea = "Full Body",
            ProgramType = "Full Body",
            WeeklyFrequency = 3,
            EstimatedDuration = "8 weeks",
            Equipment = JsonSerializer.Serialize(new List<string> { "Dumbbells", "Barbell", "Bench", "Pull-up Bar" }),
            TargetMuscles = JsonSerializer.Serialize(new List<string> { "Chest", "Back", "Legs", "Shoulders", "Arms", "Core" }),
            ProgramOverview = "This comprehensive full body program targets all major muscle groups in each session. Perfect for intermediate lifters looking to build strength and muscle mass efficiently.",
            Benefits = JsonSerializer.Serialize(new List<string> { "Builds overall strength", "Improves muscle balance", "Time efficient", "Great for beginners to intermediate" }),
            Tips = JsonSerializer.Serialize(new List<string> { "Focus on proper form", "Rest 2-3 minutes between sets", "Progressive overload", "Stay hydrated" })
        };
        
        var pplProgram = new WorkoutProgram { 
            Title = "Push Pull Legs", 
            ImageUrl = "https://images.unsplash.com/photo-1518611012118-696072aa579a?w=400", 
            Duration = "12 weeks", 
            Difficulty = "Advanced", 
            FocusArea = "Full Body",
            ProgramType = "Push Pull Legs",
            WeeklyFrequency = 6,
            EstimatedDuration = "12 weeks",
            Equipment = JsonSerializer.Serialize(new List<string> { "Dumbbells", "Barbell", "Cable Machine", "Bench" }),
            TargetMuscles = JsonSerializer.Serialize(new List<string> { "Chest", "Back", "Legs", "Shoulders", "Arms" }),
            ProgramOverview = "The Push Pull Legs split is one of the most effective training splits for muscle growth. This program alternates between pushing movements, pulling movements, and leg training.",
            Benefits = JsonSerializer.Serialize(new List<string> { "Maximum muscle growth", "Balanced development", "Allows for high volume", "Prevents overtraining" }),
            Tips = JsonSerializer.Serialize(new List<string> { "Push day: chest, shoulders, triceps", "Pull day: back, biceps", "Leg day: quads, hamstrings, calves", "Rest between each split" })
        };
        
        var upperLowerProgramNew = new WorkoutProgram { 
            Title = "Upper Lower Split", 
            ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400", 
            Duration = "10 weeks", 
            Difficulty = "Intermediate", 
            FocusArea = "Upper Body",
            ProgramType = "Upper Lower",
            WeeklyFrequency = 4,
            EstimatedDuration = "10 weeks",
            Equipment = JsonSerializer.Serialize(new List<string> { "Dumbbells", "Barbell", "Bench", "Pull-up Bar" }),
            TargetMuscles = JsonSerializer.Serialize(new List<string> { "Upper Body", "Lower Body" }),
            ProgramOverview = "The Upper Lower split divides training into upper body and lower body days, allowing for adequate recovery while maintaining training frequency.",
            Benefits = JsonSerializer.Serialize(new List<string> { "Balanced upper/lower development", "Good recovery time", "Flexible scheduling", "Suitable for most levels" }),
            Tips = JsonSerializer.Serialize(new List<string> { "Upper: chest, back, shoulders, arms", "Lower: quads, hamstrings, glutes, calves", "Rest 1-2 days between same muscle groups" })
        };

        db.WorkoutPrograms.AddRange(fullBodyProgram, pplProgram, upperLowerProgramNew);
        await db.SaveChangesAsync();

        // Add exercises to programs
        if (exercises.Count >= 10)
        {
            // Full Body Program Exercises
            db.WorkoutProgramExercises.AddRange(
                new WorkoutProgramExercise { WorkoutProgramId = fullBodyProgram.Id, ExerciseId = exercises[0].Id, OrderIndex = 1 }, // Squat
                new WorkoutProgramExercise { WorkoutProgramId = fullBodyProgram.Id, ExerciseId = exercises[1].Id, OrderIndex = 2 }, // Bench Press
                new WorkoutProgramExercise { WorkoutProgramId = fullBodyProgram.Id, ExerciseId = exercises[2].Id, OrderIndex = 3 }, // Deadlift
                new WorkoutProgramExercise { WorkoutProgramId = fullBodyProgram.Id, ExerciseId = exercises[3].Id, OrderIndex = 4 }, // Pull-ups
                new WorkoutProgramExercise { WorkoutProgramId = fullBodyProgram.Id, ExerciseId = exercises[4].Id, OrderIndex = 5 }  // Overhead Press
            );

            // PPL Program Exercises (Push day example)
            db.WorkoutProgramExercises.AddRange(
                new WorkoutProgramExercise { WorkoutProgramId = pplProgram.Id, ExerciseId = exercises[1].Id, OrderIndex = 1 }, // Bench Press
                new WorkoutProgramExercise { WorkoutProgramId = pplProgram.Id, ExerciseId = exercises[4].Id, OrderIndex = 2 }, // Overhead Press
                new WorkoutProgramExercise { WorkoutProgramId = pplProgram.Id, ExerciseId = exercises[0].Id, OrderIndex = 3 }  // Squat
            );

            // Upper Lower Program Exercises
            db.WorkoutProgramExercises.AddRange(
                new WorkoutProgramExercise { WorkoutProgramId = upperLowerProgramNew.Id, ExerciseId = exercises[1].Id, OrderIndex = 1 }, // Bench Press
                new WorkoutProgramExercise { WorkoutProgramId = upperLowerProgramNew.Id, ExerciseId = exercises[3].Id, OrderIndex = 2 }, // Pull-ups
                new WorkoutProgramExercise { WorkoutProgramId = upperLowerProgramNew.Id, ExerciseId = exercises[0].Id, OrderIndex = 3 }  // Squat
            );

            await db.SaveChangesAsync();
        }
    }

    // Clear existing articles and seed Enhanced Articles
    var existingArticles = await db.Articles.ToListAsync();
    if (existingArticles.Any())
    {
        db.Articles.RemoveRange(existingArticles);
        await db.SaveChangesAsync();
    }

    // Seed Enhanced Articles
    if (!await db.Articles.AnyAsync())
    {
        // Protein Article
        var proteinArticle = new Article 
        { 
            Title = "Protein İhtiyacınızı Nasıl Hesaplarsınız?", 
            Description = "Günlük protein ihtiyacınızı doğru şekilde hesaplamanın bilimsel yöntemleri", 
            Author = "Dr. Ahmet Yılmaz", 
            Category = "Nutrition", 
            ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400", 
            EstimatedReadMinutes = 8,
            Content = "Protein, kas gelişimi ve genel sağlık için kritik bir makro besindir. Bu makalede günlük protein ihtiyacınızı nasıl hesaplayacağınızı öğreneceksiniz.",
            DifficultyLevel = "Beginner",
            Tags = JsonSerializer.Serialize(new List<string> { "protein", "beslenme", "kas gelişimi", "makro besinler" }),
            RelatedArticles = JsonSerializer.Serialize(new List<int> { }),
            ViewCount = 15420,
            Rating = 4.8m,
            IsFeatured = true,
            CreatedAt = DateTime.UtcNow 
        };

        // Deadlift Article
        var deadliftArticle = new Article 
        { 
            Title = "Deadlift Tekniği: Doğru Form Nasıl Olmalı?", 
            Description = "Deadlift yaparken sakatlanmayı önlemek ve maksimum verim almak için doğru teknik", 
            Author = "Mehmet Kaya", 
            Category = "Workout", 
            ImageUrl = "https://images.unsplash.com/photo-1518611012118-696072aa579a?w=400", 
            EstimatedReadMinutes = 6,
            Content = "Deadlift, en etkili compound hareketlerden biridir ancak doğru teknik olmadan tehlikeli olabilir. Bu rehberde güvenli ve etkili deadlift tekniğini öğreneceksiniz.",
            DifficultyLevel = "Intermediate",
            Tags = JsonSerializer.Serialize(new List<string> { "deadlift", "teknik", "sakatlanma önleme", "compound hareket" }),
            RelatedArticles = JsonSerializer.Serialize(new List<int> { }),
            ViewCount = 8920,
            Rating = 4.6m,
            IsFeatured = false,
            CreatedAt = DateTime.UtcNow 
        };

        db.Articles.AddRange(proteinArticle, deadliftArticle);
        await db.SaveChangesAsync();

        // Add sections for protein article
        var proteinSections = new List<ArticleSection>
        {
            new ArticleSection 
            { 
                ArticleId = proteinArticle.Id, 
                Title = "Protein İhtiyacınızı Hesaplama", 
                Content = "Genel kural: Vücut ağırlığınızın kilogramı başına 0.8-2.2 gram protein almalısınız. Aktif bireyler için 1.2-1.6 g/kg, kas geliştirmek isteyenler için 1.6-2.2 g/kg önerilir.", 
                SectionOrder = 1 
            },
            new ArticleSection 
            { 
                ArticleId = proteinArticle.Id, 
                Title = "En İyi Protein Kaynakları", 
                Content = "Hayvansal proteinler (et, balık, yumurta, süt) ve bitkisel proteinler (baklagiller, kuruyemişler, tohumlar) arasında denge kurun. Hayvansal proteinler daha yüksek biyolojik değere sahiptir.", 
                SectionOrder = 2 
            },
            new ArticleSection 
            { 
                ArticleId = proteinArticle.Id, 
                Title = "Protein Timing'i", 
                Content = "Antrenman sonrası 30 dakika içinde protein alımı kas protein sentezini maksimize eder. Günlük protein ihtiyacınızı 3-4 öğüne bölerek alın.", 
                SectionOrder = 3 
            }
        };

        db.ArticleSections.AddRange(proteinSections);
        await db.SaveChangesAsync();

        // Add tips for protein article
        var proteinTips = new List<ArticleTip>
        {
            new ArticleTip { ArticleId = proteinArticle.Id, SectionId = proteinSections[0].Id, TipText = "Kahvaltıda en az 20g protein alın", TipOrder = 1 },
            new ArticleTip { ArticleId = proteinArticle.Id, SectionId = proteinSections[0].Id, TipText = "Antrenman sonrası 30 dakika içinde protein tüketin", TipOrder = 2 },
            new ArticleTip { ArticleId = proteinArticle.Id, SectionId = proteinSections[1].Id, TipText = "Hayvansal ve bitkisel proteinleri karıştırın", TipOrder = 1 },
            new ArticleTip { ArticleId = proteinArticle.Id, SectionId = proteinSections[1].Id, TipText = "Protein tozları sadece gıda takviyesi olarak kullanın", TipOrder = 2 }
        };

        db.ArticleTips.AddRange(proteinTips);
        await db.SaveChangesAsync();

        // Add warnings for protein article
        var proteinWarnings = new List<ArticleWarning>
        {
            new ArticleWarning { ArticleId = proteinArticle.Id, SectionId = proteinSections[0].Id, WarningText = "Aşırı protein böbrek yükünü artırabilir", WarningOrder = 1 },
            new ArticleWarning { ArticleId = proteinArticle.Id, SectionId = proteinSections[0].Id, WarningText = "Su tüketimini artırın", WarningOrder = 2 },
            new ArticleWarning { ArticleId = proteinArticle.Id, SectionId = proteinSections[1].Id, WarningText = "Protein tozları tek başına yeterli değildir", WarningOrder = 1 }
        };

        db.ArticleWarnings.AddRange(proteinWarnings);
        await db.SaveChangesAsync();

        // Add key takeaways for protein article
        var proteinKeyTakeaways = new List<ArticleKeyTakeaway>
        {
            new ArticleKeyTakeaway { ArticleId = proteinArticle.Id, TakeawayText = "Vücut ağırlığı başına 0.8-2.2g protein hedefleyin", TakeawayOrder = 1 },
            new ArticleKeyTakeaway { ArticleId = proteinArticle.Id, TakeawayText = "Protein kaynaklarını çeşitlendirin", TakeawayOrder = 2 },
            new ArticleKeyTakeaway { ArticleId = proteinArticle.Id, TakeawayText = "Antrenman sonrası protein timing'i önemli", TakeawayOrder = 3 },
            new ArticleKeyTakeaway { ArticleId = proteinArticle.Id, TakeawayText = "Yeterli su tüketimini unutmayın", TakeawayOrder = 4 }
        };

        db.ArticleKeyTakeaways.AddRange(proteinKeyTakeaways);
        await db.SaveChangesAsync();

        // Add references for protein article
        var proteinReferences = new List<ArticleReference>
        {
            new ArticleReference { ArticleId = proteinArticle.Id, ReferenceText = "Phillips, S. M. (2012). Dietary protein requirements and adaptive advantages in athletes", ReferenceUrl = "https://example.com/ref1", ReferenceOrder = 1 },
            new ArticleReference { ArticleId = proteinArticle.Id, ReferenceText = "Morton, R. W. (2018). A systematic review of protein requirements", ReferenceUrl = "https://example.com/ref2", ReferenceOrder = 2 }
        };

        db.ArticleReferences.AddRange(proteinReferences);
        await db.SaveChangesAsync();

        // Add sections for deadlift article
        var deadliftSections = new List<ArticleSection>
        {
            new ArticleSection 
            { 
                ArticleId = deadliftArticle.Id, 
                Title = "Temel Deadlift Pozisyonu", 
                Content = "Ayaklar kalça genişliğinde, ayak parmakları hafif dışa bakacak şekilde durun. Bar ayaklarınızın ortasında, ayak parmaklarınızın hemen arkasında olmalı.", 
                SectionOrder = 1 
            },
            new ArticleSection 
            { 
                ArticleId = deadliftArticle.Id, 
                Title = "Kaldırma Tekniği", 
                Content = "Göğsünüzü yukarıda tutun, omuzlarınızı barın üzerinde konumlandırın. Kalçalarınızı geriye iterek barı kaldırın. Bar vücudunuza yakın tutun.", 
                SectionOrder = 2 
            },
            new ArticleSection 
            { 
                ArticleId = deadliftArticle.Id, 
                Title = "İndirme ve Güvenlik", 
                Content = "Kaldırma hareketinin tersini yaparak barı kontrollü bir şekilde indirin. Sırtınızı düz tutun ve ani hareketlerden kaçının.", 
                SectionOrder = 3 
            }
        };

        db.ArticleSections.AddRange(deadliftSections);
        await db.SaveChangesAsync();

        // Add tips for deadlift article
        var deadliftTips = new List<ArticleTip>
        {
            new ArticleTip { ArticleId = deadliftArticle.Id, SectionId = deadliftSections[0].Id, TipText = "Barı kaldırmadan önce nefes alın ve core'unuzu sıkın", TipOrder = 1 },
            new ArticleTip { ArticleId = deadliftArticle.Id, SectionId = deadliftSections[1].Id, TipText = "Barı vücudunuza yakın tutun", TipOrder = 1 },
            new ArticleTip { ArticleId = deadliftArticle.Id, SectionId = deadliftSections[1].Id, TipText = "Kalçalarınızı ve dizlerinizi aynı anda açın", TipOrder = 2 },
            new ArticleTip { ArticleId = deadliftArticle.Id, SectionId = deadliftSections[2].Id, TipText = "Ağırlığı kontrollü bir şekilde indirin", TipOrder = 1 }
        };

        db.ArticleTips.AddRange(deadliftTips);
        await db.SaveChangesAsync();

        // Add warnings for deadlift article
        var deadliftWarnings = new List<ArticleWarning>
        {
            new ArticleWarning { ArticleId = deadliftArticle.Id, SectionId = deadliftSections[0].Id, WarningText = "Sırtınızı yuvarlamayın", WarningOrder = 1 },
            new ArticleWarning { ArticleId = deadliftArticle.Id, SectionId = deadliftSections[1].Id, WarningText = "Barı vücudunuzdan uzaklaştırmayın", WarningOrder = 1 },
            new ArticleWarning { ArticleId = deadliftArticle.Id, SectionId = deadliftSections[2].Id, WarningText = "Ani hareketlerden kaçının", WarningOrder = 1 }
        };

        db.ArticleWarnings.AddRange(deadliftWarnings);
        await db.SaveChangesAsync();

        // Add key takeaways for deadlift article
        var deadliftKeyTakeaways = new List<ArticleKeyTakeaway>
        {
            new ArticleKeyTakeaway { ArticleId = deadliftArticle.Id, TakeawayText = "Doğru pozisyon ve form her şeyden önemli", TakeawayOrder = 1 },
            new ArticleKeyTakeaway { ArticleId = deadliftArticle.Id, TakeawayText = "Barı vücudunuza yakın tutun", TakeawayOrder = 2 },
            new ArticleKeyTakeaway { ArticleId = deadliftArticle.Id, TakeawayText = "Core gücünüzü geliştirin", TakeawayOrder = 3 },
            new ArticleKeyTakeaway { ArticleId = deadliftArticle.Id, TakeawayText = "Ağırlığı kademeli olarak artırın", TakeawayOrder = 4 }
        };

        db.ArticleKeyTakeaways.AddRange(deadliftKeyTakeaways);
        await db.SaveChangesAsync();
    }
    }
}
catch (Exception ex)
{
    // Log error but don't fail startup - allows app to start even if DB is temporarily unavailable
    Console.WriteLine($"⚠️ Database initialization error: {ex.Message}");
    Console.WriteLine($"⚠️ Application will continue, but database features may not work until connection is restored.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FitApp API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
        
        // Configure Swagger UI
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
        c.EnableValidator();
    });
}

app.UseHttpsRedirection();

// Serve static files for uploaded images
app.UseStaticFiles();

// Use CORS
app.UseCors();

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}