using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BookApi.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Xunit;

public class BookOwnersControllerTests
{
    private BookOwnersController CreateController(HttpStatusCode statusCode, string content)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var bookOwnerService = new BookOwnerService(httpClient);

        return new BookOwnersController(bookOwnerService);
    }

    [Fact]
    public async Task GetBooks_ReturnsCategorizedBooks()
    {
        var bookOwners = new List<BookOwner>
        {
            new BookOwner { name = "John Doe", age = 30, books = new List<Book> { new Book { name = "books 1", type = "Fiction" } } },
            new BookOwner { name = "Jane Smith", age = 17, books = new List<Book> { new Book { name = "books 2", type = "Non-Fiction" } } }
        };
        var controller = CreateController(HttpStatusCode.OK, System.Text.Json.JsonSerializer.Serialize(bookOwners));

        var result = await controller.GetBooks();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var categorizedBooks = Assert.IsType<Dictionary<string, List<BookOwner>>>(okResult.Value);

        Assert.Equal(2, categorizedBooks.Count);
        Assert.Single(categorizedBooks["Adults"]);
        Assert.Single(categorizedBooks["Children"]);
    }

    [Fact]
    public async Task GetBooks_ReturnsNotFound_OnEmptyData()
    {
        var controller = CreateController(HttpStatusCode.OK, "[]");

        var result = await controller.GetBooks();
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("No book owners found.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetBooks_ReturnsInternalServerError_OnApiFailure()
    {
        var controller = CreateController(HttpStatusCode.InternalServerError, string.Empty);

        var result = await controller.GetBooks();
        var internalServerErrorResult = Assert.IsType<ObjectResult>(result);

        Assert.Equal(500, internalServerErrorResult.StatusCode);
        Assert.Contains("Error fetching data from external API", internalServerErrorResult.Value.ToString());
    }
}
