using System.Threading.Tasks;
using System.Web.Mvc;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Application.Commands.SendTransferConnectionInvitation;
using SFA.DAS.EAS.Web.Controllers;
using SFA.DAS.EAS.Web.ViewModels.TransferConnectionInvitations;

namespace SFA.DAS.EAS.Web.UnitTests.Controllers.TransferConnectionInvitationsControllerTests
{
    [TestFixture]
    public class WhenISubmitTheSendTransferConnectionInvitationPage
    {
        private const long TransferConnectionId = 123;

        private TransferConnectionInvitationsController _controller;
        private SendTransferConnectionInvitationViewModel _viewModel;
        private readonly Mock<IMediator> _mediator = new Mock<IMediator>();

        [SetUp]
        public void Arrange()
        {
            _mediator.Setup(m => m.SendAsync(It.IsAny<IAsyncRequest<long>>())).ReturnsAsync(TransferConnectionId);

            _controller = new TransferConnectionInvitationsController(null, _mediator.Object);

            _viewModel = new SendTransferConnectionInvitationViewModel
            {
                SendTransferConnectionInvitationCommand = new SendTransferConnectionInvitationCommand()
            };
        }

        [Test]
        public async Task ThenASendTransferConnectionCommandShouldBeSentIfIChoseOption1()
        {
           _viewModel.Choice = "Confirm";

            await _controller.Send(_viewModel);

            _mediator.Verify(m => m.SendAsync(_viewModel.SendTransferConnectionInvitationCommand), Times.Once);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToTheSentTransferConnectionPageIfIChoseOption1()
        {
            _viewModel.Choice = "Confirm";

            var result = await _controller.Send(_viewModel) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues.TryGetValue("action", out var actionName), Is.True);
            Assert.That(actionName, Is.EqualTo("Sent"));
            Assert.That(result.RouteValues.ContainsKey("controller"), Is.False);
            Assert.That(result.RouteValues.TryGetValue("transferConnectionInvitationId", out var transferConnectionId), Is.True);
            Assert.That(transferConnectionId, Is.EqualTo(TransferConnectionId));
        }

        [Test]
        public async Task ThenASendTransferConnectionCommandShouldNotBeSentIfIChoseOption2()
        {
            _viewModel.Choice = "GoToTransfersPage";

            await _controller.Send(_viewModel);

            _mediator.Verify(m => m.SendAsync(_viewModel.SendTransferConnectionInvitationCommand), Times.Never);
        }

        [Test]
        public async Task ThenIShouldBeRedirectedToTheTransfersPageIfIChoseOption2()
        {
            _viewModel.Choice = "GoToTransfersPage";

            var result = await _controller.Send(_viewModel) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues.TryGetValue("action", out var actionName), Is.True);
            Assert.That(actionName, Is.EqualTo("Index"));
            Assert.That(result.RouteValues.TryGetValue("controller", out var controllerName), Is.True);
            Assert.That(controllerName, Is.EqualTo("Transfers"));
        }
    }
}