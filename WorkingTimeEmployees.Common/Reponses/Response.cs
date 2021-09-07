using System;

namespace WorkingTimeEmployees.Common.Reponses
{
    public class Response
    {
        public int IdEmployees { get; set; }
        public DateTime RegistrationDateTime { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }

    }
}
