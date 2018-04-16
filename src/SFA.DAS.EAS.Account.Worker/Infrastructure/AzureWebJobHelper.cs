﻿using SFA.DAS.EAS.Account.Worker.Infrastructure.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.Account.Worker.Infrastructure
{
	public class AzureWebJobHelper : IAzureWebJobHelper
	{
		private readonly ITriggeredJobRepository _triggeredJobRepository;
		private readonly IAzureContainerRepository _azureContainerRepository;
		private readonly ILog _logger;

		public AzureWebJobHelper(ITriggeredJobRepository triggeredJobRepository, IAzureContainerRepository azureContainerRepository, ILog logger)
		{
			_triggeredJobRepository = triggeredJobRepository;
			_azureContainerRepository = azureContainerRepository;
			_logger = logger;
		}

		public void EnsureAllQueuesForTriggeredJobs()
		{
			foreach (var triggeredJob in _triggeredJobRepository.GetQueuedTriggeredJobs())
			{
				_azureContainerRepository.EnsureQueueExistsAsync(triggeredJob.Trigger.QueueName);	
			}
		}
	}
}