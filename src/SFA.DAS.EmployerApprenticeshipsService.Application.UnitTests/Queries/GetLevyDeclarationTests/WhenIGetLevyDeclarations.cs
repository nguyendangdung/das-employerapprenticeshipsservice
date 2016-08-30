﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerApprenticeshipsService.Application.Queries.GetLevyDeclaration;
using SFA.DAS.EmployerApprenticeshipsService.Application.Validation;
using SFA.DAS.EmployerApprenticeshipsService.Domain;
using SFA.DAS.EmployerApprenticeshipsService.Domain.Data;
using SFA.DAS.EmployerApprenticeshipsService.TestCommon.ObjectMothers;

namespace SFA.DAS.EmployerApprenticeshipsService.Application.UnitTests.Queries.GetLevyDeclarationTests
{
    public class WhenIGetLevyDeclarations : QueryBaseTest<GetLevyDeclarationQueryHandler,GetLevyDeclarationRequest, GetLevyDeclarationResponse>
    {
        private Mock<IDasLevyRepository> _repository;
        public override GetLevyDeclarationRequest Query { get; set; }
        public override GetLevyDeclarationQueryHandler RequestHandler { get; set; }
        public override Mock<IValidator<GetLevyDeclarationRequest>> RequestValidator { get; set; }
        private const long ExpectedAccountId = 757361;
        private List<LevyDeclarationView> _expectedLevyDeclarationViews;

        [SetUp]
        public void Arrange()
        {
            SetUp();

            Query = new GetLevyDeclarationRequest {AccountId = ExpectedAccountId};
            
            _repository = new Mock<IDasLevyRepository>();
            _expectedLevyDeclarationViews = LevyDeclarationViewsObjectMother.Create(ExpectedAccountId);
            _repository.Setup(x => x.GetAccountLevyDeclarations(It.IsAny<long>())).ReturnsAsync(_expectedLevyDeclarationViews);
            
            RequestHandler = new GetLevyDeclarationQueryHandler(_repository.Object, RequestValidator.Object);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheRepositoryIsCalled()
        {
            //Act
            await RequestHandler.Handle(Query);

            //Assert
            _repository.Verify(x=>x.GetAccountLevyDeclarations(ExpectedAccountId), Times.Once);
        }

        [Test]
        public override async Task ThenIfTheMessageIsValidTheValueIsReturnedInTheResponse()
        {
            //Act
            var actual = await RequestHandler.Handle(Query);

            //Assert
            Assert.IsNotNull(actual);
            Assert.AreEqual(_expectedLevyDeclarationViews[0].AccountId,actual.Data.AccountId);
            Assert.AreEqual(_expectedLevyDeclarationViews[0].EmpRef,actual.Data.Data[0].EmpRef);
            Assert.AreEqual(_expectedLevyDeclarationViews[0].EnglishFraction,actual.Data.Data[0].EnglishFraction);
            Assert.AreEqual(_expectedLevyDeclarationViews[0].SubmissionDate,actual.Data.Data[0].SubmissionDate);
        }
    }
}