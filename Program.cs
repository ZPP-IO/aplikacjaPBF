builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireGMOrAdmin", policy =>
        policy.RequireRole("MistrzGry", "Administrator"));
});