using Festify.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();

var apiBaseUrl = builder.Configuration["Festify:ApiBaseUrl"] ?? "http://localhost:5114";
builder.Services.AddHttpClient("Festify.Api", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/'));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();
