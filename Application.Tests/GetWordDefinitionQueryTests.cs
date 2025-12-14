using Application.UseCases;
using Domain.Entities;
using Domain.Services;
using Moq;


namespace Application.Tests
{
    public class GetWordDefinitionQueryTests
    {
       private readonly Mock<IDictionaryService> _mockService;

       private readonly GetWordDefinitionQuery _sut;

    public GetWordDefinitionQueryTests()
    {
        _mockService = new Mock<IDictionaryService>();
        _sut = new GetWordDefinitionQuery(_mockService.Object);
    }
    
    [Fact]
    public async Task ExecuteAsync_ShouldReturnDefinition_WhenServiceSucceeds()
    {
        
        string word = "hello";
        var expectedDefinition = new WordDefinition { Word = word, Phonetics = new List<PhoneticInfo>() };  
            
        _mockService.Setup(s => s.GetDefinitionAsync(word))
                    .ReturnsAsync(expectedDefinition);
            
        var result = await _sut.ExecuteAsync(word);
        
        Assert.NotNull(result);
        Assert.Equal(word, result.Word);

        _mockService.Verify(s => s.GetDefinitionAsync(word), Times.Once);
    }

   
    [Fact]
    public async Task ExecuteAsync_ShouldThrowKeyNotFoundException_WhenWordIsNotFound()
    {
        
        string word = "notfoundword";

        _mockService.Setup(s => s.GetDefinitionAsync(word))
                    .ReturnsAsync((WordDefinition)null); 

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ExecuteAsync(word));
        
        _mockService.Verify(s => s.GetDefinitionAsync(word), Times.Once);
    }
           
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExecuteAsync_ShouldThrowArgumentException_WhenWordIsInvalid(string invalidWord)
    {
         await Assert.ThrowsAsync<ArgumentException>(() => _sut.ExecuteAsync(invalidWord));

        _mockService.Verify(s => s.GetDefinitionAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPropagateApplicationException_OnServiceFailure()
    {
         string word = "networkfail";

        _mockService.Setup(s => s.GetDefinitionAsync(word))
                    .ThrowsAsync(new ApplicationException("API Timeout"));
       
        await Assert.ThrowsAsync<ApplicationException>(() => _sut.ExecuteAsync(word));
    }
}
}