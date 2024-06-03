using ExploreRussia.Domain.Models;
using ExploreRussiaWebApp.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using Moq.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ExploreRussiaTestP
{
    public class GuideControllerTests
    {
        [Fact]
        public async Task Create_Post_ReturnsRedirectToActionResult_WhenModelIsValid()
        {
            // Arrange
            var guide = new Guide
            {
                LastName = "Test",
                FirstName = "John",
                Patronymic = "Doe",
                Phone = "1234567890",
                Email = "john.doe@example.com",
                ExperienceYears = 10,
                GenderId = 1
            };
            var guides = new List<Guide>().AsQueryable();

            var uploadedFileMock = new Mock<IFormFile>();
            uploadedFileMock.Setup(f => f.Length).Returns(1);
            uploadedFileMock.Setup(f => f.FileName).Returns("test.jpg");

            var contextMock = new Mock<ExploreRussiaContext>();
            contextMock.Setup(c => c.Guides).ReturnsDbSet(guides);

            var controller = new GuideController(contextMock.Object);

            // Act
            var result = await controller.Create(guide, null);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("EditList", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Create_Post_ReturnsViewResultWithModel_WhenModelStateIsInvalid()
        {
            // Arrange
            var guide = new Guide
            {
                LastName = "Test",
                FirstName = "John"
            }; // Missing required fields
            var guides = new List<Guide>
            {
                new Guide { Id = 1, LastName = "Existing", FirstName = "Guide", Patronymic = "Pat", Phone = "0987654321", Email = "existing.guide@example.com", ExperienceYears = 5, GenderId = 1 }
            }.AsQueryable();
            var genders = new List<Gender>
            {
                new Gender { Id = 1, Name = "Male" },
                new Gender { Id = 2, Name = "Female" }
            }.AsQueryable();
            guides = guides.Append(guide);

            var contextMock = new Mock<ExploreRussiaContext>();
            contextMock.Setup(c => c.Guides).ReturnsDbSet(guides);
            contextMock.Setup(c => c.Genders).ReturnsDbSet(genders);
            var controller = new GuideController(contextMock.Object);
            controller.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await controller.Create(guide, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(guide, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_ReturnsViewResultWithError_WhenFileExtensionIsInvalid()
        {
            // Arrange
            var guide = new Guide
            {
                LastName = "Test",
                FirstName = "John",
                Patronymic = "Doe",
                Phone = "1234567890",
                Email = "john.doe@example.com",
                ExperienceYears = 10,
                GenderId = 1,
                ImageUrl = "/images/test.jpg"
            };
            var guides = new List<Guide>
            {
                new Guide { Id = 1, LastName = "Existing", FirstName = "Guide", Patronymic = "Pat", Phone = "0987654321", Email = "existing.guide@example.com", ExperienceYears = 5, GenderId = 1 }
            }.AsQueryable();

            var genders = new List<Gender>
            {
                new Gender { Id = 1, Name = "Male" },
                new Gender { Id = 2, Name = "Female" }
            }.AsQueryable();
            guides = guides.Append(guide);
            var uploadedFileMock = new Mock<IFormFile>();
            uploadedFileMock.Setup(f => f.Length).Returns(1);
            uploadedFileMock.Setup(f => f.FileName).Returns("test.png");

            var contextMock = new Mock<ExploreRussiaContext>();
            contextMock.Setup(c => c.Guides).ReturnsDbSet(guides);
            contextMock.Setup(c => c.Genders).ReturnsDbSet(genders);

            var controller = new GuideController(contextMock.Object);

            // Act
            var result = await controller.Create(guide, uploadedFileMock.Object);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(guide, viewResult.Model);
            Assert.True(controller.ModelState.ContainsKey(""));
            Assert.Contains(controller.ModelState[""].Errors, e => e.ErrorMessage == "Допустимы только файлы с расширениями .jpg или .jpeg.");
        }

        [Fact]
        public async Task Create_Post_AddsGuideToContext_WhenModelIsValid()
        {
            // Arrange
            var guide = new Guide
            {
                LastName = "Test",
                FirstName = "John",
                Patronymic = "Doe",
                Phone = "1234567890",
                Email = "john.doe@example.com",
                ExperienceYears = 10,
                GenderId = 1,
                ImageUrl = "/images/test.jpg"
            };
            var guides = new List<Guide>
            {
                new Guide { Id = 1, LastName = "Existing", FirstName = "Guide", Patronymic = "Pat", Phone = "0987654321", Email = "existing.guide@example.com", ExperienceYears = 5, GenderId = 1 }
            }.AsQueryable();

            var genders = new List<Gender>
            {
                new Gender { Id = 1, Name = "Male" },
                new Gender { Id = 2, Name = "Female" }
            }.AsQueryable();

            var contextMock = new Mock<ExploreRussiaContext>();
            contextMock.Setup(c => c.Guides).ReturnsDbSet(guides);
            contextMock.Setup(c => c.Genders).ReturnsDbSet(genders);

            var controller = new GuideController(contextMock.Object);

            // Act
            await controller.Create(guide, null);

            // Assert
            contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
