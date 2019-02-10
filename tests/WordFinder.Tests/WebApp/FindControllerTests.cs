using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeItEasy;
using Domain.Interfaces;
using WebApp.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WordFinder.Tests.WebApp
{
    [TestClass]
    public class FindControllerTests
    {
        private IWordService _wordService;
        private FindController _findController;

        [TestInitialize]
        public void Init()
        {
            _wordService = A.Fake<IWordService>();
            _findController = new FindController(_wordService);
        }

        [TestMethod]
        public void When_Character_Is_Missing_Return_Bad_Request()
        {
            //arrange
            var characters = string.Empty;

            //act
            var result = _findController.Find(characters);

            //assert
            var badResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badResult);
            Assert.AreEqual(badResult.StatusCode, 400);
            A.CallTo(() => _wordService.Find(A<string>._)).MustNotHaveHappened();
        }

        [TestMethod]
        public void When_Character_Is_Passed_Find_Words()
        {
            //arrange
            var characters = "helloworld";
            A.CallTo(() => _wordService.Find(A<string>._)).Returns(new List<string> { "One match" });

            //act
            var result = _findController.Find(characters);

            //assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(okResult.StatusCode, 200);
            A.CallTo(() => _wordService.Find(A<string>._)).MustHaveHappenedOnceExactly();
        }
    }
}
