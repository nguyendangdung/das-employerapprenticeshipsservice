﻿using System.ComponentModel.DataAnnotations;
using SFA.DAS.EAS.Application.Commands.SendTransferConnectionInvitation;
using SFA.DAS.EAS.Application.Dtos;

namespace SFA.DAS.EAS.Web.ViewModels.TransferConnectionInvitations
{
    public class SendTransferConnectionInvitationViewModel : ViewModel<SendTransferConnectionInvitationCommand>
    {
        [Required(ErrorMessage = "Option required")]
        [RegularExpression("Confirm|ReEnterAccountId", ErrorMessage = "Option required")]
        public string Choice { get; set; }
        
        public AccountDto ReceiverAccount { get; set; }

        [Required]
        public SendTransferConnectionInvitationCommand SendTransferConnectionInvitationCommand { get; set; }

        public override void Map(SendTransferConnectionInvitationCommand message)
        {
            SendTransferConnectionInvitationCommand = message;
        }
    }
}