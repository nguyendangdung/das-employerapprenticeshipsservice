namespace SFA.DAS.EAS.Domain.Models.UserProfile
{
    public class UserAccountSetting
    {
        protected UserAccountSetting()
        { }

        public virtual long Id { get; protected set; }

        public virtual long AccountId { get; protected set; }

        public virtual long UserId { get; protected set; }

        public virtual User User { get; protected set; }

        public virtual Data.Entities.Account.Account Account { get; protected set; }

        public virtual bool ReceiveNotifications { get; protected set; }
    }
}
