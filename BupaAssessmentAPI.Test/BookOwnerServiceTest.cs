using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BookApi.Models;
using Moq;
using Moq.Protected;
using Xunit;

public class BookOwnerServiceTests
{
    private BookOwnerService CreateService()
    {
        var httpClient = new HttpClient();
        return new BookOwnerService(httpClient);
    }

    [Fact]
    public async Task GetBookOwnersAsync_ReturnsBookOwners()
    {
        var service = CreateService();
        var result = await service.GetBookOwnersAsync();
        Assert.NotEmpty(result);
        var bookOwner = result[0];
        Assert.NotNull(bookOwner.name);
        Assert.InRange(bookOwner.age, 0, 120); 
        Assert.NotEmpty(bookOwner.books);
        var book = bookOwner.books[0];
        Assert.NotNull(book.name);
        Assert.NotNull(book.type);
    }

    [Fact]
    public async Task GetBookOwnersAsync_ThrowsException_OnApiFailure()
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                Content = new StringContent(string.Empty)
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var service = new BookOwnerService(httpClient);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetBookOwnersAsync());
        Assert.Contains("Error fetching data from API", exception.Message);
    }
}
