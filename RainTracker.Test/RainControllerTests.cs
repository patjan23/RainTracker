using Microsoft.Extensions.Logging;
using RainTracker.Data;
using RainTracker.Controllers;
using FluentAssertions;
using RainTracker.Models;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace RainTracker.Test
{
    public class RainControllerTests
    {
        private RainContext _context = null!;
        private RainController _controller = null!;
        private Mock<ILogger<RainController>> _mockLogger = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<RainContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;

            _context = new RainContext(options);
            _mockLogger = new Mock<ILogger<RainController>>();
            _controller = new RainController(_context, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        #region Get Tests

        [Test]
        public async Task GetRainData_WithValidUserId_ReturnsUserData()
        {
            // Arrange
            var userId = TestDataBuilder.CreateUserId();
            var rainDataList = TestDataBuilder.CreateRainDataForUser(userId, 3);
            await _context.RainRecords.AddRangeAsync(rainDataList);
            await _context.SaveChangesAsync();

            TestDataBuilder.SetupMockHttpContext(_controller, userId);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();

            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as RainDataResponse;

            response.Should().NotBeNull();
            response!.Data.Should().HaveCount(3);
            response.Data.Should().BeInDescendingOrder(x => x.Timestamp);
        }

        [Test]
        public async Task GetRainData_WithNonExistentUser_ReturnsEmptyArray()
        {
            // Arrange
            var userId = TestDataBuilder.CreateUserId();

            TestDataBuilder.SetupMockHttpContext(_controller, userId);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();

            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as RainDataResponse;

            response.Should().NotBeNull();
            response!.Data.Should().BeEmpty();
        }

        [Test]
        public async Task GetRainData_WithNullUserId_ReturnsBadRequest()
        {
            TestDataBuilder.SetupMockHttpContext(_controller, null);
            // Act
            var result = await _controller.Get();

            

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<BadRequestObjectResult>();

            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult!.Value.Should().NotBeNull();
        }

        [Test]
        public async Task GetRainData_WithEmptyUserId_ReturnsBadRequest()
        {
            TestDataBuilder.SetupMockHttpContext(_controller, string.Empty);
            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task GetRainData_WithWhitespaceUserId_ReturnsBadRequest()
        {
            TestDataBuilder.SetupMockHttpContext(_controller,"   ");
            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        
        [Test]
        public async Task GetRainData_WithMultipleUsers_ReturnsOnlyRequestedUserData()
        {
            // Arrange
            var user1 = TestDataBuilder.CreateUserId("user1");
            var user2 = TestDataBuilder.CreateUserId("user2");

            var user1Data = TestDataBuilder.CreateRainDataForUser(user1, 2);
            var user2Data = TestDataBuilder.CreateRainDataForUser(user2, 3);

            await _context.RainRecords.AddRangeAsync(user1Data);
            await _context.RainRecords.AddRangeAsync(user2Data);
            await _context.SaveChangesAsync();

            TestDataBuilder.SetupMockHttpContext(_controller, user1);

            // Act
            var result = await _controller.Get();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();

            var okResult = result.Result as OkObjectResult;
            var response = okResult!.Value as RainDataResponse;

            response.Should().NotBeNull();
            response!.Data.Should().HaveCount(2);
            response.Data.Should().OnlyContain(x => x.Rain == user1Data.First(d => d.Timestamp == x.Timestamp).Rain);
        }

        #endregion

        #region Post Tests

        #region RecordRainData Tests

        [Test]
        public async Task RecordRainData_WithValidData_ReturnsCreated()
        {
            // Arrange
            var userId = TestDataBuilder.CreateUserId();
            var request = TestDataBuilder.CreateRainDataRequest(true);

            
            TestDataBuilder.SetupMockHttpContext(_controller, userId);
            // Act

            var result = await _controller.Post(request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();

            var createdResult = result as CreatedAtActionResult;
            createdResult!.ActionName.Should().Be(nameof(RainController.Get));

            // Verify data was saved to database
            var savedData = await _context.RainRecords.FirstOrDefaultAsync(x => x.UserId == userId);
            savedData.Should().NotBeNull();
            savedData!.Rain.Should().Be(request.Rain);
            savedData.UserId.Should().Be(userId);
            savedData.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task RecordRainData_WithNullUserId_ReturnsBadRequest()
        {
            // Arrange
            var request = TestDataBuilder.CreateRainDataRequest();
                        
            TestDataBuilder.SetupMockHttpContext(_controller, null);

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();

            // Verify no data was saved
            var dataCount = await _context.RainRecords.CountAsync();
            dataCount.Should().Be(0);
        }

        [Test]
        public async Task RecordRainData_WithEmptyUserId_ReturnsBadRequest()
        {
            // Arrange
            var request = TestDataBuilder.CreateRainDataRequest();

           
            TestDataBuilder.SetupMockHttpContext(_controller, "  ");

            // Act
            var result = await _controller.Post(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Test]
        public async Task RecordRainData_WithNullRequest_ReturnsBadRequest()
        {
            // Arrange
            var userId = TestDataBuilder.CreateUserId();

            
            TestDataBuilder.SetupMockHttpContext(_controller, userId);

            // Act
            var result = await _controller.Post(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

       

        [Test]
        public async Task RecordRainData_MultipleTimes_SavesAllRecords()
        {
            // Arrange
            var userId = TestDataBuilder.CreateUserId();
            var request1 = TestDataBuilder.CreateRainDataRequest(true);
            var request2 = TestDataBuilder.CreateRainDataRequest(false);

            
            TestDataBuilder.SetupMockHttpContext(_controller, userId);

            // Act
            var result1 = await _controller.Post( request1);
            var result2 = await _controller.Post( request2);

            // Assert
            result1.Should().BeOfType<CreatedAtActionResult>();
            result2.Should().BeOfType<CreatedAtActionResult>();

            var savedData = await _context.RainRecords.Where(x => x.UserId == userId).ToListAsync();
            savedData.Should().HaveCount(2);
            savedData.Should().Contain(x => x.Rain == true);
            savedData.Should().Contain(x => x.Rain == false);
        }

        [Test]
        public async Task RecordRainData_WithFalseRain_SavesCorrectly()
        {
            // Arrange
            var userId = TestDataBuilder.CreateUserId();
            var request = TestDataBuilder.CreateRainDataRequest(false);
            TestDataBuilder.SetupMockHttpContext(_controller, userId);

            // Act
            var result = await _controller.Post( request);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();

            var savedData = await _context.RainRecords.FirstOrDefaultAsync(x => x.UserId == userId);
            savedData.Should().NotBeNull();
            savedData!.Rain.Should().BeFalse();
        }

        #endregion

        #region Integration Tests

        [Test]
        public async Task RecordAndRetrieve_DataFlow_WorksCorrectly()
        {
            // Arrange
            var userId = TestDataBuilder.CreateUserId();
            var rainData = new[] { true, false, true };


            TestDataBuilder.SetupMockHttpContext(_controller, userId);
            // Act - Record multiple data points
            foreach (var rain in rainData)
            {
                var request = TestDataBuilder.CreateRainDataRequest(rain);
                var recordResult = await _controller.Post( request);
                recordResult.Should().BeOfType<CreatedAtActionResult>();
            }

            // Act - Retrieve data
            var getResult = await _controller.Get();

            // Assert
            getResult.Should().NotBeNull();
            getResult.Result.Should().BeOfType<OkObjectResult>();

            var okResult = getResult.Result as OkObjectResult;
            var response = okResult!.Value as RainDataResponse;

            response.Should().NotBeNull();
            response!.Data.Should().HaveCount(3);
            response.Data.Should().BeInDescendingOrder(x => x.Timestamp);

            // Verify the data matches what was recorded (in reverse order due to sorting)
            var dataArray = response.Data.ToArray();
            dataArray[0].Rain.Should().Be(true);  // Last recorded (most recent)
            dataArray[1].Rain.Should().Be(false); // Second recorded
            dataArray[2].Rain.Should().Be(true);  // First recorded (oldest)
        }

        #endregion

        #endregion

    }
}