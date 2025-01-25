namespace AplikacjaMedyczna
{
    public static class SharedData
    {
        public static string PrimaryPesel { get; set; }  // patient pesel   
        public static string pesel { get; set; }  // patient pesel that can change
        public static string id { get; set; }  // worker id
        public static string rola { get; set; }  // worker role
    }
}