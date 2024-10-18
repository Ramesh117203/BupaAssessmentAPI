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
    private BookOwnersController CreateController()
    {
        var httpClient = new HttpClient();
        var bookOwnerService = new BookOwnerService(httpClient);

        return new BookOwnersController(bookOwnerService);
    }

    [Fact]
    public async Task GetBooks_ReturnsCategorizedBooks()
    {
        var controller = CreateController();
        var result = await controller.GetBooks();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var categorizedBooks = Assert.IsType<Dictionary<string, List<BookOwner>>>(okResult.Value);
        Assert.True(categorizedBooks.ContainsKey("Adults"));
        Assert.True(categorizedBooks.ContainsKey("Children"));
        Assert.NotEmpty(categorizedBooks["Adults"]);
        Assert.NotEmpty(categorizedBooks["Children"]);
    }

    [Fact]
    public async Task GetBooks_ReturnsNotFound_OnEmptyData()
    {
        var controller = CreateController();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]") 
            });
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var bookOwnerService = new BookOwnerService(httpClient);
        var controllerWithMock = new BookOwnersController(bookOwnerService);

        var result = await controllerWithMock.GetBooks();
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("No book owners found.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetBooks_ReturnsInternalServerError_OnApiFailure()
    {
        var controller = CreateController();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent(string.Empty)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var bookOwnerService = new BookOwnerService(httpClient);
        var controllerWithMock = new BookOwnersController(bookOwnerService);

        var result = await controllerWithMock.GetBooks();
        var internalServerErrorResult = Assert.IsType<ObjectResult>(result);

        Assert.Equal(500, internalServerErrorResult.StatusCode);
        Assert.Contains("Error fetching data from  API", internalServerErrorResult.Value.ToString());
    }
}
