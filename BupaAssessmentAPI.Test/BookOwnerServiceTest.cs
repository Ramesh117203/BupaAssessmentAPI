
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BookApi.Models;
using Moq;
using Moq.Protected;
using Xunit;

public class BookOwnerServiceTests
{
    private BookOwnerService CreateService(HttpStatusCode statusCode, string content)
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",ItExpr.IsAny<HttpRequestMessage>(),ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        return new BookOwnerService(httpClient);
    }
    [Fact]
    public async Task GetBookOwnersAsync_ReturnsBookOwners()
    {
        var service = CreateService(HttpStatusCode.OK, "[{\"name\":\"John Doe\",\"age\":30,\"books\":[{\"name\":\"Book 1\",\"type\":\"Fiction\"}]}]");
        var result = await service.GetBookOwnersAsync();
        var bookOwner = Assert.Single(result);
        Assert.Equal("John Doe", bookOwner.name);
        Assert.Equal(30, bookOwner.age);
        var book = Assert.Single(bookOwner.books);
        Assert.Equal("Book 1", book.name);
    }
    [Fact]
    public async Task GetBookOwnersAsync_ThrowsException_OnApiFailure()
    {
        var service = CreateService(HttpStatusCode.InternalServerError, string.Empty);
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetBookOwnersAsync());
        Assert.Contains("Error fetching data from BUPA API", exception.Message);

    }
}
