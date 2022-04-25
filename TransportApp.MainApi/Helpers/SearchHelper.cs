namespace TransportApp.MainApi.Helpers
{
    public static class SearchHelper
    {
        public static string DetectLanguage(string value)
        {
            //LanguageDetector detector = new LanguageDetector();
            //detector.AddLanguages("uk", "ru", "en");
            //return detector.Detect(value);

            return "ua";
        }

        public static string BuildSearchQuery(string searchText, string language, string cityCode)
        {
            string preQuery = (language, cityCode) switch
            {
                ("ua", _) => "Україна,Дніпро,",
                ("ru", _) => "Украина,Днепр,",
                ("en", _) => "Ukraine,Dnipro,",
                (_, _) => ""
            };
            return $"{preQuery}{searchText}";
        }
    }
}
