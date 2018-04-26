# Digital Apprenticeships Service

## Employer Apprenticeship Service

|               |               |
| ------------- | ------------- |
|![crest](https://assets.publishing.service.gov.uk/government/assets/crests/org_crest_27px-916806dcf065e7273830577de490d5c7c42f36ddec83e907efe62086785f24fb.png)|Employer Apprenticeship Service|
| Build | ![Build Status](https://sfa-gov-uk.visualstudio.com/_apis/public/build/definitions/c39e0c0b-7aff-4606-b160-3566f3bbce23/101/badge) |
| Web  | https://manage-apprenticeships.service.gov.uk/  |

## Running Jobs

### Purpose
This page describes running the style of web job used by EAS that are based on Azure WebJob API rather than the old style, which was based on creating CLIs that ran on the cloud service VM.  


### Which jobs are the new style?

New style jobs are classes that implement the IJob interface.


Job Name                        | Set for automatic run?          | Description
--------------------------------|     :-------------:             |------------------------------
PaymentIntegrityCheckerJob      | Yes - 05:30 1st of each month   |
UpdateAccountPublicHashedIdsJob | No                              |
GenerateAgreementsJob           | No                              |[Generate Agreements](GenerateAgreements.md "Generate Agreements")
ListRecentActivity              | No                              |
ListTriggeredJobs               | No                              |
   


### Which jobs are old style?

Old style jobs have a dedicated CLI.


## How do I execute a new style job?

The new style jobs may have a dedicated 'trigger' that causes it to run automatically. For example, on a timer basis or in response to posting a message to a queue. However, any job may be invoked manually by posting a message to the a specific queue.

###Running a Job Manually


> The objective here is to get a message into the queue named **adhocjobs** in the storage account used by the **SFA.DAS.EAS.Account.Worker** worker role.
 

You will need a tool such as [Microsoft Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/ "Microsoft Azure Storage Explorer") that allows you to post a message to an [Azure Storage Queues](https://azure.microsoft.com/en-gb/services/storage/queues "Azure Storage Queue"). The following screen shots will reference to Microsoft Azure Storage Explorer.
 

##Connect the tool to the  required storage account


![Open Account](AzureStorage_1.png)

##Open the Queues container in the relevant storage account 

![Open Account](AzureStorage_2.png)


The json message is posted to a queue named ashocjobs and is structured as follows:

    
    {
      "JobName": "GenerateAgreementsJob"
    }
    

The value chosen for JobName, which is not case sensitive, must be the name of a .net class (without the namespace prefix) that implements the IJob interface.
 
Once the message is posted to the queue it should disappear from the queue relatively quickly (anything from almost immediately up to a minute or so). This means the message has been picked up by the worker role hosting the web job and it is being processed.
 

**If the message does not disappear from the queue:**

- check the storage account the message was added to is the same storage account that is being monitored by the **SFA.DAS.EAS.Account.Worker** worker role.
- check that the worker role is running


**If the message appears in the adhocjobs_deadletter queue**
- the message has caused a run time exception and could not be processed. Check Kibana for error messages.

 
**If the message does appear in the dead letter queue but the job does not appear to do anything**
- check that the correct job name was specified
     

##Specific Jobs Details
[Generate Agreements](GenerateAgreements.md "Generate Agreements")
