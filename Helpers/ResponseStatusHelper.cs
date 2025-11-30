namespace GBSWarehouse.Helpers
{
    public class ResponseStatusHelper
    {
        public static ResponseStatus SuccessResponseStatus(string StatusMessageEn,string StatusMessageAr, string appLang)
        {
            return new ResponseStatus { IsSuccess = true, StatusMessage = (appLang=="ar")?StatusMessageAr:StatusMessageEn };
        }
        public static ResponseStatus ErrorResponseStatus(string StatusMessageEn, string StatusMessageAr, string appLang)
        {
            return new ResponseStatus { IsSuccess = false, StatusMessage = (appLang == "ar") ? StatusMessageAr : StatusMessageEn };
        }
        public static ResponseStatus ExceptionResponseStatus(string ErrorMessage,string appLang)
        {
            return new ResponseStatus { IsSuccess = false, StatusMessage = (appLang == "ar")? "حدثت مشكلة في الشبكة. يرجى الاتصال بمسؤول الشبكة" : "A network problem has occurred. Please contact your network administrator",ErrorMessage=ErrorMessage };
        }
    }
}
