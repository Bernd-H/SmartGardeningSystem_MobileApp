namespace MobileApp.Common.Specifications.DataAccess {
    public interface IMailClient {

        void TrySendMail(string fileAttachment, string subject = "SmartGardening_MobileApp", string body = "mail with attachment");
    }
}
