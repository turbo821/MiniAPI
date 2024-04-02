using System.Text.RegularExpressions;


List<Person> users = new List<Person>()
{
    new() { Id = Guid.NewGuid().ToString(), Name = "Tom", Age = 23 },
    new() { Id = Guid.NewGuid().ToString(), Name = "Bob", Age = 47 },
    new() { Id = Guid.NewGuid().ToString(), Name = "Sam", Age = 26 }
};


WebApplicationBuilder builder = WebApplication.CreateBuilder();
WebApplication app = builder.Build();

app.Run(async (context) =>
{
    HttpRequest request = context.Request;
    HttpResponse response = context.Response;
    PathString path = request.Path;
    //string expressionForNumber = "^/api/users/([0-9]+)$";   // если id представляет число

    // 2e752824-1657-4c7f-844b-6ec2e168e99c
    string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
    if (path == "/api/users" && request.Method == "GET")
    {
        await GetAllPersons(response);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        string? id = path.Value?.Split("/")[3];
        await GetPerson(id, response);
    }
    else if (path == "/api/users" && request.Method == "POST")
    {
        await CreatePerson(response, request);
    }
    else if (path == "/api/users" && request.Method == "PUT")
    {
        await UpdatePerson(response, request);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeletePerson(id, response);
    }
    else
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.SendFileAsync("html/index.html");
    }
});

app.Run();


async Task GetAllPersons(HttpResponse response)
{
    await response.WriteAsJsonAsync(users);
}

async Task GetPerson(string? id, HttpResponse response)
{
    Person? user = users.FirstOrDefault((u) => u.Id == id);
    if (user != null)
    {
        await response.WriteAsJsonAsync(user);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Пользователь не найден" });
    }
}

async Task DeletePerson(string? id, HttpResponse response)
{
    Person? user = users.FirstOrDefault((u) => u.Id == id);
    if (user != null)
    {
        users.Remove(user);
        await response.WriteAsJsonAsync(user);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Пользователь не найден" });
    }
}

async Task CreatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        Person? user = await request.ReadFromJsonAsync<Person>();
        if (user != null)
        {
            user.Id = Guid.NewGuid().ToString();
            users.Add(user);
            await response.WriteAsJsonAsync(user);
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}

async Task UpdatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        Person? userData = await request.ReadFromJsonAsync<Person>();
        if (userData != null)
        {
            Person? user = users.FirstOrDefault(u => u.Id == userData.Id);
            if (user != null)
            {
                user.Name = userData.Name;
                user.Age = userData.Age;
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Пользователь не найден" });
            }
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}

public class Person
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
